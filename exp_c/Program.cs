//using Cryptography;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
// 클라 생성
var BufferSize = 4096;

Thread.Sleep(3000);

TcpClient client = new();
client.Connect("localhost", 3132);

var stopWatchs = new Stopwatch[6];

for (int i = 0; i < stopWatchs.Length; i++)
    stopWatchs[i] = new Stopwatch();

stopWatchs[0].Start();

// 초기 버퍼는 4096으로 고정.
var buffer = new byte[4096];
var stream = client.GetStream();

stopWatchs[1].Start();

// 프로토콜 인증용 데이터
var vef_buf = Encoding.ASCII.GetBytes("test1");

{
    // 수신 타임아웃
    stream.ReadTimeout = 10000;
    Debug.WriteLine("프로토콜 이름 수신 대기.");
    var count = stream.Read(buffer, 0, buffer.Length);
    stream.ReadTimeout = -1;

    // 프로토콜 이름 확인
    if (!EqualSwapVefBuf(vef_buf, buffer, count))
        throw new Exception("프로토콜 불일치");

    Debug.WriteLine("프로토콜 이름 확인됨.");

    // 프로토콜 인증용 데이터
    Debug.WriteLine("프로토콜 이름 인증 송신.");
    stream.Write(vef_buf);
    stream.Flush();

    // 승인 패킷
    count = stream.Read(buffer);
    if (count <= 0) throw new Exception("서버와 연결이 끊어졌습니다.");
    if (buffer[0] != 123) throw new Exception("프로토콜 불일치");
    Debug.WriteLine("프로토콜 승인됨.");
}

stopWatchs[1].Stop();
stopWatchs[2].Start();

byte[] key, nonce;

// 클라는 일회용 rsa 공개키를 줌
{
    Debug.WriteLine("암호화 임시 RSA키 생성.");
    var rsa = GenerateRSA(out _, out var pubKey);

    // 공개키 버퍼를 보내기 전에 0번에 인식용 데이터 72를 밀어 넣음.
    ShiftBuffer(72, ref pubKey);

    Debug.WriteLine("암호화 공개키 송신.");
    stream.Write(pubKey);
    stream.Flush();

    // 암호화된 ase 키 받음
    var count = stream.Read(buffer, 0, buffer.Length);

    if (count <= 0) throw new Exception("보낸 데이터가 없음");
    if (buffer[0] != 71) throw new Exception("프로토콜 절차가 일치하지 않음.");

    stopWatchs[2].Stop();
    stopWatchs[3].Start();
    Debug.WriteLine("암호화된 AES키 수신.");

    // 받은 암호화된 aes키를 복호화 시킴
    var gen_buf = new byte[count - 1];
    Buffer.BlockCopy(buffer, 1, gen_buf, 0, gen_buf.Length);
    gen_buf = rsa.Decrypt(gen_buf, false);

    if (gen_buf.Length != 32 + 12) throw new Exception("키 길이가 일치하지 않음.");

    key = new byte[32];
    nonce = new byte[12];

    // 받은 암호화된 키를 key, nonce로 나눔
    Buffer.BlockCopy(gen_buf, 0, key, 0, key.Length);
    Buffer.BlockCopy(gen_buf, 32, nonce, 0, nonce.Length);
}


stopWatchs[3].Stop();
stopWatchs[4].Start();

Debug.WriteLine($"암호화키 받음: {BitConverter.ToString(key).Replace('-', ' ')} | {BitConverter.ToString(nonce).Replace('-', ' ')}");

