namespace MyMinecraft.Models;

public enum PacketType : byte {
    NONE, // will be used for setting up connection with client later
    REMESH,
    DESTROY_BLOCK,
    PLACE_BLOCK,
    SET_BLOCK,
    REDRAW,

}

[Serializable]
public class ServerPacket {
    //will need ip address or some other thing to know to which client to send the packet back
    public PacketType Type { get; set; }

    public ServerPacket() {
        Type = PacketType.NONE;
    }

    public ServerPacket(PacketType type) {
        Type = type;
    }

    public virtual byte[] Serialize() {
        using (MemoryStream ms = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(ms)) {
            writer.Write((byte)Type);
            return ms.ToArray();
        }
    }

    // Convert bytes back into an object
    public virtual ServerPacket Deserialize(byte[] data) {
        using (MemoryStream ms = new MemoryStream(data))
        using (BinaryReader reader = new BinaryReader(ms)) {
            var type = reader.Read();
            return new ServerPacket((PacketType)type);
        }
    }
}
