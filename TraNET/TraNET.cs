using System.Text;

namespace TraNET {
    /// <summary>
    /// 트라-넷
    /// </summary>
    public static class TraNET {
        /// <summary>
        /// 프로토콜 버전
        /// </summary>
        public static uint ProtocolVersion = BitConverter.ToUInt32(new byte[] { 0, 1, 0, 0 });

        /// <summary>
        /// 트라넷 기본 프로토콜 이름
        /// </summary>
        public static readonly ReadOnlyMemory<byte> DefaultProtocolName = new byte[] { 
            0xfa, 0xa4, 0x1e, 0xed, 0xb5, 0xeb, 0x54, 0xad,
            0xf6, 0xab, 0x62, 0x95, 0xe0, 0xb1, 0x29, 0x49,
            0x90, 0x09, 0x6c, 0x80, 0x0f, 0xfa, 0x2a, 0x37,
            0xe7, 0x84, 0x54, 0xed, 0x27, 0xaf, 0xc4, 0xc5 
        };

        /// <summary>
        /// 기본 버퍼 사이즈
        /// </summary>
        public static readonly int DefaultBufferSize = 8192;

        internal static readonly SHA256 SHA256 = SHA256.Create();
        internal static readonly Random Random = new();

        /// <summary>
        /// sha256 해시
        /// </summary>
        /// <param name="data">데이터</param>
        /// <returns>32바이트</returns>
        /// <exception cref="ArgumentNullException" />
        internal static byte[] GetSHA256Hash(this byte[] data) {
            if (data == null) throw new ArgumentNullException("data");
            return SHA256.ComputeHash(data);
        }

        /// <summary>
        /// sha256 해시
        /// </summary>
        /// <param name="data">데이터</param>
        /// <returns>32바이트</returns>
        /// <exception cref="ArgumentNullException" />
        internal static byte[] GetSHA256Hash(this string data) {
            if (data == null) throw new ArgumentNullException("data");
            return GetSHA256Hash(Encoding.UTF8.GetBytes(data));
        }
    }
}
