namespace MyMinecraft.Models;
public class Chunk {
    //public string? ID;
    public bool firstDrawing = true;
    public List<Vector3> chunkVert = new();
    public List<Vector2> chunkUVs = new();
    public List<uint> chunkInd = new();
    public const byte SIZE = 16;
    public const byte HEIGHT = 64;

    public ChunkData chunkData;

    public Vector3 position;
    public byte[,] heightMap = new byte[SIZE,SIZE];

    public BlockType[,,] chunkBlocks = new BlockType[SIZE,HEIGHT,SIZE]; 

    public uint indexCount;
    public bool Built { get; set; } = false;
    public bool AddedFaces { get; set; } = false;
    public bool ReDraw { get; set; } = true;

    public byte neighbours = 0b_0000;

    VAO? chunkVAO;
    VBO? chunkVBO;
    VBO? chunkUVVBO;
    IBO? chunkIBO;

    //Texture? texture;

    public Chunk() {

    }

    public Chunk(Vector3 pos, byte neibours = 0b_1111) {
       

        //ID = $"{(pos.X + 16 * Math.Sign(pos.X) )/ 16 + 1}," +
        //     $" {(pos.Y + 16 * Math.Sign(pos.Y))/ 16}," +
        //     $" {(pos.Z + 16 * Math.Sign(pos.Z))/ 16 + 1}";


        position = pos;
        neighbours = neibours;
        //ID = ConvertPosToChunkID(position);
        Console.WriteLine($"Chunk::ID: {position}");
       
        Console.WriteLine($"Chunk::ChunkPosition:{position}");
        GenHeightMap();
        GenChunk();
        //AddFaces();
        Console.WriteLine($"Generated chunk: [ {position} ]");
        //BuildChunk();
        Console.WriteLine($"Built chunk: [ {position} ]");

    }

    private void AddFaces() {
        for (int x = 0; x < SIZE; ++x) {
            for (int z = 0; z < SIZE; ++z) {
                for (int y = 0; y < HEIGHT; ++y) {
                    //uint addedFaces = 0;
                    //IntegrateFace(chunkBlocks[x,y,z], Faces.FRONT);
                    //IntegrateFace(chunkBlocks[x, y, z], Faces.BACK);
                    //IntegrateFace(chunkBlocks[x, y, z], Faces.LEFT);
                    //IntegrateFace(chunkBlocks[x, y, z], Faces.RIGHT);

                    //neighbours = 0b_0000;

                    if (chunkBlocks[x, y, z] != BlockType.AIR) {
                        uint addedFaces = 0; // Reset for each block
                        Vector3 blockPos = new(x, y, z);
                        // Left face
                        bool addCurrentFace = (neighbours & 0b_0001) != 0 && x == 0;
                        if (!addCurrentFace && (x == 0 || chunkBlocks[x - 1, y, z] == BlockType.AIR)) {
                            IntegrateFace(chunkBlocks[x, y, z], Faces.LEFT, blockPos);
                            addedFaces++;
                        }
                        addCurrentFace = (neighbours & 0b_0100) != 0 && x == SIZE-1;
                        // Right face
                        if (!addCurrentFace && (x == SIZE - 1 || chunkBlocks[x + 1, y, z] == BlockType.AIR)) {
                            IntegrateFace(chunkBlocks[x, y, z], Faces.RIGHT, blockPos);
                            addedFaces++;
                        }
                        
                        // Bottom face
                        if (y == 0 || chunkBlocks[x, y - 1, z] == BlockType.AIR) {
                            IntegrateFace(chunkBlocks[x, y, z], Faces.BOTTOM, blockPos);
                            addedFaces++;
                        }

                        // Top face
                        if (y == HEIGHT - 1 || chunkBlocks[x, y + 1, z] == BlockType.AIR) {
                            IntegrateFace(chunkBlocks[x, y, z], Faces.TOP, blockPos);
                            addedFaces++;
                        }
                        addCurrentFace = (neighbours & 0b_0010) != 0 && z == 0;
                        // Back face
                        if (!addCurrentFace && (z == 0 || chunkBlocks[x, y, z - 1] == BlockType.AIR)) {
                            IntegrateFace(chunkBlocks[x, y, z], Faces.BACK, blockPos);
                            addedFaces++;
                        }
                        addCurrentFace = (neighbours & 0b_1000) != 0 && z == SIZE-1;
                        // Front face
                        if (!addCurrentFace && (z == SIZE - 1 || chunkBlocks[x, y, z + 1] == BlockType.AIR)) {
                            IntegrateFace(chunkBlocks[x, y, z], Faces.FRONT, blockPos);
                            addedFaces++;
                        }

                        // Add indices for the added faces
                        AddInceces(addedFaces);
                        //chunkBlocks[x, y, z].ClearFaceData();
                    }
                }
            }
        }
        chunkInd.TrimExcess();
        chunkVert.TrimExcess();
        chunkUVs.TrimExcess();
    }

