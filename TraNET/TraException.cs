namespace TraNET {
    /// <summary>
    /// 프로토콜 초기화 예외
    /// </summary>
    [Serializable]
    public class ProtocolInitializationException : Exception {
        public ProtocolInitializationException() { }
        public ProtocolInitializationException(string message) : base(message) { }
        public ProtocolInitializationException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// 프로토콜 불일치
    /// </summary>
    [Serializable]
    public class ProtocolMismatchException : Exception {
        public ProtocolMismatchException() { }
        public ProtocolMismatchException(string message) : base(message) { }
        public ProtocolMismatchException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// 지원하지 않는 버전
    /// </summary>
    [Serializable]
    public class UnsupportedVersionException : Exception {
        public UnsupportedVersionException() { }
        public UnsupportedVersionException(string message) : base(message) { }
        public UnsupportedVersionException(string message, Exception inner) : base(message, inner) { }
        protected UnsupportedVersionException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    /// <summary>
    /// 프로토콜 취소됨
    /// </summary>
    [Serializable]
    public class ProtocolCancellationException : Exception {
        public ProtocolCancellationException() { }
        public ProtocolCancellationException(string message) : base(message) { }
        public ProtocolCancellationException(string message, int code) : base(message + $" (CODE: {code})") { }
        public ProtocolCancellationException(string message, Exception inner) : base(message, inner) { }
    }
}
