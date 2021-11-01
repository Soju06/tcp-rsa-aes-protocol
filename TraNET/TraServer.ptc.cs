namespace TraNET {
    partial class TraServer {
        protected List<TraClient> clients = new();

        public int SessionCount => clients.Count;

        protected virtual async Task Process(TcpClient client) {
            TraClient? tc = null;
            try {
                tc = new TraClient(client);
                await tc.Process_server_side(this, new TRACLIENTINFO(greetingData, BufferSize), default);
                if (!tc.IsConnected) throw new ProtocolInitializationException("초기화에 실패했습니다.");
                clients.Add(tc);
            } catch {
                client.Close();
                tc?.Dispose();
                return;
            }

            OnSessionReady?.Invoke(this, new(tc));
        }

        /// <summary>
        /// 접속 관리
        /// </summary>
        protected virtual void ConnectionManage() {
            var token = cancel.Token;
            while (!token.IsCancellationRequested) {
                clients
            }
        }
    }
}