{
    // 암호화 확인용 데이터 보냄
    using (var siv = new AesGcm(key)) {
        // 인증 스왑
        SwapVefBuf(vef_buf);
        var body = Encrypt(siv, nonce, vef_buf, out var tag);
        MakePacket(86, tag, body, out var buf);

        Debug.WriteLine("암호화 인증 송신.");
        stream.Write(buf);
        stream.Flush();

        stopWatchs[4].Stop();
        stopWatchs[5].Start();

        // 프로토콜 인포
        var count = await stream.ReadAsync(buffer);
        if (count <= 0) throw new Exception("보낸 데이터가 없음");
        if (count < 1 + 16) throw new Exception("보안 테그 없음");
        if (buffer[0] != 82) throw new Exception("프로토콜 절차가 일치하지 않음.");
        Debug.WriteLine("프로토콜 정보 수신.");

        var info = Decrypt(siv, nonce, buffer, count);
        if(info.Length < 32) throw new Exception("프로토콜 인포 데이터 길이가 짧음.");
        // 0 ~ 3    INT     buffer size
        // 0 ~ N    NONE    reserved

        // 버퍼 사이즈 설정
        var buf_size = BitConverter.ToInt32(info, 0);
        if(buf_size <= 1 + 16 + 1) throw new Exception("프로토콜 인포의 버퍼 크기가 최소 크기보다 적음.");
        BufferSize = buf_size;
        
        // 실제 버퍼 크기와 받아온 크기가 다르면 변경
        if (buffer.Length != buf_size)
            Array.Resize(ref buffer, buf_size);

        Debug.WriteLine($"버퍼 크기: {buf_size}");

        // 준비 완료됨
        SwapVefBuf(vef_buf);

        body = Encrypt(siv, nonce, vef_buf, out tag);
        MakePacket(45, tag, body, out var v_buf);

        Debug.WriteLine("준비 완료 송신.");
        stream.Write(v_buf);
        stream.Flush();
    }
}

stopWatchs[5].Stop();
stopWatchs[0].Stop();

var ns = new[] {
    "총",
    "프로토콜 인증",
    "RSA 생성 및 송신",
    "AES 수신",
    "암호화 인증",
    "프로토콜 준비"
};

Console.WriteLine($"프로토콜 연결됨.");

for (int i = 0; i < ns.Length; i++) {
    Console.WriteLine($"{ns[i]} 구간. {stopWatchs[i].ElapsedMilliseconds}ms {stopWatchs[i].ElapsedTicks}ticks");
}

Console.ReadLine();

bool EqualSwapVefBuf(byte[] vef_buf, byte[] des, int? desCount = null) {
    desCount ??= des.Length;
    if (vef_buf.Length != desCount) return false;
    for (int i = 0; i < vef_buf.Length; i++) {
        if (vef_buf[i] != buffer[i]) return false;
        vef_buf[i] = unchecked((byte)(vef_buf[i] + (149 + i)));
    } 
    return true;
}

bool EqualBuf(byte[] buf1, byte[] buf2, int? buf1Count = null) {
    buf1Count ??= buf1.Length;
    if (buf1Count != buf2.Length) return false;
    for (int i = 0; i < buf1Count; i++)
        if (buf1[i] != buf2[i]) return false;
    return true;
}

void SwapVefBuf(byte[] buf) {
    for (int i = 0; i < buf.Length; i++)
        buf[i] = unchecked((byte)(buf[i] + (149 + i)));
}

void MakePacket(byte head, byte[] tag, byte[] body, out byte[] buf) {
    buf = new byte[1 + tag.Length + body.Length];
    buf[0] = head;
    Buffer.BlockCopy(tag, 0, buf, 1, tag.Length);
    Buffer.BlockCopy(body, 0, buf, 1 + tag.Length, body.Length);
}

byte[] Encrypt(AesGcm aes, byte[] nonce, byte[] data, out byte[] tag) {
    var ciphertext = new byte[data.Length];
    tag = new byte[16];
    aes.Encrypt(nonce, data, ciphertext, tag);
    return ciphertext;
}

byte[] Decrypt(AesGcm aes, byte[] nonce, byte[] buffer, int count) {
    // 버퍼 복사
    var tag_buf = new byte[16];
    Buffer.BlockCopy(buffer, 1, tag_buf, 0, tag_buf.Length);
    var ciphertext = new byte[count - 1 - 16];
    Buffer.BlockCopy(buffer, 1 + 16, ciphertext, 0, ciphertext.Length);

    var data = new byte[ciphertext.Length];
    aes.Decrypt(nonce, ciphertext, tag_buf, data);
    return data;
}

void ShiftBuffer(byte head, ref byte[] data) {
    Array.Resize(ref data, data.Length + 1);
    for (int i = data.Length - 1; i > 0; i--)
        data[i] = data[i - 1];
    data[0] = head;
}

RSACryptoServiceProvider GenerateRSA(out byte[] priKey, out byte[] pubKey) {
    var rsa = new RSACryptoServiceProvider(2048);
    priKey = rsa.ExportRSAPrivateKey();
    pubKey = rsa.ExportSubjectPublicKeyInfo();
    return rsa;
}