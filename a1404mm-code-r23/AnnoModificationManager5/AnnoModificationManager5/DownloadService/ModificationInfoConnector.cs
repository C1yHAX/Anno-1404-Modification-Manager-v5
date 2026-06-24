using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;
using AnnoModificationManager5.UserInterface.Misc;
using AnnoModificationManager5.Misc;
using AnnoModificationManager5.Components;
using System.ComponentModel;

namespace AnnoModificationManager5.DownloadService
{
    public class ModificationInfoConnector
    {
        #region GitHub-Konfiguration
        private const string GitHubOwner = "C1yHAX";
        private const string GitHubRepo = "Modifications";
        private const string GitHubBranch = "main";
        private const string GitHubToken = "";

        private static string ApiTreeUrl
        {
            get
            {
                return "https://api.github.com/repos/" + GitHubOwner + "/" + GitHubRepo
                    + "/git/trees/" + GitHubBranch + "?recursive=1";
            }
        }

        private static string RawBaseUrl
        {
            get
            {
                return "https://raw.githubusercontent.com/" + GitHubOwner + "/" + GitHubRepo
                    + "/" + GitHubBranch + "/";
            }
        }
        #endregion

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

        private static string CacheFolder
        {
            get
            {
                string folder = DirectoryExtension.GetAMM4ApplicationDataFolder() + "\\OnlinePackageCache";
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
                return folder;
            }
        }

        public void LoadFromFolder()
        {
            AvailablePackages.Clear();

            string folder = CacheFolder;

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
            string folder = CacheFolder;

            mod.ToXml(folder + "\\" + mod.GetIdentificationString + ".xml");
            File.SetLastWriteTime(folder + "\\" + mod.GetIdentificationString + ".xml", mod.Date);
        }

        #region GitHub-Helfer
        private static string HttpGetString(string url)
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            using (TimeoutWebClient client = new TimeoutWebClient())
            {
                client.Encoding = Encoding.UTF8;
                client.Headers.Add(HttpRequestHeader.UserAgent, "AnnoModificationManager5");
                if (!string.IsNullOrEmpty(GitHubToken))
                    client.Headers.Add(HttpRequestHeader.Authorization, "token " + GitHubToken);
                return client.DownloadString(url);
            }
        }

        private static string RawUrl(string repoPath)
        {
            string[] parts = repoPath.Replace('\\', '/').Split('/');
            for (int i = 0; i < parts.Length; i++)
                parts[i] = Uri.EscapeDataString(parts[i]);
            return RawBaseUrl + string.Join("/", parts);
        }

        private static string CombineRepoDir(string repoFilePath, string relative)
        {
            string dir = repoFilePath.Replace('\\', '/');
            int idx = dir.LastIndexOf('/');
            dir = idx >= 0 ? dir.Substring(0, idx) : "";
            relative = relative.Replace('\\', '/').TrimStart('/');
            return string.IsNullOrEmpty(dir) ? relative : dir + "/" + relative;
        }

