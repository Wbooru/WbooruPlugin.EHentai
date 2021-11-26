using EHentaiAPI.Client.Data;
using System;
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
    public partial class GalleryCommentItem : UserControl
    {
        private static Regex urlRegex = new Regex(@"(http|ftp|https):\/\/([\w_-]+(?:(?:\.[\w_-]+)+))([\w.,@?^=%&:\/~+#-]*[\w@?^=%&\/~+#-])");

        public GalleryComment Comment
        {
            get { return (GalleryComment)GetValue(CommentProperty); }
            set { SetValue(CommentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Comment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommentProperty =
            DependencyProperty.Register("Comment", typeof(GalleryComment), typeof(GalleryCommentItem), new PropertyMetadata(null, (obj, e) =>
            {
                (obj as GalleryCommentItem)?.OnCommentChanged(e);
            }));

        private void OnCommentChanged(DependencyPropertyChangedEventArgs _)
        {
            if (Comment is null)
                MainRichTextBox.Document = null;

            try
            {
                var content = Comment.Comment;

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

                var xamlControls = convert(Comment.Comment) ?? new FlowDocument(new Paragraph(new Run()
                {
                    Text = Comment.Comment
                }));

                //make some url link as hyperlink object.
                IEnumerable<T> RecursiveGet<T>(DependencyObject o) where T : DependencyObject
                {
                    if (o is T t)
                        yield return t;
                    foreach (var i in LogicalTreeHelper.GetChildren(o).OfType<DependencyObject>().Select(x => RecursiveGet<T>(x)).SelectMany(x => x))
                        yield return i;
                }

                while (true)
                {
                    (var _, var match, var run, var inlines) = RecursiveGet<Run>(xamlControls).Select(sr =>
                    {
                        var match = urlRegex.Match(sr.Text);
                        return (match.Success, match, sr, (sr.Parent as Paragraph)?.Inlines);
                    }).Where(x => x.Success && x.Item4 != null)
                        .Where(zz =>
                        {
                            var parent = zz.sr.Parent;
                            if (parent != null)
                            {
                                if (parent is Hyperlink)
                                    return false;
                                parent = (parent as FrameworkContentElement)?.Parent;
                            }
                            return true;
                        }).FirstOrDefault();

                    if (match is null)
                        break;

                    var beforeRun = new Run()
                    {
                        Text = run.Text.Substring(0, match.Index - 0)
                    };

                    var link = match.Value.Trim();
                    var genHyperLink = new Hyperlink()
                    {
                        NavigateUri = new Uri(link)
                    };
                    genHyperLink.Inlines.Add(new Run() { Text = link });

                    var afterRun = new Run()
                    {
                        Text = " " + run.Text.Substring(match.Index + match.Length)
                    };

                    var insertAfter = run.PreviousInline;
                    var insertBefore = run.NextInline;
                    inlines.Remove(run);

                    if (insertAfter != null)
                    {
                        inlines.InsertAfter(insertAfter, beforeRun);
                        inlines.InsertAfter(beforeRun, genHyperLink);
                        inlines.InsertAfter(genHyperLink, afterRun);
                    }
                    else if (insertBefore != null)
                    {
                        inlines.InsertBefore(insertBefore, afterRun);
                        inlines.InsertBefore(afterRun, genHyperLink);
                        inlines.InsertBefore(genHyperLink, beforeRun);
                    }
                    else
                    {
                        inlines.Add(beforeRun);
                        inlines.Add(genHyperLink);
                        inlines.Add(afterRun);
                    }

                    Log.Debug($"make pure text url {link} as hyperlink.");
                }

                //make Hyperlink objects clickable
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

        public GalleryCommentItem()
        {
            InitializeComponent();
            MainPanel.DataContext = this;
        }
    }
}
