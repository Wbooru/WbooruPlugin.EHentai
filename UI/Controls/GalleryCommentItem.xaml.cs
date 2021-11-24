﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wbooru;

namespace WbooruPlugin.EHentai.UI.Controls
{
    /// <summary>
    /// GalleryCommentItem.xaml 的交互逻辑
    /// </summary>
    public partial class GalleryCommentContentViewer : UserControl
    {
        private static Regex unconvertLinkRegex = new Regex(@"(?<!href="")(https?://.+?)(\s+|$|(<br>))(?<!</a>)", RegexOptions.Multiline | RegexOptions.IgnoreCase);

        public string Comment
        {
            get { return (string)GetValue(CommentProperty); }
            set { SetValue(CommentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Comment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommentProperty =
            DependencyProperty.Register("Comment", typeof(string), typeof(GalleryCommentContentViewer), new PropertyMetadata("", (obj, e) =>
            {
                (obj as GalleryCommentContentViewer)?.OnCommentChanged(e);
            }));

        private void OnCommentChanged(DependencyPropertyChangedEventArgs _)
        {
            try
            {
                var content = unconvertLinkRegex.Replace(Comment, match =>
                {
                    Log<GalleryCommentContentViewer>.Debug($"replace unconverted link: {match.Value}");
                    return $"<a href=\"{match.Groups[1].Value}\">{match.Groups[1].Value}</a>";
                });

                FlowDocument convert(string c)
                {
                    try
                    {

                        var xaml = HTMLConverter.HtmlToXamlConverter.ConvertHtmlToXaml(content, true);
                        return System.Windows.Markup.XamlReader.Parse(xaml) as FlowDocument;
                    }
                    catch
                    {
                        return null;
                    }
                }

                var xamlControls = convert(content) ?? convert(Comment) ?? new FlowDocument(new Paragraph(new Run()
                {
                    Text = Comment
                }));

                var queue = new Queue<DependencyObject>();
                queue.Enqueue(xamlControls);

                while (queue.Count > 0)
                {
                    var d = queue.Dequeue();

                    if (d is Hyperlink link)
                    {
                        link.Foreground = Brushes.White;
                        link.PreviewMouseDown += Link_PreviewMouseDown;
                    }

                    foreach (var child in LogicalTreeHelper.GetChildren(d).OfType<DependencyObject>())
                        queue.Enqueue(child);
                }

                MainRichTextBox.Document = xamlControls;
            }
            catch (Exception e)
            {
                //
            }
        }

        private void Link_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((sender as Hyperlink)?.NavigateUri?.AbsoluteUri is string url && !string.IsNullOrWhiteSpace(url))
                Process.Start(new ProcessStartInfo(url)
                {
                    UseShellExecute = true
                });
        }

        public GalleryCommentContentViewer()
        {
            InitializeComponent();
        }
    }
}
