namespace TraNET {
    // 0 ~ 99 데이터 관련
    // 100 ~ 199 프로토콜 관련
    // 200 ~ 255 오류 코드
    /// <summary>
    /// 트-라 상태 코드
    /// </summary>
    public enum TraStatusCode : byte {
        /// <summary>
        /// 핼로!
        /// </summary>
        Greeting = 100,
        Block = 104,
        /// <summary>
        /// RSA 공개키
        /// </summary>
        RSAEncryption = 112,
        /// <summary>
        /// AES 공개키
        /// </summary>
        AESEncryption = 118,
        /// <summary>
        /// 세션 정보
        /// </summary>
        SessionInfo = 122,
        SessionOK = 129,

        /// <summary>
        /// 서버가 바쁨
        /// </summary>
        ServerIsBusy = 201,
        /// <summary>
        /// 내부 서버 오류
        /// </summary>
        InternalServerError = 210,
        /// <summary>
        /// 지원하지 않음
        /// </summary>
        NotSupport = 250,
        /// <summary>
        /// 일방적 취소 메시지입니다.
        /// </summary>
        Cancel = 255,
    }
}
