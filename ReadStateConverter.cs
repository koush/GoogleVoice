using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Data;

namespace GoogleVoice
{
    public class ReadStateConverter : IValueConverter
    {
		static object readStyle;
		static object unreadStyle;
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
			bool read = (bool)value;
			if (targetType == typeof(Style))
	            return read ? (readStyle ?? (readStyle = App.Current.Resources["MessageTextStyle"])) : (unreadStyle ?? (unreadStyle = App.Current.Resources["UnreadMessageTextStyle"]));
			return read ? "mark as unread" : "mark as read";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
