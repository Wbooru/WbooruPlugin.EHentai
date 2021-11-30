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
using Wbooru.Kernel;
using Wbooru.Models;
using Wbooru.Settings;
using Wbooru.UI.ValueConverters;

namespace WbooruPlugin.EHentai.UI.Pages
{
    /// <summary>
    /// EHentaiCommentListPage.xaml 的交互逻辑
    /// </summary>
    public partial class EHentaiGalleryImageListPage : Page
    {
        public uint GridItemWidth => Setting<GlobalSetting>.Current.PictureGridItemWidth;
        public uint GridItemMarginWidth => Setting<GlobalSetting>.Current.PictureGridItemMarginWidth;

        public ObservableCollection<ImageInfo> PreviewImages { get; } = new ();

        public EHentaiGalleryImageListPage(EhClient client, GalleryDetail detail)
        {
            InitializeComponent();
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigationPop();
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
