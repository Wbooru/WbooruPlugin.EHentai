using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Wbooru.Utils;

namespace WbooruPlugin.EHentai.UI.ValueConverters
{
    public class RandomSoildColorBrush : IValueConverter
    {
        public object Convert(object s, Type targetType, object p, CultureInfo culture)
        {
            return new SolidColorBrush(Color.FromRgb((byte)MathEx.Random(0,256), (byte)MathEx.Random(0, 256), (byte)MathEx.Random(0, 256)));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
