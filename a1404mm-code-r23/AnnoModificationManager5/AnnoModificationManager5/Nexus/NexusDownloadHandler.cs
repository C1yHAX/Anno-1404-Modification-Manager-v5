using System;
using System.IO;
using System.Net;
using AnnoModificationManager5.Components;
using AnnoModificationManager5.UserInterface.Misc;

namespace AnnoModificationManager5.Nexus
{
    public static class NexusDownloadHandler
    {
        public static void HandleNxm(string nxmUrl)
        {
            try
            {
                NxmUrl link = NxmUrl.Parse(nxmUrl);

                if (!NexusLoginWindow.EnsureLogin())
                    return;
                string apiKey = NexusApiKeyStore.Get();

                NexusApiClient api = new NexusApiClient(apiKey);
                string uri = api.GetDownloadUri(link.Game, link.ModId, link.FileId, link.Key, link.Expires);
                if (string.IsNullOrEmpty(uri))
                {
                    MessageWindow.Show("Kein Download-Link von Nexus erhalten. Eventuell ist Premium nötig oder der Link ist abgelaufen.");
                    return;
                }

                string extension = Path.GetExtension(new Uri(uri).AbsolutePath);
                if (!string.Equals(extension, ".zip", StringComparison.OrdinalIgnoreCase))
                {
                    MessageWindow.Show("Diese Nexus-Mod ist kein .zip im AMM-Format und kann nicht automatisch installiert werden. Es werden nur .zip-Mods unterstützt.");
                    return;
                }

                string destination = Path.Combine(Path.GetTempPath(),
                    "nexus_" + link.ModId + "_" + link.FileId + ".zip");

                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
                using (WebClient client = new WebClient())
                {
                    client.Headers.Add(HttpRequestHeader.UserAgent, "AnnoModificationManager5/5.0");
                    client.DownloadFile(uri, destination);
                }

                bool installed = ModificationHandler.Instance.AddModification(destination);
                if (installed)
                    MessageWindow.Show("Mod von Nexus Mods installiert.");
            }
            catch (Exception ex)
            {
                MessageWindow.Show("Nexus-Download fehlgeschlagen: " + ex.Message);
            }
        }
    }
}
