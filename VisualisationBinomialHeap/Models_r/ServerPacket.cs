namespace MyMinecraft.Models_r;

public enum PacketType : byte {
    NONE, // will be used for setting up connection with client later
    REMESH,
    DESTROY_BLOCK,
    PLACE_BLOCK,
    REDRAW,

}

public class ServerPacket {
    //will need ip address or some other thing to know to which client to send the packet back
    public PacketType Type { get; set; }

    public ServerPacket(PacketType type) {
        Type = type;
    }
}
