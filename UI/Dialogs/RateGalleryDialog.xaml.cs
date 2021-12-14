using AngleSharp.Common;
using EHentaiAPI.Client;
using EHentaiAPI.Client.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using Wbooru.UI.Controls;
using Wbooru.UI.Dialogs;

namespace WbooruPlugin.EHentai.UI.Dialogs
{
    /// <summary>
    /// FavoriteSelectDialog.xaml 的交互逻辑
    /// </summary>
    public partial class RateGalleryDialog : DialogContentBase
    {
        private readonly EhClient client;
        private readonly EHentaiImageGalleryImageDetail detail;

        public string ButtonContent
        {
            get { return (string)GetValue(ButtonContentProperty); }
            set { SetValue(ButtonContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ButtonContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ButtonContentProperty =
            DependencyProperty.Register("ButtonContent", typeof(string), typeof(RateGalleryDialog), new PropertyMetadata("确定投票"));

        public RateGalleryDialog(EhClient client, EHentaiImageGalleryImageDetail detail)
        {
            this.client = client;
            this.detail = detail;
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            ButtonContent = "发送中...";
            var rateResult = await client.RateGalleryAsync(detail.Detail, StarsDisplayer.Stars);
            detail.Detail.Rated = true;
            detail.Detail.RatingCount = rateResult.ratingCount;
            detail.Detail.Rating = rateResult.rating;

            Toast.ShowMessage("投票成功");
            CloseDialog();
        }
    }
}
