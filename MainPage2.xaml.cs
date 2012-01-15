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
using Microsoft.Phone.Notification;
using Microsoft.Phone.Shell;

namespace GoogleVoice
{
    public partial class MainPage2 : PhoneApplicationPage
    {
        GoogleVoiceClient mVoice;
		public MainPage2()
		{
			InitializeComponent();

			if (Settings.IsTrial)
				mAdControl.Visibility = System.Windows.Visibility.Visible;

			mVoice = App.mVoice;
			DataContext = mVoice;
			Loaded += new RoutedEventHandler(PageLoaded);
			Unloaded += new RoutedEventHandler(PageUnloaded);

			mVoice.DownloadContactsStarted += new Action(Voice_DownloadContactsStarted);
			mVoice.DownloadContactsCompleted += new Action(Voice_DownloadContactsCompleted);
			mVoice.RefreshInboxStarted += new Action(Voice_RefreshInboxStarted);
			mVoice.RefreshInboxCompleted += new Action(Voice_RefreshInboxCompleted);

			mPivot.LoadedPivotItem += new EventHandler<PivotItemEventArgs>(Pivot_LoadedPivotItem);
			mPivot.LoadingPivotItem += new EventHandler<PivotItemEventArgs>(Pivot_LoadingPivotItem);

			var settings = Settings.Instance;

			if (!settings.HasValidLogin)
			{
				ShowLogin(true);
			}
			else
			{
				mVersion.Text = "1.2";

				if (settings.LastWhatsNew != mVersion.Text)
				{
					settings.LastWhatsNew = mVersion.Text;
					mChangelog.Items.Add("* Push is available to trial version users (ads enabled)");
					mChangelog.Items.Add("* Push notifications are now sent for voicemail");
					mChangelog.Items.Add("* Call Log");
					mChangelog.Items.Add("* Search Conversations and Voicemail");
					mChangelog.Items.Add("* Added keyboard autocompletion");
					mChangelog.Items.Add("* Outgoing calls now open a call screen");
					mChangelog.Items.Add("* Fixed potential application startup crash");
					mChangelog.Items.Add("* Contact photo downloads are more reliable");
					mChangelog.Items.Add("* Progress indicator now has a textual description");
					mChangelog.Items.Add("* Conversations can be deleted");
					mChangelog.Items.Add("* Google Voice can be dialed via the application menu");
					mChangelog.Items.Add("* Direct/PIN Dial is available via the settings menu");
					mChangelog.Items.Add("* Fixed conversation sort order");
					ShowWhatsNew(true);
					settings.Save();
				}

				mProgressBar.SetIsVisible(true);
				StatusText = "Logging in...";
				mVoice.Login(settings.Username, settings.Password, (authToken, rnrse, googleVoicePhoneNumber) =>
				{
					mProgressBar.SetIsVisible(false);
					settings.AuthToken = authToken;
					settings.GoogleVoicePhoneNumber = googleVoicePhoneNumber;
					settings.RNRSE = rnrse;
					settings.Save();
					mVoice.RefreshInbox();
				},
					() =>
					{
						mProgressBar.SetIsVisible(false);
					});
			}
		}

		void Voice_DownloadContactsStarted()
		{
			mProgressBar.SetIsVisible(true);
			StatusText = "Downloading Contact Photos...";
		}

		void Voice_DownloadContactsCompleted()
		{
			mProgressBar.SetIsVisible(false);
			StatusText = string.Empty;
		}

		void Pivot_LoadingPivotItem(object sender, PivotItemEventArgs e)
		{
			if (e.Item == mDialerPivot)
				mCurrentNumber.Text = string.Empty;
		}

		void Pivot_LoadedPivotItem(object sender, PivotItemEventArgs e)
		{
			(ApplicationBar.Buttons[2] as ApplicationBarIconButton).IsEnabled = e.Item != mDialerPivot;
		}

		void Voice_RefreshInboxCompleted()
		{
			mProgressBar.SetIsVisible(false);
			mVoice.GetPhotosForNumbers(Settings.Instance.Username, Settings.Instance.Password);
		}

		void Voice_RefreshInboxStarted()
		{
			StatusText = "Refreshing Inbox...";
			mProgressBar.SetIsVisible(true);
		}

		void PageUnloaded(object sender, RoutedEventArgs e)
		{
			//if (App.myChannel != null)
			//	App.myChannel.ShellToastNotificationReceived -= mToastHandler;
		}

