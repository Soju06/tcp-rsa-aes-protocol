namespace TraNET {
    partial class TraServer {
        protected List<TraClient> clients = new();

        /// <summary>
        /// 세션 수
        /// </summary>
        public int SessionCount => clients.Count;

        /// <summary>
        /// 프로토콜 이름이 같은지 확인합니다.
        /// </summary>
        public bool ProtocolNameMatch { get; set; }

        protected virtual async Task Process(TcpClient client) {
            TraClient? tc = null;
            IPEndPoint? ip = null;
            try {
                tc = new TraClient(client);
                ip = new(tc.Address, tc.Port);
                
                await tc.Process_server_side(this, new TRACLIENTINFO(greetingData, BufferSize, ProtocolNameMatch), default);
                
                if (!tc.IsConnected) throw new ProtocolInitializationException("초기화에 실패했습니다.");
                
                clients.Add(tc);
                OnSessionReady?.Invoke(this, new(tc));
            } catch (ProtocolViolationException ex) {
                RaiseSessionDisconnectedEvent(ip, SessionDisconnectedStatusCode.ProtocolViolation, 0, ex.Message);
            } catch (ProtocolMismatchException ex) {
                RaiseSessionDisconnectedEvent(ip, SessionDisconnectedStatusCode.ProtocolMismatch, 0, ex.Message);
            } catch (ProtocolInitializationException ex) {
                RaiseSessionDisconnectedEvent(ip, SessionDisconnectedStatusCode.Disconnected, 0, ex.Message);
            } catch (ProtocolCancellationException ex) {
                RaiseSessionDisconnectedEvent(ip, SessionDisconnectedStatusCode.Canceled, ex.Code, ex.Message);
            } catch (Exception ex) {
                client.Close();
                tc?.Dispose();
                RaiseSessionDisconnectedEvent(ip, SessionDisconnectedStatusCode.InternalError, 0, ex.ToString());
            }
        }

        /// <summary>
        /// 접속 관리
        /// </summary>
        protected virtual void ConnectionManage() {
            var token = cancel.Token;
            while (!token.IsCancellationRequested) {
                try {
                    foreach (var item in clients) {
                        if (item != null && item.IsConnected != true) {
                            item.Close();
                            clients.Remove(item);
                            RaiseSessionDisconnectedEvent(item.IPEndPoint, 
                                SessionDisconnectedStatusCode.Disconnected, 0, "연결이 끊겼습니다.");
                        }
                    }
                } catch {

                }
                Thread.Sleep(500);
            }
        }
    }
}
