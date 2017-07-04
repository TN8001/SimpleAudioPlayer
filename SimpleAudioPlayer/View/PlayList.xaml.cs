using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SimpleAudioPlayer
{
    public partial class PlayList : Window
    {
        private ViewModel vm => DataContext as ViewModel;

        public PlayList()
        {
            InitializeComponent();
        }

        protected void ListViewItem_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            var playItem = ((ListViewItem)sender).Content as PlayItem;
            vm.Player.PlayItemCommand.Execute(playItem);
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            vm.ShowPlayList = false;
            e.Cancel = true;
        }
        private void Window_PreviewDragOver(object sender, DragEventArgs e)
        {
            if(!e.Data.GetDataPresent(DataFormats.FileDrop, true)) return;

            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }
        private void Window_Drop(object sender, DragEventArgs e)
        {
            var list = vm.Setting.PlayList;
            var files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if(files == null) return;

            foreach(var s in files)
                list.Add(new PlayItem(s));
        }
    }
}
