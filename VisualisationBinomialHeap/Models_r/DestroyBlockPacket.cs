namespace MyMinecraft.Models_r;
public class DestroyBlockPacket : ServerPacket {

    public Vector3 position;
    public Vector3 front;
    public float playerRange;


    public DestroyBlockPacket(Vector3 playerPos, Vector3 front, float playerRange):base(PacketType.DESTROY_BLOCK) {
        this.position = playerPos;
        this.front = front;
        this.playerRange = playerRange;

    }
}
