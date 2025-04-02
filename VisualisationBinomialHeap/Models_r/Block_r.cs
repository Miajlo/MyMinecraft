namespace MyMinecraft.Models_r; 
public struct Block_r {
    public BlockType type;
    public byte lightLevel;
    public Faces rotation;

    public Block_r() {
        type = BlockType.AIR;
        lightLevel = 0;
        rotation = Faces.TOP;
    }
}
