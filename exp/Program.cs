//using Cryptography;
using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

var random = new Random();
const int BufferSize = 10000;
// 서버 시작
TcpListener Listener = new(new IPEndPoint(0, 3132));
Listener.Start();
Listener.BeginAcceptTcpClient(BeginAccept, null);

Console.ReadLine();

// 비동기 접속
void BeginAccept(IAsyncResult result) {
    var client = Listener.EndAcceptTcpClient(result);

    //프로세스
    Task.Factory.StartNew(() => Process(client));
}

// 프로세스
async void Process(TcpClient client) {
    var stream = client.GetStream();
    // 초기 버퍼는 4096으로 고정.
    var buffer = new byte[4096];

    byte[] key, nonce;

    // 프로토콜 초기화
    {
        Console.WriteLine("프로토콜 인증 송신.");
        var vef_buf = Encoding.ASCII.GetBytes("test1");
        stream.Write(vef_buf);
        stream.Flush();

        for (int i = 0; i < vef_buf.Length; i++)
            vef_buf[i] = unchecked((byte)(vef_buf[i] + (149 + i)));

        Console.WriteLine("프로토콜 인증 수신.");
        // 10초 안에 데이터가 보내지지 않으면 터짐
        var count = await stream.ReadAsync(buffer, new CancellationTokenSource(10000).Token);

        if (count != vef_buf.Length) throw new Exception("프로토콜 불일치");

        for (int i = 0; i < vef_buf.Length; i++) 
            if (vef_buf[i] != buffer[i]) 
                throw new Exception("프로토콜 불일치");

        Console.WriteLine("프로토콜 승인 송신.");
        stream.Write(new byte[] { 123 });
        stream.Flush();

        // 15초 안에 공개키를 보내지 않으면 터짐
        count = await stream.ReadAsync(buffer, new CancellationTokenSource(15000).Token);

        if (count <= 0) throw new Exception("보낸 데이터가 없음");
        if (buffer[0] != 72) throw new Exception("프로토콜 절차가 일치하지 않음.");

        // 받은 공개키
        var pubKey = Encoding.ASCII.GetString(buffer, 1, count - 1);

        Console.WriteLine("암호화 공개키 수신.");

        // aes 키 생성
        key = new byte[32];
        RandomNumberGenerator.Fill(key);
        nonce = new byte[12];
        RandomNumberGenerator.Fill(nonce);

        // 공개키로 대칭키 암호화
        using (var rsa = new RSACryptoServiceProvider()) {
            rsa.FromXmlString(pubKey);

            // 키값 합치기
            var gen_key = new byte[32 + 12];
            Buffer.BlockCopy(key, 0, gen_key, 0, key.Length);
            Buffer.BlockCopy(nonce, 0, gen_key, 32, nonce.Length);

            // 공개키로 암호화된 ase키를 보냄
            var enc_key = rsa.Encrypt(gen_key, false);
            // aes 키 스왑
            ShiftBuffer(71, ref enc_key);

            // 암호화된 키 보냄
            stream.Write(enc_key);
            stream.Flush();
            
            Console.WriteLine($"암호화키 송신: {BitConverter.ToString(key).Replace('-', ' ')} | {BitConverter.ToString(nonce).Replace('-', ' ')}");
        }

        // 암호화 확인
        count = await stream.ReadAsync(buffer);
        Console.WriteLine("암호화 인증 수신.");

        if (count <= 0) throw new Exception("보낸 데이터가 없음");
        if (count < 1 + 16) throw new Exception("보안 테그 없음");
        if (buffer[0] != 86) throw new Exception("프로토콜 절차가 일치하지 않음.");

        using (var siv = new AesGcm(key)) {
            var data = Decrypt(siv, nonce, buffer, count);
            if (data.Length != vef_buf.Length) throw new Exception("암호화 인증 불일치");

            for (int i = 0; i < vef_buf.Length; i++)
                if (vef_buf[i] != data[i])
                    throw new Exception("암호화 인증 불일치");

            Console.WriteLine("암호화 확인됨.");

            // 프로토콜 인포
            // 버퍼 사이즈 설정
            // 0 ~ 3    INT     buffer size
            // 0 ~ N    NONE    reserved
            var info = new byte[32];
            Buffer.BlockCopy(BitConverter.GetBytes(BufferSize), 0, info, 0, sizeof(int));

            var body = Encrypt(siv, nonce, info, out var tag);
            MakePacket(82, tag, body, out var buf);

            Console.WriteLine("프로토콜 정보 송신.");
            stream.Write(buf);
            stream.Flush();
        }
    }

    Console.WriteLine("프로토콜 연결됨.");
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

enum ProtocolHeadType : byte {
    Step0 = 80,
    ProtocolName = 80,
    Step1 = 110,
    Accept = 110,
    Step2 = 50,
    RSAKey = 50,
    Step3 = 130,
    EncAESKey = 130,
    Step4 = 160,
    Step4 = 160,
    Step5 = 90,
    Step5 = 90,
    Step6 = 120,
    Step6 = 120,
    Step7 = 70,
    Step7 = 70,
    Step8 = 230,
    Step8 = 230,
    Step9 = 140
    Step9 = 140
}