using CGM_by_ZnZ.Model;
using CGM_by_ZnZ.VM;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace CGM_by_ZnZ.Converters
{
    public class AddCharMaskConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                string c = (string)values[0];
                string chars = (string) values[1];
                if (string.IsNullOrWhiteSpace(c))
                    return null;
                return new CharMaskModel()
                {
                    Char = c[0],
                    Chars = chars
                };
            }
            catch
            {
                return null;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
