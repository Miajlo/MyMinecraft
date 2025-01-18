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

        int XbottomBound = (int)(currChunkPos.X) - renderBound * Chunk.SIZE; 
        int ZbottomBound = (int)(currChunkPos.Z) - renderBound * Chunk.SIZE;
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
