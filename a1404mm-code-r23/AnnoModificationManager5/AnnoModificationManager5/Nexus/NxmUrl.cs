using System;
using System.Web;

namespace AnnoModificationManager5.Nexus
{
    public class NxmUrl
    {
        public string Game;
        public int ModId;
        public int FileId;
        public string Key;
        public long Expires;
        public int UserId;

        public static NxmUrl Parse(string url)
        {
            Uri uri = new Uri(url);
            if (!string.Equals(uri.Scheme, "nxm", StringComparison.OrdinalIgnoreCase))
                throw new FormatException("Not an nxm:// link.");

            NxmUrl result = new NxmUrl();
            result.Game = uri.Host;

            string[] segments = uri.AbsolutePath.Trim('/').Split('/');
            for (int i = 0; i + 1 < segments.Length; i += 2)
            {
                if (segments[i] == "mods")
                    int.TryParse(segments[i + 1], out result.ModId);
                else if (segments[i] == "files")
                    int.TryParse(segments[i + 1], out result.FileId);
            }

            var query = HttpUtility.ParseQueryString(uri.Query);
            result.Key = query["key"];
            long.TryParse(query["expires"] ?? "0", out result.Expires);
            int.TryParse(query["user_id"] ?? "0", out result.UserId);
            return result;
        }
    }
}
