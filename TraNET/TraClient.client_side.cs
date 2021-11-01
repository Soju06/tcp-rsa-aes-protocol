using System.Text;

namespace TraNET {
    partial class TraClient {
        protected virtual void Process_client_side(byte[] vef_buf, CancellationToken token) {
            if (!client.Connected) throw new ArgumentException("클라이언트와 연결되어있지 않습니다.");

            token.ThrowIfCancellationRequested();
            stream = client.GetStream();

            var receive = new byte[1024];
            byte vef_swap = TraCrypto.Random.NextByte();
            int count = -1, index = 0;
            byte[] vef_hash;

            {
                #region 인사

                #region 클라이언트 정보 송신

                stream.SendPacket(
                    TraStatusCode.Greeting,
                    (SessionInfo.Info_1F = new TraServerInfo_1f(
                        vef_swap,
                        TraNet.ProtocolVersion,
                        vef_buf
                    )).Create()
                );

                #endregion

                token.ThrowIfCancellationRequested();

                #region 서버 정보 수신

                count = stream.Receive(receive);

                // 차단 확인
                if (receive[0] == (byte)TraStatusCode.Block)
                    ThrowCancelException(count, receive, "대상이 클라이언트를 차단했습니다.");

                EnsureDataLength(ref index, ref count, receive, TraStatusCode.Greeting);

                // 서버 정보
                var info = TraServerInfo_1f.Parse(receive, index, count);
                // 스왑 번호
                if (info.SwapNumber != vef_swap) ThrowViolationPacketException(TraStatusCode.Greeting);
                // 서버 인포에 푸시
                SessionInfo.Info_1F = info;

                #endregion

                token.ThrowIfCancellationRequested();

                #endregion
            }

            {
                #region 암호화

                using (var rsa = TraCrypto.GenerateRSA(out _, out var pub_key)) {
                    #region RSA 송신

                    // 스왑
                    TraBuffer.SwapBuffer(pub_key, vef_swap, false);
                    // 송신
                    stream.SendPacket(TraStatusCode.RSAEncryption, pub_key);

                    #endregion

                    token.ThrowIfCancellationRequested();

                    #region AES 수신

                    count = stream.Receive(receive);
                    EnsureDataLength(ref index, ref count, receive, TraStatusCode.AESEncryption);
                    var aes_key = rsa.Decrypt(TraBuffer.Section(receive, index, count), false);
                    TraBuffer.SwapBuffer(aes_key, vef_swap, true);
                    aes = TraAES.Create(aes_key);

                    #endregion

                    token.ThrowIfCancellationRequested();

                    #region AES 확인 송신

                    stream.Send(TraPacket.CombinePacket(TraStatusCode.SessionInfo,
                        aes.Encrypt(vef_hash = TraCrypto.GetSHA256Hash(new[] { vef_swap }), out var tag), tag));

                    #endregion

                    token.ThrowIfCancellationRequested();
                }

                #endregion
            }
            
            {
                #region 2차 서버 정보 및 최종 데이터 스왑 번호

                #region 서버 정보 수신

                count = stream.Receive(receive);
                EnsureDataLength(ref index, ref count, receive, TraStatusCode.SessionInfo);
                TraPacket.SplitAES(receive, index, count, out var tag, out var data);
                var serverInfo = SessionInfo.Info_2F = TraServerInfo_2f.Parse(aes.Decrypt(data, tag));

                #endregion

                token.ThrowIfCancellationRequested();

                #region 준비됨 송신

                TraBuffer.SwapBuffer(vef_hash, serverInfo.TagSwapNumber, false);
                stream.Send(TraPacket.CombinePacket(TraStatusCode.SessionOK,
                    aes.Encrypt(vef_hash, out tag), tag));

                #endregion

                token.ThrowIfCancellationRequested();

                #endregion
            }

            isConnected = true;
        }
    }
}
