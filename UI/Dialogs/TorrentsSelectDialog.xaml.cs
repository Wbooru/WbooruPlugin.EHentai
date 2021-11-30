using AngleSharp.Common;
using EHentaiAPI.Client;
using EHentaiAPI.Client.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
    public partial class TorrentsSelectDialog : DialogContentBase
    {
        private readonly EhClient client;
        private readonly EHentaiImageGalleryImageDetail detail;

        public class TorrentItem
        {
            public string Name { get; set; }
            public string Url { get; set; }
        }

        public ObservableCollection<TorrentItem> DisplayItems { get; } = new();

        public TorrentsSelectDialog(EhClient client, EHentaiImageGalleryImageDetail detail)
        {
            this.client = client;
            this.detail = detail;
            InitializeComponent();
            InitializeTorrentList();
        }

        private async void InitializeTorrentList()
        {
            var torrents = await client.GetTorrentListAsync(detail.Detail);

            foreach (var pair in torrents)
            {
                DisplayItems.Add(new()
                {
                    Name = pair.Value,
                    Url = pair.Key
                });
            }
        }

        private void OnlineFavoriteClicked(TorrentItem item)
        {
            Process.Start(new ProcessStartInfo(item.Url)
            {
                UseShellExecute = true
            });
            CloseDialog();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OnlineFavoriteClicked((sender as FrameworkElement)?.DataContext as TorrentItem);
        }
    }
}
