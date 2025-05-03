using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevelopmentTools.Editors.ListModule.AutoAssign
{
    public class SourceFileAutoAssign
    {
        public static Dictionary<string, string> Assigns = new Dictionary<string, string>();

        static SourceFileAutoAssign()
        {
            Load();
        }

        public static void Load()
        {
            Assigns.Clear();

            foreach (string i in Properties.Settings.Default.ListModuleCreator_AutoAssign.Split(';'))
            {
                if (!string.IsNullOrEmpty(i))
                {
                    Assigns.Add(i.Split('=')[0], i.Split('=')[1]);
                }
            }
        }

        public static void Save()
        {
            string list = "";
            foreach (KeyValuePair<string, string> ass in Assigns)
            {
                list += ass.Key + "=" + ass.Value + ";";
            }

            Properties.Settings.Default.ListModuleCreator_AutoAssign = list;
            Properties.Settings.Default.Save();
        }

        public static void Assign(string destination, string source)
        {
            if (!string.IsNullOrEmpty(destination) & !string.IsNullOrEmpty(source))
            {
                if (Assigns.ContainsKey(destination))
                    Assigns.Remove(destination);

                Assigns.Add(destination, source);
                Save();
            }
        }
    }
}
