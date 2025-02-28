namespace MyMinecraft.Models_r;
public class PlaceBlockPacket : DestroyBlockPacket {
    public BlockType selectedBlock;
    public PlaceBlockPacket(Vector3 playerPos, Vector3 front, float playerRange,BlockType selectedBlock) : base(playerPos, front, playerRange) {
        Type = PacketType.PLACE_BLOCK;
        this.selectedBlock = selectedBlock;
    }
}
