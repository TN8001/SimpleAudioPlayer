using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace SimpleAudioPlayer
{
    public partial class MarqueeControl : UserControl
    {
        [Category("Common Properties")]
        [Description("この要素のテキスト内容を取得または設定します。")]
        public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }
        public static readonly DependencyProperty TextProperty
            = DependencyProperty.Register("Text", typeof(string), typeof(MarqueeControl), new PropertyMetadata(null));

        public MarqueeControl()
        {
            InitializeComponent();
            Loaded += (s, e) => RestAnimation();
            marquee.SizeChanged += (s, e) => RestAnimation();
        }

        private void RestAnimation()
        {
            marquee.BeginAnimation(Canvas.LeftProperty, null);
            tb2.Visibility = Visibility.Hidden;
            if(userControl.ActualWidth > tb.ActualWidth) return;

            tb2.Visibility = Visibility.Visible;
            var doubleAnimation = new DoubleAnimation()
            {
                Duration = new Duration(TimeSpan.FromSeconds(tb.ActualWidth / 50)),
                From = 0,
                To = -marquee.ActualWidth / 2 - 25,
                RepeatBehavior = RepeatBehavior.Forever,
            };
            marquee.BeginAnimation(Canvas.LeftProperty, doubleAnimation);
        }
    }
}
