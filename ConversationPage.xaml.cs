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
using SQLite;
using System.Threading;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Notification;
using System.Text.RegularExpressions;

namespace GoogleVoice
{
    public partial class ConversationPage : PhoneApplicationPage
    {
        public ConversationPage()
        {
            InitializeComponent();

			mPlayButton = ApplicationBar.Buttons[1] as ApplicationBarIconButton;
			mPauseButton = ApplicationBar.Buttons[2] as ApplicationBarIconButton;
			mStopButton = ApplicationBar.Buttons[3] as ApplicationBarIconButton;
			mMarkAsUnread = ApplicationBar.MenuItems[0] as ApplicationBarMenuItem;

			if (Settings.IsTrial)
				mAdControl.Visibility = System.Windows.Visibility.Visible;

            Settings settings = Settings.Instance;
			mVoice = App.mVoice;
            Loaded += new RoutedEventHandler(ConversationPage_Loaded);
			Unloaded += new RoutedEventHandler(ConversationPage_Unloaded);

			mVoicemailTimer.Interval = new TimeSpan(0, 0, 0, 0, 50);
			mVoicemailTimer.Tick += new EventHandler(mVoicemailTimer_Tick);

			mMediaPlayer.MediaFailed += new EventHandler<ExceptionRoutedEventArgs>(mMediaPlayer_MediaFailed);
			mMediaPlayer.MediaEnded += new RoutedEventHandler(mMediaPlayer_MediaEnded);
			mMediaPlayer.AutoPlay = false;
		}


