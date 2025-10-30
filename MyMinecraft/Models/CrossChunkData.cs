namespace MyMinecraft.Models;
public class CrossChunkData {
    public Dictionary<Vector3i, Block> crossChunkBlocks = [];

    public bool TryAddBlock(Vector3i position, Block block) {      
        if(!crossChunkBlocks.ContainsKey(position) && block.type != BlockType.AIR) //add new if not in list yet
            return crossChunkBlocks.TryAdd(position, block);
 
        return false;
    }

    public void AddAllBlocks(Dictionary<Vector3i, Block> blocksToAdd) {
        foreach (var block in blocksToAdd)
            TryAddBlock(block.Key, new Block(block.Value.type));
    }
    //public BlockType blockType;
    //public Vector3i blockPos;
    //public CrossChunkData(BlockType blockType, Vector3i globalBlockPos) {
    //    this.blockType = blockType;
    //    this.blockPos = globalBlockPos;
    //}
}
