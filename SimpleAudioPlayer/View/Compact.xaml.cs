using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SimpleAudioPlayer
{
    public partial class Compact : UserControl
    {
        public Compact()
        {
            InitializeComponent();
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
            => Application.Current.MainWindow.DragMove();
    }
}
