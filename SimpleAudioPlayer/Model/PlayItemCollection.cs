using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace SimpleAudioPlayer
{
    [CollectionDataContract(Namespace = "", Name = "PlayList")]
    public class PlayItemCollection : ObservableCollection<PlayItem>
    {
        public PlayItemCollection() : base() { }
        public PlayItemCollection(List<PlayItem> list) : base(list) { }
    }
}
