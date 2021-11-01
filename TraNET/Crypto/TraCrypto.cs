namespace TraNET {
    /// <summary>
    /// Tra 암호화
    /// </summary>
    public static class TraCrypto {
        internal static readonly Random Random = new();
        internal static readonly SHA256 SHA256 = SHA256.Create();

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

        /// <summary>
        /// RSA를 만듭니다.
        /// </summary>
        /// <param name="priKey">공개키</param>
        /// <param name="pubKey">비공개 키</param>
         public static RSACryptoServiceProvider GenerateRSA(out byte[] priKey, out byte[] pubKey) {
            var rsa = new RSACryptoServiceProvider(2048);
            priKey = rsa.ExportRSAPrivateKey();
            pubKey = rsa.ExportSubjectPublicKeyInfo();
            return rsa;
        }
    }
}