		EventHandler<Microsoft.Phone.Notification.NotificationEventArgs> mToastHandler;
		void PageLoaded(object sender, RoutedEventArgs e)
		{
//            var inject = "<html><body>" +
//                "<script type=\"text/javascript\"><!--" +
//  "// XHTML should not attempt to parse these strings, declare them CDATA." +
//  "/* <![CDATA[ */" +
//  "window.googleAfmcRequest = {" +
//    "client: 'ca-mb-pub-0806259031233516'," +
//    "format: '320x50_mb'," +
//    "output: 'html'," +
//    "slotname: '1516197906'," +
//  "};" +
//  "/* ]]> */" +
//"//--></script>" +
//"<script type=\"text/javascript\"    src=\"http://pagead2.googlesyndication.com/pagead/show_afmc_ads.js\"></script>" +
//"</body></html>";
//            mAd.NavigateToString(inject);
	
			SetupChannel();
		}

		void SetupChannel()
		{
			if (App.myChannel != null)
			{
				//mToastHandler = new EventHandler<NotificationEventArgs>(ShellToastNotificationReceived);
				//App.myChannel.ShellToastNotificationReceived += mToastHandler;
			}
		}

        private void Refresh_Click(object sender, EventArgs e)
        {
            mVoice.RefreshInbox();
        }

        private void Compose_Click(object sender, EventArgs e)
        {
			if (mPivot.SelectedItem == mDialerPivot)
			{
				string curNumber = mCurrentNumber.Text;
				if (string.IsNullOrEmpty(curNumber))
					return;

				mCurrentNumber.Text = string.Empty;
				Dispatcher.BeginInvoke(() =>
				{
					NavigationService.Navigate(new Uri(string.Format("/ConversationPage.xaml?phone={0}", Uri.EscapeDataString(curNumber)), UriKind.Relative));
				});
			}
			else
			{

				PhoneNumberChooserTask phoneTask = new PhoneNumberChooserTask();
				phoneTask.Completed += (o, r) =>
				{
					if (r.TaskResult != TaskResult.OK)
						return;

					Dispatcher.BeginInvoke(() =>
						{
							NavigationService.Navigate(new Uri(string.Format("/ConversationPage.xaml?phone={0}", Uri.EscapeDataString(r.PhoneNumber)), UriKind.Relative));
						});
				};
				try
				{
					phoneTask.Show();
				}
				catch (Exception)
				{
				}
			}
        }

        private void Call_Click(object sender, EventArgs e)
        {
			if (mPivot.SelectedItem == mDialerPivot)
			{
				string curNumber = mCurrentNumber.Text;
				if (string.IsNullOrEmpty(curNumber))
					return;
				mCurrentNumber.Text = string.Empty;
				App.Call(curNumber, curNumber, NavigationService);
			}
			else
			{
				PhoneNumberChooserTask phoneTask = new PhoneNumberChooserTask();
				phoneTask.Completed += (o, r) =>
				{
					if (r.TaskResult != TaskResult.OK)
						return;
					Dispatcher.BeginInvoke(() =>
						{
							App.Call(r.PhoneNumber, r.PhoneNumber, NavigationService);
						});
				};
				phoneTask.Show();
			}
        }

		void LockControls()
		{
			mPinDialCheckBox.IsEnabled = false;
			mPinNumberText.IsEnabled = false;
			mLoginButton.IsEnabled = false;
			mUsernameText.IsEnabled = false;
			mPasswordText.IsEnabled = false;
			mPhoneNumberText.IsEnabled = false;
			mProgressBar.SetIsVisible(true);
		}

		private void mLoginButton_Click(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrEmpty(mPhoneNumberText.Text))
			{
				mError.Text = "Please enter your phone number.";
				return;
			}

			if (mPinDialCheckBox.IsChecked != null && mPinDialCheckBox.IsChecked.Value)
			{
				if (string.IsNullOrEmpty(mPinNumberText.Text))
				{
					mError.Text = "Please enter your Google Voice PIN number or use havee Google Voice call you to connect your outgoing calls.";
					return;
				}
			}

