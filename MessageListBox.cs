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

namespace GoogleVoice
{
    public class MessageListBox : ListBox
    {
        class MessageListBoxItem : ListBoxItem
        {
            public MessageListBoxItem()
            {
            }

            protected override void OnContentChanged(object oldContent, object newContent)
            {
                base.OnContentChanged(oldContent, newContent);

                var message = newContent as Message;
				if (message != null)
                {
                    HorizontalAlignment = message.Incoming ? HorizontalAlignment.Left : System.Windows.HorizontalAlignment.Right;
                    UpdateLayout();
                }
            }
        }

        public MessageListBox()
        {
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new MessageListBoxItem();
        }
    }
}
