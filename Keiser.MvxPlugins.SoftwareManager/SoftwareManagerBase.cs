namespace Keiser.MvxPlugins.SoftwareManager
{
    using Cirrious.CrossCore.Platform;
    using System;
    using System.Threading.Tasks;

    public class SoftwareManagerBase : ISoftwareManager
    {
        public event EventHandler UpdaveAvailableHandler;
        protected static IMvxJsonConverter JsonConverter;

        public SoftwareManagerBase(IMvxJsonConverter jsonConverter)
        {
            JsonConverter = jsonConverter;
        }

        private object _currentVersionLocker = new object();
        private SoftwareVersion _currentVersion;
        public virtual SoftwareVersion CurrentVersion
        {
            get
            {
                lock (_currentVersionLocker)
                    return _currentVersion;
            }
            protected set
            {
                lock (_currentVersionLocker)
                    _currentVersion = value;
            }
        }

        private object _updateAvailableLocker = new object();
        private bool _updateAvailable = false;
        public virtual bool UpdateAvailable
        {
            get
            {
                lock (_updateAvailableLocker)
                    return _updateAvailable;
            }
            protected set
            {
                lock (_updateAvailableLocker)
                    _updateAvailable = true;
                if (value)
                    UpdateAvailableEvent();
            }
        }

        private object _updateVersionLocker = new object();
        private SoftwareVersion _updateVersion;
        public virtual SoftwareVersion UpdateVersion
        {
            get
            {
                lock (_updateVersionLocker)
                    return _updateVersion;
            }
            protected set
            {
                lock (_updateVersionLocker)
                    _updateVersion = value;
            }
        }

        public virtual void CheckForUpdate(string url) { throw new NotImplementedException(); }

        public virtual Task CheckForUpdateTask(string url)
        {
            return new Task(() => { CheckForUpdate(url); });
        }

        public virtual void DoUpdate() { throw new NotImplementedException(); }

        public virtual Task DoUpdateTask()
        {
            return new Task(DoUpdate);
        }

        protected void UpdateAvailableEvent()
        {
            Task UpdateAvailableEventTask = new Task(() => { UpdaveAvailableHandler(this, EventArgs.Empty); });
            UpdateAvailableEventTask.Start();
        }

        public virtual Platform Platform { get { throw new NotImplementedException(); } }

        private static object _updateLocker = new object();
        private static bool _doingUpdate = false;
        public bool DoingUpdate { get { lock (_updateLocker) return _doingUpdate; } protected set { lock (_updateLocker) _doingUpdate = value; } }
    }
}
