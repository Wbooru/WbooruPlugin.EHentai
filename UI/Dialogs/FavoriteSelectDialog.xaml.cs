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
    public partial class FavoriteSelectDialog : DialogContentBase
    {
        private readonly EhClient client;
        private readonly EHentaiImageGalleryImageDetail detail;

        public class FavoriteDisplayItem : INotifyPropertyChanged
        {
            public string Name { get; set; }
            public int FavoriteSlot { get; set; }

            private bool isBusy;
            public bool IsBusy
            {
                get
                {
                    return isBusy;
                }
                set
                {
                    isBusy = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsBusy)));
                }
            }

            private bool isFavorited;
            public bool IsFavorited
            {
                get
                {
                    return isFavorited;
                }
                set
                {
                    isFavorited = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsFavorited)));
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
        }

        public List<FavoriteDisplayItem> DisplayItems { get; } = new();

        public FavoriteSelectDialog(EhClient client, EHentaiImageGalleryImageDetail detail)
        {
            this.client = client;
            this.detail = detail;
            InitializeDisplayItems();
            InitializeComponent();
        }

        private void InitializeDisplayItems()
        {
            /*
            DisplayItems.Add(new FavoriteDisplayItem()
            {
                Name = "本地收藏",
            });
            */
            for (int i = 0; i <= 9; i++)
            {
                FavoriteDisplayItem item = default;
                item = new FavoriteDisplayItem()
                {
                    Name = $"Favorites {i}",
                    FavoriteSlot = i,
                    IsFavorited = detail.Detail.FavoriteSlot == i,
                    IsBusy = false
                };
                DisplayItems.Add(item);
            }
        }

        private async void OnlineFavoriteClicked(FavoriteDisplayItem item)
        {
            if (item.IsBusy)
                return;

            DisplayItems.ForEach(x => x.IsBusy = true);

            if (!item.IsFavorited)
            {
                await client.AddFavoriteAsync(detail.Detail, item.FavoriteSlot, string.Empty);
                detail.Detail.IsFavorited = true;
                detail.Detail.FavoriteSlot = item.FavoriteSlot;
                detail.Detail.FavoriteName = item.Name;
                Toast.ShowMessage("添加收藏成功");
            }
            else
            {
                await client.RemoveFavoriteAsync(detail.Detail);
                detail.Detail.IsFavorited = false;
                detail.Detail.FavoriteSlot = -2;
                detail.Detail.FavoriteName = string.Empty;
                Toast.ShowMessage("取消收藏成功");
            }


            DisplayItems.ForEach(x => x.IsBusy = false);
            CloseDialog();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OnlineFavoriteClicked((sender as FrameworkElement)?.DataContext as FavoriteDisplayItem);
        }
    }
}
