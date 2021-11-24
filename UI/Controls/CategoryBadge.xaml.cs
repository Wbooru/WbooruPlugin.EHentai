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
    /// CategoryBadge.xaml 的交互逻辑
    /// </summary>
    public partial class CategoryBadge : UserControl
    {
        public int Category
        {
            get { return (int)GetValue(CategoryProperty); }
            set { SetValue(CategoryProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Category.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CategoryProperty =
            DependencyProperty.Register("Category", typeof(int), typeof(CategoryBadge), new PropertyMetadata(1));

        public CategoryBadge()
        {
            InitializeComponent();
            this.MainLabel.DataContext = this;
        }
    }
}
