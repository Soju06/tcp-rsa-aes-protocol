namespace TraNET {
    /// <summary>
    /// 트-라 패킷
    /// </summary>
    public static class TraPacket {
        /// <summary>
        /// 헤더를 자릅니다.
        /// </summary>
        /// <param name="buf">버퍼</param>
        /// <param name="index">헤더를 제외한 위치</param>
        /// <exception cref="ArgumentException"></exception>
        public static TraStatusCode SplitHeader(byte[] buf, out int index) {
            if (buf == null) throw new ArgumentNullException("buf");
            if (buf.Length < 1) throw new ArgumentException("패킷 길이는 0보다 커야합니다.");
            index = 1;
            return (TraStatusCode)buf[0];
        }

        /// <summary>
        /// 헤더를 자릅니다.
        /// </summary>
        /// <param name="buf">버퍼</param>
        /// <param name="index">헤더를 제외한 위치</param>
        /// <exception cref="ArgumentException"></exception>
        public static void SplitAES(byte[] buf, int index, int count, out byte[] tag, out byte[] data) {
            if (buf == null) throw new ArgumentNullException("buf");
            if (buf.Length < count) throw new ArgumentException("갯수는 버퍼 크기보다 작아야합니다.");
            if (count < TraAES.AES_TagLength) throw new ArgumentException($"패킷 길이는 {TraAES.AES_TagLength} 보다 커야합니다.");
            tag = new byte[TraAES.AES_TagLength];
            data = new byte[count - tag.Length];
            Buffer.BlockCopy(buf, index, tag, 0, tag.Length);
            if(count - TraAES.AES_TagLength > 0)
                Buffer.BlockCopy(buf, index + tag.Length, data, 0, count - tag.Length);
        }

        /// <summary>
        /// 패킷을 만듭니다.
        /// </summary>
        /// <param name="statusCode">상태 코드</param>
        /// <param name="buf">버퍼</param>
        /// <param name="tag">테그</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static byte[] CombinePacket(TraStatusCode statusCode, byte[] buf, byte[] tag) {
            if (buf == null) throw new ArgumentNullException("buf");
            if (tag == null) throw new ArgumentNullException("tag");
            var packet = TraBuffer.Combine(tag, buf);
            TraBuffer.ShiftBuffer(statusCode, ref packet);
            return packet;
        }

        /// <summary>
        /// 패킷을 만듭니다.
        /// </summary>
        /// <param name="statusCode">상태 코드</param>
        /// <param name="buf">버퍼</param>
        /// <param name="tag">테그</param>
        /// <param name="tagSwap">테그 스왑</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static byte[] CombinePacket(TraStatusCode statusCode, byte[] buf, byte[] tag, byte tagSwap) {
            if (buf == null) throw new ArgumentNullException("buf");
            if (tag == null) throw new ArgumentNullException("tag");
            TraBuffer.SwapBuffer(tag, tagSwap, false);
            var packet = TraBuffer.Combine(tag, buf);
            TraBuffer.ShiftBuffer(statusCode, ref packet);
            return packet;
        }
    }

    /// <summary>
    /// 트-라 세션 인포
    /// </summary>
    public class TraSessionInfo {
        /// <summary>
        /// 1차 정보
        /// </summary>
        public TraServerInfo_1f Info_1F { get; internal set; }

        /// <summary>
        /// 2차 정보
        /// </summary>
        public TraServerInfo_2f Info_2F { get; internal set; }

        /// <summary>
        /// 프로토콜 이름
        /// </summary>
        /// <exception cref="NotSupportedException" />
        public ReadOnlyMemory<byte> ProtocolName => 
            !Info_1F.ProtocolName.IsEmpty ? Info_1F.ProtocolName 
            : throw new NotSupportedException("1차 정보롤 받지 않았습니다.");

        /// <summary>
        /// 프로토콜 버전
        /// </summary>
        /// <exception cref="NotSupportedException" />
        public uint ProtocolVersion =>
            !Info_1F.ProtocolName.IsEmpty ? Info_1F.ProtocolVersion
            : throw new NotSupportedException("1차 정보롤 받지 않았습니다.");

        /// <summary>
        /// 프로토콜 버전
        /// </summary>
        /// <exception cref="NotSupportedException" />
        public int BufferSize =>
            Info_2F.BufferSize != 0 ? Info_2F.BufferSize
            : throw new NotSupportedException("2차 정보롤 받지 않았습니다.");

        /// <summary>
        /// 프로토콜 버전
        /// </summary>
        /// <exception cref="NotSupportedException" />
        public ReadOnlyMemory<byte>? State =>
            Info_2F.BufferSize != 0 ? Info_2F.State
            : throw new NotSupportedException("2차 정보롤 받지 않았습니다.");
    }

    /// <summary>
    /// 1차 트-라 서버 정보
    /// </summary>
    public readonly struct TraServerInfo_1f {
        public static readonly int MinSize = 1 + sizeof(uint) + 1;

        /// <summary>
        /// 1차 트-라 서버 정보 
        /// </summary>
        /// <param name="swapNumber">교환 번호</param>
        /// <param name="protoolVersion">프로토콜 버전</param>
        /// <param name="protocolName">프로토콜 이름</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public TraServerInfo_1f(byte swapNumber, uint protoolVersion, ReadOnlyMemory<byte> protocolName) {
            if (protocolName.IsEmpty || protocolName.Length < 1) throw new ArgumentNullException(nameof(protocolName));
            if (protocolName.Length < 1) throw new ArgumentException("프로토콜 이름은 1보다 커야합니다.");
            this.SwapNumber = swapNumber;
            ProtocolVersion = protoolVersion;
            ProtocolName = protocolName;
        }

        /// <summary>
        /// 교환 숫자
        /// </summary>
        public byte SwapNumber { get; }

        /// <summary>
        /// 프로토콜 버전
        /// </summary>
        public uint ProtocolVersion { get; }

        /// <summary>
        /// 프로토콜 이름
        /// </summary>
        public ReadOnlyMemory<byte> ProtocolName { get; }

        /// <summary>
        /// 바이너리 데이터를 만듭니다.
        /// </summary>
        public byte[] Create() {
            var protocolName = ProtocolName.ToArray();
            var nameLength = protocolName.Length;
            var buf = new byte[1 + sizeof(uint) + nameLength];
            buf[0] = SwapNumber;
            Buffer.BlockCopy(BitConverter.GetBytes(ProtocolVersion), 0, buf, 1, sizeof(uint));
            Buffer.BlockCopy(protocolName, 0, buf, 1 + sizeof(uint), nameLength);
            return buf;
        }

        /// <summary>
        /// 데이터를 파싱합니다.
        /// </summary>
        /// <param name="info">데이터</param>
        /// <exception cref="ArgumentException"></exception>
        public static TraServerInfo_1f Parse(byte[] info) =>
            Parse(info, 0, info.Length);

        /// <summary>
        /// 데이터를 파싱합니다.
        /// </summary>
        /// <param name="info">데이터</param>
        /// <param name="index">시작</param>
        /// <exception cref="ArgumentException"></exception>
        public static TraServerInfo_1f Parse(byte[] info, int index) =>
            Parse(info, index, info.Length - index);

        /// <summary>
        /// 데이터를 파싱합니다.
        /// </summary>
        /// <param name="info">데이터</param>
        /// <param name="index">시작</param>
        /// <param name="count">길이</param>
        /// <exception cref="ArgumentException"></exception>
        public static TraServerInfo_1f Parse(byte[] info, int index, int count) {
            if (count < MinSize) throw new ArgumentException($"데이터 크기는 {MinSize}보다 커야합니다.");

            var namePos = 1 + sizeof(uint);

            return new (
                info[index],
                BitConverter.ToUInt32(info, index + 1),
                TraBuffer.Section(info, index + namePos, count - namePos)
            );
        }
    }

    /// <summary>
    /// 2차 서버 정보
    /// </summary>
    public readonly struct TraServerInfo_2f {
        public static readonly int MinSize = 256;
        public static readonly int MaxStateLength = 512;
        public static readonly int MaxPacketLength = 256 + 512;

        public TraServerInfo_2f(int bufferSize, byte tagSwapNumber, byte[]? state = null) {
            BufferSize = bufferSize;
            TagSwapNumber = tagSwapNumber;
            if (state != null) {
                if (state.Length > MaxStateLength) throw new ArgumentOutOfRangeException(nameof(state),
                    $"상태 데이터는 {MaxStateLength} 보다 클 수 없습니다.");
                State = state;
            }
            else State = null;
        }

        /// <summary>
        /// 버퍼 크기
        /// </summary>
        public int BufferSize { get; }

        /// <summary>
        /// 테그 스왑 번호
        /// </summary>
        public byte TagSwapNumber { get; }

        /// <summary>
        /// 상태
        /// </summary>
        public ReadOnlyMemory<byte>? State { get; }

        /// <summary>
        /// 바이너리 데이터를 만듭니다.
        /// </summary>
        public byte[] Create() {
            var info = new byte[256];
            Buffer.BlockCopy(BitConverter.GetBytes(BufferSize), 0, info, 0, sizeof(int));
            info[sizeof(int)] = TagSwapNumber;
            // 예약됨.

            if (State != null) {
                var state = State.Value.ToArray();
                var stateLength = state.Length;
                Array.Resize(ref info, 256 + stateLength);
                Buffer.BlockCopy(state, 0, info, 256, stateLength);
            }

            return info;
        }
        
        /// <summary>
        /// 데이터를 파싱합니다.
        /// </summary>
        /// <param name="info">데이터</param>
        /// <exception cref="ArgumentException"></exception>
        public static TraServerInfo_2f Parse(byte[] info) =>
            Parse(info, 0, info.Length);

        /// <summary>
        /// 데이터를 파싱합니다.
        /// </summary>
        /// <param name="info">데이터</param>
        /// <param name="index">시작</param>
        /// <exception cref="ArgumentException"></exception>
        public static TraServerInfo_2f Parse(byte[] info, int index) =>
            Parse(info, index, info.Length - index);

        /// <summary>
        /// 데이터를 파싱합니다.
        /// </summary>
        /// <param name="info">데이터</param>
        /// <param name="index">시작</param>
        /// <param name="count">길이</param>
        /// <exception cref="ArgumentException"></exception>
        public static TraServerInfo_2f Parse(byte[] info, int index, int count) {
            if (count < MinSize) throw new ArgumentException($"데이터 크기는 {MinSize}보다 커야합니다.");
            var stateLeneth = info.Length - MinSize;

            return new (
                BitConverter.ToInt32(info, index),
                info[index + sizeof(int)],
                stateLeneth > 0 ? TraBuffer.Section(info, MinSize, stateLeneth) : null
            );
        }
    }
}
