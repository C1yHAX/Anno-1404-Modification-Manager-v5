using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AnnoModificationManager4.ModificationTypes.XmlModule.XMLModifiers;
using AnnoModificationManager4.ModificationTypes.ListModule.ListModifiers;
using AnnoModificationManager4.ModificationTypes;
using System.IO;
using AnnoModificationManager4.Misc;
using AnnoModificationManager4.ModificationTypes.XmlModule;
using AnnoModificationManager4.ModificationTypes.ListModule;

namespace DevelopmentTools.Editors.ModuleManager
{
    public class ModuleManager_OriginalFile
    {
        public string File { get; set; }

        public List<IXMLModifier> XmlModifiers = new List<IXMLModifier>();
        public List<IListModifier> ListModifiers = new List<IListModifier>();

        public void SetFile(string newfile)
        {
            //Set IXML and ILIST
            foreach (IXMLModifier mod in XmlModifiers)
                mod.File = newfile;
            foreach (IListModifier mod in ListModifiers)
                mod.File = newfile;

            //Finally, set dest.
            File = newfile;

            //Refresh NodeDictionary
            Modification.Development_CurrentModification.ModificationUtils.RefreshModifierDictionaries();
        }

        #region generate
        public static List<ModuleManager_OriginalFile> Generate()
        {
            List<ModuleManager_OriginalFile> output = new List<ModuleManager_OriginalFile>();

            foreach (IXMLModifier mod in Modification.Development_CurrentModification.ModificationUtils.Xml_AllModififers)
            {
                ModuleManager_OriginalFile toadd = output.Find(mo => (mo.File == mod.File));
                //If not found
                if (toadd == null)
                {
                    toadd = new ModuleManager_OriginalFile()
                    {
                        File = mod.File
                    };
                    output.Add(toadd);
                }

                //Add to gen.
                toadd.XmlModifiers.Add(mod);
            }
            foreach (IListModifier mod in Modification.Development_CurrentModification.ModificationUtils.List_AllModififers)
            {
                ModuleManager_OriginalFile toadd = output.Find(mo => (mo.File == mod.File));
                //If not found
                if (toadd == null)
                {
                    toadd = new ModuleManager_OriginalFile()
                    {
                        File = mod.File
                    };
                    output.Add(toadd);
                }

                //Add to gen.
                toadd.ListModifiers.Add(mod);
            }

            return output;
        }
        #endregion
    }
}
