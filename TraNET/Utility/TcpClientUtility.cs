namespace TraNET {
    internal static class TcpClientUtility {
        public static async Task SendAsync(this NetworkStream stream, byte[] buf, CancellationToken token = default) {
            await stream.WriteAsync(buf, token);
            await stream.FlushAsync(token);
        }

        public static void Send(this NetworkStream stream, byte[] buf) {
            stream.Write(buf);
            stream.Flush();
        }

        public static async Task SendAsync(this NetworkStream stream, byte value, CancellationToken token = default) {
            stream.WriteByte(value);
            await stream.FlushAsync(token);
        }
        
        public static async Task SendPacketAsync(this NetworkStream stream, TraStatusCode statusCode, byte[] buf, CancellationToken token = default) {
            TraBuffer.ShiftBuffer(statusCode, ref buf);
            await stream.WriteAsync(buf, token);
            await stream.FlushAsync(token);
        }
        
        public static void SendPacket(this NetworkStream stream, TraStatusCode statusCode, byte[] buf) {
            TraBuffer.ShiftBuffer(statusCode, ref buf);
            stream.Write(buf);
            stream.Flush();
        }

        public static async Task<int> ReceiveAsync(this NetworkStream stream, byte[] buf, CancellationToken token = default) =>
            await stream.ReadAsync(buf, token);

        public static int Receive(this NetworkStream stream, byte[] buf) =>
            stream.Read(buf);
    }
}
