namespace MyMinecraft.Models; 
public class Chunk {
    public string? ID;
    public bool firstDrawing = true;
    public List<Vector3> chunkVert = new();
    public List<Vector2> chunkUVs = new();
    public List<uint> chunkInd = new();

    public const int SIZE = 16;
    public const int HEIGHT = 16;

    public Vector3 position;

    public Block[,,] chunkBlocks = new Block[SIZE,HEIGHT,SIZE]; 

    public uint indexCount;
    public bool Built { get; set; } = false;
    public bool Rendered { get; set; } = false;
    VAO chunkVAO;
    VBO chunkVBO;
    VBO chunkUVVBO;
    IBO chunkIBO;

    Texture texture;

    public Chunk() {

    }

    public Chunk(Vector3 pos) {
       

        //ID = $"{(pos.X + 16 * Math.Sign(pos.X) )/ 16 + 1}," +
        //     $" {(pos.Y + 16 * Math.Sign(pos.Y))/ 16}," +
        //     $" {(pos.Z + 16 * Math.Sign(pos.Z))/ 16 + 1}";


        position = pos;
        ID = ConvertPosToChunkID(position);
        Console.WriteLine($"Chunk::ID: {ID}");
       
        Console.WriteLine($"Chunk::ChunkPosition:{position}");
        GenChunk();
        AddFaces();
        Console.WriteLine($"Generated chunk: [ {ID} ]");
        //BuildChunk();
        Console.WriteLine($"Built chunk: [ {ID} ]");

        
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


                    if (chunkBlocks[x, y, z].Type != BlockType.EMPTY) {
                        uint addedFaces = 0; // Reset for each block

                        // Left face
                        if (x == 0 || chunkBlocks[x - 1, y, z].Type == BlockType.EMPTY) {
                            IntegrateFace(chunkBlocks[x, y, z], Faces.LEFT);
                            addedFaces++;
                        }

                        // Right face
                        if (x == SIZE - 1 || chunkBlocks[x + 1, y, z].Type == BlockType.EMPTY) {
                            IntegrateFace(chunkBlocks[x, y, z], Faces.RIGHT);
                            addedFaces++;
                        }

                        // Bottom face
                        if (y == 0 || chunkBlocks[x, y - 1, z].Type == BlockType.EMPTY) {
                            IntegrateFace(chunkBlocks[x, y, z], Faces.BOTTOM);
                            addedFaces++;
                        }

                        // Top face
                        if (y == HEIGHT - 1 || chunkBlocks[x, y + 1, z].Type == BlockType.EMPTY) {
                            IntegrateFace(chunkBlocks[x, y, z], Faces.TOP);
                            addedFaces++;
                        }

                        // Back face
                        if (z == 0 || chunkBlocks[x, y, z - 1].Type == BlockType.EMPTY) {
                            IntegrateFace(chunkBlocks[x, y, z], Faces.BACK);
                            addedFaces++;
                        }

                        // Front face
                        if (z == SIZE - 1 || chunkBlocks[x, y, z + 1].Type == BlockType.EMPTY) {
                            IntegrateFace(chunkBlocks[x, y, z], Faces.FRONT);
                            addedFaces++;
                        }

                        // Add indices for the added faces
                        AddInceces(addedFaces);
                        chunkBlocks[x, y, z].ClearFaceData();
                    }
                }
            }
        }
        chunkInd.TrimExcess();
        chunkVert.TrimExcess();
        chunkUVs.TrimExcess();
    }

    public void GenChunk() {
        for (int y = 0; y < HEIGHT; ++y) {
            for (int i = 0; i < SIZE; ++i) {
                Random rnd = new Random();
                for (int j = 0; j < SIZE; ++j) {
                    Block block = new(new(i, y, j), y < 16
                                                      ? BlockType.STONE
                                                      : BlockType.EMPTY); ;

                    chunkBlocks[i, y, j] = block;

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

    public void IntegrateFace(Block block, Faces face) {
        block.AddFace(face);
        var faceData = block.GetFace(face);
        chunkVert.AddRange(faceData.vertices!);
        chunkUVs.AddRange(faceData!.uvs!);
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

        if (!Built && !firstDrawing)
            AddFaces();

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

        texture = new("../../../Resources/MyDirtBlock.png");

        Built = true;
    }

    public void Render(ShaderProgram program) {
        if (!Built)
            BuildChunk();

        program.Bind();
        chunkVAO.Bind();
        chunkIBO.Bind();
        Matrix4 model = Matrix4.CreateTranslation(position);
        int modelLocation = GL.GetUniformLocation(program.ID, "model");
        GL.UniformMatrix4(modelLocation, true, ref model);
        GL.ActiveTexture(TextureUnit.Texture0);
        texture.Bind();
        GL.Uniform1(GL.GetUniformLocation(program.ID, "texture0"), 0);
        GL.DrawElements(PrimitiveType.Triangles, chunkInd.Count,
                        DrawElementsType.UnsignedInt, 0);

        if (firstDrawing) {
            Console.WriteLine($"Drew chunk: [ {ID} ]");
            firstDrawing = false;
        } 

        texture.Unbind();
        chunkVAO.Unbind();
        chunkIBO.Unbind();
        chunkUVVBO.Unbind();
        chunkVBO.Unbind();
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
        chunkIBO.Unbind();
        chunkVAO.Unbind();
        chunkVBO.Unbind();
        texture.Unbind();
        Built = false;
    }

    public void Delete() {
        chunkIBO.Delete();
        chunkVAO.Delete();
        chunkVBO.Delete();
        texture.Delete();

        chunkInd = null;
        chunkVert = null;
        chunkUVs = null;

        indexCount = 0;

        Built = false;
    }

    public void ClearBlockFaceData() {
        Parallel.For(0, HEIGHT, y => 
        {
            for (int i = 0; i < SIZE; ++i) {
                for (int j = 0; j < SIZE; ++j) {
                    chunkBlocks[i, y, j].ClearFaceData();
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
}
