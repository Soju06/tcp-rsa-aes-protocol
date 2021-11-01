namespace TraNET {
    public readonly struct TRACLIENTINFO {
        public ReadOnlyMemory<byte> _greetingData { get; }
        public int _bufferSize { get; }

        public TRACLIENTINFO(ReadOnlyMemory<byte> greetingData, int bufferSize) {
            _greetingData = greetingData;
            _bufferSize = bufferSize;
        }
    }
}
