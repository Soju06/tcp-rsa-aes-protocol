namespace TraNET {
    partial class TraServer {
        protected ReadOnlyMemory<byte> greetingData = TraNET.DefaultProtocolName;

        /// <summary>
        /// 인사 데이터
        /// </summary>
        /// <exception cref="ArgumentNullException" />
        public ReadOnlyMemory<byte> GreetingData {
            get => greetingData;
            set {
                if (value.IsEmpty || value.Length < 1) throw new ArgumentException("데이터가 없습니다.");
                greetingData = value;
            }
        }

        /// <summary>
        /// 인사 데이터
        /// </summary>
        public string GreetingText {
            set {
                if (value == null) throw new ArgumentNullException("value");
                greetingData = new(TraNET.GetSHA256Hash(value));
            }
        }

        protected virtual async Task Process(TcpClient client) {
            try {
                
            } catch (Exception ex) {

            }
            client.Close();
        }
    }

    public readonly struct TRACLIENTINFO {
        public ReadOnlyMemory<byte> _greetingData { get; }
        public int _bufferSize { get; }

        public TRACLIENTINFO(ReadOnlyMemory<byte> greetingData, int bufferSize) {
            _greetingData = greetingData;
            _bufferSize = bufferSize;
        }
    }
}
