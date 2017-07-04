using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace SimpleAudioPlayer
{
    [DataContract]
    public abstract class BindableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged { add { handler += value; } remove { handler -= value; } }

        private PropertyChangedEventHandler handler;

        public BindableBase()
        {
            Init();
        }

        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if(Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        [OnDeserializing]
        protected void OnDeserializing(StreamingContext sc) => Init();
        ///<summary>デシリアライズ時の初期値設定</summary>
        protected virtual void Init() { }
    }
}