			bool hadValidLogin = Settings.Instance.HasValidLogin;
			LockControls();
			mVoice.Login(mUsernameText.Text, mPasswordText.Password, (authToken, rnrse, googleVoicePhoneNumber) =>
			{
				var settings = Settings.Instance;
				settings.Username = mUsernameText.Text;
				settings.Password = mPasswordText.Password;
				settings.DevicePhoneNumber = mPhoneNumberText.Text;
                settings.AuthToken = authToken;
                settings.RNRSE = rnrse;
				settings.GoogleVoicePhoneNumber = googleVoicePhoneNumber;
				settings.PIN = mPinNumberText.Text;
				settings.UsePINDialPrefix = mPinDialCheckBox.IsChecked.HasValue && mPinDialCheckBox.IsChecked.Value;
				settings.Save();
				App.SendChannelUri();
				ShowLogin(false);
				mVoice.PIN = mPinNumberText.Text;
				mVoice.DevicePhoneNumber = mPhoneNumberText.Text;
				mVoice.RefreshInbox();
				if (!hadValidLogin)
				{
					ShowPushSettings(true);
				}
			},
				() =>
				{
					mError.Text = "Login failed. Please verify you are connected to the internet and that your username and password are correct.";
					UnlockControls();
				});
		}

		void UnlockControls()
		{
			mPinDialCheckBox.IsEnabled = true;
			mPinNumberText.IsEnabled = true;
			mLoginButton.IsEnabled = true;
			mUsernameText.IsEnabled = true;
			mPasswordText.IsEnabled = true;
			mPhoneNumberText.IsEnabled = true;
			mProgressBar.SetIsVisible(false);
		}

		public string StatusText
		{
			set
			{
				mStatus.Text = value;
			}
		}

		void ShowWhatsNew(bool show)
		{
			if (show)
			{
				ApplicationBar.IsVisible = false;
				mPivot.IsEnabled = false;
				mWhatsNewPopup.IsOpen = true;

			}
			else
			{
				ApplicationBar.IsVisible = true;
				mPivot.IsEnabled = true;
				mWhatsNewPopup.IsOpen = false;
			}
		}

		void ShowLogin(bool show)
		{
			if (show)
			{
				mAdControl.Visibility = System.Windows.Visibility.Collapsed;
				ApplicationBar.IsVisible = false;
				mPinDialCheckBox.IsChecked = Settings.Instance.ShouldUsePINDial;
				if (Settings.Instance.PIN != null)
					mPinNumberText.Text = Settings.Instance.PIN;
				PinDial_Checked(null, null);
				UnlockControls();
				mLoginPopup.IsOpen = true;
				mPivot.Visibility = System.Windows.Visibility.Collapsed;
				ApplicationBar.IsVisible = false;
				mUsernameText.Text = Settings.Instance.Username ?? string.Empty;
				mPasswordText.Password = Settings.Instance.Password ?? string.Empty;
				mPhoneNumberText.Text = Settings.Instance.DevicePhoneNumber ?? string.Empty;
				mPinNumberText.Text = Settings.Instance.PIN ?? string.Empty;
#if DEBUG
				mUsernameText.Text = "koush@koushikdutta.com";
				mPasswordText.Password = "k9copusa";
				mPhoneNumberText.Text = "2066617407";
#endif
			}
			else
			{
				if (Settings.IsTrial)
					mAdControl.Visibility = System.Windows.Visibility.Visible;
				ApplicationBar.IsVisible = true;
				mLoginPopup.IsOpen = false;
				mPivot.Visibility = Visibility.Visible;
				ApplicationBar.IsVisible = true;
			}
		}

		private void PinDial_Checked(object sender, RoutedEventArgs e)
		{
			// we null check here because we change the check setting on startup
			if (mPinDialCheckBox == null || mPinDialCheckBox.IsChecked == null)
				return;
			if (mPinDialCheckBox.IsChecked.Value)
			{
				mPinNumberBlock.Visibility = System.Windows.Visibility.Visible;
				mPinNumberText.Visibility = System.Windows.Visibility.Visible;
			}
			else
			{
				mPinNumberBlock.Visibility = System.Windows.Visibility.Collapsed;
				mPinNumberText.Visibility = System.Windows.Visibility.Collapsed;
			}
		}

		private void AccountSettings_Click(object sender, EventArgs e)
		{
			ShowLogin(true);
		}

		private void CallGoogleVoice_Click(object sender, EventArgs e)
		{
			PhoneCallTask task = new PhoneCallTask();
			task.DisplayName = "Voicemail";
			task.PhoneNumber = GoogleVoiceClient.NumbersOnly(Settings.Instance.GoogleVoicePhoneNumber);
			task.Show();
		}

