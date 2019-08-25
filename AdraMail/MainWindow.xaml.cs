using Microsoft.Win32;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;

namespace AdraMail
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Open(object sender, RoutedEventArgs e)
        {
            HandleErrors(() =>
            {
                var openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() == true)
                {
                    var text = File.ReadAllText(openFileDialog.FileName);
                    Mail.Text = text;
                    SetStatus($"Soubor {openFileDialog.FileName} byl otevřen");
                }
            }, "Soubor nelze otevřít");
        }

        private void Paste(object sender, RoutedEventArgs e)
        {
            HandleErrors(() =>
            {
                var text = Clipboard.GetData(DataFormats.Text) as string;
                if (!string.IsNullOrWhiteSpace(text))
                {
                    Mail.Text = text;
                    SetStatus("Obsah schránky byl vložen");
                }
                else
                {
                    SetStatus("Obsah schránky nelze vložit");
                }
            }, "Obsah schránky nelze vložit");
        }

        private void Copy(object sender, RoutedEventArgs e)
        {
            HandleErrors(() =>
                {
                    Clipboard.SetData(DataFormats.UnicodeText, Mail.Text);
                    SetStatus("Text byl zkopírován do schránky");
                },
                "Obsah schránky nelze vložit");
        }

        private void HandleErrors(Action action, string errorMessage)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, errorMessage, MessageBoxButton.OK);
            }
        }

        private void SetStatus(string status)
        {
            Status.Text = status;
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            HandleErrors(() =>
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Soubor HTML (*.html)|*.html",
                    AddExtension = true,
                    DefaultExt = "html"
                };
                if (saveFileDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveFileDialog.FileName, Mail.Text);
                    SetStatus($"Soubor {saveFileDialog.FileName} byl uložen");
                }
            }, "Soubor se nepodařilo uložit");
        }

        private void Replace(object sender, RoutedEventArgs e)
        {
            HandleErrors(() =>
            {
                var mail = Mail.Text
                    .RemoveHead()
                    .RemoveMC()
                    .RemoveDataFileId()
                    .RemoveBodyAttributes()
                    .RemoveStyle()
                    .RemoveMeta()
                    .RemoveXmlns()
                    .RemoveMcPreviewText()
                    .RemoveUnsubscribe1()
                    .RemoveUnsubscribe2()
                    .RemoveBrowserLink()
                    .RemoveDoctype();

                Mail.Text = mail;

                SetStatus("Text byl opraven");
            }, "Text se nepodařilo nahradit");
        }

        
    }

    public static class Replaces
    {
        public static string RemoveHead(this string text) => text.ReplaceTempalte(t =>
        {
            var regex = new Regex("\\s*<head>.+</head>", RegexOptions.Singleline);
            return regex.Replace(t, "");
        });

        public static string RemoveMC(this string text) => text.ReplaceTempalte(t =>
        {
            var regex = new Regex(" mc:?[a-zA-Z0-9-_]+(=\"[^\"]+\")?", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            return regex.Replace(t, "");
        });

        public static string RemoveDataFileId(this string text) => text.ReplaceTempalte(t =>
        {
            var regex = new Regex(" data-file-id(=\"[^\"]+\")?", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            return regex.Replace(t, "");
        });

        public static string RemoveBodyAttributes(this string text) => text.ReplaceTempalte(t =>
        {
            var regex = new Regex("( leftmargin=\"\\d+\"| topmargin=\"\\d+\"| marginwidth=\"\\d+\"| marginheight=\"\\d+\"| offset=\"\\d+\")", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            return regex.Replace(t, "");
        });

        public static string RemoveStyle(this string text) => text.ReplaceTempalte(t =>
        {
            var regex = new Regex("\\s*<style( type=\"text/css\")?>.+</style>", RegexOptions.Singleline);
            return regex.Replace(t, "");
        });

        public static string RemoveMeta(this string text) => text.ReplaceTempalte(t =>
        {
            var regex = new Regex("\\s*<meta charset=\"utf-8\">\\s*", RegexOptions.Singleline);
            return regex.Replace(t, "");
        });

        public static string RemoveXmlns(this string text) => text.ReplaceTempalte(t =>
        {
            var regex = new Regex(" xmlns=\"[^\"]+\"", RegexOptions.Singleline);
            return regex.Replace(t, "");
        });

        public static string RemoveMcPreviewText(this string text) => text.ReplaceTempalte(t =>
        {
            var regex = new Regex("<\\!--\\*\\|IF:MC_PREVIEW_TEXT\\|\\*-->.+<\\!--\\*\\|END:IF\\|\\*-->", RegexOptions.Singleline);
            return regex.Replace(t, "");
        });

        public static string RemoveUnsubscribe1(this string text) => text.ReplaceTempalte(t =>
        {
            var regex = new Regex("</center>\\s*<center>.+</center>", RegexOptions.Singleline);
            return regex.Replace(t, "</center>");
        });

        public static string RemoveUnsubscribe2(this string text) => text.ReplaceTempalte(t =>
        {
            var regex = new Regex("<div style=\"text-align: center;\"><a class=\"utilityLink\" href=\"\\*\\|UNSUB\\|\\*\">unsubscribe from this list</a>&nbsp;&nbsp;&nbsp;<br>\\s*<a class=\"utilityLink\" href=\"\\*\\|UPDATE_PROFILE\\|\\*\">update subscription preferences</a>&nbsp;</div>", RegexOptions.Singleline);
            return regex.Replace(t, "");
        });

        public static string RemoveBrowserLink(this string text) => text.ReplaceTempalte(t =>
        {
            var regex = new Regex("<a href=\"\\*\\|ARCHIVE\\|\\*\" target=\"_blank\">View this email in your browser</a>", RegexOptions.Singleline);
            return regex.Replace(t, "");
        });

        public static string RemoveDoctype(this string text) => text.ReplaceTempalte(t =>
        {
            var regex = new Regex("<\\!DOCTYPE[^>]*>", RegexOptions.Singleline);
            return regex.Replace(t, "");
        });

        private static string ReplaceTempalte(this string text, Func<string, string> replaceFunc)
        {
            return string.IsNullOrWhiteSpace(text)
                ? text
                : replaceFunc(text);
        }
    }
}
