namespace TraNET {
    /// <summary>
    /// 트-라 버퍼
    /// </summary>
    public static class TraBuffer {
        /// <summary>
        /// 버퍼의 일부만 복제합니다.
        /// </summary>
        /// <param name="data">데이터</param>
        /// <param name="index">위치</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static byte[] Section(byte[] data, int index) {
            if (data == null) throw new ArgumentNullException("data");
            var length = data.Length;
            if (length - 1 <= index) throw new ArgumentOutOfRangeException("index", "섹션의 최소 크기는 1 이상이여야 합니다.");
            var buf = new byte[length - index];
            Buffer.BlockCopy(data, index, buf, 0, buf.Length);
            return buf;
        }

        /// <summary>
        /// 버퍼의 일부만 복제합니다.
        /// </summary>
        /// <param name="data">데이터</param>
        /// <param name="index">위치</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static byte[] Section(byte[] data, int index, int count) {
            if (data == null) throw new ArgumentNullException("data");
            if (count < 1) throw new ArgumentOutOfRangeException("count", "최소 갯수는 1 이상이여야 합니다.");
            var length = data.Length;
            if (length - count < index) throw new ArgumentOutOfRangeException("index", $"섹션의 최소 크기는 {count} 이상이여야 합니다.");
            var buf = new byte[count];
            Buffer.BlockCopy(data, index, buf, 0, buf.Length);
            return buf;
        }

        /// <summary>
        /// 쉬프트 버퍼
        /// </summary>
        /// <param name="head">헤더</param>
        /// <param name="data">데이터</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void ShiftBuffer(TraStatusCode head, ref byte[] data) {
            if (data == null) throw new ArgumentNullException("data");
            Array.Resize(ref data, data.Length + 1);
            for (int i = data.Length - 1; i > 0; i--)
                data[i] = data[i - 1];
            data[0] = (byte)head;
        }

        /// <summary>
        /// 버퍼가 일치하는지 여부를 가져옵니다.
        /// </summary>
        /// <param name="buf1">대상 1</param>
        /// <param name="buf2">대상 2</param>
        /// <param name="buf1Count">대상 1의 대체 길이</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool EqualBuffer(byte[] buf1, byte[] buf2, int? buf1Count = null) {
            if (buf1 == null) throw new ArgumentNullException("buf1");
            if (buf2 == null) throw new ArgumentNullException("buf2");
            buf1Count ??= buf1.Length;
            if (buf1Count != buf2.Length) return false;
            for (int i = 0; i < buf1Count; i++)
                if (buf1[i] != buf2[i]) return false;
            return true;
        }

        /// <summary>
        /// 버퍼를 swap로 바이너리 합을 구합니다.
        /// </summary>
        /// <param name="buf">대상</param>
        /// <param name="swap">숫자</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void SwapBuffer(byte[] buf, int swap, bool reverse) {
            if (buf == null) throw new ArgumentNullException("buf");
            for (int i = 0; i < buf.Length; i++)
                buf[i] = unchecked((byte)(buf[i] + ((reverse ? -1 : +1) * (swap + i))));
        }

        /// <summary>
        /// 버퍼를 자릅니다.
        /// </summary>
        /// <param name="source">소스</param>
        /// <param name="pos">buf2 위치</param>
        /// <param name="buf1">버퍼 1</param>
        /// <param name="buf2">버퍼 2</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static void Split(byte[] source, int pos, out byte[] buf1, out byte[] buf2) {
            if (source == null) throw new ArgumentNullException("source");
            var length = source.Length;
            if (length - 1 < pos) throw new ArgumentOutOfRangeException("pos");
            int size2 = length - pos, size1 = length - size2;
            buf1 = new byte[size1];
            buf2 = new byte[size2];
            Buffer.BlockCopy(source, 0, buf1, 0, buf1.Length);
            Buffer.BlockCopy(source, pos, buf2, 0, buf2.Length);
        }

        /// <summary>
        /// 버퍼를 합칩니다.
        /// </summary>
        /// <param name="buf1">버퍼 1</param>
        /// <param name="buf2">버퍼 2</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static byte[] Combine(byte[] buf1, byte[] buf2) {
            if (buf1 == null) throw new ArgumentNullException("buf1");
            if (buf2 == null) throw new ArgumentNullException("buf2");
            int l1 = buf1.Length, l2 = buf2.Length;
            var buf = new byte[l1 + l2];
            Buffer.BlockCopy(buf1, 0, buf, 0, l1);
            Buffer.BlockCopy(buf2, 0, buf, l1, l2);
            return buf;
        }
    }
}
