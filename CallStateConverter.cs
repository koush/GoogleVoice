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
	public class CallStateConverter : IValueConverter
	{
		object missed;
		object outgoing;
		object incoming;
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			int type = (int)value;
			switch (type)
			{
				case 0:
					return missed ?? (missed = App.Current.Resources["MissedCall"]);
				case 1:
					return incoming ?? (incoming = App.Current.Resources["IncomingCall"]);
				case 7:
				case 8:
					return outgoing ?? (outgoing = App.Current.Resources["OutgoingCall"]);
			}
			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
