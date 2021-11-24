using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WbooruPlugin.EHentai.UI.ValueConverters
{
    public class FavoriteStarsStatusConverter : IValueConverter
    {
        public object Convert(object s, Type targetType, object p, CultureInfo culture)
        {
            var starsValue = (float)s;
            var starPosition = int.Parse(p.ToString());

            //normalize
            starsValue = Math.Min(Math.Max(0, starsValue - starPosition), 1);

            return starsValue != 0 && starsValue != 1 ? 0.5 : starsValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
