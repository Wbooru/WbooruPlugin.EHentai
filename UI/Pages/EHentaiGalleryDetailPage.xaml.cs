using EHentaiAPI.Client;
using EHentaiAPI.Client.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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
using Wbooru.Models.Gallery;
using Wbooru.Settings;
using Wbooru.UI.Controls;
using Wbooru.UI.Pages;
using WbooruPlugin.EHentai.UI.Controls;

namespace WbooruPlugin.EHentai.UI.Pages
{
    /// <summary>
    /// EHentaiGalleryDetailPage.xaml 的交互逻辑
    /// </summary>
    public partial class EHentaiGalleryDetailPage : DetailImagePageBase
    {
        public class ImageInfo
        {
            public Size PreviewImageSize { get; set; }
            public string ImageUrl { get; set; }
        }

        public uint GridItemWidth => Setting<GlobalSetting>.Current.PictureGridItemWidth;
        public uint GridItemMarginWidth => Setting<GlobalSetting>.Current.PictureGridItemMarginWidth;

        public ObservableCollection<ImageInfo> PreviewImages { get; } = new ObservableCollection<ImageInfo>();

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

            var list = await client.GetPreviewSetAsync(Detail.Detail,0);

            foreach (var item in Enumerable.Repeat(new ImageInfo()
            {
                PreviewImageSize = new Size()
                {
                    Width = 100,
                    Height = 200
                },
                ImageUrl = ""
            }, 20))
            {
                PreviewImages.Add(item);
            }
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (CommentPageButtonState == -1)
                return;
            var commentPage = new EHentaiCommentListPage(client, Detail.Detail);
            NavigationHelper.NavigationPush(commentPage);
        }
    }
}
