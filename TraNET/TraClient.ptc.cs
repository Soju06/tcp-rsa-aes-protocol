namespace TraNET {
    partial class TraClient {
        /// <summary>
        /// 연결을 끊습니다.
        /// </summary>
        protected virtual void Disconnect() =>
            Disconnect(0);

        /// <summary>
        /// 연결을 끊습니다.
        /// </summary>
        /// <param name="code">코드</param>
        protected virtual void Disconnect(int code) =>
            SendMessage(TraStatusCode.Cancel, 0);
    }
}
