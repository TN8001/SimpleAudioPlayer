using System.Windows;

namespace SimpleAudioPlayer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new ViewModel();
            Closing += (s, e) => ((ViewModel)DataContext).SaveCommand.Execute();
        }
    }
}
