using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace PullRequestMonitor.ViewModel
{
    public sealed class CountToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new BitmapImage(ConvertCore(value, targetType, parameter, culture));
        }

        public Uri ConvertCore(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Uri bitmapUri;
            var count = value as int?;

            if (count.HasValue && count.Value > 9) count = 9;

            switch (count)
            {
                case 0:
                    bitmapUri = new Uri("pack://application:,,,/Resources/zero.ico");
                    break;
                case 1:
                    bitmapUri = new Uri("pack://application:,,,/Resources/one.ico");
                    break;
                case 2:
                    bitmapUri = new Uri("pack://application:,,,/Resources/two.ico");
                    break;
                case 3:
                    bitmapUri = new Uri("pack://application:,,,/Resources/three.ico");
                    break;
                case 4:
                    bitmapUri = new Uri("pack://application:,,,/Resources/four.ico");
                    break;
                case 5:
                    bitmapUri = new Uri("pack://application:,,,/Resources/five.ico");
                    break;
                case 6:
                    bitmapUri = new Uri("pack://application:,,,/Resources/six.ico");
                    break;
                case 7:
                    bitmapUri = new Uri("pack://application:,,,/Resources/seven.ico");
                    break;
                case 8:
                    bitmapUri = new Uri("pack://application:,,,/Resources/eight.ico");
                    break;
                case 9:
                    bitmapUri = new Uri("pack://application:,,,/Resources/nineplus.ico");
                    break;
                default:
                    bitmapUri = new Uri("pack://application:,,,/Resources/unknown.ico");
                    break;
            }

            return bitmapUri;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("Bindings relying on this converter should be OneWay or OneTime");
        }
    }
}