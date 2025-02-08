namespace MyMinecraft.Models; 
public struct ChunkData {
    public BlockType[,,] blocks;
    public Vector3 position;
    public ushort heightMap;
}
