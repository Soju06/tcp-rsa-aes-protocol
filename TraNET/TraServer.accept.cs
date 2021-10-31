namespace TraNET {
    partial class TraServer {
        /// <summary>
        /// 작업 대기열
        /// </summary>
        protected HashSet<Task> ProcessQuery = new();

        /// <summary>
        /// 비동기 접속
        /// </summary>
        protected virtual void BeginAccept(IAsyncResult result) {
            var client = listener.EndAcceptTcpClient(result);

            lock (ProcessQuery) {
                // 최대 접속 수를 넘으면 연결 끊음.
                if (ProcessQuery.Count >= MaxConnections)
                    EndConnection(client, TraStatusCode.ServerIsBusy);

                // 프로세스 시작
                var processTask = Process(client);
                processTask.ContinueWith(EndConnectionProcess, null, cancel.Token);
                ProcessQuery.Add(processTask);
            }
        }

        protected virtual void EndConnectionProcess(Task task, object state) {
            ProcessQuery.Remove(task);
            task?.Dispose();
        }

        /// <summary>
        /// 서버 상태코드 송신
        /// </summary>
        /// <param name="client">클라</param>
        /// <param name="statusCode">상태 코드</param>
        protected virtual void EndConnection(TcpClient client, TraStatusCode statusCode) {
            using (var stream = client.GetStream()) {
                stream.WriteByte((byte)statusCode);
                stream.Flush();
            }
            client.Close();
        }
    }
}