		static readonly Regex mEmailAddressRegex = new Regex(@".*?(?<Email>(([^<>()[\]\\.,;:\s@\""]+"
                        + @"(\.[^<>()[\]\\.,;:\s@\""]+)*)|(\"".+\""))@"
                        + @"((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}"
                        + @"\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+"
                        + @"[a-zA-Z]{2,}))).*?");
			//new Regex("(?<Email>(?:(?:\\r\\n)?[\\t])*(?:(?:(?:[^()<>@,;:\\\\\".\\[\\]\\000-\\031]+(?:(?:(?:\\r\\n)?[\\t])+|\\Z|(?=[\\[\"()<>@,;:\\\\\".\\[\\]]))|\"(?:[^\\\"\\r\\\\]|\\\\.|(?:(?:\\r\\n)?[\\t]))*\"(?:(?:\\r\\n)?[\\t])*)(?:\\.(?:(?:\\r\\n)?[\\t])*(?:[^()<>@,;:\\\\\".\\[\\]\\000-\\031]+(?:(?:(?:\\r\\n)?[\\t])+|\\Z|(?=[\\[\"()<>@,;:\\\\\".\\[\\]]))|\"(?:[^\\\"\\r\\\\]|\\\\.|(?:(?:\\r\\n)?[\\t]))*\"(?:(?:\\r\\n)?[\\t])*))*@(?:(?:\\r\\n)?[\\t])*(?:[^()<>@,;:\\\\\".\\[\\]\\000-\\031]+(?:(?:(?:\\r\\n)?[\\t])+|\\Z|(?=[\\[\"()<>@,;:\\\\\".\\[\\]]))|\\[([^\\[\\]\\r\\\\]|\\\\.)*\\](?:(?:\\r\\n)?[\\t])*)(?:\\.(?:(?:\\r\\n)?[\\t])*(?:[^()<>@,;:\\\\\".\\[\\]\\000-\\031]+(?:(?:(?:\\r\\n)?[\\t])+|\\Z|(?=[\\[\"()<>@,;:\\\\\".\\[\\]]))|\\[([^\\[\\]\\r\\\\]|\\\\.)*\\](?:(?:\\r\\n)?[\\t])*))*|(?:[^()<>@,;:\\\\\".\\[\\]\\000-\\031]+(?:(?:(?:\\r\\n)?[\\t])+|\\Z|(?=[\\[\"()<>@,;:\\\\\".\\[\\]]))|\"(?:[^\\\"\\r\\\\]|\\\\.|(?:(?:\\r\\n)?[\\t]))*\"(?:(?:\\r\\n)?[\\t])*)*\\<(?:(?:\\r\\n)?[\\t])*(?:@(?:[^()<>@,;:\\\\\".\\[\\]\\000-\\031]+(?:(?:(?:\\r\\n)?[\\t])+|\\Z|(?=[\\[\"()<>@,;:\\\\\".\\[\\]]))|\\[([^\\[\\]\\r\\\\]|\\\\.)*\\](?:(?:\\r\\n)?[\\t])*)(?:\\.(?:(?:\\r\\n)?[\\t])*(?:[^()<>@,;:\\\\\".\\[\\]\\000-\\031]+(?:(?:(?:\\r\\n)?[\\t])+|\\Z|(?=[\\[\"()<>@,;:\\\\\".\\[\\]]))|\\[([^\\[\\]\\r\\\\]|\\\\.)*\\](?:(?:\\r\\n)?[\\t])*))*(?:,@(?:(?:\\r\\n)?[\\t])*(?:[^()<>@,;:\\\\\".\\[\\]\\000-\\031]+(?:(?:(?:\\r\\n)?[\\t])+|\\Z|(?=[\\[\"()<>@,;:\\\\\".\\[\\]]))|\\[([^\\[\\]\\r\\\\]|\\\\.)*\\](?:(?:\\r\\n)?[\\t])*)(?:\\.(?:(?:\\r\\n)?[\\t])*(?:[^()<>@,;:\\\\\".\\[\\]\\000-\\031]+(?:(?:(?:\\r\\n)?[\\t])+|\\Z|(?=[\\[\"()<>@,;:\\\\\".\\[\\]]))|\\[([^\\[\\]\\r\\\\]|\\\\.)*\\](?:(?:\\r\\n)?[\\t])*))*)*:(?:(?:\\r\\n)?[\\t])*)?(?:[^()<>@,;:\\\\\".\\[\\]\\000-\\031]+(?:(?:(?:\\r\\n)?[\\t])+|\\Z|(?=[\\[\"()<>@,;:\\\\\".\\[\\]]))|\"(?:[^\\\"\\r\\\\]|\\\\.|(?:(?:\\r\\n)?[\\t]))*\"(?:(?:\\r\\n)?[\\t])*)(?:\\.(?:(?:\\r\\n)?[\\t])*(?:[^()<>@,;:\\\\\".\\[\\]\\000-\\031]+(?:(?:(?:\\r\\n)?[\\t])+|\\Z|(?=[\\[\"()<>@,;:\\\\\".\\[\\]]))|\"(?:[^\\\"\\r\\\\]|\\\\.|(?:(?:\\r\\n)?[\\t]))*\"(?:(?:\\r\\n)?[\\t])*))*@(?:(?:\\r\\n)?[\\t])*(?:[^()<>@,;:\\\\\".\\[\\]\\000-\\031]+(?:(?:(?:\\r\\n)?[\\t])+|\\Z|(?=[\\[\"()<>@,;:\\\\\".\\[\\]]))|\\[([^\\[\\]\\r\\\\]|\\\\.)*\\](?:(?:\\r\\n)?[\\t])*)(?:\\.(?:(?:\\r\\n)?[\\t])*(?:[^()<>@,;:\\\\\".\\[\\]\\000-\\031]+(?:(?:(?:\\r\\n)?[\\t])+|\\Z|(?=[\\[\"()<>@,;:\\\\\".\\[\\]]))|\\[([^\\[\\]\\r\\\\]|\\\\.)*\\](?:(?:\\r\\n)?[\\t])*))*\\>(?:(?:\\r\\n)?[\\t])*)|(?:[^()<>@,;:\\\\\".\\[\\]\\000-\\031]+(?:(?:(?:\\r\\n)?[\\t])+|\\Z|(?=[\\[\"()<>@,;:\\\\\".\\[\\]]))|\"(?:[^\\\"\\r\\\\]|\\\\.|(?:(?:\\r\\n)?[\\t]))*\"(?:(?:\\r\\n)?[\\t])*)*:(?:(?:\\r\\n)?[\\t])*(?:(?:(?:[^()<>@,;:\\\\\".\\[\\]\\000-\\031]+(?:(?:(?:\\r\\n)?[\\t])+|\\Z|(?=[\\[\"()<>@,;:\\\\\".\\[\\]]))|\"(?:[^\\\"\\r\\\\]|\\\\.|(?:(?:\\r\\n)?[\\t]))*\"(?:(?:\\r\\n)?[\\t])*)(?:\\.(?:(?:\\r\\n)?[\\t])*(?:[^()<>@,;:\\\\\".\\[\\]\\000-\\031]+(?:(?:(?:\\r\\n)?[\\t])+|\\Z|(?=[\\[\"()<>@,;:\\\\\".\\[\\]]))|\"(?:[^\\\"\\r\\\\]|\\\\.|(?:(?:\\r\\n)?[\\t]))*\"(?:(?:\\r\\n)?[\\t])*))*@(?:(?:\\r\\n)?[\\t])*(?:[^()<>@,;:\\\\\".\\[\\]\\000-\\031]+(?:(?:(?:\\r\\n)?[\\t])+|\\Z|(?=[\\[\"()<>@,;:\\\\\".\\[\\]]))|\\[([^\\[\\]\\r\\\\]|\\\\.)*\\](?:(?:\\r\\n)?[\\t])*)(?:\\.(?:(?:\\r\\n)?[\\t])*(?:[^()<>@,;:\\\\\".\\[\\]\\000-\\031]+(?:(?:(?:\\r\\n)?[\\t])+|\\Z|(?=[\\[\"()<>@,;:\\\\\".\\[\\]]))|\\[([^\\[\\]\\r\\\\]|\\\\.)*\\](?:(?:\\r\\n)?[\\t])*))*|(?:[^()<>@,;:\\\\\".\\[\\]\\000-\\031]+(?:(?:(?:\\r\\n)?[\\t])+|\\Z|(?=[\\[\"()<>@,;:\\\\\".\\[\\]]))|\"(?:[^\\\"\\r\\\\]|\\\\.|(?:(?:\\r\\n)?[\\t]))*\"(?:(?:\\r\\n)?[\\t])*)*\\<(?:(?:\\r\\n)?[\\t])*(?:@(?:[^()<>@,;:\\\\\".\\[\\]\\000-\\031]+(?:(?:(?:\\r\\n)?[\\t])+|\\Z|(?=[\\[\"()<>@,;:\\\\\".\\[\\]]))|\\[([^\\[\\]\\r\\\\]|\\\\.)*\\](?:(?:\\r\\n)?[\\t])*)(?:\\.(?:(?:\\r\\n)?[\\t])*(?:[^()<>@,;:\\\\\".\\[\\]\\000-\\031]+(?:(?:(?:\\r\\n)?[\\t])+|\\Z|(?=[\\[\"()<>@,;:\\\\\".\\[\\]]))|\\[([^\\[\\]\\r\\\\]|\\\\.)*\\](?:(?:\\r\\n)?[\\t])*))*(?:,@(?:(?:\\r\\n)?[\\t])*(?:[^()<>@,;:\\\\\".\\[\\]\\000-\\031]+(?:(?:(?:\\r\\n)?[\\t])+|\\Z|(?=[\\[\"()<>@,;:\\\\\".\\[\\]]))|\\[([^\\[\\]\\r\\\\]|\\\\.)*\\](?:(?:\\r\\n)?[\\t])*)(?:\\.(?:(?:\\r\\n)?[\\t])*(?:[^()<>@,;:\\\\\".\\[\\]\\000-\\031]+(?:(?:(?:\\r\\n)?[\\t])+|\\Z|(?=[\\[\"()<>@,;:\\\\\".\\[\\]]))|\\[([^\\[\\]\\r\\\\]|\\\\.)*\\](?:(?:\\r\\n)?[\\t])*))*)*:(?:(?:\\r\\n)?[\\t])*)?(?:[^()<>@,;:\\\\\".\\[\\]\\000-\\031]+(?:(?:(?:\\r\\n)?[\\t])+|\\Z|(?=[\\[\"()<>@,;:\\\\\".\\[\\]]))|\"(?:[^\\\"\\r\\\\]|\\\\.|(?:(?:\\r\\n)?[\\t]))*\"(?:(?:\\r\\n)?[\\t])*)(?:\\.(?:(?:\\r\\n)?[\\t])*(?:[^()<>@,;:\\\\\".\\[\\]\\000-\\031]+(?:(?:(?:\\r\\n)?[\\t])+|\\Z|(?=[\\[\"()<>@,;:\\\\\".\\[\\]]))|\"(?:[^\\\"\\r\\\\]|\\\\.|(?:(?:\\r\\n)?[\\t]))*\"(?:(?:\\r\\n)?[\\t])*))*@(?:(?:\\r\\n)?[\\t])*(?:[^()<>@,;:\\\\\".\\[\\]\\000-\\031]+(?:(?:(?:\\r\\n)?[\\t])+|\\Z|(?=[\\[\"()<>@,;:\\\\\".\\[\\]]))|\\[([^\\[\\]\\r\\\\]|\\\\.)*\\](?:(?:\\r\\n)?[\\t])*)(?:\\.(?:(?:\\r\\n)?[\\t])*(?:[^()<>@,;:\\\\\".\\[\\]\\000-\\031]+(?:(?:(?:\\r\\n)?[\\t])+|\\Z|(?=[\\[\"()<>@,;:\\\\\".\\[\\]]))|\\[([^\\[\\]\\r\\\\]|\\\\.)*\\](?:(?:\\r\\n)?[\\t])*))*\\>(?:(?:\\r\\n)?[\\t])*)(?:,\\s*(?:(?:[^()<>@,;:\\\\\".\\[\\]\\000-\\031]+(?:(?:(?:\\r\\n)?[\\t])+|\\Z|(?=[\\[\"()<>@,;:\\\\\".\\[\\]]))|\"(?:[^\\\"\\r\\\\]|\\\\.|(?:(?:\\r\\n)?[\\t]))*\"(?:(?:\\r\\n)?[\\t])*)(?:\\.(?:(?:\\r\\n)?[\\t])*(?:[^()<>@,;:\\\\\".\\[\\]\\000-\\031]+(?:(?:(?:\\r\\n)?[\\t])+|\\Z|(?=[\\[\"()<>@,;:\\\\\".\\[\\]]))|\"(?:[^\\\"\\r\\\\]|\\\\.|(?:(?:\\r\\n)?[\\t]))*\"(?:(?:\\r\\n)?[\\t])*))*@(?:(?:\\r\\n)?[\\t])*(?:[^()<>@,;:\\\\\".\\[\\]\\000-\\031]+(?:(?:(?:\\r\\n)?[\\t])+|\\Z|(?=[\\[\"()<>@,;:\\\\\".\\[\\]]))|\\[([^\\[\\]\\r\\\\]|\\\\.)*\\](?:(?:\\r\\n)?[\\t])*)(?:\\.(?:(?:\\r\\n)?[\\t])*(?:[^()<>@,;:\\\\\".\\[\\]\\000-\\031]+(?:(?:(?:\\r\\n)?[\\t])+|\\Z|(?=[\\[\"()<>@,;:\\\\\".\\[\\]]))|\\[([^\\[\\]\\r\\\\]|\\\\.)*\\](?:(?:\\r\\n)?[\\t])*))*|(?:[^()<>@,;:\\\\\".\\[\\]\\000-\\031]+(?:(?:(?:\\r\\n)?[\\t])+|\\Z|(?=[\\[\"()<>@,;:\\\\\".\\[\\]]))|\"(?:[^\\\"\\r\\\\]|\\\\.|(?:(?:\\r\\n)?[\\t]))*\"(?:(?:\\r\\n)?[\\t])*)*\\<(?:(?:\\r\\n)?[\\t])*(?:@(?:[^()<>@,;:\\\\\".\\[\\]\\000-\\031]+(?:(?:(?:\\r\\n)?[\\t])+|\\Z|(?=[\\[\"()<>@,;:\\\\\".\\[\\]]))|\\[([^\\[\\]\\r\\\\]|\\\\.)*\\](?:(?:\\r\\n)?[\\t])*)(?:\\.(?:(?:\\r\\n)?[\\t])*(?:[^()<>@,;:\\\\\".\\[\\]\\000-\\031]+(?:(?:(?:\\r\\n)?[\\t])+|\\Z|(?=[\\[\"()<>@,;:\\\\\".\\[\\]]))|\\[([^\\[\\]\\r\\\\]|\\\\.)*\\](?:(?:\\r\\n)?[\\t])*))*(?:,@(?:(?:\\r\\n)?[\\t])*(?:[^()<>@,;:\\\\\".\\[\\]\\000-\\031]+(?:(?:(?:\\r\\n)?[\\t])+|\\Z|(?=[\\[\"()<>@,;:\\\\\".\\[\\]]))|\\[([^\\[\\]\\r\\\\]|\\\\.)*\\](?:(?:\\r\\n)?[\\t])*)(?:\\.(?:(?:\\r\\n)?[\\t])*(?:[^()<>@,;:\\\\\".\\[\\]\\000-\\031]+(?:(?:(?:\\r\\n)?[\\t])+|\\Z|(?=[\\[\"()<>@,;:\\\\\".\\[\\]]))|\\[([^\\[\\]\\r\\\\]|\\\\.)*\\](?:(?:\\r\\n)?[\\t])*))*)*:(?:(?:\\r\\n)?[\\t])*)?(?:[^()<>@,;:\\\\\".\\[\\]\\000-\\031]+(?:(?:(?:\\r\\n)?[\\t])+|\\Z|(?=[\\[\"()<>@,;:\\\\\".\\[\\]]))|\"(?:[^\\\"\\r\\\\]|\\\\.|(?:(?:\\r\\n)?[\\t]))*\"(?:(?:\\r\\n)?[\\t])*)(?:\\.(?:(?:\\r\\n)?[\\t])*(?:[^()<>@,;:\\\\\".\\[\\]\\000-\\031]+(?:(?:(?:\\r\\n)?[\\t])+|\\Z|(?=[\\[\"()<>@,;:\\\\\".\\[\\]]))|\"(?:[^\\\"\\r\\\\]|\\\\.|(?:(?:\\r\\n)?[\\t]))*\"(?:(?:\\r\\n)?[\\t])*))*@(?:(?:\\r\\n)?[\\t])*(?:[^()<>@,;:\\\\\".\\[\\]\\000-\\031]+(?:(?:(?:\\r\\n)?[\\t])+|\\Z|(?=[\\[\"()<>@,;:\\\\\".\\[\\]]))|\\[([^\\[\\]\\r\\\\]|\\\\.)*\\](?:(?:\\r\\n)?[\\t])*)(?:\\.(?:(?:\\r\\n)?[\\t])*(?:[^()<>@,;:\\\\\".\\[\\]\\000-\\031]+(?:(?:(?:\\r\\n)?[\\t])+|\\Z|(?=[\\[\"()<>@,;:\\\\\".\\[\\]]))|\\[([^\\[\\]\\r\\\\]|\\\\.)*\\](?:(?:\\r\\n)?[\\t])*))*\\>(?:(?:\\r\\n)?[\\t])*))*)?;\\s*))");
		static readonly Regex mPhoneNumberRegex = new Regex(".*?(?<Number>(?:(?:\\+?1\\s*(?:[.-]\\s*)?)?(?:\\(\\s*([2-9]1[02-9]|[2-9][02-8]1|[2-9][02-8][02-9])\\s*\\)|([2-9]1[02-9]|[2-9][02-8]1|[2-9][02-8][02-9]))\\s*(?:[.-]\\s*)?)?([2-9]1[02-9]|[2-9][02-9]1|[2-9][02-9]{2})\\s*(?:[.-]\\s*)?([0-9]{4})(?:\\s*(?:#|x\\.?|ext\\.?|extension)\\s*(\\d+))?).*?");

		void mVoicemailTimer_Tick(object sender, EventArgs e)
		{
			if (mNextWord > mMediaPlayer.Position.TotalMilliseconds)
				return;

			mWordIndex++;
			mConversation.Messages[1].UnderlinedWord = mWordIndex - 1;
			if (mWordIndex >= mWords.starttime.Length)
				return;
			mNextWord = int.Parse(mWords.starttime[mWordIndex].Value);
		}

		void ConversationPage_Unloaded(object sender, RoutedEventArgs e)
		{
			mVoice.MessageAdded -= mMessageHandler;
			//if (App.myChannel != null)
			//	App.myChannel.ShellToastNotificationReceived -= mToastHandler;
		}

		internal static Message dummy = new Message() {
			Text = "\n\n\n"
		};
		void PrepareConversation()
		{
			mConversation.Dispatcher = Dispatcher;
			mConversation.Messages.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Messages_CollectionChanged);
			mConversation.Messages.Insert(0, dummy);
		}

        GoogleVoiceClient mVoice;
		words mWords = null;
        Conversation mConversation = null;
		bool mFoundOtherParticipant = false;
		Action<Conversation, Message> mMessageHandler;
		void ConversationPage_Loaded(object sender, RoutedEventArgs e)
		{
			mMessageHandler = new Action<Conversation, Message>(Voice_MessageAdded);
			mVoice.MessageAdded += mMessageHandler;

			string id = null;
			NavigationContext.QueryString.TryGetValue("id", out id);
			string phone = null;
			NavigationContext.QueryString.TryGetValue("phone", out phone);
			if (phone == null)
			{
				if (!mVoice.Conversations.Dictionary.TryGetValue(id, out mConversation))
				{
					var conn = SqliteDatabase.Connection;
					lock (SqliteDatabase.Instance)
					{
						mConversation = conn.Query<Conversation>("SELECT * FROM Conversation WHERE Id=?", id).FirstOrDefault();
					}
				}
				if (mConversation == null)
					throw new InvalidOperationException("Conversation not found?");
				mMarkAsUnread.IsEnabled = mConversation.IsRead;
				mFoundOtherParticipant = true;
				phone = mConversation.PhoneNumber;
				PrepareConversation();
				DataContext = mConversation;
			}
			else
			{
				mConversation = new Conversation();
				mConversation.DisplayNumber = mConversation.PhoneNumber = phone;
				// try to determine the person name from the number
				FindOtherParticipant();
			}

			if (mConversation.Type != 2)
			{
				ApplicationBar.Buttons.Remove(mPlayButton);
				ApplicationBar.Buttons.Remove(mStopButton);
				ApplicationBar.Buttons.Remove(mPauseButton);
			}
			else
			{
				mVoicemailFile = mConversation.Id + ".mp3";
				mTimingFile = mConversation.Id + ".xml";

				if (!System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication().FileExists(mTimingFile))
				{
					WebClient timingClient = mVoice.GetAuthorizedClient();
					timingClient.OpenReadCompleted += (s, r) =>
					{
						try
						{
							var stream = r.Result;
							byte[] buf = new byte[10000];
							using (var outputStream = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication().OpenFile(mTimingFile, System.IO.FileMode.Create))
							{
								int read = 0;
								do
								{
									read = stream.Read(buf, 0, 10000);
									outputStream.Write(buf, 0, read);
								}
								while (read > 0);
							}

							Dispatcher.BeginInvoke(() =>
							{
								LoadTimingFile();
							});
						}
						catch (Exception)
						{
						}
					};
					timingClient.OpenReadAsync(new Uri("https://www.google.com/voice/media/transcriptWords?id=" + mConversation.Id));
				}
				else
				{
					LoadTimingFile();
				}

				if (!System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication().FileExists(mVoicemailFile))
				{
					mPlayButton.IsEnabled = false;
					mPauseButton.IsEnabled = false;
					mStopButton.IsEnabled = false;
					mProgressBar.Visibility = System.Windows.Visibility.Visible;

					WebClient wc = mVoice.GetAuthorizedClient();
					wc.OpenReadCompleted += (s, r) =>
						{
							ThreadPool.QueueUserWorkItem(o =>
								{
									try
									{
										var stream = r.Result;
										byte[] buf = new byte[10000];
										using (var outputStream = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication().OpenFile(mVoicemailFile, System.IO.FileMode.Create))
										{
											int read = 0;
											do
											{
												read = stream.Read(buf, 0, 10000);
												outputStream.Write(buf, 0, read);
											}
											while (read > 0);
										}

										Dispatcher.BeginInvoke(() =>
											{
												mPlayButton.IsEnabled = true;
												mPauseButton.IsEnabled = true;
												mStopButton.IsEnabled = true;
											});
									}
									catch (Exception ex)
									{
										System.Diagnostics.Debug.WriteLine(ex);
									}
									finally
									{
										Dispatcher.BeginInvoke(() =>
											{
												mProgressBar.Visibility = System.Windows.Visibility.Collapsed;
											});
									}
								});
						};
					wc.OpenReadAsync(new Uri(mVoice.GetVoicemailUrl(mConversation)));
				}
				else
				{
					mPlayButton.IsEnabled = true;
					mPauseButton.IsEnabled = true;
					mStopButton.IsEnabled = true;
				}
			}
		}

		string mVoicemailFile;
		string mTimingFile;

		void LoadTimingFile()
		{
			try
			{
				var inputStream = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication().OpenFile(mTimingFile, System.IO.FileMode.Open);
				var ser = new System.Xml.Serialization.XmlSerializer(typeof(words));
				mWords = ser.Deserialize(inputStream) as words;
			}
			catch (Exception)
			{
				System.Diagnostics.Debug.WriteLine(mWords);
			}
		}

		bool mLoadedVoicemailFile = false;
		void LoadVoicemailFile()
		{
			var inputStream = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication().OpenFile(mVoicemailFile, System.IO.FileMode.Open);
			mMediaPlayer.SetSource(inputStream);
			mLoadedVoicemailFile = true;
		}

		void mMediaPlayer_MediaEnded(object sender, RoutedEventArgs e)
		{
			try
			{
				mVoicemailTimer.Stop();
				mNextWord = 0;
				mWordIndex = 0;
				mMediaPlayer.Stop();
				mConversation.Messages[1].UnderlinedWord = -1;
			}
			catch (Exception)
			{
			}
		}

		void mMediaPlayer_MediaFailed(object sender, ExceptionRoutedEventArgs e)
		{
		}

		void Voice_MessageAdded(Conversation c, Message m)
		{
			if (!m.Incoming)
				return;
			if (!GoogleVoiceClient.NumbersMatch(c.PhoneNumber, mConversation.PhoneNumber))
				return;
			if (c.OtherParticipant != mConversation.OtherParticipant)
				mConversation.OtherParticipant = c.OtherParticipant;
			mConversation.Messages.Add(m);
		}

		void FindOtherParticipant()
		{
			if (mFoundOtherParticipant)
				return;
			var conn = SqliteDatabase.Connection;
			lock (SqliteDatabase.Instance)
			{
				PrepareConversation();
				var findName = conn.Query<Conversation>(string.Format("SELECT OtherParticipant, Id FROM Conversation WHERE PhoneNumber LIKE '%{0}%' LIMIT 1", GoogleVoiceClient.NumbersOnly(mConversation.PhoneNumber))).FirstOrDefault();
				if (findName != null)
				{
					mFoundOtherParticipant = true;
					mConversation.OtherParticipant = findName.OtherParticipant;
				}
				DataContext = mConversation;
			}
		}

		Message AddMessage(bool incoming, string message)
		{
			Message m = new Message()
			{
				Time = DateTime.Now.ToShortTimeString(),
				Text = message,
				Incoming = incoming,
				ConversationId = mConversation.Id
			};
			mConversation.Messages.Add(m);
			return m;
		}

		void Messages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			if (mConversation == null)
				return;
			// need to begin invoke this to force a relayout
			Dispatcher.BeginInvoke(() =>
				{
					var last = mConversation.Messages.LastOrDefault();
					mListBox.ScrollIntoView(last);
					mListBox.UpdateLayout();
				});
		}

		private void Call_Click(object sender, RoutedEventArgs e)
		{
			App.Call(mConversation.OtherParticipant, mConversation.PhoneNumber, NavigationService);
		}

		private void Send_Click(object sender, EventArgs e)
		{
			var message = mMessageText.Text;
			mMessageText.Text = string.Empty;
			if (string.IsNullOrEmpty(message))
				return;

			mVoice.Send(mConversation.PhoneNumber, message, () =>
			{
				AddMessage(false, message);
			},
			() =>
			{
			});
		}

		System.Windows.Threading.DispatcherTimer mVoicemailTimer = new System.Windows.Threading.DispatcherTimer();
		int mNextWord = 0;
		int mWordIndex = 0;
		private void mPlayButton_Click(object sender, EventArgs e)
		{
			if (!mLoadedVoicemailFile)
				LoadVoicemailFile();
			try
			{
				mVoicemailTimer.Start();
				mMediaPlayer.Play();
				mConversation.Messages[1].UnderlinedWord = 0;
			}
			catch (Exception)
			{
			}
		}

		private void mPauseButton_Click(object sender, EventArgs e)
		{
			if (!mLoadedVoicemailFile)
				return;
			try
			{
				mVoicemailTimer.Stop();
				mMediaPlayer.Pause();
			}
			catch (Exception)
			{
			}
		}

		private void mStopButton_Click(object sender, EventArgs e)
		{
			if (!mLoadedVoicemailFile)
				return;
			try
			{
				mNextWord = 0;
				mWordIndex = 0;
				mMediaPlayer.Stop();
			}
			catch (Exception)
			{
			}
		}

		private void DeleteConversation_Click(object sender, EventArgs e)
		{
			if (mConversation != null)
				mVoice.Trash(mConversation);
			NavigationService.GoBack();
		}

		private void MarkConversationUnread_Click(object sender, EventArgs e)
		{
			if (mConversation == null)
				return;
			if (mConversation.IsRead)
			{
				mVoice.MarkAsRead(mConversation, false);
				mMarkAsUnread.IsEnabled = false;
			}
		}

		private void ContextMenu_Opened(object sender, RoutedEventArgs e)
		{
			var menu = sender as Microsoft.Phone.Controls.ContextMenu;
			Message message = menu.DataContext as Message;
			if (message == null)
				return;

			menu.Items.Clear();

			Match m = mPhoneNumberRegex.Match(message.Text);
			while (m.Success)
			{
				MenuItem item = new MenuItem();
				item.Click += (s, args) =>
					{
					};
				item.Header = "call: " + m.Groups["Number"].Value;
				menu.Items.Add(item);

				item = new MenuItem();
				item.Click += (s, args) =>
				{
				};
				item.Header = "sms: " + m.Groups["Number"].Value;
				menu.Items.Add(item);
				
				m = m.NextMatch();
			}

			m = mEmailAddressRegex.Match(message.Text);
			while (m.Success)
			{
				MenuItem item = new MenuItem();
				item.Click += (s, args) =>
				{
				};
				item.Header = "email: " + m.Groups["Email"].Value;
				menu.Items.Add(item);
				m = m.NextMatch();
			}
		}

		private void ContextMenu_Loaded(object sender, RoutedEventArgs e)
		{

		}
	}
}