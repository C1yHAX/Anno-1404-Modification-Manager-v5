using System;
using System.Diagnostics;
using System.Windows;

namespace AnnoModificationManager5.Nexus
{
    public partial class NexusLoginWindow : Window
    {
        private const string ApiKeyPageUrl = "https://www.nexusmods.com/users/myaccount?tab=api";

        public NexusLoginWindow()
        {
            InitializeComponent();

            string existing = NexusApiKeyStore.Get();
            if (!string.IsNullOrEmpty(existing))
            {
                txt_Key.Text = existing;
                lbl_Status.Text = "Gespeicherter API-Key vorhanden – 'Anmelden' zum Prüfen.";
            }
        }

        public static bool EnsureLogin()
        {
            if (NexusApiKeyStore.HasKey)
                return true;

            NexusLoginWindow window = new NexusLoginWindow();
            window.ShowDialog();
            return NexusApiKeyStore.HasKey;
        }

        private void btn_OpenApiPage_Click(object sender, RoutedEventArgs e)
        {
            try { Process.Start(ApiKeyPageUrl); }
            catch (Exception ex) { lbl_Status.Text = "Konnte den Browser nicht öffnen: " + ex.Message; }
        }

        private void btn_Login_Click(object sender, RoutedEventArgs e)
        {
            string key = (txt_Key.Text ?? "").Trim();
            if (string.IsNullOrEmpty(key))
            {
                lbl_Status.Text = "Bitte einen API-Key eingeben.";
                return;
            }

            btn_Login.IsEnabled = false;
            lbl_Status.Text = "Prüfe API-Key …";
            try
            {
                NexusApiClient api = new NexusApiClient(key);
                NexusUser user = api.ValidateUser();
                NexusApiKeyStore.Set(key);
                lbl_Status.Text = "Angemeldet als " + user.Name +
                    (user.IsPremium ? " (Premium)" : " (kein Premium)") + ".";
                DialogResult = true;
            }
            catch (Exception ex)
            {
                lbl_Status.Text = "Anmeldung fehlgeschlagen: " + ex.Message;
            }
            finally
            {
                btn_Login.IsEnabled = true;
            }
        }

        private void btn_Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
