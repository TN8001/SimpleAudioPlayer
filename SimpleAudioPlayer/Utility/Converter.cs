using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SimpleAudioPlayer
{
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        private BooleanToVisibilityConverter converter = new BooleanToVisibilityConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = converter.Convert(value, targetType, parameter, culture) as Visibility?;
            return result == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = converter.ConvertBack(value, targetType, parameter, culture) as bool?;
            return result == true ? false : true;
        }
    }

    public class TimeSpan2ShortTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(!(value is TimeSpan t)) throw new ArgumentException(nameof(value));
            return t.Hours > 0 ? t.ToString("g") : t.ToString(@"mm\:ss");
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class TimeSpan2TotalSecondsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(!(value is TimeSpan t)) throw new ArgumentException(nameof(value));
            return t.TotalSeconds;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(!(value is double d)) throw new ArgumentException(nameof(value));
            return TimeSpan.FromSeconds(d);
        }
    }
}
