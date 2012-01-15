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
using System.Threading;
using System.Windows.Threading;

namespace GoogleVoice
{
	public partial class SearchPage : PhoneApplicationPage
	{
		Conversations mFiltered;
		GoogleVoiceClient mVoice;
		public SearchPage()
		{
			InitializeComponent();

			mVoice = App.mVoice;

			mFiltered = new Conversations();
			mFiltered.Dispatcher = Dispatcher;

			mSearchList.ItemsSource = mFiltered;

			mTimer.Tick += new EventHandler(mTimer_Tick);
			mTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
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


		void mTimer_Tick(object sender, EventArgs e)
		{
			mTimer.Stop();

			if (mIsSearching)
				return;

			string search = mSearchBox.Text;

			if (mLastSearch == search)
				return;

			mLastSearch = search;

			try
			{
				search = "%" + search + "%";
				lock (SqliteDatabase.Instance)
				{
					//var messages = SqliteDatabase.Connection.Query<Conversation>("SELECT Conversation.LastMessageTimeText, Conversation.DisplayNumber, Conversation.Id, Conversation.StartTime, Conversation.OtherParticipant, Conversation.IsRead, Conversation.PhoneNumber, Conversation.DisplayStartTime, Conversation.Type, Message.Text as LastMessageText FROM Conversation, Message WHERE Message.Text LIKE ? AND Conversation.Id=Message.ConversationId ORDER BY StartTime DESC LIMIT 5", search);
					var messages = SqliteDatabase.Connection.DeferredQuery<Conversation>("SELECT *, Conversation.Id as Id, Message.Text as LastMessageText FROM Conversation, Message WHERE Message.Text LIKE ? AND Conversation.Id=Message.ConversationId AND (Type=2 OR Type=10 OR Type=11) ORDER BY StartTime DESC LIMIT 10", search);
					var people = SqliteDatabase.Connection.DeferredQuery<Conversation>("SELECT * FROM Conversation WHERE OtherParticipant LIKE ? ORDER BY StartTime DESC LIMIT 2", search);

					var union = messages.Union(people);

					var missing = mFiltered.Except(union).ToList();
					foreach (var m in missing)
					{
						mFiltered.Remove(m.Id);
					}


					int added = 0;
					foreach (var m in messages)
					{
						if (mFiltered.Contains(m.Id))
							continue;
						if (added++ == 5)
							break;
						mFiltered.BinaryInsert(m);
					}

					foreach (var m in people)
					{
						if (mFiltered.Contains(m.Id))
							continue;
						if (added++ == 5)
							break;
						m.Dispatcher = Dispatcher;
						mFiltered.BinaryInsert(m);
					}
				}
			}
			catch (Exception)
			{
			}
			finally
			{
				mIsSearching = false;
			}
		}

		DispatcherTimer mTimer = new DispatcherTimer();
		bool mIsSearching = false;
		string mLastSearch;
		private void mSearchBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			mTimer.Stop();
			mTimer.Start();
		}
	}
}