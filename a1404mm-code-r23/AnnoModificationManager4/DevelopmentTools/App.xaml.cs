using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace DevelopmentTools
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //DevelopmentTools.Properties.Settings.Default.Upgrade();
            //Load all Plugins
            PluginSystem.PluginHandler.LoadPlugins();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            //Dispose all Plugins
            PluginSystem.PluginHandler.DisposePlugins();
        }
    }
}
