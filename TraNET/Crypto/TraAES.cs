namespace TraNET {
    /// <summary>
    /// 트-라 AES
    /// </summary>
    public class TraAES : IDisposable {
        /// <summary>
        /// AES 키 길이
        /// </summary>
        public static readonly int AES_KeyLength = 32;
        /// <summary>
        /// AES 논스 길이
        /// </summary>
        public static readonly int AES_NonceLength = 12;
        /// <summary>
        /// AES 테그 길이
        /// </summary>
        public static readonly int AES_TagLength = 16;

        protected bool disposedValue;
        protected AesGcm aes;
        protected byte[] key, nonce;

        protected TraAES(byte[] key, byte[] nonce) {
            if (key.Length != AES_KeyLength) throw new ArgumentException($"키 길이는 {AES_KeyLength}여야 합니다.", "key");
            if (nonce.Length != AES_NonceLength) throw new ArgumentException($"키 길이는 {AES_NonceLength}여야 합니다.", "nonce");
            aes = new AesGcm(key);
            this.key = key;
            this.nonce = nonce;
        }

        /// <summary>
        /// 암호화 합니다.
        /// </summary>
        /// <param name="plaintext">평문</param>
        /// <param name="tag">테그</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public byte[] Encrypt(byte[] plaintext, out byte[] tag) {
            if (disposedValue) throw new ObjectDisposedException("this");
            if (plaintext == null) throw new ArgumentNullException("plaintext");
            if (plaintext.Length < 1) throw new ArgumentException("plaintext", "데이터의 최소 크기는 1 이상이여야 합니다.");
            var ciphertext = new byte[plaintext.Length];
            tag = new byte[16];
            aes.Encrypt(nonce, plaintext, ciphertext, tag);
            return ciphertext;
        }

        /// <summary>
        /// 암호화 합니다.
        /// </summary>
        /// <param name="plaintext">평문</param>
        /// <param name="tag">테그</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public byte[] Encrypt(byte[] plaintext, int index, int count, out byte[] tag) {
            if (disposedValue) throw new ObjectDisposedException("this");
            if (plaintext == null) throw new ArgumentNullException("plaintext");
            var length = plaintext.Length;
            if (length < 1) throw new ArgumentException("plaintext", "데이터의 최소 크기는 1 이상이여야 합니다.");
            if (count < 1) throw new ArgumentOutOfRangeException("count", "최소 갯수는 1 이상이여야 합니다.");
            if (length < count) throw new ArgumentOutOfRangeException("count", "갯수보다 데이터 길이가 짧습니다.");
            if (length - count < index) throw new ArgumentOutOfRangeException("index", $"대상의 최소 크기는 {count} 이상이여야 합니다.");
            var ciphertext = new byte[plaintext.Length];
            tag = new byte[16];
            aes.Encrypt(nonce, plaintext, ciphertext, tag);
            return ciphertext;
        }

        /// <summary>
        /// 복호화 합니다.
        /// </summary>
        /// <param name="ciphertext">암호문</param>
        /// <param name="tag">테그</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public byte[] Decrypt(byte[] ciphertext, byte[] tag) {
            if (disposedValue) throw new ObjectDisposedException("this");
            if (ciphertext == null) throw new ArgumentNullException("ciphertext");
            if (tag == null) throw new ArgumentNullException("tag");
            var plaintext = new byte[ciphertext.Length];
            aes.Decrypt(nonce, ciphertext, tag, plaintext);
            return plaintext;
        }

        /// <summary>
        /// 복호화 합니다.
        /// </summary>
        /// <param name="ciphertext">암호문</param>
        /// <param name="tag">테그</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public byte[] Decrypt(byte[] ciphertext, byte[] tag, int index, int count) {
            if (disposedValue) throw new ObjectDisposedException("this");
            if (ciphertext == null) throw new ArgumentNullException("ciphertext");
            if (tag == null) throw new ArgumentNullException("tag");
            var length = ciphertext.Length;
            if (length < 1) throw new ArgumentException("plaintext", "데이터의 최소 크기는 1 이상이여야 합니다.");
            if (count < 1) throw new ArgumentOutOfRangeException("count", "최소 갯수는 1 이상이여야 합니다.");
            if (length < count) throw new ArgumentOutOfRangeException("count", "갯수보다 데이터 길이가 짧습니다.");
            if (length - count < index) throw new ArgumentOutOfRangeException("index", $"대상의 최소 크기는 {count} 이상이여야 합니다.");
            var plaintext = new byte[length];
            aes.Decrypt(nonce, ciphertext, tag, plaintext);
            return plaintext;
        }

        /// <summary>
        /// AES 키를 내보냅니다.
        /// </summary>
        /// <exception cref="ObjectDisposedException"></exception>
        public byte[] Export() {
            if (disposedValue) throw new ObjectDisposedException("this");
            return TraBuffer.Combine(key, nonce);
        }

        /// <summary>
        /// 트-라 AES
        /// </summary>
        public static TraAES Create() =>
            new(TraCrypto.Random.NextBytes(AES_KeyLength), 
                TraCrypto.Random.NextBytes(AES_NonceLength)
            );

        /// <summary>
        /// 트-라 AES
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static TraAES Create(byte[] key, byte[] nonce) =>
            new(key, nonce);

        /// <summary>
        /// 트-라 AES
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static TraAES Create(byte[] pkey) {
            TraBuffer.Split(pkey, AES_KeyLength, out var key, out var nonce);
            return new(key, nonce);
        }

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    aes?.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose() {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
