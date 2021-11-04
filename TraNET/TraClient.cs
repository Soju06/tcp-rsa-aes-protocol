namespace TraNET {
    /// <summary>
    /// 트라 클-라이언트
    /// </summary>
    public partial class TraClient : IDisposable {
        IPAddress address;
        int port = -1;
        TraAES aes;
        TcpClient client;
        NetworkStream stream;
        CancellationTokenSource cancel;
        bool isConnected;
        protected bool disposedValue;

        public TraClient() {
            IsClientSide = true;
            client = new();
            cancel = new();
        }

        /// <summary>
        /// 서버에 연결합니다.
        /// </summary>
        /// <param name="hostName">호스트</param>
        /// <param name="port">포트</param>
        /// <param name="greetingText">인사 데이터</param>
        /// <exception cref="ArgumentNullException" />
        public void Connect(string hostName, int port, string greetingText) =>
            Connect(hostName, port, TraCrypto.GetSHA256Hash(Encoding.UTF8.GetBytes(greetingText)));


        /// <summary>
        /// 서버에 연결합니다.
        /// </summary>
        /// <param name="hostName">호스트</param>
        /// <param name="port">포트</param>
        /// <param name="greetingData">인사 데이터</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="ArgumentOutOfRangeException" />
        /// <exception cref="SocketException" />
        /// <exception cref="ObjectDisposedException" />
        public void Connect(string hostName, int port, byte[] greetingData) {
            if (disposedValue) throw new ObjectDisposedException("this");
            if (hostName == null) throw new ArgumentNullException("host");
            if (greetingData == null) throw new ArgumentNullException("greetingData");

            client.Connect(hostName, port);
            var endPoint = (IPEndPoint)client.Client.RemoteEndPoint;
            this.port = port;
            address = endPoint.Address;
            IPEndPoint = endPoint;
            ConnectionProcess(greetingData);
        }

        /// <summary>
        /// 서버에 연결합니다.
        /// </summary>
        /// <param name="endPoint">끝점</param>
        /// <param name="greetingText">인사 데이터</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="ArgumentOutOfRangeException" />
        /// <exception cref="SocketException" />
        /// <exception cref="ObjectDisposedException" />
        public void Connect(IPEndPoint endPoint, string greetingText) =>
            Connect(endPoint, TraCrypto.GetSHA256Hash(Encoding.UTF8.GetBytes(greetingText)));

        /// <summary>
        /// 서버에 연결합니다.
        /// </summary>
        /// <param name="endPoint">끝점</param>
        /// <param name="greetingData">인사 데이터</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="ArgumentOutOfRangeException" />
        /// <exception cref="SocketException" />
        /// <exception cref="ObjectDisposedException" />
        public void Connect(IPEndPoint endPoint, byte[] greetingData) { 
            if (disposedValue) throw new ObjectDisposedException("this");
            if (endPoint == null) throw new ArgumentNullException("endPoint");
            if (greetingData == null) throw new ArgumentNullException(nameof(greetingData));

            client.Connect(endPoint);
            port = endPoint.Port;
            address = endPoint.Address;
            IPEndPoint = endPoint;
            ConnectionProcess(greetingData);
        }

        /// <summary>
        /// 서버에 연결합니다.
        /// </summary>
        /// <param name="address">주소</param>
        /// <param name="port">포트</param>
        /// <param name="greetingText">인사 데이터</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="ArgumentOutOfRangeException" />
        /// <exception cref="SocketException" />
        /// <exception cref="ObjectDisposedException" />
        public void Connect(IPAddress address, int port, string greetingText) =>
            Connect(address, port, TraCrypto.GetSHA256Hash(Encoding.UTF8.GetBytes(greetingText)));

        /// <summary>
        /// 서버에 연결합니다.
        /// </summary>
        /// <param name="address">주소</param>
        /// <param name="port">포트</param>
        /// <param name="greetingData">인사 데이터</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="ArgumentOutOfRangeException" />
        /// <exception cref="SocketException" />
        /// <exception cref="ObjectDisposedException" />
        public void Connect(IPAddress address, int port, byte[] greetingData) {
            if (disposedValue) throw new ObjectDisposedException("this");
            if (address == null) throw new ArgumentNullException(nameof(address));
            if (greetingData == null) throw new ArgumentNullException(nameof(greetingData));

            client.Connect(address, port);
            this.port = port;
            this.address = address;
            IPEndPoint = new IPEndPoint(address, port);
            ConnectionProcess(greetingData);
        }

        protected virtual void ConnectionProcess(byte[] greetingData) =>
            Process_client_side(greetingData, cancel.Token);

        /// <summary>
        /// 클라이언트를 종료합니다.
        /// 종료를 확인합니다.
        /// </summary>
        public void Close() =>
            Dispose();

        /// <summary>
        /// 서버 주소
        /// </summary>
        /// <exception cref="NotSupportedException" />
        public IPAddress Address => address ?? throw new NotSupportedException("주소가 없습니다.");

        /// <summary>
        /// 서버 포트
        /// </summary>
        /// <exception cref="NotSupportedException" />
        public int Port => port != -1 ? port : throw new NotSupportedException("주소가 없습니다.");

        /// <summary>
        /// IP 주소
        /// </summary>
        public IPEndPoint IPEndPoint { get; private set; }

        /// <summary>
        /// 세션 정보
        /// </summary>
        public TraSessionInfo SessionInfo { get; } = new();

        /// <summary>
        /// 서버측 여부
        /// </summary>
        public bool IsServerSide { get; }

        /// <summary>
        /// 클라이언트측 여부
        /// </summary>
        public bool IsClientSide { get; }

        /// <summary>
        /// 버퍼 크기
        /// </summary>
        public int BufferSize { get; }

        /// <summary>
        /// 연결된
        /// </summary>
        public bool IsConnected => isConnected && (client?.Connected == true);

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    try {
                        if (cancel?.IsCancellationRequested == false)
                            cancel?.Cancel();

                        stream?.Dispose();

                        try {
                            Disconnect();
                        } catch {

                        }

                        client?.Close();
                        cancel?.Dispose();
                    } catch {

                    }
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
