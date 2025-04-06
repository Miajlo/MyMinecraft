namespace MyMinecraft.Models_r; 
public class Chunk_r {
    #region CONSTANTS
    public const byte SIZE = 16;
    public const short HEIGHT = 64;
    #endregion

    #region CHUNK_DATA
    public Vector3i position;
    public Block_r[,,] chunkBlocks;
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
    public volatile bool Built;
    public volatile bool Redraw;
    public volatile bool AddedFaces;
    public volatile bool FirstDrawing;
    public volatile bool Generated;
    public volatile bool DecorationAdded;
    public volatile bool Dirty;
    public volatile bool ShouldClearData;
    public volatile bool UsedForStructGen;
    #endregion

    #region CONSTRUCTORS
    public Chunk_r() {
        Built= false;
        Redraw = true;
        AddedFaces = false;
        FirstDrawing = true;
        Dirty = false;
        ShouldClearData = false;
        UsedForStructGen = false;

        chunkBlocks = new Block_r[SIZE, HEIGHT, SIZE];
        heightMap = new short[SIZE, SIZE];

        InitializeBlocks();

        chunkUVs = [];
        chunkVert = [];
        chunkInd = [];

        indexCount = 0;     
    }

    public Chunk_r(Vector3i position) : this() {
        this.position = position;
        GenHeightMap();
        //GenHeightmapSpline();
    }
    #endregion

    #region GENERATION_METHODS
    public void GenChunk() {

        
        GenChunkBlocks();
        //var treeLeftovers = GenTrees();

        Console.WriteLine($"Generated chunk: {position}");

        //return treeLeftovers;
    }

    private void InitializeBlocks() {
        for (int x = 0; x < SIZE; x++) {
            for (int y = 0; y < HEIGHT; y++) {
                for (int z = 0; z < SIZE; z++) {
                    chunkBlocks[x, y, z] = new();  // Custom initialization
                }
            }
        }
    }

    private void GenChunkBlocks() {
        for (int x = 0; x < SIZE; ++x) {
            for (int z = 0; z < SIZE; ++z) {
                for (int y = 0; y < HEIGHT; ++y) { 
                    chunkBlocks[x, y, z].type = chunkBlocks[x,y,z].type == BlockType.AIR ?
                                           GetBlockType(x, y, z):chunkBlocks[x,y,z].type;
                }
            }
        }
    }

    private void GenHeightMap() {
        FastNoiseLite fnl = new();
        FastNoiseLite fnl2 = new();
        fnl.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        fnl.SetFrequency(0.01f);

        fnl.SetFractalType(FastNoiseLite.FractalType.DomainWarpIndependent);  // fBm (Fractional Brownian Motion)
        fnl.SetFractalOctaves(4);      // Number of octaves
        fnl.SetFractalLacunarity(1.0f); // Frequency multiplier per octave
        fnl.SetFractalGain(0.25f);      // Amplitude multiplier per octave


        fnl2.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        fnl2.SetFrequency(0.00001f);

        fnl2.SetFractalType(FastNoiseLite.FractalType.FBm);  // fBm (Fractional Brownian Motion)
        fnl2.SetFractalOctaves(8);      // Number of octaves
        fnl2.SetFractalLacunarity(4f); // Frequency multiplier per octave
        fnl2.SetFractalGain(0.7f);

        for (var x = 0; x < SIZE; ++x) {
            for (var z = 0; z < SIZE; ++z) {
                float posX = position.X + x;
                float posZ = position.Z + z;
                float noiseValue = fnl.GetNoise(posX, posZ)*HEIGHT*0.5f + fnl2.GetNoise(posX, posZ)*HEIGHT*0.35f + HEIGHT/2;
                heightMap[x, z] = (short)Math.Clamp((noiseValue + 1), 0, HEIGHT - 1);
            }
        }
    }

    //public void GenHeightmapSpline() {
    //    TerrainEvaluator tfe = new(HEIGHT);

