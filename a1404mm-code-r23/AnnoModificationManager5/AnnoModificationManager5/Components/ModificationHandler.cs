using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AnnoModificationManager5.ModificationTypes;
using System.Threading;
using System.IO;
using AnnoModificationManager5.Misc;
using Ionic.Zip;
using System.Xml;
using AnnoModificationManager5.UserInterface.Misc;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using AnnoModificationManager5.UserInterface.MainUI;
using AnnoModificationManager5.Language.DictionarySystem;

namespace AnnoModificationManager5.Components
{
    public class ModificationHandler
    {
        #region Singleton
        private static ModificationHandler _Instance;
        public static ModificationHandler Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new ModificationHandler();
                return _Instance;
            }
        }
        #endregion

        private static List<Modification> _Modifications = new List<Modification>();
        public static List<Modification> Modifications
        {
            get
            {
                return _Modifications;
            }
            set
            {
                _Modifications = value;
            }
        }

        public static Dictionary<Modification, ModificationActivationResponse> ActivationResponses
            = new Dictionary<Modification, ModificationActivationResponse>();

        private ModificationHandler()
        {
        }

        public void UpdateActivationResponses(BackgroundWorker wrk)
        {
            wrk.DoWork += delegate(object sender, DoWorkEventArgs e)
            {
                ActivationResponses.Clear();
                wrk.ReportProgress(0);

                for (int i = 0; i < Modifications.Count; i++)
                {
                    //wrk.ReportProgress(ProgressBarExtension.Calculate(i, Modifications.Count));
                    ActivationResponses.Add(Modifications[i], Modifications[i].CheckActivation());
                }
            };
            wrk.RunWorkerAsync();
        }

        public void LoadModifications(BackgroundWorker wrk)
        {
            //_LoadModifications(wrk);

            wrk.DoWork += delegate(object sender, DoWorkEventArgs e)
            {
                _LoadModifications(wrk);
            };
            wrk.RunWorkerAsync();
        }

        /// <summary>
        /// Sync main method
        /// </summary>
        /// <param name="wrk"></param>
        public void _LoadModifications(BackgroundWorker wrk)
        {
            Modifications.Clear();
            ActivationResponses.Clear();

            string folder = DirectoryExtension.GetAMM4ApplicationDataFolder() + "\\Modifications";

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string[] dirs = Directory.GetDirectories(folder);
            float max = dirs.Length;

            int progressc = 0;
            for (int i = 0; i < dirs.Length; i++)
            {
                if (wrk != null)
                {
                    int progress = ProgressBarExtension.Calculate(i, max);
                    wrk.ReportProgress(progress);

                    if (progress >= progressc)
                    {
                        progressc += 20;
                        wrk.ReportProgress(0);
                    }
                }

                bool remd = false;

                //Load modification
                Modification mod = new Modification();
                //try
                {
                    App.Current.Dispatch(app =>
                    {
                        Modifications.Add(mod);
                    });
                    if (File.Exists(dirs[i].TrimEnd('\\') + "\\config.xml"))
                    {
                        mod.OpenFolder(dirs[i]);
                    }
                    else
                    {
                        try
                        {
                            Directory.Delete(dirs[i], true);
                        }
                        catch (Exception) { }
                    }

                    //Check compatibility
                    if (!mod.Info.SupportsAMM4RDA)
                    {
                        try
                        {
                            Directory.Delete(dirs[i], true);
                        }
                        catch (Exception) { }

                        continue;
                    }

                    //Check Activation                  
                    ActivationResponses.Add(mod, mod.CheckActivation());
                }
                /*catch (Exception)
                {
                    ////////
                    throw;

                    try
                    {
                        Directory.Delete(dirs[i], true);
                        remd = true;
                    }
                    catch (Exception) { }
                    App.Current.Dispatch(app =>
                    {
                        if (Modifications.Contains(mod))
                            Modifications.Remove(mod);
                    });
                }

                if (remd)
                {
                    MessageWindow.Show(LanguageDictionary.Get("Modification", "OnError"));
                }*/
            }
        }

        public bool AddModification(string filename)
        {
            try
            {
                return AddModificationNoErrorRoutine(filename);
            }
            catch (Exception ex)
            {
                MessageWindow.Show(ex.Message);
                return false;
            }
        }

        public bool AddModificationNoErrorRoutine(string filename)
        {
            string folder = DirectoryExtension.GetAMM4ApplicationDataFolder() + "\\Modifications";

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            ZipFile zip = new ZipFile(filename);

            //Read config.xml 
            MemoryStream stream = new MemoryStream();
            zip["config.xml"].Extract(stream);
            stream.Position = 0;

            string identifier = "";
            string shortidentifier = "";

            using (StreamReader read = new StreamReader(stream))
            {
                string xml = read.ReadToEnd();

                ModificationInfo mi = ModificationInfo.FromXmlData(xml);
                identifier = mi.GetIdentificationString;
                shortidentifier = mi.GetShortIdentificationString;
            }

            stream.Close();

            if (Modifications.Find(m => m.Info.GetIdentificationString == identifier) != null)
            {
                MessageWindow.Show(Language.DictionarySystem.LanguageDictionary.
                  Get("Modification", "Handler_Add_AlreadyExisting").Replace("{0}", Path.GetFileName(filename)));

                //Dispose
                if (zip != null)
                {
                    zip.Dispose();
                    zip = null;
                    GC.Collect();
                }

                return false;
            }

            while (Directory.Exists(folder + "\\" + shortidentifier))
            {
                //Make unique
                shortidentifier += RandomProvider.Random.Next(0, 9);
            }

            //Create folder
            Directory.CreateDirectory(folder + "\\" + shortidentifier);

            //Extract
            try
            {
                zip.ExtractAll(folder + "\\" + shortidentifier, ExtractExistingFileAction.OverwriteSilently);
            }
            catch (PathTooLongException)
            {
                (new ChangeDataFolderDialog()).ShowDialog();

                folder = DirectoryExtension.GetAMM4ApplicationDataFolder() + "\\Modifications";
                zip.ExtractAll(folder + "\\" + shortidentifier, ExtractExistingFileAction.OverwriteSilently);
            }

            //Dispose
            zip.Dispose();
            zip = null;
            GC.Collect();

            return true;
        }

        public void RemoveModification(Modification mod)
        {
        }

        public bool IsCompatible(Modification mod)
        {
            return AnnoVersionHandler.IsCompatible(mod.Info.AnnoVersions);
        }
    }
}
