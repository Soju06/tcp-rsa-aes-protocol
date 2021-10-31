namespace TraNET {
    /// <summary>
    /// 트-라 버전
    /// </summary>
    public static class TraVersion {
        /// <summary>
        /// 상호 버전 지원 여부.
        /// 라이브러리 버전에 따라 지원 여부 판단이 달라질 수 있습니다.
        /// </summary>
        /// <param name="target1">버전1</param>
        /// <param name="target2">버전2</param>
        public static bool CanSupport(uint target1, uint target2) {
            byte[] t1 = BitConverter.GetBytes(target1), t2 = BitConverter.GetBytes(target2);
            // 주 버전
            if (t1[0] != t2[0]) return false;
            // 부 버전
            if (t1[1] != t2[1]) return false;
            // 빌드 버전
            // 수정 번호
            return true;
        }
    }
}
