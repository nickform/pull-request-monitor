using System;
using System.Globalization;
using System.Windows.Data;
using Humanizer;

namespace PullRequestMonitor.ViewModel
{
    public class DateTimeToHumanFriendlyStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is DateTime)) return null;

            return ((DateTime) value).Humanize();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}