    public void AddFacesWithMesh() {

    }

    public void GenHeightMap() {
        FastNoiseLite fnl = new();
        fnl.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        fnl.SetFrequency(0.01f);
        for (var x=0; x<SIZE;++x ) {
            for(var z=0; z<SIZE; ++z) {
                float noiseValue = fnl.GetNoise(x+position.X, z+position.Z);
                heightMap[x, z] = (byte)Math.Clamp(((noiseValue + 1) * 0.5f) * HEIGHT, 0, HEIGHT - 1);
            }
        }
    }

    public void GenChunk() {
        for (int x = 0; x < SIZE; ++x) {
            for (int z = 0; z < SIZE; ++z) {              
                for (int y = 0; y < HEIGHT; ++y) {
                    
                    //Block block = new(new Vector3(x, y, z), GetBlockType(y,x,z));
                    

                    chunkBlocks[x, y, z] = GetBlockType(y, x, z);

                    //IntegrateFace(block, Faces.FRONT);
                    //IntegrateFace(block, Faces.BACK);
                    //IntegrateFace(block, Faces.LEFT);
                    //IntegrateFace(block, Faces.RIGHT);
                    //IntegrateFace(block, Faces.TOP);
                    //IntegrateFace(block, Faces.BOTTOM);

                    //AddInceces(6);
                }
            }
        }
    }

    public void IntegrateFace(BlockType block, Faces face, Vector3 blockPos) {
        //block.AddFace(face);
        var faceData = Block.GetFaceData(face, blockPos);
        chunkVert.AddRange(faceData);
        chunkUVs.AddRange(TextureData.GetUVs(block, face));
    }

    public void AddInceces(uint indCount) {
        for(int i=0; i < indCount; ++i) {
            chunkInd.Add(0 + indexCount);
            chunkInd.Add(1 + indexCount);
            chunkInd.Add(2 + indexCount);
            chunkInd.Add(2 + indexCount);
            chunkInd.Add(3 + indexCount);
            chunkInd.Add(0 + indexCount);

            indexCount += 4;
        }
    }

    public void BuildChunk() {
        chunkInd ??= new();
        chunkVert ??= new();
        chunkUVs ??= new();

        //if (!Built)
        //    AddFaces();

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

        if (firstDrawing) {
            Console.WriteLine($"Drew chunk: [ {position} ]");
            firstDrawing = false;
        } 

        Texture.Unbind();
        chunkVAO?.Unbind();
        chunkIBO?.Unbind();
        chunkUVVBO?.Unbind();
        chunkVBO?.Unbind();
    }

    public Vector3 NormalizedChunkPos {
        get {
            int posX, posY, posZ;

            posX = (int)((position.X -  position.X % 16)/ 16 + 1 * Math.Sign(position.X));
            posY = 0;
            posZ = (int)((position.Z -  position.Z % 16) / 16 + 1 * Math.Sign(position.Z));
            return new(posX, posY, posZ);
        }
    }

