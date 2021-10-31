namespace TraNET {
    /// <summary>
    /// 트라 서-버
    /// </summary>
    public partial class TraServer {
        readonly CancellationTokenSource cancel = new();
        readonly TcpListener listener;
        readonly IPAddress address;
        readonly int port;
        int bufferSize, maxConnections = 500;

        public TraServer(int port) : this(IPAddress.Any, port, TraNET.DefaultBufferSize) {

        }

        public TraServer(IPEndPoint endPoint) : this(endPoint.Address, endPoint.Port, TraNET.DefaultBufferSize) {

        }

        public TraServer(IPAddress address, int port, int bufferSize) {
            this.address = address;
            this.port = port;
            this.bufferSize = bufferSize;
            listener = new TcpListener(address, port);
        }

        /// <summary>
        /// Tra 서버를 시작합니다.
        /// </summary>
        public virtual void Start() => Start(0);

        /// <summary>
        /// Tra 서버를 시작합니다.
        /// </summary>
        /// <param name="backlog">접속 대기 최대 수</param>
        public virtual void Start(int backlog) {
            listener.Start(backlog);
            listener.BeginAcceptTcpClient(BeginAccept, null);
        }
        
        /// <summary>
        /// Tra 서버를 종료합니다.
        /// </summary>
        public virtual void Stop() {
            cancel.Cancel();
            listener.Stop();
        }

        /// <summary>
        /// 최대 접속 수를 제한합니다.
        /// 0 일 경우 제한하지 않습니다.
        /// </summary>
        public int MaxConnections {
            get => maxConnections;  
            set {
                if (value < 0) throw new ArgumentException("최소 접속자 수는 -1 보다 높아야합니다.");
                maxConnections = value;
            }
        }

        public int BufferSize {
            get => bufferSize;
            set {
                if (value < 128) throw new ArgumentException("버퍼 크기는 128 보다 높아야합니다.");
                bufferSize = value;
            }
        }

        /// <summary>
        /// 서버 주소
        /// </summary>
        public IPAddress Address => address;

        /// <summary>
        /// 서버 포트
        /// </summary>
        public int Port => port;
    }
}
