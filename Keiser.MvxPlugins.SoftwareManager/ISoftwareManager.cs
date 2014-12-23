namespace Keiser.MvxPlugins.SoftwareManager
{
    using System;
    using System.Threading.Tasks;

    public interface ISoftwareManager
    {
        SoftwareVersion CurrentVersion { get; }
        bool UpdateAvailable { get; }
        SoftwareVersion UpdateVersion { get; }
        void CheckForUpdate(string url);
        Task CheckForUpdateTask(string url);
        void DoUpdate();
        Task DoUpdateTask();
        event EventHandler UpdaveAvailableHandler;
        Platform Platform { get; }
    }
}