		void SyncPushSettings()
		{
			var settings = Settings.Instance;
			mToggleToast.IsChecked = settings.ToastNotificationEnabled;
			mToggleTile.IsChecked = settings.TileNotificationEnabled;
		}

		void ShowPushSettings(bool show)
		{
			if (show)
			{
				ApplicationBar.IsVisible = false;
				SyncPushSettings();
				mPivot.IsEnabled = false;
				mPushFailedPanel.Visibility = Visibility.Collapsed;
				mPushRetrievedPanel.Visibility = Visibility.Collapsed;
				mRetrievingPushPanel.Visibility = System.Windows.Visibility.Visible;
				mPushPopup.IsOpen = true;

				WebClient client = new WebClient();
				client.DownloadStringCompleted += (s, e) =>
					{
						try
						{
							var result = Newtonsoft.Json.Linq.JObject.Parse(e.Result);
							string url = result["data"].Value<string>("url");
							if (string.IsNullOrEmpty(url))
								throw new Exception();

							mRetrievingPushPanel.Visibility = System.Windows.Visibility.Collapsed;
							mPushRetrievedPanel.Visibility = System.Windows.Visibility.Visible;
							mPushSetupUrl.Text = url;
						}
						catch (Exception)
						{
						}
					};

				client.DownloadStringAsync(new Uri("http://api.bit.ly/v3/shorten?login=koush&apiKey=R_b12315a324b430e592df51c655fa2691&longUrl=" + Uri.EscapeDataString("http://www.koushikdutta.com/2010/11/push-notification-setup-for-gvoice-on.html?gvoice_unique_id=" + Settings.Instance.DeviceId)));
			}
			else
			{
				ApplicationBar.IsVisible = true;
				mPivot.IsEnabled = true;
				mPushPopup.IsOpen = false;
			}
		}

		private void PushSettings_Click(object sender, EventArgs e)
		{
			ShowPushSettings(true);
		}

		private void PhoneButton_Click(object sender, RoutedEventArgs e)
		{
			Button button = sender as Button;
			if (button == null)
				return;
			string command = button.DataContext as string;
			if (command == null)
				return;

			string curNumber = mCurrentNumber.Text;
			if (command == "Delete")
			{
				if (!string.IsNullOrEmpty(curNumber))
					curNumber = curNumber.Substring(0, curNumber.Length - 1);
			}
			else
			{
				curNumber += command;
			}
			mCurrentNumber.Text = curNumber;
		}

		protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
		{
			base.OnBackKeyPress(e);

			if (mPushPopup.IsOpen)
			{
				e.Cancel = true;
				ShowPushSettings(false);
			}

			if (mWhatsNewPopup.IsOpen)
			{
				e.Cancel = true;
				ShowWhatsNew(false);
			}

			if (mLoginPopup.IsOpen && Settings.Instance.HasValidLogin)
			{
				e.Cancel = true;
				ShowLogin(false);
			}
		}

		private void ToggleToast_Checked(object sender, RoutedEventArgs e)
		{
			var settings = Settings.Instance;
			settings.ToastNotificationEnabled = mToggleToast.IsChecked.Value;
			settings.Save();
			App.SyncWithPushSettings();
		}
		
		private void ToggleTile_Checked(object sender, RoutedEventArgs e)
		{
			var settings = Settings.Instance;
			settings.TileNotificationEnabled = mToggleTile.IsChecked.Value;
			settings.Save();
			App.SyncWithPushSettings();
		}

		private void mDoneWithWhatsNew_Click(object sender, RoutedEventArgs e)
		{
			ShowWhatsNew(false);
		}

		private void Search_Click(object sender, EventArgs e)
		{
			Dispatcher.BeginInvoke(() =>
			{
				NavigationService.Navigate(new Uri("/SearchPage.xaml", UriKind.Relative));
			});
		}

		private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			try
			{
				ListBox lb = (sender as ListBox);
				if (lb == null)
					return;
				Conversation convo = lb.SelectedItem as Conversation;
				lb.SelectedItem = null;
				if (convo == null)
					return;
				if (!convo.IsRead)
					mVoice.MarkAsRead(convo, true);
				NavigationService.Navigate(new Uri(string.Format("/ConversationPage.xaml?id={0}", convo.Id), UriKind.Relative));
			}
			catch (Exception)
			{
			}
		}
	}
}