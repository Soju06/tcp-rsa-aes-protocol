using System;
using System.Collections.Generic;
using System.Text;

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
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static byte[] Section(byte[] data, int index) {
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
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static byte[] Section(byte[] data, int index, int count) {
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
        public static void ShiftBuffer(TraStatusCode head, ref byte[] data) {
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
        public static bool EqualBuffer(byte[] buf1, byte[] buf2, int? buf1Count = null) {
            buf1Count ??= buf1.Length;
            if (buf1Count != buf2.Length) return false;
            for (int i = 0; i < buf1Count; i++)
                if (buf1[i] != buf2[i]) return false;
            return true;
        }

        /// <summary>
        /// vef 버퍼를 swap로 바이너리 합을 구합니다.
        /// </summary>
        /// <param name="buf">대상</param>
        /// <param name="swap">숫자</param>
        public static void SwapVefBuffer(byte[] buf, byte swap) {
            for (int i = 0; i < buf.Length; i++)
                buf[i] = unchecked((byte)(buf[i] + (swap + i)));
        }
    }
}
