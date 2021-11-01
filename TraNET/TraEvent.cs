using System.ComponentModel;

namespace TraNET {
    public delegate void ConnectionEventHandler(object sender, ConnectionEventArgs e);
    public delegate void SessionStartEventHandler(object sender, SessionInfoEventArgs e);
    public delegate void SessionReadyEventHandler(object sender, SessionReadyEventArgs e);

    /// <summary>
    /// 접속 이벤트
    /// </summary>
    public class ConnectionEventArgs {
        /// <summary>
        /// 주소
        /// </summary>
        public IPEndPoint IPEndPoint { get; }

        /// <summary>
        /// 프로토콜 이름
        /// </summary>
        public ReadOnlyMemory<byte> ProtocolName { get; }

        /// <summary>
        /// 프로토콜 버전
        /// </summary>
        public uint ProtocolVersion { get; }

        /// <summary>
        /// 차단 여부
        /// </summary>
        internal bool isBlock;

        /// <summary>
        /// 코드
        /// </summary>
        internal int code;

        public ConnectionEventArgs(IPEndPoint endPoint, uint protocolVersion, ReadOnlyMemory<byte> protocolName) { 
            IPEndPoint = endPoint;
            ProtocolVersion = protocolVersion;
            ProtocolName = protocolName;
        }

        /// <summary>
        /// 접속을 차단합니다.
        /// </summary>
        public void Block() {
            isBlock = true;
        }

        /// <summary>
        /// 접속을 차단합니다.
        /// </summary>
        /// <param name="code">블럭 코드</param>
        public void Block(int code) {
            this.code = code;
            isBlock = true;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Deconstruct(out bool isBlock, out int code) {
            isBlock = this.isBlock;
            code = this.code;
        }
    }

    public class SessionInfoEventArgs {
        /// <summary>
        /// 주소
        /// </summary>
        public IPEndPoint IPEndPoint { get; }

        /// <summary>
        /// 버퍼 크기
        /// </summary>
        public int BufferSize { get; set; }

        internal byte[]? state = null;

        /// <summary>
        /// 상태
        /// 최대 크기: <see cref="TraServerInfo_2f.MaxStateLength"/>
        /// </summary>
        public byte[]? State {
            get => state;
            set {
                if (value?.Length > TraServerInfo_2f.MaxStateLength) throw new ArgumentOutOfRangeException(nameof(state),
                    $"상태 데이터는 {TraServerInfo_2f.MaxStateLength} 보다 클 수 없습니다.");
                state = value;
            }
        }

        public SessionInfoEventArgs(IPEndPoint endPoint, int bufferSize) {
            IPEndPoint = endPoint;
            BufferSize = bufferSize;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Deconstruct(out int bufferSize, out byte[]? state) {
            bufferSize = BufferSize;
            state = this.state;
        }
    }

    /// <summary>
    /// 세션 준비됨.
    /// </summary>
    public class SessionReadyEventArgs { 
        /// <summary>
        /// 클라이언트
        /// </summary>
        public TraClient Client { get; }

        public SessionReadyEventArgs(TraClient client) {
            Client = client;
        }
    }
}
