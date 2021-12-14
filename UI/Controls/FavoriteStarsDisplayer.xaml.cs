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

namespace WbooruPlugin.EHentai.UI.Controls
{
    /// <summary>
    /// FavoriteStarsDisplayer.xaml 的交互逻辑
    /// </summary>
    public partial class FavoriteStarsDisplayer : UserControl
    {
        public bool Clickable
        {
            get { return (bool)GetValue(ClickableProperty); }
            set { SetValue(ClickableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Clickable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ClickableProperty =
            DependencyProperty.Register("Clickable", typeof(bool), typeof(FavoriteStarsDisplayer), new PropertyMetadata(false));

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

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is not TextBlock t)
                return;
            var p = e.GetPosition(t);
            var stars = int.Parse(t.Name.Replace("Star","")) + (p.X < t.ActualWidth / 2 ? 0.5f : 1);
            Stars = stars;
        }
    }
}
