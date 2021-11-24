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

namespace WbooruPlugin.EHentai.UI.Controls
{
    /// <summary>
    /// FavoriteStarsDisplayer.xaml 的交互逻辑
    /// </summary>
    public partial class FavoriteStarsDisplayer : UserControl
    {
        public float Stars
        {
            get { return (float)GetValue(StarsProperty); }
            set { SetValue(StarsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Stars.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StarsProperty =
            DependencyProperty.Register("Stars", typeof(float), typeof(FavoriteStarsDisplayer), new PropertyMetadata(0f));

        public FavoriteStarsDisplayer()
        {
            InitializeComponent();
            MainPanel.DataContext = this;
        }
    }
}
