using EHentaiAPI.Client;
using System;
using System.Collections.Generic;
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
using Wbooru.UI.Pages;

namespace WbooruPlugin.EHentai.UI.Pages
{
    /// <summary>
    /// EHentaiGalleryDetailPage.xaml 的交互逻辑
    /// </summary>
    public partial class EHentaiGalleryDetailPage : DetailImagePageBase
    {
        public EHentaiImageGalleryImageDetail Detail
        {
            get { return (EHentaiImageGalleryImageDetail)GetValue(DetailProperty); }
            set { SetValue(DetailProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Detail.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DetailProperty =
            DependencyProperty.Register("Detail", typeof(EHentaiImageGalleryImageDetail), typeof(EHentaiGalleryDetailPage), new PropertyMetadata(null));
        
        private readonly EhClient client;

        public EHentaiGalleryDetailPage(EhClient client)
        {
            InitializeComponent();
            this.client = client;
        }

        public override void ApplyItem(Gallery g, GalleryItem i)
        {
            if (!(g is EHentaiGallery gallery && i is EHentaiImageGalleryInfo info))
            {
                Log<EHentaiGalleryDetailPage>.Error("gallery != EHentaiGallery or info != EHentaiImageGalleryInfo");
                return;
            }

            Detail = gallery.GetImageDetial(info) as EHentaiImageGalleryImageDetail;
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var commentPage = new EHentaiCommentListPage(client, Detail.Detail);
            NavigationHelper.NavigationPush(commentPage);
        }
    }
}
