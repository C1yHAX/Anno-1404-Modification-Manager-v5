using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;
using System.Web;
using AnnoModificationManager4.UserInterface.Misc;
using AnnoModificationManager4.Misc;
using AnnoModificationManager4.Components;
using System.ComponentModel;

namespace AnnoModificationManager4.DownloadService
{
    public class ModificationInfoConnector
    {
        #region Singleton
        private static ModificationInfoConnector _Instance;
        public static ModificationInfoConnector Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new ModificationInfoConnector();
                return _Instance;
            }
        }
        #endregion

        public List<ModificationInfo> AvailablePackages = new List<ModificationInfo>();

        private ModificationInfoConnector()
        {
            LoadFromFolder();
        }

        public void LoadFromFolder()
        {
            AvailablePackages.Clear();

            string folder = DirectoryExtension.GetAMM4ApplicationDataFolder() + "\\OnlinePackageCache";

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            foreach (string file in Directory.GetFiles(folder, "*.xml"))
            {
                ModificationInfo loaded = ModificationInfo.FromXml(file);
                if (loaded != null)
                {
                    loaded.Date = File.GetLastWriteTime(file);
                    AvailablePackages.Add(loaded);
                }
            }
        }

        public void SaveToFile(ModificationInfo mod)
        {
            string folder = DirectoryExtension.GetAMM4ApplicationDataFolder() + "\\OnlinePackageCache";

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            mod.ToXml(folder + "\\" + mod.GetIdentificationString + ".xml");
            File.SetLastWriteTime(folder + "\\" + mod.GetIdentificationString + ".xml", mod.Date);
        }

        public void Upload(ModificationInfo mod)
        {
            //Save
            string folder = Path.GetTempPath().Trim('\\') + "\\DevelopmentTools4\\Temp";
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            if (File.Exists(folder + "\\" + mod.GetIdentificationString + ".xml"))
                File.Delete(folder + "\\" + mod.GetIdentificationString + ".xml");

            mod.ToXml(folder + "\\" + mod.GetIdentificationString + ".xml");

            //Upload             
            System.Net.WebClient Client = new System.Net.WebClient();
            Client.Headers.Add("Content-Type", "binary/octet-stream");
            byte[] result = Client.UploadFile("http://tilegame.bplaced.net/AMM4.2Modifications/uploadFile.php", "POST", folder + "\\" + mod.GetIdentificationString + ".xml");
            string s = System.Text.Encoding.UTF8.GetString(result, 0, result.Length);

            MessageWindow.Show(s);
        }

        public void Delete(ModificationInfo mod)
        {
            WebRequest req = WebRequest.Create("http://tilegame.bplaced.net/AMM4.2Modifications/deleteFile.php?file=" + mod.GetIdentificationString + ".xml");

            req.Credentials = CredentialCache.DefaultCredentials;
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

            Stream resp_stream = resp.GetResponseStream();
            StreamReader resp_strem_reader = new StreamReader(resp_stream);

            string resp_output = resp_strem_reader.ReadToEnd();

            resp_strem_reader.Close();
            resp_stream.Close();
            resp.Close();

            MessageWindow.Show(resp_output);
        }

        public void RefreshAsync(BackgroundWorker wrk, bool errormessages)
        {
            wrk.DoWork += delegate(object sender, DoWorkEventArgs e)
            {
                Refresh(wrk, errormessages);
            };
            wrk.RunWorkerAsync();
        }

        public void Refresh(BackgroundWorker wrk, bool errormessages)
        {
            LoadFromFolder();
            List<ModificationInfo> loadedPackages = new List<ModificationInfo>();

            try
            {
                WebRequest req = WebRequest.Create("http://tilegame.bplaced.net/AMM4.2Modifications/listFiles.php");
                req.Timeout = 2500;
                req.Credentials = CredentialCache.DefaultCredentials;
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

                Stream resp_stream = resp.GetResponseStream();
                StreamReader resp_strem_reader = new StreamReader(resp_stream);

                string resp_output = resp_strem_reader.ReadToEnd();

                resp_strem_reader.Close();
                resp_stream.Close();
                resp.Close();

                int progress_count = resp_output.Split('\n').Length;
                int progress_current = 0;
                if (wrk != null)
                    wrk.ReportProgress(0);

                foreach (string file in resp_output.Split('\n'))
                {
                    progress_current++;
                    if (wrk != null)
                        wrk.ReportProgress(ProgressBarExtension.Calculate(progress_current, progress_count));

                    try
                    {
                        if (Path.GetExtension(file.Split('|')[0]).ToLower() == ".xml")
                        {
                            string fileName = file.Split('|')[0];
                            DateTime date = DateTime.Parse(file.Split('|')[1]);

                            ModificationInfo info = AvailablePackages.Find(av =>
                            {
                                return av.GetIdentificationString == Path.GetFileNameWithoutExtension(fileName)
                                    && av.Date.Equals(date);
                            });
                            if (info == null)
                            {
                                TimeoutWebClient web = new TimeoutWebClient();
                                web.Encoding = Encoding.UTF8;
                                string xml = web.DownloadString("http://tilegame.bplaced.net/AMM4.2Modifications/" + fileName);

                                ModificationInfo loaded = ModificationInfo.FromXmlData(xml);
                                if (loaded != null)
                                {
                                    loaded.Date = date;
                                    AvailablePackages.Add(loaded);
                                    loadedPackages.Add(loaded);
                                }

                                SaveToFile(loaded);
                            }
                            else
                                loadedPackages.Add(info);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                if (errormessages)
                {
                    App.Current.Dispatch(app => MessageWindow.Show("Error: " + ex.Message));
                }
            }

            AvailablePackages.RemoveAll(z => !loadedPackages.Contains(z));

            //Get FileSizes
            //foreach (ModificationInfo info in AvailablePackages)
            //{
            //    info.DownloadUrlFileSize = WebExtension.GetFileSize(info.DownloadUrl);
            //}
        }

        public List<ModificationInfo> Filter(bool showhidden)
        {
            List<ModificationInfo> output = new List<ModificationInfo>();
            foreach (ModificationInfo mod in AvailablePackages)
            {
                if (!showhidden)
                {
                    if (mod.IsHidden)
                        continue;
                }
                if (ModificationHandler.Modifications.ToList().Find(m => m.Info.GetIdentificationString == mod.GetIdentificationString) != null)
                    continue;
                if (AnnoVersionHandler.IsCompatible(mod.AnnoVersions))
                    output.Add(mod);
            }

            return output;
        }
    }
}
