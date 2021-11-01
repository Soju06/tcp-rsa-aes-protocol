using System.Diagnostics;
using System.Text;
using TraNET;

class program {
    public static void Main() {
        var server = new TraServer(System.Net.IPAddress.Loopback, 1011);
        server.Start();

        server.OnSessionStart += (sender, args) => {
            args.State = Encoding.UTF8.GetBytes("핼로우");
        };

        var client = new TraClient();
        client.Connect(System.Net.IPAddress.Loopback, 1011, TraNet.DefaultProtocolName.ToArray());

        Console.WriteLine(Encoding.UTF8.GetString(client.SessionInfo.State.Value.ToArray()) + ".");
    }
}