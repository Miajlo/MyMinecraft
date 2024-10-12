using VisualisationBinomialHeap.Graphics;

namespace VisualisationBinomialHeap.Models; 
public class Chunk {
    public string ID;
    public bool firstDrawing = true;
    public List<Vector3> chunkVert = new();
    public List<Vector2> chunkUVs = new();
    public List<uint> chunkInd = new();

    public const int SIZE = 16;
    public const int HEIGHT = 16;

    public Vector3 position;

    public Block[,,] chunkBlocks = new Block[SIZE,HEIGHT,SIZE]; 

    public uint indexCount;

    VAO chunkVAO;
    VBO chunkVBO;
    VBO chunkUVVBO;
    IBO chunkIBO;

    Texture texture;

    public Chunk() {

    }

    public Chunk(Vector3 pos) {
        position = pos;
        ID = $"{pos.X}, {pos.Y}, {pos.Z}";
        GenChunk();
        AddFaces();
        Console.WriteLine($"Generated chunk: [ {ID} ]");
        BuildChunk();
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
                    }
                }
            }
        }
    }

    public void GenChunk() {
        for (int y = 0; y < HEIGHT; ++y) {
            for (int i = 0; i < SIZE; ++i) {
                for (int j = 0; j < SIZE; ++j) {
                    Block block = new(new(i, y, j), BlockType.STONE);

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

        texture = new("../../../Resources/MyStoneBlock.png");
    }

    public void Render(ShaderProgram program) {      
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
    public void Delete() {
        chunkIBO.Delete();
        chunkVAO.Delete();
        chunkVBO.Delete();
        texture.Delete();
    }
}
