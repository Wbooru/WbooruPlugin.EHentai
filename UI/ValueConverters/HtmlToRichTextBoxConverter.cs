using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Xaml;

namespace WbooruPlugin.EHentai.UI.ValueConverters
{
    public class HtmlToRichTextBoxConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var html = value?.ToString();
                var xaml = HTMLConverter.HtmlToXamlConverter.ConvertHtmlToXaml(html,false);
                var xamlControls = System.Windows.Markup.XamlReader.Parse(xaml) as FlowDocument;

                return xamlControls;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
