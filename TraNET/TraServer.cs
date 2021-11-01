namespace TraNET {
    /// <summary>
    /// 트라 서-버
    /// </summary>
    public partial class TraServer : IDisposable {
        protected ReadOnlyMemory<byte> greetingData = TraNet.DefaultProtocolName;
        readonly CancellationTokenSource cancel = new();
        readonly TcpListener listener;
        readonly IPAddress address;
        readonly int port;
        int bufferSize, maxConnections = 500;
        bool listening;
        private bool disposedValue;

        public TraServer(int port) : this(IPAddress.Any, port, TraNet.DefaultBufferSize) {

        }

        public TraServer(IPEndPoint endPoint, int bufferSize) : this(endPoint.Address, endPoint.Port, bufferSize) {

        }

        public TraServer(IPEndPoint endPoint) : this(endPoint.Address, endPoint.Port, TraNet.DefaultBufferSize) {

        }
        
        public TraServer(IPAddress address, int port) : this(address, port, TraNet.DefaultBufferSize) {

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
            if (disposedValue) throw new ObjectDisposedException("this");
            lock (listener) {
                if (listening) throw new InvalidOperationException("이미 서버가 실행 중입니다.");
                listener.Start(backlog);
                Task.Factory.StartNew(ConnectionManage, cancel.Token);
                listener.BeginAcceptTcpClient(BeginAccept, null);
                listening = true;
            }
        }
        
        /// <summary>
        /// Tra 서버를 종료합니다.
        /// </summary>
        public virtual void Stop() {
            try {
                listener.Stop();
                if (cancel?.IsCancellationRequested == false)
                    cancel?.Cancel();
            } finally {
                listening = false;
            }
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
                greetingData = new(TraCrypto.GetSHA256Hash(value));
            }
        }

        protected virtual void Dispose(bool disposing) { {
                if (disposing) {
                    Stop();
                }


                disposedValue = true;
            }
        }

        public void Dispose() {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
