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

namespace WbooruPlugin.EHentai.UI.Pages
{
    /// <summary>
    /// EHentaiCommentListPage.xaml 的交互逻辑
    /// </summary>
    public partial class EHentaiGalleryInfoPage : Page
    {
        public ObservableCollection<KeyValuePair<string, string>> Infos { get; private set; } = new();

        public EHentaiGalleryInfoPage(GalleryDetail detail)
        {
            InitializeComponent();

            foreach (var item in detail.GetType().GetProperties())
                Infos.Add(KeyValuePair.Create(item.Name, item.GetValue(detail)?.ToString()));
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigationPop();
        }
    }
}
