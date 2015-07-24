namespace Keiser.MvxPlugins.SoftwareManager.Droid
{
    using Android.Content;
    using Android.Content.PM;
    using Cirrious.CrossCore;
    using Cirrious.CrossCore.Platform;
    using Java.IO;
    using Java.Net;
    using Keiser.MvxPlugins.SoftwareManager;
    using System;
    using System.IO;
    using System.Text;

    public class SoftwareManager : SoftwareManagerBase
    {
        protected static Context Context = Android.App.Application.Context;

        public SoftwareManager(IMvxJsonConverter jsonConverter)
            : base(jsonConverter)
        {
            CurrentVersion = new SoftwareVersion
            {
                Name = CurrentPackage.VersionName,
                Number = CurrentPackage.VersionCode
            };
        }


        private bool IsNonPlayAppAllowed
        {
            get
            {
                return Android.Provider.Settings.Secure.GetInt(Context.ContentResolver, Android.Provider.Settings.Secure.InstallNonMarketApps) == 1;
            }
        }

        public override void CheckForUpdate(string url)
        {
            URL urlObject = new URL(url);
            HttpURLConnection connection = (HttpURLConnection)urlObject.OpenConnection();
            string streamString = "";
            try
            {
                connection.Connect();
            }
#if DEBUG
            catch (Exception e)
            {
                Trace.Error("Connection Error: " + e);
#else
            catch
            {
#endif
                return;
            }


            if (connection.ResponseCode == HttpStatus.Ok)
            {
                Stream inStream = connection.InputStream;
                byte[] buffer = new byte[1024];
                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = inStream.Read(buffer, totalBytesRead, buffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == buffer.Length)
                    {
                        int nextByte = inStream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[buffer.Length * 2];
                            Buffer.BlockCopy(buffer, 0, temp, 0, buffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            buffer = temp;
                            totalBytesRead++;
                        }
                    }
                }
                streamString = Encoding.UTF8.GetString(buffer, 0, totalBytesRead);
            }
            connection.Disconnect();
            SoftwareVersion updateVersion = GetSoftwareVersion(streamString);
#if DEBUG
            Trace.Info("Current Version: " + CurrentVersion.Number + " Update Vesrion:" + updateVersion.Number);
#endif
            if (true)
            {
                UpdateVersion = updateVersion;
                UpdateAvailable = true;
            }
#if DEBUG
            Trace.Info("Update Available: " + UpdateAvailable);
#endif
        }

        public override void DoUpdate()
        {
            if (DoingUpdate || !UpdateAvailable)
                return;
            DoingUpdate = true;
            if (!IsNonPlayAppAllowed)
            {
                Trace.Info(Context.PackageName);
                Intent intent = new Intent(Intent.ActionView, Android.Net.Uri.Parse("market://details?id=" + Context.PackageName));
                intent.SetFlags(ActivityFlags.NewTask);
                Context.StartActivity(intent);
            }
            else
            {
                string fileName = GetFileName(UpdateVersion.BinaryLink);
                string storageDirectory = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.Path, fileName);
                System.IO.File.Create(storageDirectory);
                URL url = new URL(UpdateVersion.BinaryLink);
                HttpURLConnection connection = (HttpURLConnection)url.OpenConnection();

                try
                {
                    connection.Connect();
                }
                catch
                {
                    return;
                }

                if (connection.ResponseCode == HttpStatus.Ok)
                {
                    FileOutputStream fileStream = new FileOutputStream(storageDirectory);

                    Stream inStream = connection.InputStream;
                    byte[] buffer = new byte[8192];
                    int totalBytesRead = 0;
                    int bytesRead;

                    while ((bytesRead = inStream.Read(buffer, totalBytesRead, buffer.Length - totalBytesRead)) > 0)
                    {
                        totalBytesRead += bytesRead;

                        if (totalBytesRead == buffer.Length)
                        {
                            int nextByte = inStream.ReadByte();
                            if (nextByte != -1)
                            {
                                byte[] temp = new byte[buffer.Length * 2];
                                Buffer.BlockCopy(buffer, 0, temp, 0, buffer.Length);
                                Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                                buffer = temp;
                                totalBytesRead++;
                            }
                        }
                    }
                    fileStream.Write(buffer, 0, totalBytesRead);
                    inStream.Close();

                    fileStream.Close();
                    connection.Disconnect();

                    Intent intent = new Intent(Intent.ActionView);
                    intent.SetDataAndType(Android.Net.Uri.Parse("file://" + storageDirectory), "application/vnd.android.package-archive");
                    intent.SetFlags(ActivityFlags.NewTask);
                    Context.StartActivity(intent);
                }
                else
                    connection.Disconnect();
            }
            DoingUpdate = false;
        }

        protected String GetFileName(string url)
        {
            Uri uri = new Uri(url);
            return uri.Segments[uri.Segments.Length - 1];
        }

        private PackageInfo _currentPackage;
        protected PackageInfo CurrentPackage
        {
            get
            {
                if (_currentPackage == null)
                    _currentPackage = Context.PackageManager.GetPackageInfo(Context.PackageName, (Android.Content.PM.PackageInfoFlags)0);
                return _currentPackage;
            }
        }

        public SoftwareVersion GetSoftwareVersion(string json)
        {
            return JsonConverter.DeserializeObject<SoftwareVersion>(json);
        }

        public override Platform Platform { get { return Platform.Droid; } }
    }
}
