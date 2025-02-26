using System.Drawing;

namespace MyMinecraft.Models; 
public struct ChunkData {
    public BlockType[,,] blocks;
    public Vector3 position;
    public ushort[,] heightMap;

    public ChunkData(Vector3 position) {
        this.blocks = new BlockType[Chunk.SIZE, Chunk.HEIGHT, Chunk.SIZE];
        this.position = position;
        this.heightMap = new ushort[Chunk.SIZE, Chunk.SIZE];
    }
}