    //    FastNoiseLite erosion = new();
    //    FastNoiseLite continent = new();
    //    FastNoiseLite peakAndVally = new();
    //    erosion.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
    //    erosion.SetFrequency(10f);
    //    erosion.SetFractalType(FastNoiseLite.FractalType.DomainWarpIndependent);
    //    erosion.SetFractalOctaves(4);
    //    erosion.SetFractalLacunarity(1.0f);
    //    erosion.SetFractalGain(1.1f);

    //    // Continentalness noise
    //    continent.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
    //    continent.SetFrequency(0.01f);
    //    continent.SetFractalType(FastNoiseLite.FractalType.DomainWarpIndependent);
    //    continent.SetFractalOctaves(8);
    //    continent.SetFractalLacunarity(4f);
    //    continent.SetFractalGain(10f);

    //    // Peaks and valleys noise
    //    peakAndVally.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
    //    peakAndVally.SetFrequency(0.005f); // Add a base frequency
    //    peakAndVally.SetFractalType(FastNoiseLite.FractalType.DomainWarpProgressive);
    //    peakAndVally.SetFractalOctaves(8);
    //    peakAndVally.SetFractalLacunarity(10f);
    //    peakAndVally.SetFractalGain(0.5f);

    //    for (var x = 0; x < SIZE; ++x) {
    //        for (var z = 0; z < SIZE; ++z) {
    //            float posX = position.X + x;
    //            float posZ = position.Z + z;
    //            float erosionVal = erosion.GetNoise(posX, posZ);
    //            float continentVal = continent.GetNoise(posX, posZ);
    //            float peakAndValley = peakAndVally.GetNoise(posX, posZ);

    //            float height = tfe.EvaluateHeight(erosionVal, continentVal, peakAndValley);

    //            heightMap[x, z] = (short)Math.Min(height, HEIGHT - 1);
    //        }
    //    }

    //}

    public Dictionary<Vector3i,CrossChunkData> GenTrees() {
        FastNoiseLite treeNoise = new FastNoiseLite();
        treeNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);
        treeNoise.SetFrequency(0.5f);  // Slightly higher frequency for better spread
        treeNoise.SetFractalOctaves(8);      // Number of octaves
        treeNoise.SetFractalLacunarity(0.0001f); // Frequency multiplier per octave
        treeNoise.SetFractalGain(0.0004f);
        treeNoise.SetSeed(98765);

        Dictionary<Vector3i,CrossChunkData> treeLeftovers = [];

        for (int x = 0; x < SIZE; x++) {
            for (int z = 0; z < SIZE; z++) {
                float treeValue = treeNoise.GetNoise(position.X+x, position.Z+z);

                // Less strict threshold & extra randomness to break patterns
                if (treeValue > 0.55f && treeValue < 0.6f && Hash(x, z) % 3 == 0) {
                    PlaceTree(x, z, treeLeftovers);
                }
            }
        }

