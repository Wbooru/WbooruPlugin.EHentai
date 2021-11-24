using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WbooruPlugin.EHentai.UI.ValueConverters
{
    public class YourRateStatusConverter : IValueConverter
    {
        public object Convert(object s, Type targetType, object p, CultureInfo culture)
        {
            return (float)s == 0 ? "您还没投票" : $"您已投{s}分";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
