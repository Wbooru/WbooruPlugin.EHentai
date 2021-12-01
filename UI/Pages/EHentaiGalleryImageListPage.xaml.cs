using EHentaiAPI.Client;
using EHentaiAPI.Client.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using Wbooru.Kernel;
using Wbooru.Models;
using Wbooru.Settings;
using Wbooru.UI.Controls;
using Wbooru.UI.ValueConverters;

namespace WbooruPlugin.EHentai.UI.Pages
{
    using ImageInfoEx = ImageInfo<(int ReferencePageIndex, string url)>;

    /// <summary>
    /// EHentaiCommentListPage.xaml 的交互逻辑
    /// </summary>
    public partial class EHentaiGalleryImageListPage : Page
    {
        public class PageRange : INotifyPropertyChanged
        {
            private int pageStart;

            public int PageStart
            {
                get
                {
                    return pageStart;
                }
                set
                {
                    pageStart = value;
                    PropertyChanged?.Invoke(this, new(nameof(PageStart)));
                }
            }

            private int pageEnd;

            public int PageEnd
            {
                get
                {
                    return pageEnd;
                }
                set
                {
                    pageEnd = value;
                    PropertyChanged?.Invoke(this, new(nameof(PageEnd)));
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            public override string ToString() => $"[{PageStart} - {PageEnd}]";
        }

        private static Regex imageSizeParser = new Regex(@"(\d+)-(\d+)(-\w*)?\.\w+");

        private readonly EhClient client;
        public GalleryDetail Detail { get; init; }

        public uint GridItemWidth => Setting<GlobalSetting>.Current.PictureGridItemWidth;
        public uint GridItemMarginWidth => Setting<GlobalSetting>.Current.PictureGridItemMarginWidth;
        public int PullImagesCount => Setting<GlobalSetting>.Current.GetPictureCountPerLoad;
        public PageRange LoadedPagesRange { get; set; } = new();
        private Task pullingTask;

        public ObservableCollection<ImageInfoEx> PreviewImages { get; } = new();

        public EHentaiGalleryImageListPage(EhClient client, GalleryDetail detail)
        {
            this.client = client;
            this.Detail = detail;

            InitializeComponent();
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigationPop();
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private async void ScrollViewer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var height = (sender as ScrollViewer).ScrollableHeight;

            if (e.Delta > 0 && height == 0)
                await TryReadMore(false);
        }

        private async void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            Log.Debug($"e.ViewportHeightChange : {e.VerticalOffset}");
            var height = (sender as ScrollViewer).ScrollableHeight;
            var at_end = e.VerticalOffset >= height;

            if (at_end)
                await TryReadMore();
        }

        private async Task TryReadMore(bool isLoadNextPage = true)
        {
            if (pullingTask != null)
                return;
            var taskSource = new TaskCompletionSource();
            pullingTask = taskSource.Task;

            Log.Debug($"Begin try read more , Range : {LoadedPagesRange} , isLoadNextPage : {isLoadNextPage}");

            static ImageInfoEx Parse(GalleryPreview preview, int referencePageIndex)
            {
                var match = imageSizeParser.Match(preview.ImageUrl);
                if (match.Success)
                {
                    var width = int.Parse(match.Groups[1].Value);
                    var height = int.Parse(match.Groups[2].Value);

                    return new()
                    {
                        ImageAsync = new ImageAsyncLoadingParam()
                        {
                            ImageUrl = preview.ImageUrl,
                            PreviewImageDownloadUrl = preview.ImageUrl
                        },
                        PreviewImageSize = new ImageSize()
                        {
                            Width = width,
                            Height = height
                        },
                        Param = (referencePageIndex, preview.ImageUrl),
                    };
                }
                return default;
            }

            if (isLoadNextPage)
            {
                if (LoadedPagesRange.PageEnd >= Detail.PreviewPages)
                {
                    Log.Debug($"No more images provided. PageEnd : {LoadedPagesRange.PageEnd} >= {Detail.PreviewPages}");
                    Toast.ShowMessage("no more");
                    taskSource.SetResult();
                    pullingTask = null;
                    return;
                }
            }
            else
            {
                if (LoadedPagesRange.PageStart <= 0)
                {
                    Log.Debug($"No more images provided. PageStart : {LoadedPagesRange.PageStart} <= 0");
                    taskSource.SetResult();
                    pullingTask = null;
                    return;
                }
            }

            if (isLoadNextPage)
            {
                var item = await client.GetPreviewSetAsync(Detail, LoadedPagesRange.PageEnd);
                if (item.Value != 0)
                {
                    LoadedPagesRange.PageEnd++;

                    for (int i = 0; i < item.Key.Size; i++)
                    {
                        var preview = item.Key.GetGalleryPreview(Detail.Gid, i);
                        if (Parse(preview, LoadedPagesRange.PageEnd) is ImageInfoEx info)
                            PreviewImages.Add(info);
                    }
                }
            }
            else
            {
                var item = await client.GetPreviewSetAsync(Detail, LoadedPagesRange.PageStart);

                if (item.Value != 0)
                {
                    LoadedPagesRange.PageStart--;

                    for (int i = 0; i < item.Key.Size; i++)
                    {
                        var preview = item.Key.GetGalleryPreview(Detail.Gid, i);
                        if (Parse(preview, LoadedPagesRange.PageStart) is ImageInfoEx info)
                            PreviewImages.Insert(0, info);
                    }
                }
            }

            taskSource.SetResult();
            Log.Debug($"Page Range : {LoadedPagesRange}");
            pullingTask = null;
        }

        private void PageJumpLabel_Click(object sender, RoutedEventArgs e)
        {
            JumpPageInput.Text = LoadedPagesRange.PageEnd.ToString();
            PageJumpPopup.IsOpen = true;
        }

        private void StackPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            PageJumpPopup.IsOpen = false;
        }

        private async void JumpConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (pullingTask != null)
                await pullingTask;

            var targetStartPage = Math.Max(1, Math.Min(Detail.PreviewPages, int.Parse(JumpPageInput.Text)));

            if (LoadedPagesRange.PageStart <= targetStartPage && targetStartPage <= LoadedPagesRange.PageEnd)
            {
                Log.Debug($"targetStartPage in LoadedPagesRange {LoadedPagesRange}");
                //在已读范围内，可以计算跳转
                if (PreviewImages.FirstOrDefault(x => x.Param.ReferencePageIndex == targetStartPage) is ImageInfoEx info)
                {
                    var refContainer = PreviewImageList.ItemContainerGenerator.ContainerFromItem(info) as FrameworkElement;
                    refContainer.BringIntoView();
                }
            }
            else
            {
                //钦定新的
                LoadedPagesRange = new() { PageStart = targetStartPage - 1, PageEnd = targetStartPage - 1 };
                PreviewImages.Clear();
                await TryReadMore();
            }
        }
    }
}