        return treeLeftovers;
    }

    private int Hash(int x, int z) {
        return (x * 735761 + z * 902002) ^ 54321;
    }

    private void PlaceTree(int sx, int sz, Dictionary<Vector3i, CrossChunkData> dict) {      
        int height = heightMap[sx, sz];
        sx-=3;
        sz-=3;

        for (int y = 0; y < Tree.HEIGHT; ++y) {
            for (int x = 0; x < Tree.SIZE; ++x) {
                for (int z = 0; z< Tree.SIZE; ++z) {
                    Vector3i setBlockPos = new(sx+x, y+height, sz+z);
                    if (InvalidBlockCoords(setBlockPos)) {
                        Vector3i copySetBlockPos = setBlockPos;                        
                        Vector3i chunkPos = (Vector3i)ConvertToChunkCoords((Vector3i)(setBlockPos+position));
                        copySetBlockPos = Chunk_r.ConvertToChunkBlockCoord(copySetBlockPos);

                        Vector3i chunkBlockPos = Chunk_r.ConvertToChunkBlockCoord(copySetBlockPos);

                        // Ensure the chunk entry exists
                        if (!dict.TryGetValue(chunkPos, out var data)) {
                            data = new(); // or whatever type
                            dict[chunkPos] = data;
                        }

                        data.TryAddBlock(chunkBlockPos, new Block_r((BlockType)Tree.treeBlocks[y, x, z]));



                        continue; // for now ignore chunk borders
                    }
                    if (chunkBlocks[sx+x, y+height, sz+z].type == BlockType.AIR)
                        SetBlockAt(setBlockPos, (BlockType)Tree.treeBlocks[y, x, z]);
                }
            }
        }
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

    public void DeleteGL() {
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
    public void Delete() {
        chunkVBO?.Delete();
        chunkUVVBO?.Delete();
        chunkVAO?.Delete();
        chunkIBO?.Delete();

        GL.BindVertexArray(0);
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);


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
        chunkUVs.AddRange(TextureData.GetUVs(block, face, Faces.TOP));
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
        chunkBlockPos = Chunk_r.ConvertToChunkBlockCoord(chunkBlockPos);
        chunkBlocks[(int)chunkBlockPos.X, (int)chunkBlockPos.Y, (int)chunkBlockPos.Z].type = blockType;
    }

    public void PlaceBlockAt(Vector3 chunkBlockPos, BlockType blockType, Faces rotation) {
        if (InvalidBlockCoords(chunkBlockPos)) {
            Console.WriteLine($"Invalid block position to set: {chunkBlockPos}");
            return;
        }
        chunkBlockPos = Chunk_r.ConvertToChunkBlockCoord(chunkBlockPos);
        chunkBlocks[(int)chunkBlockPos.X, (int)chunkBlockPos.Y, (int)chunkBlockPos.Z].type = blockType;
        chunkBlocks[(int)chunkBlockPos.X, (int)chunkBlockPos.Y, (int)chunkBlockPos.Z].rotation = rotation;

    }

    public BlockType GetBlockAt(Vector3 chunkBlockPos) {

        if (InvalidBlockCoords(chunkBlockPos)) {
            throw new ArgumentException($"Invalid chunk block pos: {chunkBlockPos}, chunk:{position}");
        }

        return chunkBlocks[(int)chunkBlockPos.X, (int)chunkBlockPos.Y, (int)chunkBlockPos.Z].type;
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
        return chunkBlockPos.Y < 0 || chunkBlockPos.Y >= Chunk_r.HEIGHT ||
               chunkBlockPos.X < 0 || chunkBlockPos.X >= Chunk_r.SIZE ||
               chunkBlockPos.Z < 0 || chunkBlockPos.Z >= Chunk_r.SIZE;
    }

    public static Vector3 ConvertToChunkCoords(Vector3 pos) {        
        int posX, posY, posZ;
        posX = (int)Math.Floor(pos.X / Chunk_r.SIZE);
        posY = 0;
        posZ = (int)Math.Floor(pos.Z / Chunk_r.SIZE);
        return new Vector3(posX, posY, posZ)*Chunk_r.SIZE;
    }

    public static Vector3i ConvertToChunkBlockCoord(Vector3 pos) {
        int chunkX = (int)Math.Floor(pos.X / Chunk_r.SIZE);
        int chunkZ = (int)Math.Floor(pos.Z / Chunk_r.SIZE);

        return new Vector3i(
            (int)Math.Floor(pos.X) - chunkX * Chunk_r.SIZE,
            Math.Clamp((int)Math.Floor(pos.Y), 0, Chunk_r.HEIGHT - 1),
            (int)Math.Floor(pos.Z) - chunkZ * Chunk_r.SIZE
        );
    }

    //private static int ModFloor(int value, int mod) {
    //    int result = value % mod;
    //    if (result < 0) result += mod; // Fix negative values
    //    return result;
    //}

    private static int ModFloor(int value, int mod) {
        return ((value % mod) + mod) % mod;
    }


    public static void ConvertToWorldCoords(ref Vector3i currChunkPos) {
        if (currChunkPos.X > 0)
            --currChunkPos.X;
        if (currChunkPos.Z > 0)
            --currChunkPos.Z;

        currChunkPos *= Chunk_r.SIZE;
    }
    #endregion
}
