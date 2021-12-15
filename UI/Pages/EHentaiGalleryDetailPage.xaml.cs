using EHentaiAPI.Client;
using EHentaiAPI.Client.Data;
using EHentaiAPI.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
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
using Wbooru.Galleries;
using Wbooru.Kernel;
using Wbooru.Models;
using Wbooru.Models.Gallery;
using Wbooru.Settings;
using Wbooru.UI.Controls;
using Wbooru.UI.Dialogs;
using Wbooru.UI.Pages;
using Wbooru.UI.ValueConverters;
using Wbooru.Utils.Resource;
using WbooruPlugin.EHentai.UI.Controls;
using WbooruPlugin.EHentai.UI.Dialogs;
using WbooruPlugin.EHentai.Utils;

namespace WbooruPlugin.EHentai.UI.Pages
{
    /// <summary>
    /// EHentaiGalleryDetailPage.xaml 的交互逻辑
    /// </summary>
    public partial class EHentaiGalleryDetailPage : DetailImagePageBase
    {
        public class TagGroup
        {
            public string Name { get; set; }
            public List<TagItem> Tags { get; set; } = new List<TagItem>();
        }

        public class TagItem : INotifyPropertyChanged
        {
            public string Name { get; set; }
            public TagGroup RefGroup { get; set; }

            private bool check { get; set; }
            public bool Check
            {
                get { return check; }
                set
                {
                    check = value;
                    PropertyChanged?.Invoke(this, new(nameof(Check)));
                }
            }

