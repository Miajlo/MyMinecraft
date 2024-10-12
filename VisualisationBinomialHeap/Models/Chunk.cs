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

    public Chunk(Vector3 pos) {
        position = pos;
        ID = $"{pos.X}, {pos.Y}, {pos.Z}";
        GenChunk();
        Console.WriteLine($"Generated chunk: [ {ID} ]");
        BuildChunk();
        Console.WriteLine($"Built chunk: [ {ID} ]");
    }

    public void GenChunk() {
        for (int y = 0; y < HEIGHT; ++y) {
            for (int i = 0; i < SIZE; ++i) {
                for (int j = 0; j < SIZE; ++j) {
                    Block block = new(new(i, y, j));

                    chunkBlocks[i, y, j] = block;

                    var frontData = block.GetFace(Faces.FRONT);
                    chunkVert.AddRange(frontData.vertices!);
                    chunkUVs.AddRange(frontData!.uvs!);
                    
                    var backData = block.GetFace(Faces.BACK);
                    chunkVert.AddRange(backData.vertices!);
                    chunkUVs.AddRange(backData!.uvs!);

                    var leftData = block.GetFace(Faces.LEFT);
                    chunkVert.AddRange(leftData.vertices!);
                    chunkUVs.AddRange(leftData!.uvs!);

                    var rightData = block.GetFace(Faces.RIGHT);
                    chunkVert.AddRange(rightData.vertices!);
                    chunkUVs.AddRange(rightData!.uvs!);

                    var topData = block.GetFace(Faces.TOP);
                    chunkVert.AddRange(topData.vertices!);
                    chunkUVs.AddRange(topData!.uvs!);

                    var bottomData = block.GetFace(Faces.BOTTOM);
                    chunkVert.AddRange(bottomData.vertices!);
                    chunkUVs.AddRange(bottomData!.uvs!);

                    AddInceces(6);
                }
            }
        }
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
