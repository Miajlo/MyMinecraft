namespace MyMinecraft.Models_r; 
public class CrossChunkData {
    public BlockType blockType;
    public Vector3i blockPos;
    public CrossChunkData(BlockType blockType, Vector3i globalBlockPos) {
        this.blockType = blockType;
        this.blockPos = globalBlockPos;
    }
}