            private bool marked { get; set; }
            public bool Mark
            {
                get { return marked; }
                set
                {
                    marked = value;
                    PropertyChanged?.Invoke(this, new(nameof(Mark)));
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
        }

        public ObservableCollection<TagGroup> Tags { get; } = new();

        private static Regex imageSizeParser = new Regex(@"(\d+)-(\d+)(-\w*)?\.\w+");

        public uint GridItemWidth => Setting<GlobalSetting>.Current.PictureGridItemWidth;
        public uint GridItemMarginWidth => Setting<GlobalSetting>.Current.PictureGridItemMarginWidth;

        public ObservableCollection<ImageInfo> PreviewImages { get; } = new();

        private EhPageImageSpider<System.Drawing.Image> spider;

        public EHentaiImageGalleryImageDetail Detail
        {
            get { return (EHentaiImageGalleryImageDetail)GetValue(DetailProperty); }
            set { SetValue(DetailProperty, value); }
        }

        public ObservableCollection<GalleryCommentItem> PreviewCommentItems { get; } = new ObservableCollection<GalleryCommentItem>();

        // Using a DependencyProperty as the backing store for Detail.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DetailProperty =
            DependencyProperty.Register("Detail", typeof(EHentaiImageGalleryImageDetail), typeof(EHentaiGalleryDetailPage), new PropertyMetadata(null));

        /**
         * -1 : 无评论
         * 0 : 预览评论区已经显示全部内容(但还是可以点开详细列表去评分)
         * 1 : 打开评论页面浏览全部评论
         */
        public int CommentPageButtonState
        {
            get { return (int)GetValue(CommentPageButtonStateProperty); }
            set { SetValue(CommentPageButtonStateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CommentPageButtonState.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommentPageButtonStateProperty =
            DependencyProperty.Register("CommentPageButtonState", typeof(int), typeof(EHentaiGalleryDetailPage), new PropertyMetadata(0));

        public bool HasMorePages
        {
            get { return (bool)GetValue(HasMorePagesProperty); }
            set { SetValue(HasMorePagesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HasMorePages.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HasMorePagesProperty =
            DependencyProperty.Register("HasMorePages", typeof(bool), typeof(EHentaiGalleryDetailPage), new PropertyMetadata(false));

        private readonly EhClient client;

        public EHentaiGalleryDetailPage(EhClient client)
        {
            InitializeComponent();
            this.client = client;
        }

        public override async void ApplyItem(Gallery g, GalleryItem i)
        {
            if (!(g is EHentaiGallery gallery && i is EHentaiImageGalleryInfo info))
            {
                Log<EHentaiGalleryDetailPage>.Error("gallery != EHentaiGallery or info != EHentaiImageGalleryInfo");
                return;
            }

            Detail = gallery.GetImageDetial(info) as EHentaiImageGalleryImageDetail;
            Log.Debug($"Thumb = {Detail.Detail.Thumb}");

            this.spider = new EhPageImageSpider<System.Drawing.Image>(client, Detail.Detail, async (url, reporter) =>
            {
                var image = await ImageResourceManager.RequestImageAsync(url, url, false, curDLStatus =>
                {
                    reporter.CurrentDownloadingLength = curDLStatus.downloaded_bytes;
                    if (curDLStatus.content_bytes != reporter.CurrentDownloadingLength)
                        reporter.CurrentDownloadingLength = curDLStatus.content_bytes;
                });
                return image;
            });

            //标签
            Tags.Clear();
            foreach (var tagGroup in Detail.Detail.Tags)
            {
                if (tagGroup.Size == 0 || tagGroup.TagList.Count == 0)
                    continue;
                var tagManager = Wbooru.Container.Get<ITagManager>();
                var group = new TagGroup()
                {
                    Name = tagGroup.TagGroupName
                };
                foreach (var asyncItem in tagGroup.TagList.Select(async x => new TagItem()
                {
                    Name = x,
                    Mark = await tagManager.ContainTag(x, EHentaiGallery.GalleryNameConst, TagRecord.TagRecordType.Marked)
                }))
                {
                    var item = await asyncItem;
                    item.RefGroup = group;
                    group.Tags.Add(item);
                }
                Tags.Add(group);
            }

            //动态计算几条预览评论
            //限制显示5个评论或者总添加超过300的
            var comments = Detail.Detail.Comments.Comments;
            var remainHeight = 300.0;

            for (int z = 0; z < Math.Min(5, comments.Length); z++)
            {
                var comment = comments[z];
                var item = new GalleryCommentItem()
                {
                    Comment = comment
                };
                item.InvalidateMeasure();
                var height = item.ActualHeight;
                if (remainHeight < height)
                    break;
                PreviewCommentItems.Add(item);
                remainHeight -= height;
            }

            //最后一条隐藏一下线
            if (PreviewCommentItems.LastOrDefault() is GalleryCommentItem lastGenItem)
                lastGenItem.HideBaseLine();

            //更新一下状态
            if (comments.Length == 0)
                CommentPageButtonState = -1;
            else if (comments.Length > PreviewCommentItems.Count)
                CommentPageButtonState = 1;
            else
                CommentPageButtonState = 0;

            PreviewImages.Clear();

            var list = await client.GetPreviewSetAsync(Detail.Detail, 0);
            var previewImagesCount = Setting<EhentaiSetting>.Current.DetailPagePreviewImagesCount;

            for (int r = 0; r < Math.Min(list.Key.Size, previewImagesCount); r++)
            {
                var preview = list.Key.GetGalleryPreview(Detail.Detail.Gid, r);
                var imageUrl = preview?.ImageUrl ?? "";
                var match = imageSizeParser.Match(imageUrl);
                if (match.Success)
                {
                    var width = int.Parse(match.Groups[1].Value);
                    var height = int.Parse(match.Groups[2].Value);

                    PreviewImages.Add(new()
                    {
                        PreviewImageSize = new ImageSize()
                        {
                            Width = width,
                            Height = height
                        },
                        ImageAsync = new ImageAsyncLoadingParam()
                        {
                            ImageUrl = imageUrl,
                            PreviewImageDownloadUrl = imageUrl
                        },
                        Preview = preview
                    });
                }
            }

            HasMorePages = Detail.Detail.Pages > PreviewCommentItems.Count;
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (CommentPageButtonState == -1)
                return;
            var commentPage = new EHentaiCommentListPage(client, Detail.Detail);
            NavigationHelper.NavigationPush(commentPage);
        }

        private void TextBlock_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            //查看列表
            NavigationHelper.NavigationPush(new EHentaiGalleryImageListPage(client, Detail.Detail, this.spider));
        }

        private void ForceRefreshDetail()
        {
            var d = Detail;
            Detail = null;
            Detail = d;
            //InvalidateProperty(DetailProperty);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            //收藏管理
            var dialog = new FavoriteSelectDialog(client, Detail);
            await Dialog.ShowDialog(dialog);
            ForceRefreshDetail();
        }

        private async void TextBlock_MouseDown_2(object sender, MouseButtonEventArgs e)
        {
            //查看画廊信息
            await NavigationHelper.NavigationPush(new EHentaiGalleryInfoPage(Detail.Detail));
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //种子下载
            var dialog = new TorrentsSelectDialog(client, Detail);
            await Dialog.ShowDialog(dialog);
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigationPop();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is not ImageInfo imageInfo)
                return;

            NavigationHelper.NavigationPush(new EHentaiImageViewPage(Detail.Detail, imageInfo.Preview, spider));
        }

        private async void FavoriteStarsDisplayer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var dialog = new RateGalleryDialog(client, Detail);
            await Dialog.ShowDialog(dialog);
            ForceRefreshDetail();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is TagItem item)
            {
                item.Check = !item.Check;
            }
        }

        private async void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var tagManager = Wbooru.Container.Get<ITagManager>();
            if (sender is FrameworkElement f && f.DataContext is TagItem item)
            {
                if (item.Mark)
                {
                    await tagManager.RemoveTag(tagManager.MarkedTags.FirstOrDefault(x => x.FromGallery == EHentaiGallery.GalleryNameConst && x.Tag.Name == item.Name));
                    Toast.ShowMessage("取消收藏标签成功");
                    item.Mark = false;
                }
                else
                {
                    await tagManager.AddTag(new Tag()
                    {
                        Name = item.Name,
                        Type = TagConverter.ConvertTagGroupToTagType(item.RefGroup.Name)
                    }, EHentaiGallery.GalleryNameConst, TagRecord.TagRecordType.Marked);
                    Toast.ShowMessage("收藏标签成功");
                    item.Mark = true;
                }
            }
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement f && f.DataContext is TagItem item)
            {
                Process.Start(new ProcessStartInfo($"https://ehwiki.org/wiki/{WebUtility.HtmlEncode(item.Name)}")
                {
                    UseShellExecute = true
                });
            }
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            var selectTags = Tags.SelectMany(x => x.Tags).Where(x => x.Check).ToArray();
            if (selectTags.Length == 0)
            {
                Toast.ShowMessage("请先至少选择一个标签");
                return;
            }

            if (Wbooru.Container.GetAll<Gallery>().OfType<EHentaiGallery>().FirstOrDefault() is Gallery gallery)
            {
                NavigationHelper.NavigationPush(new MainGalleryPage(selectTags.Select(x => TagConverter.WrapTagForSearching(x.Name, x.RefGroup.Name)), gallery));
            }
        }
    }
}
