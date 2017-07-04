using System.Windows.Controls;
using System.Windows.Input;

namespace SimpleAudioPlayer
{
    public class SeekBar : ProgressBar
    {
        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            var range = Maximum - Minimum;
            var par = e.GetPosition(this).X / ActualWidth;
            Value = range * par + Minimum;
        }
    }
}
