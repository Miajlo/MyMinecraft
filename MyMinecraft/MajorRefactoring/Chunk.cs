namespace MyMinecraft.MajorRefactoring; 
public class Chunk {
    public static byte Width => World.Options!.ChunkWidth;
    public static uint Height => World.Options!.ChunkHeight;
    public static int SubchunkHeight => World.Options!.SubchunkHeight;

    public Vector3i _position;
    public Block[] _chunkBlocks;
    public volatile bool Built;
    public volatile bool Redraw;
    public volatile bool AddedFaces;
    public volatile bool FirstDrawing;
    public volatile bool Generated;
    public volatile bool DecorationAdded;
    public volatile bool Dirty;
    public volatile bool ShouldClearData;
    public volatile bool UsedForStructGen;

    VAO? chunkVAO;
    VBO? chunkVBO;
    VBO? chunkUVVBO;
    IBO? chunkIBO;

    public List<Vector3> _chunkVert;
    public List<Vector2> _chunkUVs;
    public List<uint> _chunkInd;

    public Chunk() {
        var blockCount = Width*Width*Height;
        _chunkBlocks = new Block[blockCount];
        _chunkInd = [];
        _chunkUVs = [];
        _chunkVert = [];
    }
}
