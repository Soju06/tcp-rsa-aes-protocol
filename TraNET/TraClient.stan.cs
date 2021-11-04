namespace TraNET {
    partial class TraClient {
        protected byte tagSwapNumber;

        protected virtual void ThrowViolationPacketException(TraStatusCode statusCode) =>
                    throw new ProtocolViolationException($"11 대상이 올바르지 않은 패킷을 보냈습니다. status: {statusCode}");

        protected virtual void ThrowCancelException(int count, byte[] receive, string message) {
            if (count < 1 + sizeof(int)) throw new ProtocolCancellationException(message);
            else throw new ProtocolCancellationException(message, BitConverter.ToInt32(receive, 1));
        }

        protected virtual void EnsureDataLength(ref int index, ref int count, byte[] receive, TraStatusCode statusCode) {
            if (count <= 0) throw new ProtocolViolationException(
                $"{statusCode} task, 수신된 데이터가 데이터가 없습니다.");
            if (receive[0] == (byte)TraStatusCode.Cancel)
                ThrowCancelException(count, receive, "대상이 통신을 취소했습니다.");
            else if (receive[0] == (byte)TraStatusCode.NotSupport)
                ThrowCancelException(count, receive, "대상이 프로토콜을 지원하지 않습니다.");
            else if (IsClientSide && receive[0] == (byte)TraStatusCode.InternalServerError)
                ThrowCancelException(count, receive, "대상 서버에서 오류가 발생하였습니다.");
            else if (TraPacket.SplitHeader(receive, out index) != statusCode)
                ThrowViolationPacketException(statusCode);
            else count -= index;
        }

        protected async Task SendMessageAsync(TraStatusCode statusCode, int code, CancellationToken token) =>
            await stream.SendPacketAsync(statusCode, BitConverter.GetBytes(code), token);

        protected void SendMessage(TraStatusCode statusCode, int code) =>
            stream.SendPacket(statusCode, BitConverter.GetBytes(code));
    }
}