    public static string NormalizeChunkID(Vector3 position) {
        int posX, posY, posZ;

        posX = (int)(position.X -  position.X % 16)/ 16;
        posY = 0;
        posZ = (int)(position.Z -  position.Z % 16) / 16;

        posX += posX < 0 ? 1 : 0;
        posZ += posZ < 0 ? 1 : 0;

        return $"{posX},{posZ}";
    }

    public static string ConvertPosToChunkID(Vector3 pos) {
        int xID = (int)Math.Floor(pos.X / 16);
        int zID = (int)Math.Floor(pos.Z / 16);



        if (xID >= 0)
            ++xID;
        if (zID >= 0)
            ++zID;


        //if (xID == 1 && zID == 1) {
        //    xID = 1;
        //    zID = 1;
        //}
        return $"{xID},{zID}";
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
        //texture?.Delete();
        //GL.DeleteTexture(texture!.ID);  // Explicitly delete texture
        GL.Finish();

        chunkInd.Clear();
        chunkVert.Clear();
        chunkUVs.Clear();

        indexCount = 0;

        Built = false;
    }

    public void ClearBlockFaceData() {
        Parallel.For(0, HEIGHT, y => 
        {
            for (int i = 0; i < SIZE; ++i) {
                for (int j = 0; j < SIZE; ++j) {
                    //chunkBlocks[i, y, j].ClearFaceData();
                }
            }
        });
        //for (int y = 0; y < HEIGHT; ++y) {
            
        //}
    }

    public static void ConvertToWorldCoords(ref Vector3 currChunkPos) {
        if (currChunkPos.X > 0)
            --currChunkPos.X;
        if (currChunkPos.Z > 0)
            --currChunkPos.Z;

        currChunkPos *= Chunk.SIZE;
    }

    private BlockType GetBlockType(int height, int x, int z) {
        if (height == heightMap[x, z])
            return BlockType.GRASS_BLOCK;
        else if (height < heightMap[x, z])
            return BlockType.STONE;
        else
            return BlockType.AIR;
    }


    public void SetBlockAt(Vector3 chunkBlockPos, BlockType blockType) {
        if (InvalidBlockCoords(chunkBlockPos))
            return;
        chunkBlockPos = Chunk.ConvertToChunkBlockCoord(chunkBlockPos);
        chunkBlocks[(int)chunkBlockPos.X, (int)chunkBlockPos.Y, (int)chunkBlockPos.Z] = blockType;
    }

    public static bool InvalidBlockCoords(Vector3 chunkBlockPos) {
        return chunkBlockPos.Y < 0 || chunkBlockPos.Y >= Chunk.HEIGHT ||
               chunkBlockPos.X < 0 || chunkBlockPos.X >= Chunk.SIZE ||
               chunkBlockPos.Z < 0 || chunkBlockPos.Z >= Chunk.SIZE;
    }

    public BlockType GetBlockAt(Vector3 chunkBlockPos) {

        if(InvalidBlockCoords(chunkBlockPos))
            return BlockType.AIR;

        return chunkBlocks[(int)chunkBlockPos.X, (int)chunkBlockPos.Y, (int)chunkBlockPos.Z];
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

    public static Vector3 ConvertToChunkRelativeCoord(Vector3 pos) {
        Vector3 alignedPos = Camera.GetChunkPos(pos);
        Chunk.ConvertToWorldCoords(ref alignedPos);

        // Compute local position within the chunk
        return new Vector3(
            ((int)pos.X - alignedPos.X),
            (int)pos.Y, // Y remains unchanged
            ((int)pos.Z - alignedPos.Z)
        );
    }
    private static int ModFloor(int value, int mod) {
        int result = value % mod;
        if (result < 0) result += mod; // Fix negative values
        //if (result > 0) result = (int)value - ;
        return result;
    }
}
