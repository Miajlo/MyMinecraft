using System.Net;
using System.Net.Sockets;

namespace MyMinecraft.Models;

public class SocketClient {
    private Socket socket;
    private NetworkStream stream;

    public SocketClient(string ip, int port) {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(new IPEndPoint(IPAddress.Parse(ip), port));
        stream = new NetworkStream(socket);
    }

    public void SendPacket(ServerPacket packet) {
        byte[] data = packet.Serialize();

        lock (stream) {
            byte[] lengthPrefix = BitConverter.GetBytes(data.Length);
            stream.Write(lengthPrefix, 0, lengthPrefix.Length);
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }
    }

    public void Close() {
        socket.Close();
    }
}