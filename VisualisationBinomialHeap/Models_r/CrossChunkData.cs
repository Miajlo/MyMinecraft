namespace MyMinecraft.Models_r; 
public class CrossChunkData {
    public Dictionary<Vector3i, Block_r> crossChunkBlocks = [];

    public bool TryAddBlock(Vector3i position, Block_r block) {      
        if(!crossChunkBlocks.ContainsKey(position) && block.type != BlockType.AIR) //add new if not in list yet
            return crossChunkBlocks.TryAdd(position, block);
 
        return false;
    }

    public void AddAllBlocks(Dictionary<Vector3i, Block_r> blocksToAdd) {
        foreach (var block in blocksToAdd)
            TryAddBlock(block.Key, new Block_r(block.Value.type));
    }
    //public BlockType blockType;
    //public Vector3i blockPos;
    //public CrossChunkData(BlockType blockType, Vector3i globalBlockPos) {
    //    this.blockType = blockType;
    //    this.blockPos = globalBlockPos;
    //}
}
