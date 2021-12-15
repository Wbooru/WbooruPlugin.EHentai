using EHentaiAPI.Client.Data;
using EHentaiAPI.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Wbooru.Kernel;
using Wbooru.UI.Controls;
using Wbooru.Utils;
using Wbooru.Utils.Resource;
using static EHentaiAPI.Utils.EhPageImageSpider<System.Drawing.Image>;

namespace WbooruPlugin.EHentai.UI.Pages
{
    /// <summary>
    /// EHentaiImageViewPage.xaml 的交互逻辑
    /// </summary>
    public partial class EHentaiImageViewPage : Page
    {
        public SpiderTask SpiderTask
        {
            get { return (SpiderTask)GetValue(SpiderTaskProperty); }
            set { SetValue(SpiderTaskProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SpiderTaskProperty =
            DependencyProperty.Register("SpiderTask", typeof(int), typeof(EHentaiImageViewPage), new PropertyMetadata(default));

        public GalleryDetail GalleryDetail
        {
            get { return (GalleryDetail)GetValue(GalleryDetailProperty); }
            set { SetValue(GalleryDetailProperty, value); }
        }

        // Using a DependencyProperty as the backing store for GalleryDetail.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GalleryDetailProperty =
            DependencyProperty.Register("GalleryDetail", typeof(GalleryDetail), typeof(EHentaiImageViewPage), new PropertyMetadata(null, (e, p) =>
            {
                (e as EHentaiImageViewPage)?.OnDetailChanged();
            }));

        public GalleryPreview Preview
        {
            get { return (GalleryPreview)GetValue(PreviewProperty); }
            set { SetValue(PreviewProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Preview.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PreviewProperty =
            DependencyProperty.Register("Preview", typeof(GalleryPreview), typeof(EHentaiImageViewPage), new PropertyMetadata(null, (e, p) =>
            {
                (e as EHentaiImageViewPage)?.OnPreviewChanged();
            }));
        private readonly EhPageImageSpider<System.Drawing.Image> spider;

        public EHentaiImageViewPage(GalleryDetail detail, GalleryPreview galleryPreview, EhPageImageSpider<System.Drawing.Image> spider)
        {
            InitializeComponent();
            this.spider = spider;
            GalleryDetail = detail;
            Preview = galleryPreview;
        }

        private async void OnPreviewChanged()
        {
            DetailImageBox.ImageSource = default;

            if (GalleryDetail is null || Preview is null)
                return;
            var preview = Preview;
            var spiderTask = spider.RequestPage(Preview.Position);
            var notify_content = "正在加载图片...";
            using var notify = LoadingStatus.BeginBusy(notify_content);

            var reporterCallback = new PropertyChangedEventHandler((_, p) =>
            {
                notify.Description = $"{spiderTask.CurrentStatus} ({spiderTask.CurrentDownloadingLength * 1.0 / spiderTask.TotalDownloadLength * 100:F2}%) {notify_content}";
            });
            spiderTask.PropertyChanged += reporterCallback;
            var image = await spiderTask.DownloadTask;
            spiderTask.PropertyChanged -= reporterCallback;

            if (image is null)
                Toast.ShowMessage("加载图片失败");

            if (preview == Preview)
                DetailImageBox.ImageSource = image?.ConvertToBitmapImage();
        }

        private void OnDetailChanged()
        {

        }

        private void MenuItem_Click(object sender, RoutedEventArgs sb)
        {
            try
            {
                if (DetailImageBox.ImageSource is BitmapSource source)
                {
                    Clipboard.SetImage(source);

                    Toast.ShowMessage("复制成功");
                }
                else
                    Toast.ShowMessage("复制失败,图片未加载");
            }
            catch (Exception e)
            {
                ExceptionHelper.DebugThrow(e);
                Toast.ShowMessage("复制失败," + e.Message);
            }
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            //next page.
            if ((await this.spider.Previews.GetAsync(Preview.Position + 1)) is not GalleryPreview preview)
            {
                Toast.ShowMessage("已看到最后");
                return;
            }

            Preview = preview;
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //prev page.
            if ((await this.spider.Previews.GetAsync(Preview.Position - 1)) is not GalleryPreview preview)
            {
                Toast.ShowMessage("已看到最前");
                return;
            }

            Preview = preview;
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigationPop();
        }
    }
}
