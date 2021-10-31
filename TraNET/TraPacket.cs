using System;
using System.Collections.Generic;
using System.Text;

namespace TraNET {
    /// <summary>
    /// 트-라 패킷
    /// </summary>
    internal static class TraPacket {

    }

    /// <summary>
    /// 트-라 서버 인포
    /// </summary>
    public class TraServerInfo {
        /// <summary>
        /// 1차 정보
        /// </summary>
        public TraServerInfo_1f Info_1F { get; internal set; }

        /// <summary>
        /// 2차 정보
        /// </summary>
        public TraServerInfo_1f Info_2F { get; internal set; }

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
    }

    /// <summary>
    /// 1차 트-라 서버 정보
    /// </summary>
    public readonly struct TraServerInfo_1f {
        const int MinSize = 1 + sizeof(uint) + 1;

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
            var nameLength = ProtocolName.Length;
            var buf = new byte[sizeof(uint) + nameLength];
            buf[0] = SwapNumber;
            Buffer.BlockCopy(BitConverter.GetBytes(ProtocolVersion), 0, buf, 1, sizeof(uint));
            Buffer.BlockCopy(ProtocolName.ToArray(), 0, buf, 1 + sizeof(uint), nameLength);
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

            return new (
                info[index],
                BitConverter.ToUInt32(info, index + 1),
                TraBuffer.Section(info, index + 1 + sizeof(uint))
            );
        }
    }
}
