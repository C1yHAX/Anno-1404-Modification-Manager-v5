using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO;
using System.Reflection;
using AnnoModificationManager4.Misc;
using AnnoModificationManager4.Language.DictionarySystem;
using System.Threading;
using System.Xml;
using System.Diagnostics;

namespace DevelopmentTools.Tools.StartPage
{
    /// <summary>
    /// Interaction logic for UpdateButton.xaml
    /// </summary>
    public partial class UpdateButton : Button
    {
        //o = normal, 1=searching, 2=found, 3=message
        int currentMode = 0;
        string currentDownloadLink = "";

        public UpdateButton()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Content = ControlExtension.BuildImageTextblock("pack://application:,,,/Images/Icons/arrow_refresh.png", "Search updates");
            RunSearch();
        }

        public void RunSearch()
        {
            currentMode = 1;
            Content = ControlExtension.BuildImageTextblock("pack://application:,,,/Images/Icons/hourglass.png","Searching updates");

            Thread thread = new Thread(new ParameterizedThreadStart(delegate
            {
                try
                {
                    TimeoutWebClient web = new TimeoutWebClient();
                    string xml = web.DownloadString("http://tilegame.bplaced.net/AMM4Admin/Versions.xml");

                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(xml);

                    string VersionString = doc.SelectSingleNode("//Version[@Type='AMM4DEV']").Attributes["Version"].Value;
                    Version version = new Version(VersionString);

                    if (version > Assembly.GetExecutingAssembly().GetName().Version)
                    {
                        Application.Current.Dispatch(app =>
                        {
                            currentMode = 2;
                            Content = ControlExtension.BuildImageTextblock("pack://application:,,,/Images/Icons/information.png","Update DevelopmentTools");

                            currentDownloadLink =
                                       doc.SelectSingleNode("//Version[@Type='AMM4DEV']").Attributes["Download"].Value;
                        });
                    }
                    else
                        throw new Exception();
                }
                catch (Exception)
                {
                    currentMode = 3;
                    Application.Current.Dispatch(app =>
                    {
                        Content = ControlExtension.BuildImageTextblock("pack://application:,,,/Images/Icons/information.png","No updates found");
                    });

                    Thread.Sleep(2500);

                    currentMode = 0;
                    Application.Current.Dispatch(app =>
                    {
                        Content = ControlExtension.BuildImageTextblock("pack://application:,,,/Images/Icons/arrow_refresh.png","Search updates");
                    });
                }
            }));
            thread.Start();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (currentMode == 0)
                RunSearch();
            if (currentMode == 2)
            {
                Process.Start(currentDownloadLink);
            }
        }
    }
}
