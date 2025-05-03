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
using System.IO;

namespace RDAExplorerGUI.UserInterface.Misc
{
    /// <summary>
    /// Interaction logic for MessageWindow.xaml
    /// </summary>
    public partial class MessageWindow : Window
    {
        public MessageBoxResult result;

        public enum MessageWindowType
        {
            OK,
            OKCancel,
            YesNo,
            YesNoCancel,
            TextInput,
            XmlInput
        }

        public MessageWindowType MessageType;
        public bool TextInput_Multiline = false;

        public MessageWindow()
        {
            InitializeComponent();
        }

        void MessageWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //IfGettText -> Focus and Select Text
            if (MessageType == MessageWindowType.TextInput)
            {
                Activate();

                TextInput.Focus();
                Keyboard.Focus(TextInput);
                TextInput.SelectAll();
            }
        }

        public MessageWindow(MessageWindowType type)
        {
            MessageType = type;

            InitializeComponent();

            //XmlInput.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.HighlightingDefinitions[13];

            switch (MessageType)
            {
                case MessageWindowType.OK:
                    button_ok.Visibility = System.Windows.Visibility.Visible;
                    button_cancel.Visibility = System.Windows.Visibility.Collapsed;
                    button_yes.Visibility = System.Windows.Visibility.Collapsed;
                    button_no.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                case MessageWindowType.OKCancel:
                    button_ok.Visibility = System.Windows.Visibility.Visible;
                    button_cancel.Visibility = System.Windows.Visibility.Visible;
                    button_yes.Visibility = System.Windows.Visibility.Collapsed;
                    button_no.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                case MessageWindowType.TextInput:
                    button_ok.Visibility = System.Windows.Visibility.Visible;
                    button_cancel.Visibility = System.Windows.Visibility.Visible;
                    button_yes.Visibility = System.Windows.Visibility.Collapsed;
                    button_no.Visibility = System.Windows.Visibility.Collapsed;
                    TextInput.Visibility = System.Windows.Visibility.Visible;
                    break;
                //case MessageWindowType.XmlInput:
                //    button_ok.Visibility = System.Windows.Visibility.Visible;
                //    button_cancel.Visibility = System.Windows.Visibility.Visible;
                //    button_yes.Visibility = System.Windows.Visibility.Collapsed;
                //    button_no.Visibility = System.Windows.Visibility.Collapsed;
                //    XmlInput.Visibility = System.Windows.Visibility.Visible;
                //    break;
                case MessageWindowType.YesNo:
                    button_ok.Visibility = System.Windows.Visibility.Collapsed;
                    button_cancel.Visibility = System.Windows.Visibility.Collapsed;
                    button_yes.Visibility = System.Windows.Visibility.Visible;
                    button_no.Visibility = System.Windows.Visibility.Visible;
                    break;
                case MessageWindowType.YesNoCancel:
                    button_ok.Visibility = System.Windows.Visibility.Collapsed;
                    button_cancel.Visibility = System.Windows.Visibility.Visible;
                    button_yes.Visibility = System.Windows.Visibility.Visible;
                    button_no.Visibility = System.Windows.Visibility.Visible;
                    break;
            }
        }

        private void button_ok_Click(object sender, RoutedEventArgs e)
        {
            result = MessageBoxResult.OK;
            DialogResult = true;
        }

        private void button_cancel_Click(object sender, RoutedEventArgs e)
        {
            result = MessageBoxResult.Cancel;
            DialogResult = false;
        }

        private void button_yes_Click(object sender, RoutedEventArgs e)
        {
            result = MessageBoxResult.Yes;
            DialogResult = true;
        }

        private void button_no_Click(object sender, RoutedEventArgs e)
        {
            result = MessageBoxResult.No;
            DialogResult = false;
        }

        #region Static
        public static MessageBoxResult Show(string Message)
        {
            MessageWindow wnd = new MessageWindow(MessageWindowType.OK);
            wnd.Message.Text = Message;
            wnd.ShowDialog();

            return MessageBoxResult.OK;
        }

        public static MessageBoxResult Show(string Message, MessageWindowType tp)
        {
            MessageWindow wnd = new MessageWindow(tp);
            wnd.Message.Text = Message;
            wnd.ShowDialog();

            //switch (tp)
            //{
            //    case MessageWindowType.OK:
            //        return MessageBoxResult.OK;
            //    case MessageWindowType.OKCancel:
            //        return res == false ? MessageBoxResult.Cancel : MessageBoxResult.OK;
            //    case MessageWindowType.YesNo:
            //        return res == null ? MessageBoxResult.No : MessageBoxResult.Yes;
            //    case MessageWindowType.YesNoCancel:
            //        return res == null ? MessageBoxResult.No : (res == true ? MessageBoxResult.Yes : MessageBoxResult.Cancel);                    
            //}

            return wnd.result;
        }

        public static string GetText(string Message, string Example, bool MultiLine)
        {
            MessageWindow wnd = new MessageWindow(MessageWindowType.TextInput);
            wnd.Message.Text = Message;
            wnd.TextInput.Text = Example;
            wnd.TextInput_Multiline = MultiLine;

            bool? res = wnd.ShowDialog();

            return res == true /*&& wnd.TextInput.Text != Example*/ ? wnd.TextInput.Text : null;
        }

        public static string GetText(string Message, string Example)
        {
            return GetText(Message, Example, false);
        }

        private void TextInput_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!TextInput_Multiline && e.Key == Key.Enter)
            {
                DialogResult = true;
                e.Handled = true;
            }
        }

        //public static string GetXml(string Message, string Example)
        //{
        //    MessageWindow wnd = new MessageWindow(MessageWindowType.XmlInput);
        //    wnd.Message.Text = Message;
        //    wnd.XmlInput.Text = Example.Replace("><", ">\r\n<");
        //    bool? res = wnd.ShowDialog();

        //    return res == true && wnd.XmlInput.Text != Example ? wnd.XmlInput.Text : null;
        //}
        #endregion
    }
}
