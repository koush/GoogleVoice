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
using System.Collections.Generic;

namespace GoogleVoice
{
	public class PartiallyUnderlinedTextBlock : ContentControl
	{
		public static readonly DependencyProperty UnderlinedWordProperty;
		public static readonly DependencyProperty TextProperty;


		static PartiallyUnderlinedTextBlock()
		{
			UnderlinedWordProperty = DependencyProperty.Register("UnderlinedWord", typeof(int), typeof(PartiallyUnderlinedTextBlock), new PropertyMetadata(-1, new PropertyChangedCallback(OnUnderlinedWordChanged)));
			TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(PartiallyUnderlinedTextBlock), new PropertyMetadata(null, new PropertyChangedCallback(OnTextChanged)));
		}

		TextBlock mTextBlock = new TextBlock();
		public PartiallyUnderlinedTextBlock()
		{
			Content = mTextBlock;
			mTextBlock.FontSize = 21.333;
			mTextBlock.FontFamily = new FontFamily("Segoe WP Semibold");
			mTextBlock.TextWrapping = TextWrapping.Wrap;
		}

		public int UnderlinedWord
		{
			get
			{
				return (int)GetValue(UnderlinedWordProperty);
			}
			set
			{
				SetValue(UnderlinedWordProperty, value);
			}
		}

		public string Text
		{
			get
			{
				return (string)GetValue(TextProperty);
			}
			set
			{
				SetValue(TextProperty, value);
			}
		}

		public static void OnTextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			if (e.NewValue == e.OldValue) return;

			PartiallyUnderlinedTextBlock r = sender as PartiallyUnderlinedTextBlock;
			var tb = r.mTextBlock;
			tb.Text = e.NewValue as string;
		}

		public static void OnUnderlinedWordChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			if (e.NewValue == e.OldValue) return;

			PartiallyUnderlinedTextBlock r = sender as PartiallyUnderlinedTextBlock;
			var tb = r.mTextBlock;
			var inlines = tb.Inlines;

			int old = (int)e.OldValue * 2;
			if (old >= 0 && old < inlines.Count)
				inlines[old].TextDecorations = null;

			int underlined = (int)e.NewValue * 2;
			if (underlined < 0)
				return;

			if (inlines.Count <= 1)
			{
				var splits = r.mTextBlock.Text.Trim().Split(' ');
				inlines.Clear();
				for (int i = 0; i < splits.Length; i++)
				{
					var split = splits[i];
					var run = new Run();
					run.Text = split;
					inlines.Add(run);

					run = new Run();
					run.Text = " ";
					inlines.Add(run);
				}
			}

			if (underlined < inlines.Count)
				inlines[underlined].TextDecorations = TextDecorations.Underline;
			
			try
			{
			}
			catch (Exception)
			{
			}
		}
	}
}
