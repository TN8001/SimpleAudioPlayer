using System.Runtime.Serialization;

namespace SimpleAudioPlayer
{
    [DataContract(Namespace = "")]
    public class WindowModel : BindableBase
    {
        [DataMember(Order = 0, Name = "Top")]
        private double _Top;
        public double Top { get => _Top; set => SetProperty(ref _Top, value); }

        [DataMember(Order = 1, Name = "Left")]
        private double _Left;
        public double Left { get => _Left; set => SetProperty(ref _Left, value); }

        [DataMember(Order = 2, Name = "Width", EmitDefaultValue = false)]
        private double _Width;
        public double Width { get => _Width; set => SetProperty(ref _Width, value); }

        [DataMember(Order = 3, Name = "Height", EmitDefaultValue = false)]
        private double _Height;
        public double Height { get => _Height; set => SetProperty(ref _Height, value); }
    }
}
