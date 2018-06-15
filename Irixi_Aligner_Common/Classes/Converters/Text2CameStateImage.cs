using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Irixi_Aligner_Common.Classes.Converters
{
    public class Text2CameStateImage : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
           string strCameraState = value.ToString().ToUpper();
            BitmapImage bitmap = null;
            switch (strCameraState)
            {
                case "CONNECTED":
                    bitmap = new BitmapImage(new Uri(@"..\..\Resources\Images\Icons\Connected.png", UriKind.Relative));
                    break;
                case "DISCONNECTED":
                    bitmap = new BitmapImage(new Uri(@"..\..\Resources\Images\Icons\Disconnected.png", UriKind.Relative));
                    break;
                default:
                    break;
            }
            return bitmap;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
