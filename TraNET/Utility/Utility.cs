namespace TraNET {
    internal static class Utility {
        public static byte NextByte(this Random random) {
            var buf = new byte[1];
            random.NextBytes(buf);
            return buf[0];
        }

        public static byte[] NextBytes(this Random random, int length) {
            var buf = new byte[length];
            random.NextBytes(buf);
            return buf;
        }
    }
}
