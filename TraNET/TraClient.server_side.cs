namespace TraNET {
    /// <summary>
    /// 트=라 클라이언트
    /// </summary>
    partial class TraClient {
        internal TraClient(TcpClient client) {
            IsServerSide = true;
            this.client = client ?? throw new ArgumentNullException("client");
            var endPoint = (IPEndPoint)client.Client.RemoteEndPoint;
            address = endPoint.Address;
            port = endPoint.Port;
            IPEndPoint = endPoint;
        }

        internal virtual async Task Process_server_side(TraServer server, TRACLIENTINFO traInfo, CancellationToken token) {
            if (!client.Connected) throw new ArgumentException("클라이언트와 연결되어있지 않습니다.");
            stream = client.GetStream();

            var vef_buf = traInfo._greetingData.ToArray();
            var receive = new byte[1024];
            int count = -1, index = 0, bufferSize = traInfo._bufferSize;
            byte vef_swap;
            byte[] vef_hash;

            stream.ReadTimeout = 10000;

            {
                #region 인사

                #region 클라이언트 정보 수신

                count = await stream.ReceiveAsync(receive, token);
                EnsureDataLength(ref index, ref count, receive, TraStatusCode.Greeting);

                var clientInfo = TraServerInfo_1f.Parse(receive, index, count);

                #endregion

                #region 가용성 확인

                // 지원 프로토콜 버전
                if (!TraVersion.CanSupport(TraNet.ProtocolVersion, clientInfo.ProtocolVersion)) {
                    await SendMessageAsync(TraStatusCode.NotSupport, 3, token);
                    throw new UnsupportedVersionException("3 대상 프로토콜 버전을 지원하지 않습니다.");
                }

                // 이름 일치
                if (traInfo._protocolNameMatch && !TraBuffer.EqualBuffer(vef_buf, clientInfo.ProtocolName.ToArray())) {
                    await SendMessageAsync(TraStatusCode.NotSupport, 1, token);
                    throw new ProtocolMismatchException("1 프로토콜 이름이 일치하지 않습니다.");
                }

                // 만약 차단되면
                if (server.RaiseConnectionEvent(IPEndPoint, 
                    clientInfo.ProtocolVersion, clientInfo.ProtocolName, out var blockCode)) {
                    await SendMessageAsync(TraStatusCode.Block, blockCode, token);
                }

                SessionInfo.Info_1F = clientInfo;
                vef_swap = clientInfo.SwapNumber;

                #endregion

                #region 서버 정보 송신

                await stream.SendPacketAsync(
                    TraStatusCode.Greeting,
                    (SessionInfo.Info_1F = new TraServerInfo_1f(
                        vef_swap,
                        TraNet.ProtocolVersion,
                        vef_buf
                    )).Create(),
                    token
                );

                #endregion

                #endregion
            }

            {
                #region 암호화

                using (var rsa = new RSACryptoServiceProvider(2048)) {
                    #region RSA 수신

                    count = await stream.ReceiveAsync(receive, token);
                    EnsureDataLength(ref index, ref count, receive, TraStatusCode.RSAEncryption);

                    var pub_key = TraBuffer.Section(receive, index, count);
                    TraBuffer.SwapBuffer(pub_key, vef_swap, true);

                    rsa.ImportSubjectPublicKeyInfo(pub_key, out _);

                    #endregion

                    #region AES 송신

                    // aes 생성
                    aes = TraAES.Create();
                    var aes_key = aes.Export();
                    // 스왑
                    TraBuffer.SwapBuffer(aes_key, vef_swap, false);
                    // 송신
                    await stream.SendPacketAsync(TraStatusCode.AESEncryption, 
                        rsa.Encrypt(aes_key, false), token);

                    #endregion

                }

                #region AES 확인 수신

                count = await stream.ReceiveAsync(receive, token);
                EnsureDataLength(ref index, ref count, receive, TraStatusCode.SessionInfo);
                TraPacket.SplitAES(receive, index, count, out var tag, out var data);

                if (!TraBuffer.EqualBuffer(vef_hash = TraCrypto.GetSHA256Hash(new[] { vef_swap }), aes.Decrypt(data, tag))) {
                    await SendMessageAsync(TraStatusCode.Cancel, 13, token);
                    throw new ProtocolViolationException("13 대상이 암호화 인증을 실패하였습니다.");
                }

                #endregion

                #endregion
            }

            {
                #region 2차 서버 정보 및 최종 데이터 스왑 번호

                #region 2차 서버 정보 송신

                server.RaiseSessionStartEvent(IPEndPoint, ref bufferSize, out var state);

                await stream.SendAsync(TraPacket.CombinePacket(TraStatusCode.SessionInfo,
                    aes.Encrypt((SessionInfo.Info_2F = new TraServerInfo_2f(
                        bufferSize, tagSwapNumber = TraCrypto.Random.NextByte(), state)).Create(),
                            out var tag), tag), token);

                #endregion

                #region 준비됨 수신

                count = await stream.ReceiveAsync(receive, token);
                EnsureDataLength(ref index, ref count, receive, TraStatusCode.SessionOK);
                TraPacket.SplitAES(receive, index, count, out tag, out var data);
                TraBuffer.SwapBuffer(vef_hash, tagSwapNumber, false);

                if (!TraBuffer.EqualBuffer(vef_hash, aes.Decrypt(data, tag))) {
                    await SendMessageAsync(TraStatusCode.Cancel, 17, token);
                    throw new ProtocolViolationException("대상이 준비 상태 인증을 실패하였습니다.");
                }

                #endregion

                #endregion
            }

            stream.ReadTimeout = Timeout.Infinite;
            isConnected = true;
        }
    }
}
