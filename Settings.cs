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
using System.IO;
using System.IO.IsolatedStorage;
using Community.CsharpSqlite;
using System.Xml.Serialization;

namespace GoogleVoice
{
    public class Settings
    {
        public Settings()
        {
			DeviceId = Microsoft.Phone.Info.UserExtendedProperties.GetValue("ANID") as string;
			if (DeviceId == null)
				DeviceId = Guid.NewGuid().ToString();
			else
				DeviceId = Convert.ToBase64String(System.Text.UTF8Encoding.UTF8.GetBytes(DeviceId));
			var builder = new System.Text.StringBuilder();
			foreach (var c in DeviceId)
			{
				if (builder.Length > 20)
					break;
				if (!char.IsLetterOrDigit(c))
					continue;
				builder.Append(c);
			}
			DeviceId = "I" + builder.ToString().ToUpper();
        }

        static Settings mInstance = null;
        public static Settings Instance
        {
            get
            {
                if (mInstance == null)
                {
					if (IsolatedStorageSettings.ApplicationSettings.Contains("Valid"))
					{
						mInstance = new Settings();
					}
					else
					{
						try
						{
							IsolatedStorageSettings.ApplicationSettings["Valid"] = true;
							using (var appStorage = IsolatedStorageFile.GetUserStoreForApplication())
							{
								using (var file = appStorage.OpenFile("settings.xml", FileMode.Open))
								{
									XmlSerializer ser = new XmlSerializer(typeof(Settings));
									mInstance = (Settings)ser.Deserialize(file);
								}
							}
						}
						catch (Exception)
						{
							mInstance = new Settings();
						}
						finally
						{
							IsolatedStorageSettings.ApplicationSettings.Save();
						}
					}
                }
                return mInstance;
            }
        }

        public void Save()
        {
			IsolatedStorageSettings.ApplicationSettings.Save();
		}

		[XmlIgnore]
		public string DeviceId
		{
			get;
			set;
		}

		public void Set<T>(string key, T value)
		{
			mSettings[key] = value;
		}

		public T Get<T>(string key)
		{
			try
			{
				return (T)mSettings[key];
			}
			catch (Exception)
			{
				return default(T);
			}
		}

		IsolatedStorageSettings mSettings = IsolatedStorageSettings.ApplicationSettings;
		public string this[string key]
		{
			get
			{
				string ret;
				if (mSettings.TryGetValue<string>(key, out ret))
					return ret;
				return null;
			}
			set
			{
				mSettings[key] = value;
			}
		}

        public string Username
        {
			get
			{
				return this["Username"];
			}
			set
			{
				this["Username"] = value;
			}
        }

		public string Password
		{
			get
			{
				return this["Password"];
			}
			set
			{
				this["Password"] = value;
			}
		}

		public string GoogleVoicePhoneNumber
		{
			get
			{
				return this["GoogleVoicePhoneNumber"];
			}
			set
			{
				this["GoogleVoicePhoneNumber"] = value;
			}
		}

        public string DevicePhoneNumber
		{
			get
			{
				return this["DevicePhoneNumber"];
			}
			set
			{
				this["DevicePhoneNumber"] = value;
			}
		}

        public string AuthToken
		{
			get
			{
				return this["AuthToken"];
			}
			set
			{
				this["AuthToken"] = value;
			}
		}

        public string RNRSE
		{
			get
			{
				return this["RNRSE"];
			}
			set
			{
				this["RNRSE"] = value;
			}
		}

		public int LastContactSync
		{
			get
			{
				return Get<int>("LastContactSync");
			}
			set
			{
				Set("LastContactSync", value);
			}
		}

		public bool UsePINDialPrefix
		{
			get
			{
				return Get<bool>("UsePINDialPrefix");
			}
			set
			{
				Set("UsePINDialPrefix", value);
			}
		}


		public string PIN
		{
			get
			{
				return this["PIN"];
			}
			set
			{
				this["PIN"] = value;
			}
		}

		[XmlIgnore]
		public bool ShouldUsePINDial
		{
			get
			{
				return UsePINDialPrefix && !string.IsNullOrEmpty(PIN);
			}
		}

		[XmlIgnore]
		public bool HasValidLogin
		{
			get
			{
				return !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password) && !string.IsNullOrEmpty(DevicePhoneNumber);
			}
		}

		public bool TileNotificationEnabled
		{
			get
			{
				return Get<bool>("TileNotificationEnabled");
			}
			set
			{
				Set("TileNotificationEnabled", value);
			}
		}

		public bool ToastNotificationEnabled
		{
			get
			{
				return Get<bool>("ToastNotificationEnabled");
			}
			set
			{
				Set("ToastNotificationEnabled", value);
			}
		}

		public string LastWhatsNew
		{
			get
			{
				return this["LastWhatsNew"];
			}
			set
			{
				this["LastWhatsNew"] = value;
			}
		}

		public static bool IsTrial
		{
			get
			{
				var license = new Microsoft.Phone.Marketplace.LicenseInformation();
				var isTrial = license.IsTrial();
#if DEBUG
				isTrial = false;
#endif
				return isTrial;
			}
		}
    }
}
