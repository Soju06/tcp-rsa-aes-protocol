using System;
using System.Collections.Generic;
using System.Text;

namespace TraNET {
    /// <summary>
    /// 트라 클-라이언트
    /// </summary>
    public partial class TraClient : IDisposable {
        IPAddress address;
        int port = -1;
        TcpClient client;
        NetworkStream stream;
        CancellationTokenSource cancel;
        protected bool disposedValue;

        public TraClient() {
            client = new();
            cancel = new();
        }

        /// <summary>
        /// 서버에 연결합니다.
        /// </summary>
        /// <param name="hostName">호스트</param>
        /// <param name="port">포트</param>
        /// <param name="greetingText">인사 데이터</param>
        public void Connect(string hostName, int port, string greetingText) =>
            Connect(hostName, port, TraNET.GetSHA256Hash(Encoding.UTF8.GetBytes(greetingText)));


        /// <summary>
        /// 서버에 연결합니다.
        /// </summary>
        /// <param name="hostName">호스트</param>
        /// <param name="port">포트</param>
        /// <param name="greetingData">인사 데이터</param>
        public void Connect(string hostName, int port, byte[] greetingData) {
            client.Connect(hostName, port);
            var endPoint = (IPEndPoint)client.Client.RemoteEndPoint;
            this.port = port;
            address = endPoint.Address;
            ConnectionProcess(greetingData);
        }

        /// <summary>
        /// 서버에 연결합니다.
        /// </summary>
        /// <param name="endPoint">끝점</param>
        /// <param name="greetingText">인사 데이터</param>
        public void Connect(IPEndPoint endPoint, string greetingText) =>
            Connect(endPoint, TraNET.GetSHA256Hash(Encoding.UTF8.GetBytes(greetingText)));

        /// <summary>
        /// 서버에 연결합니다.
        /// </summary>
        /// <param name="endPoint">끝점</param>
        /// <param name="greetingData">인사 데이터</param>
        public void Connect(IPEndPoint endPoint, byte[] greetingData) { 
            client.Connect(endPoint);
            port = endPoint.Port;
            address = endPoint.Address;
            ConnectionProcess(greetingData);
        }

        /// <summary>
        /// 서버에 연결합니다.
        /// </summary>
        /// <param name="address">주소</param>
        /// <param name="port">포트</param>
        /// <param name="greetingText">인사 데이터</param>
        public void Connect(IPAddress address, int port, string greetingText) =>
            Connect(address, port, TraNET.GetSHA256Hash(Encoding.UTF8.GetBytes(greetingText)));

        /// <summary>
        /// 서버에 연결합니다.
        /// </summary>
        /// <param name="address">주소</param>
        /// <param name="port">포트</param>
        /// <param name="greetingData">인사 데이터</param>
        public void Connect(IPAddress address, int port, byte[] greetingData) { 
            client.Connect(address, port);
            this.port = port;
            this.address = address;
            ConnectionProcess(greetingData);
        }

        protected virtual void ConnectionProcess(byte[] greetingData) {
            Process_client_side(greetingData, cancel.Token).Wait();
        }

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
        /// 서버 정보
        /// </summary>
        public TraServerInfo ServerInfo { get; } = new();

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    try {
                        if (cancel?.IsCancellationRequested == false) {
                            cancel?.Cancel();
                        } 

                        stream?.Dispose();
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
