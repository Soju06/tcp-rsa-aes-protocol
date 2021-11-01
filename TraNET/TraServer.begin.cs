﻿namespace TraNET {
    partial class TraServer {
        /// <summary>
        /// 클라이언트가 연결되었습니다.
        /// </summary>
        public event ConnectionEventHandler OnConnection;

        /// <summary>
        /// 세션이 시작되었습니다.
        /// </summary>
        public event SessionStartEventHandler OnSessionStart;

        /// <summary>
        /// 세션이 준비되었습니다.
        /// </summary>
        public event SessionReadyEventHandler OnSessionReady;

        internal virtual bool RaiseConnectionEvent(IPEndPoint endPoint, uint protocolVersion, 
            ReadOnlyMemory<byte> protocolName, out int blockCode) {
            blockCode = 0;
            
            if (OnConnection == null) return false;

            var e = new ConnectionEventArgs(endPoint, protocolVersion, protocolName);
            OnConnection.Invoke(this, e);

            var (cBlock, cCode) = e;
            blockCode = cCode;
            return cBlock;
        }

        internal virtual void RaiseSessionStartEvent(IPEndPoint endPoint, ref int bufferSize, out byte[]? state) {
            state = null;
            if (OnSessionStart == null) return;

            var e = new SessionInfoEventArgs(endPoint, bufferSize);
            OnSessionStart.Invoke(this, e);

            var (cBufferSize, cState) = e;
            bufferSize = cBufferSize;
            state = cState;
        }
    }
}
