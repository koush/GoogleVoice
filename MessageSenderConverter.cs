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
    public class MessageSenderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
			if (targetType == typeof(Visibility))
			{
				Message message = value as Message;
				if (message == ConversationPage.dummy)
					return Visibility.Collapsed;
				return message.Incoming ^ bool.Parse(parameter as string) ? Visibility.Visible : Visibility.Collapsed;
			}
			else if (targetType == typeof(Brush))
				return (value as Message) == ConversationPage.dummy ? App.Current.Resources["TransparentBrush"] : App.Current.Resources["PhoneAccentBrush"];
			else if (targetType == typeof(ImageSource))
			{
				var convo = value as Conversation;
				return ContactPhotoConverter.GetImageSourceForNumber(convo.Dispatcher, convo.PhoneNumber);
			}
			return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
