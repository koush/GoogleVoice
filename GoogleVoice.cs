using System;
using System.Net;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using SQLite;
using System.Threading;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Windows.Threading;
using System.Windows;
using Newtonsoft.Json.Linq;

namespace GoogleVoice
{
	public class Message : INotifyPropertyChanged
	{
		[PrimaryKey, AutoIncrement]
		public int Id { get; set; }

		[Indexed]
		public string ConversationId { get; set; }

		public bool Incoming
		{
			get;
			set;
		}

		public string Text
		{
			get;
			set;
		}

		public string Time
		{
			get;
			set;
		}

		int _underlinedWord = -1;
		[Ignore]
		public int UnderlinedWord
		{
			get
			{
				return _underlinedWord;
			}
			set
			{
				_underlinedWord = value;
				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs("UnderlinedWord"));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}

	public class Conversation : INotifyPropertyChanged, IComparable<Conversation>
	{
		internal void Trigger(string property)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(property));
		}

		internal void Notify()
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs("LastMessageText"));
		}

		[Ignore]
		public Dispatcher Dispatcher
		{
			get;
			set;
		}

		internal ObservableCollection<Message> mMessages;
		[Ignore]
		public ObservableCollection<Message> Messages
		{
			get
			{
				if (mMessages == null)
				{
					mMessages = new ObservableCollection<Message>();
					if (!DesignerProperties.IsInDesignTool)
					{
						var conn = SqliteDatabase.Connection;
						lock (SqliteDatabase.Instance)
						{
							foreach (var message in (conn.Query<Message>("SELECT * FROM Message WHERE ConversationId=? ORDER BY Id DESC LIMIT 10", Id)).Reverse<Message>())
							{
								var m = message;
								mMessages.Add(m);
							}
						}
					}
				}
				return mMessages;
			}
		}

		[PrimaryKey]
		[JsonProperty("id")]
		public string Id
		{
			get;
			set;
		}

		string mOtherParticipant;
		public string OtherParticipant
		{
			get
			{
				return mOtherParticipant;
			}
			set
			{
				mOtherParticipant = value;
				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs("OtherParticipant"));
			}
		}

		[JsonProperty("phoneNumber")]
		public string PhoneNumber
		{
			get;
			set;
		}

		[JsonProperty("displayNumber")]
		public string DisplayNumber
		{
			get;
			set;
		}

		[Indexed]
		[JsonProperty("startTime")]
		public long StartTime
		{
			get;
			set;
		}

		[JsonProperty("displayStartTime")]
		public string DisplayStartTime
		{
			get;
			set;
		}


		[JsonProperty("displayStartDateTime")]
		public string DisplayStartDateTime
		{
			get;
			set;
		}


		[JsonProperty("note")]
		public string Note
		{
			get;
			set;
		}


		bool mIsRead;
		[JsonProperty("isRead")]
		public bool IsRead
		{
			get { return mIsRead; }
			set
			{
				if (mIsRead == value)
					return;
				mIsRead = value;
				Trigger("IsRead");
			}
		}


		[JsonProperty("isSpam")]
		public bool IsSpam
		{
			get;
			set;
		}

		[JsonProperty("isTrash")]
		public bool IsTrash
		{
			get;
			set;
		}

		[JsonProperty("star")]
		public bool Star
		{
			get;
			set;
		}

		List<string> mLabels = new List<string>();
		[JsonProperty("labels")]
		public List<string> Labels
		{
			get
			{
				return mLabels;
			}
		}

		public string LastMessageText
		{
			get;
			set;
		}

		public string LastMessageTimeText
		{
			get;
			set;
		}

		[Indexed]
		[JsonProperty("type")]
		public int Type
		{
			get;
			set;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public int CompareTo(Conversation other)
		{
			if (other.Id == Id)
				return 0;
			return other.StartTime.CompareTo(StartTime);
		}
	}

	public class Payload
	{
		Dictionary<string, Conversation> mConversations = new Dictionary<string, Conversation>();
		[JsonProperty("messages")]
		public Dictionary<string, Conversation> Conversations
		{
			get
			{
				return mConversations;
			}
		}

		public Payload()
		{
		}
	}

	public static class Extensions
	{
		public static void PrintTimestamp()
		{
			System.Diagnostics.Debug.WriteLine(Environment.TickCount);
		}

		public static bool GetIsVisible(this UIElement e)
		{
			return e.Opacity == 1;
		}

		public static void SetIsVisible(this UIElement e, bool value)
		{
			e.Opacity = value ? 1 : 0;
		}

		public static void BinaryInsert<T>(this Collection<T> coll, T value) where T : IComparable<T>
		{
			int left = 0;
			int right = coll.Count;
			while (left < right)
			{
				int len = right - left;
				int mid = left + (len / 2);

				T midItem = coll[mid];
				int ret = value.CompareTo(midItem);
				if (ret == 0)
				{
					left = mid;
					break;
				}

				if (ret < 0)
					right = mid;
				else
					left = mid + (len % 2);
			}
			coll.Insert(left, value);
		}
	}

	public class Conversations : KeyedCollection<string, Conversation>, INotifyCollectionChanged, INotifyPropertyChanged
	{
		internal bool SuspendPropertyChange
		{
			get;
			set;
		}

		internal IDictionary<string, Conversation> Dictionary
		{
			get
			{
				return base.Dictionary;
			}
		}

		internal void Reset()
		{
			if (CollectionChanged != null && !SuspendPropertyChange)
				CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		public string UnreadCount
		{
			get
			{
				return "(10)";
			}
		}

		protected override string GetKeyForItem(Conversation item)
		{
			return item.Id;
		}

		protected override void InsertItem(int index, Conversation item)
		{
			base.InsertItem(index, item);

			if (!DesignerProperties.IsInDesignTool && !SuspendPropertyChange)
			{
				if (CollectionChanged != null && !SuspendPropertyChange)
					CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
			}
		}

		protected override void SetItem(int index, Conversation item)
		{
			var oldItem = this[index];
			base.SetItem(index, item);
			if (!DesignerProperties.IsInDesignTool && !SuspendPropertyChange)
			{
				if (CollectionChanged != null && !SuspendPropertyChange)
					CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, oldItem, index));
			}
		}

		protected override void RemoveItem(int index)
		{
			var oldItem = this[index];
			base.RemoveItem(index);
			if (!DesignerProperties.IsInDesignTool && !SuspendPropertyChange)
			{
				if (CollectionChanged != null && !SuspendPropertyChange)
					CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItem, index));
			}
		}

		protected override void ClearItems()
		{
			base.ClearItems();
			if (!DesignerProperties.IsInDesignTool && !SuspendPropertyChange)
			{
				Reset();
			}
		}

		internal GoogleVoiceClient Session
		{
			get;
			set;
		}

		internal Dispatcher Dispatcher
		{
			get;
			set;
		}

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		public event PropertyChangedEventHandler PropertyChanged;
	}

	public class PhotoEntry
	{
		[PrimaryKey, AutoIncrement]
		public int Id { get; set; }

		public string Number
		{
			get;
			set;
		}

		public string Url
		{
			get;
			set;
		}
	}


	public class GoogleVoiceClient : INotifyPropertyChanged
	{
		public Dispatcher Dispatcher
		{
			get;
			private set;
		}

		public GoogleVoiceClient()
		{
			mConversations = new Conversations();
		}

		public GoogleVoiceClient(Dispatcher dispatcher)
		{
			Dispatcher = dispatcher;
		}

		public void GetConversationsWithFilter(string filter, ref Conversations conversations)
		{
			if (conversations != null)
				return;
			if (filter == null)
				filter = string.Empty;
			conversations = new Conversations();
			conversations.Session = this;
			conversations.Dispatcher = Dispatcher;
			if (!DesignerProperties.IsInDesignTool)
			{
				lock (SqliteDatabase.Instance)
				{
					var conn = SqliteDatabase.Connection;
					var convos = conn.DeferredQuery<Conversation>(string.Format("SELECT * FROM Conversation {0} ORDER BY StartTime DESC", filter));
					foreach (var convo in convos)
					{
						convo.Dispatcher = Dispatcher;
						if (mConversations != conversations && conversations != mCalls)
						{
							Conversation existing;
							if (mConversations.Dictionary.TryGetValue(convo.Id, out existing))
							{
								conversations.BinaryInsert(existing);
							}
							else
							{
								conversations.BinaryInsert(convo);
								mConversations.BinaryInsert(convo);
							}
							continue;
						}
						conversations.BinaryInsert(convo);
					}
				}
			}
		}

		Conversations mConversations = null;
		public Conversations Conversations
		{
			get
			{
				GetConversationsWithFilter("WHERE Type!=0 AND Type!=1 AND Type!=7 AND Type!=8", ref mConversations);
				return mConversations;
			}
		}

		Conversations mTexts = null;
		public Conversations Texts
		{
			get
			{
				GetConversationsWithFilter("WHERE Type=10 OR Type=11", ref mTexts);
				return mTexts;
			}
		}

		Conversations mCalls = null;
		public Conversations Calls
		{
			get
			{
				GetConversationsWithFilter("WHERE Type=0 OR Type=1 OR Type=7 OR Type=8", ref mCalls);
				return mCalls;
			}
		}

		Conversations mVoicemail = null;
		public Conversations Voicemail
		{
			get
			{
				GetConversationsWithFilter("WHERE Type=2", ref mVoicemail);
				return mVoicemail;
			}
		}

		static readonly Regex contact = new Regex("title=\"Go to contact\" href=\"javascript://\">(.*?)<");
		static readonly Regex message = new Regex("<span class=\"gc-message-sms-from\">([\\s\\S]*?)</span>\\W*?<span class=\"gc-message-sms-text\">([\\w\\W]*?)</span>\\W*?<span class=\"gc-message-sms-time\">([\\s\\S]*?)</span>");
		static readonly Regex voicemail = new Regex("class=\"(gc-word-.*?|gc-no-trans|gc-message-mni)\">(.*?)</(span|a)>");
		static readonly Regex voicemailTime = new Regex("gc-message-time\">(.*?)</span>");


		public static bool IsRefreshing
		{
			get;
			private set;
		}

		public WebClient GetAuthorizedClient()
		{
			return GetAuthorizedClient(AuthToken);
		}

		static WebClient GetAuthorizedClient(string authToken)
		{
			WebClient client = new WebClient();
			client.Headers["Authorization"] = "GoogleLogin auth=" + authToken;
			return client;
		}

		public string AuthToken
		{
			get;
			set;
		}

		public string RNRSE
		{
			get;
			set;
		}

		public string GoogleVoicePhoneNumber
		{
			get;
			set;
		}

		public string DevicePhoneNumber
		{
			get;
			set;
		}

		private IEnumerable<Action> RefreshInboxInternal(TaskContext context)
		{
			WebClient client = GetAuthorizedClient();
			DownloadStringCompletedEventArgs e = null;
			client.DownloadStringCompleted += (s, dscea) =>
			{
				e = dscea;
				context.TaskCompletionHandler();
			};

			var currentTime = SecondsSinceEpoch;

			string[] labels = new string[] { "sms", "voicemail", "all" };
			// detect if we're doing an initial sync
			lock (SqliteDatabase.Instance)
			{
				SQLiteCommand existingCommand;
				lock (SqliteDatabase.Instance)
				{
					existingCommand = SqliteDatabase.Connection.CreateCommand("SELECT COUNT(Id) FROM Conversation LIMIT 1");
					if (existingCommand.ExecuteScalar<int>() > 0)
					{
						labels = new string[] { "all" };
					}
				}
			}

			foreach (var label in labels)
			{
				for (int currentPage = 0; currentPage < 2; currentPage++)
				{
					string refreshUrl;
					if (currentPage == 0)
						refreshUrl = string.Format("https://www.google.com/voice/inbox/recent/{0}/", label);
					else
						refreshUrl = string.Format("https://www.google.com/voice/inbox/recent/{0}/?page=p{1}", label, currentPage + 1);
					yield return () =>
						{
							client.DownloadStringAsync(new Uri(refreshUrl));
						};

					bool addedMessagesOnThisPage = false;
					yield return () =>
						{
							ThreadPool.QueueUserWorkItem(o =>
								{
									try
									{
										var xe = XElement.Parse(e.Result);

										var json = Newtonsoft.Json.Linq.JObject.Parse(xe.Element("json").Value);
										Payload payload = JsonConvert.DeserializeObject<Payload>(json.ToString());

										var html = xe.Element("html").Value;
										Regex ids = new Regex("<div id=\"(.*?)\"");
										var match = ids.Match(html);
										int last = 0;
										string id = null;
										Dictionary<string, string> blobs = new Dictionary<string, string>();
										while (match.Success)
										{
											if (last != 0)
												blobs[id] = html.Substring(last, match.Index - last);
											id = match.Groups[1].Value;
											last = match.Index;
											match = match.NextMatch();
										}
										if (last != 0)
										{
											blobs[id] = html.Substring(last, html.Length - last);
										}

										var conn = SqliteDatabase.Connection;

										Dictionary<string, List<Message>> messages = new Dictionary<string, List<Message>>();
										foreach (var key in blobs.Keys)
										{
											try
											{
												Conversation convo = null;

												var blob = blobs[key];
												// see if the conversation already exists in the model, create as necessary

												SQLiteCommand existingCommand;
												bool convoExistsAlready;
												lock (SqliteDatabase.Instance)
												{
													existingCommand = conn.CreateCommand("SELECT COUNT(Id) FROM Conversation WHERE Id=?", key);
													convoExistsAlready = existingCommand.ExecuteScalar<int>() > 0;
												}


												List<Message> convoMessages = new List<Message>();
												if (payload.Conversations.TryGetValue(key, out convo))
												{
													convo.Dispatcher = Dispatcher;
													switch (convo.Type)
													{
														// voicemail
														case 2:
															{
																match = voicemail.Match(blob);
																var builder = new System.Text.StringBuilder();
																while (match.Success)
																{
																	builder.AppendFormat("{0} ", match.Groups[2].Value);
																	match = match.NextMatch();
																}
																Message m = new Message();
																m.ConversationId = convo.Id;
																m.Text = HttpUtility.HtmlDecode(builder.ToString());
																match = voicemailTime.Match(blob);
																if (!match.Success)
																	continue;
																DateTime time = DateTime.Parse(match.Groups[1].Value);
																m.Time = time.ToShortTimeString();
																convo.LastMessageText = m.Text;
																convo.LastMessageTimeText = m.Time;

																convoMessages.Add(m);
															}
															break;
														// normal texts, not sure what the difference is
														case 10:
														case 11:
															{
																// find the conversation text blob
																match = message.Match(blob);
																while (match.Success)
																{
																	Message m = new Message();
																	var participant = HttpUtility.HtmlDecode(match.Groups[1].Value.Trim().TrimEnd(':'));
																	m.Incoming = participant != "Me";
																	if (m.Incoming)
																		convo.OtherParticipant = participant;
																	m.Text = HttpUtility.HtmlDecode(match.Groups[2].Value.Trim());
																	m.Time = HttpUtility.HtmlDecode(match.Groups[3].Value.Trim());
																	m.ConversationId = convo.Id;
																	convo.LastMessageText = m.Text;
																	convo.LastMessageTimeText = m.Time;
																	convoMessages.Add(m);

																	match = match.NextMatch();
																}
															}
															break;
														// no processing needed for calls, just grab the person we're calling and bail
														case 0:
															convo.LastMessageText = "Missed Call";
															convo.LastMessageTimeText = convo.DisplayStartTime;
															break;
														case 1:
															convo.LastMessageText = "Incoming Call";
															convo.LastMessageTimeText = convo.DisplayStartTime;
															break;
														case 7:
														case 8:
															convo.LastMessageText = "Outgoing Call";
															convo.LastMessageTimeText = convo.DisplayStartTime;
															break;
														default:
															continue;
													}

													// find the other participant
													match = contact.Match(blob);
													if (match.Success)
														convo.OtherParticipant = HttpUtility.HtmlDecode(match.Groups[1].Value);

													// make sure we could parse the other participant, or just fail out...
													if (convo.OtherParticipant == null)
														convo.OtherParticipant = "Unknown";
												}
												else
													continue;

												int existingCount;
												if (!convoExistsAlready)
												{
													lock (SqliteDatabase.Instance)
													{
														conn.Insert(convo);
														conn.Execute("DELETE FROM Message WHERE ConversationId=?", convo.Id);
													}
													Dispatcher.BeginInvoke(() =>
													{
														if (ConversationAdded != null)
															ConversationAdded(convo);
													});
													existingCount = 0;
												}
												else
												{
													SQLiteCommand cmd;
													lock (SqliteDatabase.Instance)
													{
														cmd = conn.CreateCommand("SELECT COUNT(Id) FROM Message WHERE ConversationId=?", convo.Id);
													}
													existingCount = cmd.ExecuteScalar<int>();
												}

												// no need to process messages from calls of any sort
												switch (convo.Type)
												{
													case 2:
													case 10:
													case 11:
														break;
													default:
														continue;
												}

												// insert any new messages
												bool hasChanged = false;
												for (int i = existingCount; i < convoMessages.Count; i++)
												{
													hasChanged = true;
													addedMessagesOnThisPage = true;
													// this is to offset messages that get updated at once...
													var m = convoMessages[i];
													lock (SqliteDatabase.Instance)
													{
														conn.Insert(m);
													}
													Dispatcher.BeginInvoke(() =>
													{
														if (MessageAdded != null)
															MessageAdded(convo, m);
													});
												}
												if (hasChanged)
												{
													if (convoExistsAlready)
													{
														lock (SqliteDatabase.Instance)
														{
															conn.Update(convo);
														}
													}
													Dispatcher.BeginInvoke(() =>
													{
														UpdateMatchingConversations(convo, (convos) =>
															{
																convos.Remove(convo.Id);
																convos.BinaryInsert(convo);
															});
													});
												}
												else
												{
													// see if we need to update read status
													Dispatcher.BeginInvoke(() =>
													{
														Conversation existingConvo;
														if (mConversations.Contains(convo.Id))
														{
															existingConvo = mConversations[convo.Id];
															if (existingConvo.IsRead != convo.IsRead)
															{
																existingConvo.IsRead = convo.IsRead;
																lock (SqliteDatabase.Instance)
																{
																	conn.Update(existingConvo);
																}
															}
														}
													});
												}
											}
											catch (Exception ex)
											{
												System.Diagnostics.Debug.WriteLine("Warning: error while parsing conversation (skipped): " + key);
												System.Diagnostics.Debug.WriteLine(ex);
											}
										}
									}
									catch (Exception ex)
									{
										System.Diagnostics.Debug.WriteLine("Unrecoverable failure while processing (internet connection?): " + refreshUrl);
										System.Diagnostics.Debug.WriteLine(ex);
										context.SetException(ex);
									}
									finally
									{
										context.TaskCompletionHandler();
									}
								});
						};
					// if we didn't add any messages during that last pull, don't bother pulling more
					if (!addedMessagesOnThisPage)
						break;
				}
			}
		}

		void UpdateMatchingConversations(Conversation convo, Action<Conversations> handler)
		{
			Conversations specific = null;
			switch (convo.Type)
			{
				case 2:
					handler(Conversations);
					specific = Voicemail;
					break;
				case 10:
				case 11:
					handler(Conversations);
					specific = Texts;
					break;
				case 0:
				case 1:
				case 7:
				case 8:
					specific = Calls;
					break;
			}

			handler(specific);
		}

		public void RefreshInbox()
		{
			lock (SqliteDatabase.Instance)
			{
				if (IsRefreshing)
					return;
				IsRefreshing = true;
			}
			if (RefreshInboxStarted != null)
				RefreshInboxStarted();

			TaskContext ctx = new TaskContext();
			ctx.Attach(RefreshInboxInternal(ctx), () =>
				{
					lock (SqliteDatabase.Instance)
					{
						IsRefreshing = false;
					}
					Dispatcher.BeginInvoke(() =>
					{
						if (RefreshInboxCompleted != null)
							RefreshInboxCompleted();
					});
				});
		}

		public static void Login(string username, string password, string service, Action<string> loginComplete, Action loginFailed)
		{
			WebClient client = new WebClient();
			// Send the requeset with username and password
			client.Headers["Content-Type"] = "application/x-www-form-urlencoded";

			client.UploadStringCompleted += (sender, e) =>
			{
				if (e.Error != null || string.IsNullOrEmpty(e.Result))
				{
					loginFailed();
					return;
				}

				// The response is plain text containing:
				// Auth=<authorization code>
				string authToken = null;
				string[] split = e.Result.Split('\n');
				foreach (string s in split)
				{
					string[] nvsplit = s.Split('=');
					if (nvsplit.Length == 2)
					{
						if (nvsplit[0] == "Auth")
						{
							authToken = nvsplit[1];
							break;
						}
					}
				}
				if (authToken == null)
				{
					loginFailed();
					return;
				}

				loginComplete(authToken);
			};
			client.UploadStringAsync(new Uri("https://www.google.com/accounts/ClientLogin"), string.Format("accountType=GOOGLE&Email={0}&Passwd={1}&service={2}&source=ClockworkMod", username, password, service));
		}

		public void Login(string username, string password, Action<string, string, string> loginComplete, Action loginFailed)
		{
			Login(username, password, "grandcentral", (authToken) =>
				{
					WebClient rnrClient = GetAuthorizedClient(authToken);
					rnrClient.DownloadStringCompleted += (sender2, e2) =>
						{
							ThreadPool.QueueUserWorkItem(o =>
								{
									Regex rnrSearch = new Regex("('_rnr_se':) '(.+)'");
									var match = rnrSearch.Match(e2.Result);
									if (match.Success)
									{
										var rnrse = match.Groups[2].Value;

										Regex numberSearch = new Regex("'raw': '\\+(\\d+)'");
										match = numberSearch.Match(e2.Result);
										if (match.Success)
										{
											AuthToken = authToken;
											RNRSE = rnrse;
											GoogleVoicePhoneNumber = match.Groups[1].Value;
											Dispatcher.BeginInvoke(() =>
												{
													loginComplete(AuthToken, RNRSE, GoogleVoicePhoneNumber);
												});
											return;
										}
									}
									loginFailed();
								});
						};
					rnrClient.DownloadStringAsync(new Uri("https://www.google.com/voice/#inbox"));
				},
				loginFailed);
		}

		public void GetPhoto(string username, string password, string number, PhotoEntry photoEntry, Action complete)
		{
			Login(username, password, "cp", (authToken) =>
				{
					var client = GetAuthorizedClient(authToken);
					client.OpenReadCompleted += (s, e) =>
						{
							ThreadPool.QueueUserWorkItem(o =>
							{
								try
								{
									string file = NumbersOnly(number) + ".png";
									var stream = e.Result;
									int len = (int)stream.Length;
									byte[] bytes = new byte[len];
									int read = stream.Read(bytes, 0, len);
									using (var ofile = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication().OpenFile(file, System.IO.FileMode.Create))
									{
										ofile.Write(bytes, 0, len);
									}
								}
								catch (Exception ex)
								{
									System.Diagnostics.Debug.WriteLine(ex);
								}
								finally
								{
									complete();
								}
							});
						};
					client.OpenReadAsync(new Uri(photoEntry.Url));
				},
				() =>
				{
				});
		}

		void GetContacts(string username, int startIndex, IEnumerable<Conversation> conversations, string authToken, Action<int, bool> completed)
		{
			var cc = GetAuthorizedClient(authToken);
			cc.DownloadStringCompleted += (s, e) =>
				{
					ThreadPool.QueueUserWorkItem(o =>
						{
							try
							{

								var json = JObject.Parse(e.Result);
								var entries = json["feed"]["entry"];
								var picturesWithNumbers =
									from entry in entries
									let numbers = entry["gd$phoneNumber"]
									let link = entry["link"]
									let image = (from linkEntry in link where linkEntry["rel"].Value<string>() == "http://schemas.google.com/contacts/2008/rel#photo" select linkEntry).FirstOrDefault()
									where numbers != null && link != null &&
									image != null
									select new
									{
										Numbers = from number in numbers select number["$t"].Value<string>(),
										Photo = image["href"].Value<string>()
									};
								var photoEntries = from entry in picturesWithNumbers
												   from number in entry.Numbers
												   select new PhotoEntry()
												   {
													   Number = number,
													   Url = entry.Photo
												   };

								lock (SqliteDatabase.Instance)
								{
									var conn = SqliteDatabase.Connection;
									conn.InsertAll(photoEntries);
								}

								int newIndex = startIndex + int.Parse(json["feed"]["openSearch$itemsPerPage"]["$t"].Value<string>());
								completed(newIndex, false);
							}
							catch (Exception ex)
							{
								System.Diagnostics.Debug.WriteLine(ex);
								completed(-1, true);
							}
						});
				};
			cc.DownloadStringAsync(new Uri(string.Format("https://www.google.com/m8/feeds/contacts/{0}/full?alt=json{1}", username, startIndex != 0 ? string.Format("&start-index={0}", startIndex) : string.Empty)));
		}

		public static int SecondsSinceEpoch
		{
			get
			{
				return (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
			}
		}

		bool mGettingPhotos = false;
		public void GetPhotosForNumbers(string username, string password)
		{
			var settings = Settings.Instance;
			if (DownloadContactsStarted != null)
				DownloadContactsStarted();
			if (SecondsSinceEpoch - settings.LastContactSync < TimeSpan.FromDays(7).TotalSeconds || mGettingPhotos)
			{
				if (DownloadContactsCompleted != null)
					DownloadContactsCompleted();
				return;
			}

			mGettingPhotos = true;
			if (!username.Contains('@'))
				username = username + "@gmail.com";
			WebClient client = new WebClient();
			client.Headers["Content-Type"] = "application/x-www-form-urlencoded";
			Login(username, password, "cp", (authToken) =>
				{
					IEnumerable<Conversation> conversations = null;
					lock (SqliteDatabase.Instance)
					{
						var conn = SqliteDatabase.Connection;
						conn.Execute("DELETE FROM PhotoEntry");
						conversations = SqliteDatabase.Connection.Query<Conversation>("SELECT Id, PhoneNumber FROM Conversation");
					}
					Action<int, bool> completionHandler = null;
					completionHandler = (newIndex, complete) =>
						{
							if (!complete)
							{
								GetContacts(username, newIndex, conversations, authToken, completionHandler);
							}
							else
							{
								Dispatcher.BeginInvoke(() =>
									{
										mGettingPhotos = false;
										settings.LastContactSync = SecondsSinceEpoch;
										settings.Save();

										ContactPhotoConverter.RefreshFiles();
										Conversations.Reset();
										Texts.Reset();
										Voicemail.Reset();
										Calls.Reset();
										if (DownloadContactsCompleted != null)
											DownloadContactsCompleted();
									});
							}
						};
					completionHandler(0, false);
				},
				() =>
				{
					mGettingPhotos = false;
				});
		}

		public static string NumbersOnly(string number)
		{
			if (number == null)
				return null;
			var builder = new System.Text.StringBuilder();
			foreach (var c in number)
			{
				if (char.IsNumber(c))
					builder.Append(c);
			}
			return builder.ToString();
		}

		public void Call(string number)
		{
			var client = GetAuthorizedClient();
			client.Headers["Content-type"] = "application/x-www-form-urlencoded;charset=utf-8";
			client.UploadStringCompleted += (sender2, e2) =>
			{
				try
				{
					Console.WriteLine(e2.Result);
				}
				catch (Exception)
				{
				}
			};
			System.Diagnostics.Debug.WriteLine(NumbersOnly(number));
			System.Diagnostics.Debug.WriteLine(NumbersOnly(DevicePhoneNumber));
			System.Diagnostics.Debug.WriteLine(RNRSE);
			System.Diagnostics.Debug.WriteLine(AuthToken);
			client.UploadStringAsync(new Uri("https://www.google.com/voice/call/connect/"), string.Format("outgoingNumber={0}&forwardingNumber={1}&phoneType=2&remember=1&subscriberNumber=undefined&_rnr_se={2}", NumbersOnly(number), NumbersOnly(DevicePhoneNumber), RNRSE));
		}

		public void Send(string number, string message, Action onSuccess, Action onFailure)
		{
			var client = GetAuthorizedClient();
			client.Headers["Content-type"] = "application/x-www-form-urlencoded;charset=utf-8";
			client.UploadStringCompleted += (sender2, e2) =>
			{
				try
				{
					var result = Newtonsoft.Json.Linq.JObject.Parse(e2.Result);
					if (result.Value<bool>("ok") && onSuccess != null)
						onSuccess();
				}
				catch (Exception)
				{
					if (onFailure != null)
						onFailure();
				}
			};
			client.UploadStringAsync(new Uri("https://www.google.com/voice/sms/send/"), string.Format("id=&phoneNumber={0}&text={1}&&_rnr_se={2}", NumbersOnly(number), Uri.EscapeDataString(message), RNRSE));
		}

		public static bool NumbersMatch(string n1, string n2)
		{
			if (n1 == null && n2 == null)
				return true;
			if (n1 == null || n2 == null)
				return false;
			n1 = NumbersOnly(n1);
			n2 = NumbersOnly(n2);
			return n1.Contains(n2) || n2.Contains(n1);
		}

		public string PIN
		{
			get;
			set;
		}

		public void MarkAsRead(Conversation conversation, bool read)
		{
			WebClient client = GetAuthorizedClient();
			client.Headers["Content-Type"] = "application/x-www-form-urlencoded";
			client.UploadStringCompleted += (s, e) =>
			{
				try
				{
					var result = Newtonsoft.Json.Linq.JObject.Parse(e.Result);
					if (result.Value<bool>("ok"))
						conversation.IsRead = read;
				}
				catch (Exception)
				{
				}
			};
			client.UploadStringAsync(new Uri("https://www.google.com/voice/inbox/mark/"), string.Format("messages={0}&read={1}&_rnr_se={2}", conversation.Id, read ? 1 : 0, RNRSE));
		}


		public void Trash(Conversation conversation)
		{
			lock (SqliteDatabase.Instance)
			{
				SqliteDatabase.Connection.Delete<Conversation>(conversation);
				SqliteDatabase.Connection.Execute("DELETE FROM Message WHERE ConversationId=?", conversation.Id);
			}
			UpdateMatchingConversations(conversation, (convos) =>
			{
				convos.Remove(conversation.Id);
			});
			WebClient client = GetAuthorizedClient();
			client.Headers["Content-Type"] = "application/x-www-form-urlencoded";
			client.UploadStringCompleted += (s, e) =>
			{
				try
				{
					var result = Newtonsoft.Json.Linq.JObject.Parse(e.Result);
				}
				catch (Exception)
				{
				}
			};
			client.UploadStringAsync(new Uri("https://www.google.com/voice/inbox/deleteMessages/"), string.Format("messages={0}&trash=1&_rnr_se={1}", conversation.Id, RNRSE));
		}

		public string GetDirectDialNumber(string number)
		{
			number = NumbersOnly(number);
			return GoogleVoicePhoneNumber + ",," + PIN + ",,2,," + number + "#";
		}

		public string GetVoicemailUrl(Conversation convo)
		{
			return "https://www.google.com/voice/media/send_voicemail/" + convo.Id;
		}

		public event Action DownloadContactsStarted;
		public event Action DownloadContactsCompleted;
		public event Action RefreshInboxStarted;
		public event Action RefreshInboxCompleted;
		public event PropertyChangedEventHandler PropertyChanged;
		public event Action<Conversation> ConversationAdded;
		public event Action<Conversation> ConversationUpdated;
		public event Action<Conversation, Message> MessageAdded;
	}
}