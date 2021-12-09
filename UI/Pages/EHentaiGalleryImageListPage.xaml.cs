using EHentaiAPI.Client;
using EHentaiAPI.Client.Data;
using MikiraSora.VirtualizingStaggeredPanel;
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
using Wbooru.Utils;

namespace WbooruPlugin.EHentai.UI.Pages
{
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

        public ObservableCollection<DetailImageInfo> PreviewImages { get; } = new();

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

        private async void ScrollViewer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = (sender as ScrollViewer);
            var verticalOffset = scrollViewer.VerticalOffset;

            if (e.Delta > 0 && verticalOffset == 0)
                await TryReadBefore();
        }

        private async void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var height = (sender as ScrollViewer).ScrollableHeight;
            var at_end = e.VerticalOffset >= height;

            if (at_end)
                await TryReadMore();
            else if (e.VerticalChange < 0 && e.VerticalOffset == 0)
                await TryReadBefore();

            Log.Debug($"Scroll Changed, VerticalOffset : {e.VerticalOffset} , e : {e.VerticalChange}");
        }

        private async Task TryReadBefore()
        {
            var beforePosItem = PreviewImages.FirstOrDefault();

            //如果实际有东西添加了就滚回一下
            if (await TryReadMore(false))
            {
                if (beforePosItem is DetailImageInfo info)
                {
                    var refContainer = PreviewImageList.ItemContainerGenerator.ContainerFromItem(info) as FrameworkElement;
                    refContainer.BringIntoView();
                }
            }
        }

        private async Task<bool> TryReadMore(bool isLoadNextPage = true)
        {
            if (pullingTask != null)
                return false;
            var taskSource = new TaskCompletionSource<bool>();
            pullingTask = taskSource.Task;

            Log.Debug($"Begin try read more , Range : {LoadedPagesRange} , isLoadNextPage : {isLoadNextPage}");

            static DetailImageInfo Parse(GalleryPreview preview, int referencePageIndex)
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
                    taskSource.SetResult(false);
                    pullingTask = null;
                    return false;
                }
            }
            else
            {
                if (LoadedPagesRange.PageStart <= 0)
                {
                    Log.Debug($"No more images provided. PageStart : {LoadedPagesRange.PageStart} <= 0");
                    taskSource.SetResult(false);
                    pullingTask = null;
                    return false;
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
                        if (Parse(preview, LoadedPagesRange.PageEnd) is DetailImageInfo info)
                            PreviewImages.Add(info);
                    }
                }
            }
            else
            {
                var item = await client.GetPreviewSetAsync(Detail, LoadedPagesRange.PageStart - 1);

                if (item.Value != 0)
                {
                    LoadedPagesRange.PageStart--;

                    for (int i = item.Key.Size - 1; i >= 0; i--)
                    {
                        var preview = item.Key.GetGalleryPreview(Detail.Gid, i);
                        if (Parse(preview, LoadedPagesRange.PageStart) is DetailImageInfo info)
                            PreviewImages.Insert(0, info);
                    }
                }
            }

            taskSource.SetResult(true);
            Log.Debug($"Page Range : {LoadedPagesRange}");
            pullingTask = null;
            return true;
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
                if (PreviewImages.FirstOrDefault(x => x.Param.ReferencePageIndex == targetStartPage) is DetailImageInfo info)
                {
                    if (VisualTreeHelperEx.GetAllRecursively<VirtualizingStaggeredPanel>(PreviewImageList).FirstOrDefault() is VirtualizingStaggeredPanel panel)
                    {
                        if (panel.FindScrollOffsetByItem(info) is double offset)
                            panel.ScrollOwner?.ScrollToVerticalOffset(offset);
                    }
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

