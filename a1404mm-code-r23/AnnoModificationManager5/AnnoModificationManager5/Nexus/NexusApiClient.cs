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
    }

    public class NexusUser
    {
        public string Name;
        public int UserId;
        public bool IsPremium;
    }
}
