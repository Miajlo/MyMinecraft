namespace MyMinecraft.Models_r; 
public class Chunk_r {
    #region CONSTANTS
    public const byte SIZE = 16;
    public const short HEIGHT = 64;
    #endregion

    #region CHUNK_DATA
    public Vector3 position;
    public BlockType[,,] chunkBlocks;
    public short[,] heightMap;
    #endregion

    #region GRAPHICS_CONTEXT
    VAO? chunkVAO;
    VBO? chunkVBO;
    VBO? chunkUVVBO;
    IBO? chunkIBO;

    public List<Vector3> chunkVert;
    public List<Vector2> chunkUVs;
    public List<uint> chunkInd;

    uint indexCount;
    #endregion

    #region FLAGS
    public bool Built { get; set; }
    public bool Redraw { get; set; }
    public bool AddedFaces { get; set; }
    public bool FirstDrawing { get; set; }
    public bool Dirty { get; set; }
    #endregion

    #region CONSTRUCTORS
    public Chunk_r() {
        Built= false;
        Redraw = false;
        AddedFaces = false;
        FirstDrawing = true;
        Dirty = false;

        chunkBlocks = new BlockType[SIZE, HEIGHT, SIZE];
        heightMap = new short[SIZE, SIZE];

        chunkUVs = [];
        chunkVert = [];
        chunkInd = [];

        indexCount = 0;
    }

    public Chunk_r(Vector3 position) : base() {
        this.position = position;
    }
    #endregion

    #region GENERATION_METHODS
    public void GenChunk() {
        GenHeightMap();
        GenChunkBlocks();
        GenTrees();

        Console.WriteLine($"Generated chunk: {position}");
    }

    private void GenChunkBlocks() {
        for (int x = 0; x < SIZE; ++x) {
            for (int z = 0; z < SIZE; ++z) {
                for (int y = 0; y < HEIGHT; ++y) { 
                    chunkBlocks[x, y, z] = GetBlockType(x, y, z);
                }
            }
        }
    }

    private void GenHeightMap() {
        FastNoiseLite fnl = new();
        fnl.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        fnl.SetFrequency(0.01f);
        for (var x = 0; x < SIZE; ++x) {
            for (var z = 0; z < SIZE; ++z) {
                float noiseValue = fnl.GetNoise(x + position.X, z + position.Z);
                heightMap[x, z] = (short)Math.Clamp(((noiseValue + 1) * 0.5f) * HEIGHT, 0, HEIGHT - 1);
            }
        }
    }

    private void GenTrees() {
        //TODO: implment it using white noise
    }
    #endregion

    #region RENDER_METHODS
    public void Render(ShaderProgram program) {
        if (!Built)
            BuildChunk();

        program?.Bind();


        chunkVAO?.Bind();
        chunkIBO?.Bind();
        Matrix4 model = Matrix4.CreateTranslation(position);
        int modelLocation = GL.GetUniformLocation(program.ID, "model");
        GL.UniformMatrix4(modelLocation, true, ref model);
        GL.ActiveTexture(TextureUnit.Texture0);
        Texture.Bind();
        GL.Uniform1(GL.GetUniformLocation(program.ID, "texture0"), 0);
        GL.DrawElements(PrimitiveType.Triangles, chunkInd.Count,
                        DrawElementsType.UnsignedInt, 0);

        if (FirstDrawing) {
            Console.WriteLine($"Drew chunk: [ {position} ]");
            FirstDrawing = false;
        }

        Texture.Unbind();
        chunkVAO?.Unbind();
        chunkIBO?.Unbind();
        chunkUVVBO?.Unbind();
        chunkVBO?.Unbind();
    }

    public void BuildChunk() {
        chunkInd ??= new();
        chunkVert ??= new();
        chunkUVs ??= new();

        chunkVAO = new();
        chunkVAO.Bind();

        chunkVBO = new(chunkVert);
        chunkVBO.Bind();
        chunkVAO.LinkToVAO(0, 3, chunkVBO);

        chunkUVVBO = new(chunkUVs);
        chunkUVVBO.Bind();
        chunkVAO.LinkToVAO(1, 2, chunkUVVBO);

        chunkIBO = new(chunkInd);
        chunkIBO.Bind();

        Built = true;
    }

