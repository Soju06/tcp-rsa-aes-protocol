namespace TraNET {
    public readonly struct TRACLIENTINFO {
        public ReadOnlyMemory<byte> _greetingData { get; }
        public int _bufferSize { get; }
        public bool _protocolNameMatch { get; }

        public TRACLIENTINFO(ReadOnlyMemory<byte> greetingData, int bufferSize, bool protocolNameMatch) {
            _greetingData = greetingData;
            _bufferSize = bufferSize;
            _protocolNameMatch = protocolNameMatch;
        }
    }
}
