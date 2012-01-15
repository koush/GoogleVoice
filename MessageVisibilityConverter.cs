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
using System.Windows.Media.Imaging;
using System.IO.IsolatedStorage;
using System.Collections.Generic;

namespace GoogleVoice
{
	public class MessageVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			string type = parameter as string;
			if (type == null)
				return Visibility.Visible;
			Conversation convo = value as Conversation;
			switch (convo.Type)
			{
				case 10:
				case 11:
					if (type == "sms")
						return Visibility.Visible;
					break;
				case 2:
					if (type == "voicemail")
						return Visibility.Visible;
					break;
			}
			return Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
