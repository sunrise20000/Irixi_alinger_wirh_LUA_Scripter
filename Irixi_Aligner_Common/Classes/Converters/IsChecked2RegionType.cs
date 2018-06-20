using Irixi_Aligner_Common.Vision;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Irixi_Aligner_Common.Classes.Converters
{
    class IsChecked2RegionType : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (Enum_REGION_TYPE)value == Vision.Enum_REGION_TYPE.CIRCLE;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Enum_REGION_TYPE.CIRCLE : Enum_REGION_TYPE.RECTANGLE;
        }
    }
}
