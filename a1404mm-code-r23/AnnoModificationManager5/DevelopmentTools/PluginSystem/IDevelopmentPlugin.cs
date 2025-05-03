using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace DevelopmentTools.PluginSystem
{
    public interface IDevelopmentPlugin
    {
        Project Project { get; set; }

        ImageSource Icon { get; }
        string Name { get; }
        string Description { get; }
        Version Version { get; }

        void RunPlugin();
        void Dispose();
    }
}
