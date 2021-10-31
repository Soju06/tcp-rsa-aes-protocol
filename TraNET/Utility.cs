namespace TraNET {
    internal static class Utility {
        public static byte NextByte(this Random random) {
            var buf = new byte[1];
            random.NextBytes(buf);
            return buf[0];
        }
    }
}