    public void Unload() {
        chunkIBO?.Unbind();
        chunkVAO?.Unbind();
        chunkVBO?.Unbind();
        Texture.Unbind();
        Built = false;
    }

    public void Delete() {
        chunkVBO?.Delete();
        chunkUVVBO?.Delete();
        chunkVAO?.Delete();
        chunkIBO?.Delete();        
        GL.Finish();

        chunkInd.Clear();
        chunkVert.Clear();
        chunkUVs.Clear();

        indexCount = 0;

        Built = false;
    }
    #endregion


    #region DATA_METHODS
    public void IntegrateFace(BlockType block, Faces face, Vector3 blockPos) {
        var faceData = Block.GetFaceData(face, blockPos);
        chunkVert.AddRange(faceData);
        chunkUVs.AddRange(TextureData.GetUVs(block, face));
    }

    public void AddIndeces(uint indCount) {
        for (int i = 0; i < indCount; ++i) {
            chunkInd.Add(0 + indexCount);
            chunkInd.Add(1 + indexCount);
            chunkInd.Add(2 + indexCount);
            chunkInd.Add(2 + indexCount);
            chunkInd.Add(3 + indexCount);
            chunkInd.Add(0 + indexCount);

            indexCount += 4;
        }
    }


    public void SetBlockAt(Vector3 chunkBlockPos, BlockType blockType) {
        if (InvalidBlockCoords(chunkBlockPos)) {
            Console.WriteLine($"Invalid block position to set: {chunkBlockPos}");
            return;
        }
        chunkBlockPos = Chunk.ConvertToChunkBlockCoord(chunkBlockPos);
        chunkBlocks[(int)chunkBlockPos.X, (int)chunkBlockPos.Y, (int)chunkBlockPos.Z] = blockType;
    }

    public BlockType GetBlockAt(Vector3 chunkBlockPos) {

        if (InvalidBlockCoords(chunkBlockPos)) {
            throw new ArgumentException($"Invalid chunk block pos: {chunkBlockPos}, chunk:{position}");
        }

        return chunkBlocks[(int)chunkBlockPos.X, (int)chunkBlockPos.Y, (int)chunkBlockPos.Z];
    }

    public void SaveToFile() {
        return;
    }

    public void LoadFromFile() {
        return;
    }
    #endregion


    #region UTIL_METHODS
    public BlockType GetBlockType(int x, int y, int z) {
        if(y == heightMap[x, z])
            return BlockType.GRASS_BLOCK;
        else if (y < heightMap[x, z])
            return BlockType.STONE;
        else
            return BlockType.AIR;
    }

    public static bool InvalidBlockCoords(Vector3 chunkBlockPos) {
        return chunkBlockPos.Y < 0 || chunkBlockPos.Y >= Chunk.HEIGHT ||
               chunkBlockPos.X < 0 || chunkBlockPos.X >= Chunk.SIZE ||
               chunkBlockPos.Z < 0 || chunkBlockPos.Z >= Chunk.SIZE;
    }

    public static Vector3 ConvertToChunkCoords(Vector3 pos) {
        int posX, posY, posZ;
        posX = (int)Math.Floor(pos.X / 16);
        posY = 0;
        posZ = (int)Math.Floor(pos.Z / 16);
        return new Vector3(posX, posY, posZ)*Chunk.SIZE;
    }

    public static Vector3 ConvertToChunkBlockCoord(Vector3 pos) {
        return new Vector3(
            ModFloor((int)pos.X, Chunk.SIZE),
            ModFloor((int)pos.Y, Chunk.HEIGHT),
            ModFloor((int)pos.Z, Chunk.SIZE)
        );
    }

    private static int ModFloor(int value, int mod) {
        int result = value % mod;
        if (result < 0) result += mod; // Fix negative values
        return result;
    }

    public static void ConvertToWorldCoords(ref Vector3 currChunkPos) {
        if (currChunkPos.X > 0)
            --currChunkPos.X;
        if (currChunkPos.Z > 0)
            --currChunkPos.Z;

        currChunkPos *= Chunk.SIZE;
    }
    #endregion
}
