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

        /// <summary>
        /// 서버가 바쁨
        /// </summary>
        ServerIsBusy = 201,
    }
}
