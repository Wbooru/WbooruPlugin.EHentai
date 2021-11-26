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
    public partial class EHentaiCommentListPage : Page, INavigatableAction
    {
        private readonly EhClient client;
        private readonly GalleryDetail detail;

        public ObservableCollection<GalleryComment> Comments { get; set; } = new ObservableCollection<GalleryComment>();

        public bool HasMore
        {
            get { return (bool)GetValue(HasMoreProperty); }
            set { SetValue(HasMoreProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HasMore.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HasMoreProperty =
            DependencyProperty.Register("HasMore", typeof(bool), typeof(EHentaiCommentListPage), new PropertyMetadata(true));

        public EHentaiCommentListPage(EhClient client, GalleryDetail detail)
        {
            InitializeComponent();
            this.client = client;
            this.detail = detail;
            Comments.Clear();

            foreach (var comment in detail.Comments.Comments)
                Comments.Add(comment);

            HasMore = detail.Comments.HasMore;
        }

        public void OnNavigationBackAction()
        {
            NavigationHelper.NavigationPop();
        }

        public void OnNavigationForwardAction()
        {
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            OnNavigationBackAction();
        }
    }
}
