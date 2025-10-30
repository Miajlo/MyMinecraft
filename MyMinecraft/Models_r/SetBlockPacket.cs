namespace MyMinecraft.Models_r; 
public class SetBlockPacket : ServerPacket{
    public Vector3i blockPos;
    public BlockType blockType;

    public SetBlockPacket(Vector3i blockPos, BlockType blockType):base(PacketType.SET_BLOCK) {
        this.blockPos = blockPos;
        this.blockType = blockType;
    }
}
