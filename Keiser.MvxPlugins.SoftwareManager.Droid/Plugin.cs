namespace Keiser.MvxPlugins.SoftwareManager.Droid
{
    using Cirrious.CrossCore;
    using Cirrious.CrossCore.Plugins;


    public class Plugin
        : IMvxPlugin
    {
        public void Load()
        {
            Mvx.RegisterType<ISoftwareManager, SoftwareManager>();
        }
    }
}
