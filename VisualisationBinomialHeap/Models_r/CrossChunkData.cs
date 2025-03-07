namespace MyMinecraft.Models_r; 
public class CrossChunkData {
    public BlockType blockType;
    public Vector3i globalBlockPos;

    public CrossChunkData(BlockType blockType, Vector3i globalBlockPos) {
        this.blockType = blockType;
        this.globalBlockPos = globalBlockPos;
    }
}
