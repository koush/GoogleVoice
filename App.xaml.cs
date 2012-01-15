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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Notification;
using Microsoft.Phone.Shell;

namespace GoogleVoice
{
    public partial class App : Application
    {
        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
			Microsoft.Advertising.Mobile.UI.AdControl.TestMode = false;
			
			// Global handler for uncaught exceptions. 
            UnhandledException += Application_UnhandledException;

            // Show graphics profiling information while debugging.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode, 
                // which shows areas of a page that are being GPU accelerated with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;
            }

            // Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

			CreatingANotificationChannel();

			var settings = Settings.Instance;
			mVoice = new GoogleVoiceClient(RootVisual.Dispatcher);
			mVoice.DevicePhoneNumber = settings.DevicePhoneNumber;
			mVoice.PIN = settings.PIN;
			mVoice.AuthToken = settings.AuthToken;
			mVoice.RNRSE = settings.RNRSE;
			
			var conn = SqliteDatabase.Connection;
			conn.CreateTable<Conversation>();
			conn.CreateTable<Message>();
			conn.CreateTable<PhotoEntry>();

			//SqliteDatabase.Connection.Execute("DROP TABLE Conversation");
			//SqliteDatabase.Connection.Execute("DROP TABLE Message");
			//SqliteDatabase.Connection.Execute("DELETE FROM Conversation");
			//SqliteDatabase.Connection.Execute("DELETE FROM Message");
		}


		public static void Call(string name, string number, NavigationService nav)
		{
			nav.Navigate(new Uri(string.Format("/CallPage.xaml?name={0}&phone={1}", name, number), UriKind.Relative));
			//if (Settings.Instance.ShouldUsePINDial)
			//{
			//    var t = new Microsoft.Phone.Tasks.PhoneCallTask();
			//    t.DisplayName = name;
			//    t.PhoneNumber = mVoice.GetDirectDialNumber(number);
			//    t.Show();
			//}
			//else
			//{
			//    mVoice.Call(number);
			//}
		}

		static internal HttpNotificationChannel myChannel;

		static internal GoogleVoiceClient mVoice;
		static internal void SendChannelUri()
		{
			var settings = Settings.Instance;
			if (settings.Username == null)
				return;
			WebClient client = new WebClient();
			client.Headers["Content-Type"] = "application/x-www-form-urlencoded";
			client.UploadStringCompleted += (s, e) =>
			{
				try
				{
					System.Diagnostics.Debug.WriteLine("Result: {0}", e.Result);

					var tileClearer = new WebClient();
					tileClearer.Headers["Content-Type"] = "application/x-www-form-urlencoded";
					Uri clearUri = new Uri("https://wp7voice.appspot.com/reset");
					tileClearer.UploadStringCompleted += (s2, e2) =>
					{
						System.Diagnostics.Debug.WriteLine("Reset background.");
					};
					string googleId = settings.Username;
					if (googleId == null)
						googleId = string.Empty;
					tileClearer.UploadStringAsync(clearUri, string.Format("clientId={0}&googleId={1}", Uri.EscapeDataString(settings.DeviceId), Uri.EscapeDataString(googleId)));
				}
				catch (Exception e2)
				{
					System.Diagnostics.Debug.WriteLine(e2.ToString());
				}
			};
			Uri uri = new Uri("https://tickleservice.appspot.com/register");
			System.Diagnostics.Debug.WriteLine("Registering: {0}", settings.DeviceId);
			client.UploadStringAsync(uri, string.Format("clientId={0}&applicationId=ClockworkModGoogleVoice&registrationId={1}&deviceType=wp7", Uri.EscapeDataString(settings.DeviceId), Uri.EscapeDataString(myChannel.ChannelUri.ToString())));
		}

		void DoHandlers()
		{
			myChannel.ErrorOccurred += (s, e) =>
				{
					System.Diagnostics.Debug.WriteLine(e);
				};
			myChannel.ChannelUriUpdated += (s, e) =>
			{
				var s2 = myChannel.ChannelUri;
				SendChannelUri();
				System.Diagnostics.Debug.WriteLine(s2);
			};
			myChannel.ShellToastNotificationReceived += new EventHandler<NotificationEventArgs>(ShellToastNotificationReceived);
		}

		void ShellToastNotificationReceived(object sender, NotificationEventArgs e)
		{
			mVoice.Dispatcher.BeginInvoke(() =>
				{
					mVoice.RefreshInbox();
				});
		}

		static public void SyncWithPushSettings()
		{
			if (myChannel != null)
			{
				var settings = Settings.Instance;
				if (myChannel.IsShellToastBound != settings.ToastNotificationEnabled)
				{
					if (settings.ToastNotificationEnabled)
						myChannel.BindToShellToast();
					else
						myChannel.UnbindToShellToast();
				}
				if (myChannel.IsShellTileBound != settings.TileNotificationEnabled)
				{
					if (settings.TileNotificationEnabled)
						myChannel.BindToShellTile();
					else
						myChannel.UnbindToShellTile();
				}
			}
		}

		void CreatingANotificationChannel()
		{
			myChannel = HttpNotificationChannel.Find("GoogleVoice");

			if (myChannel == null)
			{
				// Only one notification channel name is supported per application.
				myChannel = new HttpNotificationChannel("GoogleVoice");

				DoHandlers();
				// After myChannel.Open() is called, the notification channel URI will be sent to the application through the ChannelUriUpdated delegate.
				// If your application requires a timeout for setting up a notification channel, start it after the myChannel.Open() call. 
				myChannel.Open();
			}
			else
			{
				DoHandlers();
				SendChannelUri();
			}

			SyncWithPushSettings();
		}

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            // Ensure that required application state is persisted here.
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion

		private void ToggleRead_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				var convo = (sender as MenuItem).DataContext as Conversation;
				mVoice.MarkAsRead(convo, !convo.IsRead);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex);
			}
		}

		private void Trash_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				var convo = (sender as MenuItem).DataContext as Conversation;
				mVoice.Trash(convo);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex);
			}
		}

		private void Call_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				var convo = (sender as MenuItem).DataContext as Conversation;
				(RootVisual as PhoneApplicationFrame).Navigate(new Uri(string.Format("/CallPage.xaml?name={0}&phone={1}", convo.OtherParticipant, convo.PhoneNumber), UriKind.Relative));
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex);
			}
		}
	}
}