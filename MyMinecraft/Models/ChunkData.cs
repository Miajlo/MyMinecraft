using MyMinecraft.Models_r;
using System.Drawing;

namespace MyMinecraft.Models; 
public struct ChunkData {
    public BlockType[,,] blocks;
    public Vector3 position;
    public ushort[,] heightMap;

    public ChunkData(Vector3 position) {
        this.blocks = new BlockType[Chunk_r.SIZE, Chunk_r.HEIGHT, Chunk_r.SIZE];
        this.position = position;
        this.heightMap = new ushort[Chunk_r.SIZE, Chunk_r.SIZE];
    }
}
