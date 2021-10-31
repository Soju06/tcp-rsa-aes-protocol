using System;
using System.Collections.Generic;
using System.Text;

namespace TraNET {
    /// <summary>
    /// 트=라 클라이언트
    /// </summary>
    partial class TraClient {
        internal TraClient(TcpClient client) {
            this.client = client ?? throw new ArgumentNullException("client");
        }

        protected virtual async Task Process_server_side(TRACLIENTINFO traInfo, CancellationToken token) {
            if (!client.Connected) throw new ArgumentException("클라이언트와 연결되어있지 않습니다.");
            stream = client.GetStream();

            var vef_buf = traInfo._greetingData.ToArray();
            var vef_swap = TraNET.Random.NextByte();
            var receive = new byte[1024];
            int count = -1;

            // 인사
            {
                await SendPacket(TraStatusCode.Greeting,
                    (ServerInfo.Info_1F = new TraServerInfo_1f(
                        vef_swap,
                        TraNET.ProtocolVersion,
                        traInfo._greetingData
                    )).Create()
                );

                stream.ReadTimeout = 10000;
                await Receive();
                EnsureDataLength(TraStatusCode.Greeting);
                stream.ReadTimeout = Timeout.Infinite;

                var clientInfo = TraServerInfo_1f.Parse(receive);

                // 스왑 번호 일치
                if (clientInfo.SwapNumber != vef_swap)
                    throw new ProtocolMismatchException("대상이 올바르지 않은 패킷을 송신했습니다.");

                // 지원 프로토콜 버전
                if (!TraVersion.CanSupport(TraNET.ProtocolVersion, clientInfo.ProtocolVersion))
                    throw new UnsupportedVersionException("대상 프로토콜 버전을 지원하지 않습니다.");

                // 인증 스왑
                TraBuffer.SwapVefBuffer(vef_buf, vef_swap);

                // 이름 일치
                if (!TraBuffer.EqualBuffer(vef_buf, clientInfo.ProtocolName.ToArray())) 
                    throw new ProtocolMismatchException("프로토콜 이름이 일치하지 않습니다.");


            }

            void EnsureDataLength(TraStatusCode statusCode) {
                if (count <= 0) throw new ProtocolInitializationException(
                    $"{statusCode} task, 수신된 데이터가 데이터가 없습니다.");
            }

            async Task Receive() {
                count = await stream.ReadAsync(receive, token);
            }

            async Task SendByte(byte byt) {
                stream.WriteByte(byt);
                await stream.FlushAsync(token);
            }

            async Task Send(byte[] buf) {
                await stream.WriteAsync(buf, token);
                await stream.FlushAsync(token);
            }
            
            async Task SendPacket(TraStatusCode status, byte[] buf) {
                TraBuffer.ShiftBuffer(status, ref buf);
                await stream.WriteAsync(buf, token);
                await stream.FlushAsync(token);
            }
        }
    }
}
