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
using System.Linq;
using Microsoft.Phone;
using System.Windows.Threading;

namespace GoogleVoice
{
    public class ContactPhotoConverter : IValueConverter
    {
		static internal Dictionary<string, ImageSource> mContactImages = new Dictionary<string, ImageSource>();
		static IEnumerable<PhotoEntry> mAvailableEntries = null;

		static ContactPhotoConverter()
		{
			RefreshFiles();
		}
		static internal void RefreshFiles()
		{
			if (System.ComponentModel.DesignerProperties.IsInDesignTool)
				return;
			SqliteDatabase.Connection.CreateTable<PhotoEntry>();
			mAvailableEntries = SqliteDatabase.Connection.Query<PhotoEntry>("SELECT * FROM PhotoEntry");
			foreach (string img in IsolatedStorageFile.GetUserStoreForApplication().GetFileNames("*.png"))
			{
				// wow... it returns everything regardless of wildcard.
				if (!img.EndsWith(".png"))
					continue;
				if (mContactImages.ContainsKey(img))
					continue;
				mContactImages.Add(img, null);
			}
		}

		static ImageSource LoadImage(string img)
		{
			if (!IsolatedStorageFile.GetUserStoreForApplication().FileExists(img))
				return null;
			using (var s = IsolatedStorageFile.GetUserStoreForApplication().OpenFile(img, System.IO.FileMode.Open))
			{
				var ff = PictureDecoder.DecodeJpeg(s);
				mContactImages[img] = ff;
				return ff;
			}
		}

		static Dictionary<string, bool> mPendingImages = new Dictionary<string, bool>();

		internal static ImageSource GetImageSourceForNumber(Dispatcher dispatcher, string number)
		{
			if (string.IsNullOrEmpty(number))
				return null;
			ImageSource ret;
			// first check if we have it
			string img = GoogleVoiceClient.NumbersOnly(number) + ".png";
			if (mContactImages.TryGetValue(img, out ret))
			{
				if (ret != null)
					return ret;
				try
				{
					return LoadImage(img);
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine(ex);
					try
					{
						 IsolatedStorageFile.GetUserStoreForApplication().DeleteFile(img);
					}
					catch (Exception)
					{
					}
					mContactImages.Remove(img);
				}
			}
			// now let's see if we can download it
			if (mAvailableEntries != null)
			{
				PhotoEntry pe = (from entry in mAvailableEntries where GoogleVoiceClient.NumbersMatch(number, entry.Number) select entry).FirstOrDefault();
				if (pe != null)
				{
					// trigger the download, set unknown contact for now
					mContactImages.Add(img, (unknown ?? (unknown = App.Current.Resources["UnknownContact"])) as ImageSource);
					mPendingImages[img] = true;
					App.mVoice.GetPhoto(Settings.Instance.Username, Settings.Instance.Password, number, pe, () =>
						{
							dispatcher.BeginInvoke(() =>
								{
									LoadImage(img);
									mPendingImages.Remove(img);
									if (mPendingImages.Count == 0)
									{
										App.mVoice.Conversations.Reset();
										App.mVoice.Texts.Reset();
										App.mVoice.Voicemail.Reset();
										App.mVoice.Calls.Reset();
									}
								});
						});
				}
			}

			return null;
		}

		internal static object unknown;
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
			Conversation convo = value as Conversation;
			var ret = GetImageSourceForNumber(convo.Dispatcher, convo.PhoneNumber);
			if (ret != null)
				return ret;
			//return convo.IsRead ^ Boolean.Parse(parameter as string) ? read ?? (read = App.Current.Resources["ReadMessageSmall"]) : unread ?? (unread = App.Current.Resources["UnreadMessageSmall"]);
			return unknown ?? (unknown = App.Current.Resources["UnknownContact"]);
			//return (value as Conversation).IsRead ^ Boolean.Parse(parameter as string) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
