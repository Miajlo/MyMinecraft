using MyMinecraft.Graphics;
using System.Collections.Concurrent;

namespace MyMinecraft.Models; 
public class World {
    public Dictionary<string, Chunk> allChunks = new();
    public ConcurrentBag<Chunk> forRendering = new();
    
    public int renderDistance = 3;
    public bool readyToRender = false;
    public object locker = new();
    public void AddChunk(Chunk chunk) {
        if (allChunks.TryAdd(chunk.ID!, chunk))
            Console.WriteLine($"Succesfully saved chunk: {chunk.ID}");
        else
            Console.WriteLine($"Error adding chunk: {chunk.ID}");
    }

    public void UpdateChunkRanderList(Vector3 pos) {
        //RemoveFarChunks(pos);
        //AddChunksToRender(pos);
        Task.Run(() => {
            RemoveFarChunks(pos);
            AddChunksToRender(pos);
        });
    }

    private void AddChunksToRender(Vector3 pos) {
        readyToRender = false;
        var currChunkPos = Camera.GetChunkPos(pos);
        Chunk.ConvertToWorldCoords(ref currChunkPos);
        var chunkID = $"{currChunkPos.X}, {currChunkPos.Y}, {currChunkPos.Z}";
        Console.WriteLine($"World::currChunkID: {chunkID}");
        Chunk toAdd = new();

        int renderBound = renderDistance;
        int totalIterations = 0;

        int XbottomBound = (int)(currChunkPos.X) - (renderBound) * Chunk.SIZE; 
        int ZbottomBound = (int)(currChunkPos.Z) - (renderBound) * Chunk.SIZE;
        int XtopBound = XbottomBound + (renderBound + 1) * Chunk.SIZE;
        int ZtopBound = ZbottomBound + (renderBound + 1) * Chunk.SIZE;

  

        for (int i = XbottomBound; i <= XtopBound; i+=Chunk.SIZE) {
            for(int j = ZbottomBound; j <= ZtopBound; j+=Chunk.SIZE) {
                Console.WriteLine($"i = {i}, j = {j}");
                Vector3 copyChunkPos = new(i, 0, j);
                chunkID = Chunk.ConvertPosToChunkID(copyChunkPos);

                if (!allChunks.TryGetValue(chunkID, out toAdd)) {
                    
                    Console.WriteLine($"World::currCunkPosition:{copyChunkPos}");
                    toAdd = new(copyChunkPos);
                    allChunks.Add(chunkID, toAdd);
                }
                else
                    Console.WriteLine($"Loaded chunk: {chunkID}");
                if (!forRendering.Contains(toAdd)) {
                    forRendering.Add(toAdd);
                }
                ++totalIterations;
            }
            
        }
        Console.WriteLine($"TotalIterations: {totalIterations}");
    }

    public void RenderChunks(ShaderProgram program) {      
        foreach (var chunk in forRendering) {
            if (!chunk.Built)
                chunk.BuildChunk();
            chunk.Render(program);
            chunk.Rendered = true;
        }      
    }

    public void DrawChunkBorders(ShaderProgram program, Vector3 pos) {
        
        program.Bind();

        var currChunk = Camera.GetChunkPos(pos);

        List<Vector3> verts = new List<Vector3> {
            new Vector3(0.0f, 0.0f, 0.0f),  // Start of line
            new Vector3(0.0f, 255.0f, 0.0f) // End of line
        };

        List<uint> ind = new List<uint> { 0, 1 };  // Indices for line: vertex 0 -> vertex 1

        Matrix4 model = Matrix4.CreateTranslation(new(0,0,0));
        int modelLocation = GL.GetUniformLocation(program.ID, "model");
        GL.UniformMatrix4(modelLocation, true, ref model);

        // Create VAO for the line
        VAO lineVao = new VAO();
        lineVao.Bind();

        // Create VBO for the vertex data
        VBO lineVBO = new VBO(verts);
        lineVBO.Bind();
        lineVao.LinkToVAO(0, 3, lineVBO);  // 0 is the attribute index, 3 components per vertex (x, y, z)

        // Create IBO for the index data
        IBO lineIBO = new IBO(ind);
        lineIBO.Bind();
        GL.LineWidth(5.0f);

        lineVao.Bind();
        lineVBO.Bind();
        lineIBO.Bind();

        GL.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);
        //GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        // Draw the line
        GL.DrawElements(PrimitiveType.Lines, ind.Count, DrawElementsType.UnsignedInt, 0);

        // Unbind the buffers
        lineIBO.Unbind();
        lineVao.Unbind();
        lineVBO.Unbind();
    }

    private void RemoveFarChunks(Vector3 pos) {
        List<Chunk> toRemove = new(); 

        foreach(var chunk in forRendering) {
            var playerPos = Camera.GetChunkPos(pos);
            var chunkPos = chunk.NormalizedChunkPos;
            if (Math.Abs(playerPos.X - chunkPos.X) >= renderDistance ||
                Math.Abs(playerPos.Z - chunkPos.Z) >= renderDistance) {
                //chunk.Unload();
                toRemove.Add(chunk);
            }
        }

        var newForRendering = new ConcurrentBag<Chunk>(forRendering.Except(toRemove));
        forRendering = newForRendering;
    }
}