        private static void ResolveUrls(ModificationInfo info, string xmlPath)
        {
            if (string.IsNullOrEmpty(info.DownloadUrl) ||
                !info.DownloadUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                string siblingZip = xmlPath.Substring(0, xmlPath.Length - 4) + ".zip";
                info.DownloadUrl = RawUrl(siblingZip);
            }

            if (!string.IsNullOrEmpty(info.ImageUrl) &&
                !info.ImageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                info.ImageUrl = RawUrl(CombineRepoDir(xmlPath, info.ImageUrl));

            for (int i = 0; i < info.Images.Count; i++)
            {
                if (!string.IsNullOrEmpty(info.Images[i]) &&
                    !info.Images[i].StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    info.Images[i] = RawUrl(CombineRepoDir(xmlPath, info.Images[i]));
            }
        }
        #endregion

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
            List<ModificationInfo> loadedPackages = new List<ModificationInfo>();
            HashSet<string> liveShas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                string json = HttpGetString(ApiTreeUrl);

                JavaScriptSerializer ser = new JavaScriptSerializer();
                ser.MaxJsonLength = int.MaxValue;
                Dictionary<string, object> root = ser.DeserializeObject(json) as Dictionary<string, object>;
                if (root == null || !root.ContainsKey("tree"))
                    throw new Exception("Unerwartete GitHub-Antwort (kein 'tree').");

                object[] tree = (object[])root["tree"];

                List<KeyValuePair<string, string>> xmlBlobs = new List<KeyValuePair<string, string>>();
                foreach (object item in tree)
                {
                    Dictionary<string, object> entry = item as Dictionary<string, object>;
                    if (entry == null)
                        continue;

                    string type = entry.ContainsKey("type") ? entry["type"] as string : null;
                    string path = entry.ContainsKey("path") ? entry["path"] as string : null;
                    string sha = entry.ContainsKey("sha") ? entry["sha"] as string : null;

                    if (type != "blob" || string.IsNullOrEmpty(path))
                        continue;
                    if (path.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                        xmlBlobs.Add(new KeyValuePair<string, string>(path, sha));
                }

                int progress_count = Math.Max(1, xmlBlobs.Count);
                int progress_current = 0;
                if (wrk != null)
                    wrk.ReportProgress(0);

                foreach (KeyValuePair<string, string> blob in xmlBlobs)
                {
                    progress_current++;
                    if (wrk != null)
                        wrk.ReportProgress(ProgressBarExtension.Calculate(progress_current, progress_count));

                    string path = blob.Key;
                    string sha = blob.Value;
                    if (!string.IsNullOrEmpty(sha))
                        liveShas.Add(sha);

                    try
                    {
                        string cacheFile = string.IsNullOrEmpty(sha)
                            ? null
                            : CacheFolder + "\\" + sha + ".xml";

                        ModificationInfo info;
                        if (cacheFile != null && File.Exists(cacheFile))
                        {
                            info = ModificationInfo.FromXml(cacheFile);
                            if (info == null)
                                continue;
                            info.Date = File.GetLastWriteTime(cacheFile);
                        }
                        else
                        {
                            string xml = HttpGetString(RawUrl(path));
                            info = ModificationInfo.FromXmlData(xml);
                            if (info == null)
                                continue;
                            ResolveUrls(info, path);
                            info.Date = DateTime.Now;
                            if (cacheFile != null)
                                info.ToXml(cacheFile);
                        }

                        loadedPackages.Add(info);
                    }
                    catch (Exception)
                    {
                    }
                }

                foreach (string f in Directory.GetFiles(CacheFolder, "*.xml"))
                {
                    string stem = Path.GetFileNameWithoutExtension(f);
                    if (!liveShas.Contains(stem))
                    {
                        try { File.Delete(f); }
                        catch (Exception) { }
                    }
                }
            }
            catch (Exception ex)
            {
                if (errormessages)
                {
                    App.Current.Dispatch(app => MessageWindow.Show(
                        "Fehler beim Laden der Online-Pakete (GitHub): " + ex.Message));
                }

                LoadFromFolder();
                return;
            }

            AvailablePackages = loadedPackages
                .GroupBy(m => m.GetIdentificationString)
                .Select(g => g.OrderByDescending(m => m.Date).First())
                .ToList();
        }

        #region Veröffentlichen
        private static string RepoFolderConfigFile
        {
            get { return DirectoryExtension.GetAMM4ApplicationDataFolder() + "\\GitHubRepoFolder.txt"; }
        }

        public static string GetLocalRepoFolder(bool askIfMissing)
        {
            string path = null;
            try
            {
                if (File.Exists(RepoFolderConfigFile))
                    path = File.ReadAllText(RepoFolderConfigFile).Trim();
            }
            catch (Exception) { }

            if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
                return path;

            if (!askIfMissing)
                return null;

            using (System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog())
            {
                dlg.Description = "Lokalen Klon des GitHub-Repositorys 'Modifications' wählen " +
                                  "(hier werden .xml/.zip abgelegt).";
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    path = dlg.SelectedPath;
                    try { File.WriteAllText(RepoFolderConfigFile, path); }
                    catch (Exception) { }
                    return path;
                }
            }
            return null;
        }

        private static string SanitizeFileName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "modification";
            foreach (char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name;
        }

        public void Upload(ModificationInfo mod)
        {
            string folder = GetLocalRepoFolder(true);
            if (string.IsNullOrEmpty(folder))
            {
                MessageWindow.Show("Veröffentlichung abgebrochen: kein Repo-Ordner gewählt.");
                return;
            }

            string stem = SanitizeFileName(mod.GetShortIdentificationString);
            string xmlPath = folder + "\\" + stem + ".xml";

            mod.DownloadUrl = "";
            mod.ToXml(xmlPath);

            MessageWindow.Show(
                "Metadaten gespeichert:\n" + xmlPath + "\n\n" +
                "Lege die zugehörige Datei \"" + stem + ".zip\" in denselben Ordner und committe/pushe " +
                "das Repository, damit die Modifikation im Online-Browser erscheint.");
        }

        public void Delete(ModificationInfo mod)
        {
            string folder = GetLocalRepoFolder(true);
            if (string.IsNullOrEmpty(folder))
                return;

            string stem = SanitizeFileName(mod.GetShortIdentificationString);
            string xmlPath = folder + "\\" + stem + ".xml";

            try
            {
                if (File.Exists(xmlPath))
                    File.Delete(xmlPath);
            }
            catch (Exception ex)
            {
                MessageWindow.Show(ex.Message);
                return;
            }

            MessageWindow.Show(
                "Metadaten entfernt:\n" + xmlPath + "\n\n" +
                "Entferne ggf. auch \"" + stem + ".zip\" und committe/pushe das Repository.");
        }
        #endregion

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
