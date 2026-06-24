using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web.Script.Serialization;

namespace AnnoModificationManager5.Nexus
{
    public class NexusApiClient
    {
        private const string BaseUrl = "https://api.nexusmods.com";
        private readonly string _apiKey;

        public NexusApiClient(string apiKey)
        {
            _apiKey = apiKey;
        }

        private string GetJson(string url)
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "GET";
            req.Accept = "application/json";
            req.UserAgent = "AnnoModificationManager5/5.0";
            req.Headers["apikey"] = _apiKey;
            req.Headers["Application-Name"] = "AnnoModificationManager5";
            req.Headers["Application-Version"] = "5.0.0.0";

            using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
            using (StreamReader reader = new StreamReader(resp.GetResponseStream()))
                return reader.ReadToEnd();
        }

        private static object Deserialize(string json)
        {
            JavaScriptSerializer ser = new JavaScriptSerializer();
            ser.MaxJsonLength = int.MaxValue;
            return ser.DeserializeObject(json);
        }

        private static string StripHtml(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "";
            text = System.Text.RegularExpressions.Regex.Replace(text, "<[^>]+>", " ");
            text = System.Net.WebUtility.HtmlDecode(text);
            return System.Text.RegularExpressions.Regex.Replace(text, "\\s+", " ").Trim();
        }

        public NexusUser ValidateUser()
        {
            var data = (Dictionary<string, object>)Deserialize(GetJson(BaseUrl + "/v1/users/validate.json"));
            NexusUser user = new NexusUser();
            user.Name = data.ContainsKey("name") ? data["name"] as string : null;
            user.UserId = data.ContainsKey("user_id") ? Convert.ToInt32(data["user_id"]) : 0;
            user.IsPremium = data.ContainsKey("is_premium") && Convert.ToBoolean(data["is_premium"]);
            return user;
        }

        public Dictionary<string, object> GetModInfo(string game, int modId)
        {
            return (Dictionary<string, object>)Deserialize(
                GetJson(BaseUrl + "/v1/games/" + game + "/mods/" + modId + ".json"));
        }

        public Dictionary<string, object> GetFileInfo(string game, int modId, int fileId)
        {
            return (Dictionary<string, object>)Deserialize(
                GetJson(BaseUrl + "/v1/games/" + game + "/mods/" + modId + "/files/" + fileId + ".json"));
        }

        public string GetDownloadUri(string game, int modId, int fileId, string key, long expires)
        {
            string url = BaseUrl + "/v1/games/" + game + "/mods/" + modId + "/files/" + fileId + "/download_link.json";
            if (!string.IsNullOrEmpty(key))
                url += "?key=" + Uri.EscapeDataString(key) + "&expires=" + expires;

            object[] links = (object[])Deserialize(GetJson(url));
            foreach (object link in links)
            {
                var entry = link as Dictionary<string, object>;
                if (entry != null && entry.ContainsKey("URI"))
                    return entry["URI"] as string;
            }
            return null;
        }

        public List<NexusMod> GetModList(string game, string category)
        {
            object[] arr = (object[])Deserialize(
                GetJson(BaseUrl + "/v1/games/" + game + "/mods/" + category + ".json"));
            List<NexusMod> result = new List<NexusMod>();
            foreach (object o in arr)
            {
                var d = o as Dictionary<string, object>;
                if (d == null)
                    continue;
                if (d.ContainsKey("status") && (d["status"] as string) != "published")
                    continue;
                NexusMod mod = new NexusMod();
                mod.ModId = d.ContainsKey("mod_id") ? Convert.ToInt32(d["mod_id"]) : 0;
                mod.Name = d.ContainsKey("name") ? d["name"] as string : "";
                mod.Author = d.ContainsKey("author") ? d["author"] as string : "";
                mod.Summary = StripHtml(d.ContainsKey("summary") ? d["summary"] as string : "");
                mod.Version = d.ContainsKey("version") ? d["version"] as string : "";
                mod.PictureUrl = d.ContainsKey("picture_url") ? d["picture_url"] as string : "";
                if (mod.ModId > 0 && !string.IsNullOrEmpty(mod.Name))
                    result.Add(mod);
            }
            return result;
        }

        public NexusFile GetPrimaryFile(string game, int modId)
        {
            var data = (Dictionary<string, object>)Deserialize(
                GetJson(BaseUrl + "/v1/games/" + game + "/mods/" + modId + "/files.json"));
            if (!data.ContainsKey("files"))
                return null;
            object[] files = (object[])data["files"];
            NexusFile primary = null, mainCategory = null, first = null;
            foreach (object o in files)
            {
                var d = o as Dictionary<string, object>;
                if (d == null)
                    continue;
                NexusFile file = new NexusFile();
                file.FileId = d.ContainsKey("file_id") ? Convert.ToInt32(d["file_id"]) : 0;
                file.FileName = d.ContainsKey("file_name") ? d["file_name"] as string : "";
                file.Category = d.ContainsKey("category_name") ? d["category_name"] as string : "";
                if (first == null)
                    first = file;
                if (mainCategory == null && file.Category == "MAIN")
                    mainCategory = file;
                if (primary == null && d.ContainsKey("is_primary") && Convert.ToBoolean(d["is_primary"]))
                    primary = file;
            }
            return primary ?? mainCategory ?? first;
        }
    }

    public class NexusUser
    {
        public string Name;
        public int UserId;
        public bool IsPremium;
    }

    public class NexusMod
    {
        public int ModId { get; set; }
        public string Name { get; set; }
        public string Author { get; set; }
        public string Summary { get; set; }
        public string Version { get; set; }
        public string PictureUrl { get; set; }
    }

    public class NexusFile
    {
        public int FileId;
        public string FileName;
        public string Category;
    }
}
