namespace MyMinecraft.Models_r;

[Serializable]
public class DestroyBlockPacket : ServerPacket {

    public Vector3 position;
    public Vector3 front;
    public float playerRange;


    public DestroyBlockPacket(Vector3 playerPos, Vector3 front, float playerRange) {
        Type = PacketType.DESTROY_BLOCK;
        this.position = playerPos;
        this.front = front;
        this.playerRange = playerRange;
    }


    public byte[] Serialize() {
        using (MemoryStream ms = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(ms)) {
            writer.Write(position.X);
            writer.Write(position.Y);
            writer.Write(position.Z);
            writer.Write(front.X);
            writer.Write(front.Y);
            writer.Write(front.Z);
            writer.Write(playerRange);
            return ms.ToArray();
        }
    }

    // Convert bytes back into an object
    public override ServerPacket Deserialize(byte[] data) {
        using (MemoryStream ms = new MemoryStream(data))
        using (BinaryReader reader = new BinaryReader(ms)) {
            Vector3 pos = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            Vector3 front = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            float range = reader.ReadSingle();
            return new DestroyBlockPacket(pos, front, range);
        }
    }
}
