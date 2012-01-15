using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;

namespace GoogleVoice
{
	public partial class CallPage : PhoneApplicationPage
	{
		String mName;
		String mPhone;
		GoogleVoiceClient mVoice;
		System.Windows.Threading.DispatcherTimer mTimer = new System.Windows.Threading.DispatcherTimer();

		public CallPage()
		{
			InitializeComponent();

			mVoice = App.mVoice;

			Loaded += new RoutedEventHandler(CallPage_Loaded);
		}

		void CallPage_Loaded(object sender, RoutedEventArgs e)
		{
			NavigationContext.QueryString.TryGetValue("name", out mName);
			NavigationContext.QueryString.TryGetValue("phone", out mPhone);

			if (mName == mPhone)
				mContactNumber.Visibility = System.Windows.Visibility.Collapsed;

			mContactName.Text = mName;
			mContactNumber.Text = mPhone;

			var img = ContactPhotoConverter.GetImageSourceForNumber(Dispatcher, mPhone);
			if (img == null)
			{
				var conn = SqliteDatabase.Connection;
				lock (SqliteDatabase.Instance)
				{
					var findName = conn.Query<Conversation>(string.Format("SELECT OtherParticipant, Id FROM Conversation WHERE PhoneNumber LIKE '%{0}%' LIMIT 1", GoogleVoiceClient.NumbersOnly(mPhone))).FirstOrDefault();
					if (findName != null)
						img = ContactPhotoConverter.GetImageSourceForNumber(Dispatcher, findName.PhoneNumber);
				}
			}
			if (img != null && img != ContactPhotoConverter.unknown)
				mContactImage.Source = img;

			if (Settings.Instance.ShouldUsePINDial)
			{
				var t = new Microsoft.Phone.Tasks.PhoneCallTask();
				t.DisplayName = mName;
				t.PhoneNumber = mVoice.GetDirectDialNumber(mPhone);
				t.Show();
			}
			else
			{
				mVoice.Call(mPhone);
			}

			mTimer.Interval = new TimeSpan(0, 0, 90);
			mTimer.Tick += (a, b) =>
				{
					try
					{
						mTimer.Stop();
						NavigationService.GoBack();
					}
					catch (Exception)
					{
					}
				};
			mTimer.Start();
		}

		private void DialWithoutGoogleVoice_Click(object sender, RoutedEventArgs e)
		{
			PhoneCallTask task = new PhoneCallTask();
			task.DisplayName = mName;
			task.PhoneNumber = mPhone;
			task.Show();
		}
	}
}