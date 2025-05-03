using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using AnnoModificationManager5.Misc;
using System.Reflection;

namespace DevelopmentTools.PluginSystem
{
    public class PluginHandler
    {
        public static List<IDevelopmentPlugin> Plugins
            = new List<IDevelopmentPlugin>();

        public static void LoadPlugins()
        {
            Plugins.Clear();

            if (Directory.Exists(DirectoryExtension.GetApplicationFolder() + "\\DevelopmentPlugins"))
            {
                foreach (string file in 
                    Directory.GetFiles(DirectoryExtension.GetApplicationFolder() + "\\DevelopmentPlugins", "*.dll"))
                {
                    try
                    {
                        AddPlugin(file);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        public static void UpdatePlugins()
        {
            foreach (IDevelopmentPlugin plug in Plugins)
                plug.Project = Project.Development_CurrentProject;           
        }

        public static void DisposePlugins()
        {
            foreach (IDevelopmentPlugin plug in Plugins)
                plug.Dispose();
            Plugins.Clear();
        }

        private static void AddPlugin(string file)
        {
            Assembly assembly = Assembly.LoadFile(file);

            foreach (Type pluginType in assembly.GetTypes())
            {
                if (pluginType.IsPublic && !pluginType.IsAbstract)
                {
                    Type typeInterface = pluginType.GetInterface("DevelopmentTools.PluginSystem.IDevelopmentPlugin", true);
                    if (typeInterface != null)
                    {
                        IDevelopmentPlugin plugin = (IDevelopmentPlugin)Activator.CreateInstance(pluginType);
                        Plugins.Add(plugin);
                    }
                }
            }
        }
    }
}
