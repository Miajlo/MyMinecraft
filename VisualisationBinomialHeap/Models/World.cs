using System.Collections.Concurrent;

namespace MyMinecraft.Models; 
public class World {
    public Dictionary<string, Chunk> allChunks = new();
    public ConcurrentBag<Chunk> forRendering = new();
    public int renderDistance = 2;
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
        //var chunkID = $"{currChunkPos.X}, {currChunkPos.Y}, {currChunkPos.Z}";
        //Console.WriteLine($"World::currChunkID: {chunkID}");
        Chunk toAdd = new();

        int renderBound = renderDistance*renderDistance + 1;

        for (int i = 0; i < renderBound; ++i) {
            for (int j = 0; j < renderBound; ++j) {
                var chunkID = $"{currChunkPos.X + i}, {currChunkPos.Y}, {currChunkPos.Z}";
                Vector3 copyChunkPos = new(currChunkPos.X, currChunkPos.Y, currChunkPos.Z);
                copyChunkPos.X += i;
                copyChunkPos.Z += j;
                if (!allChunks.TryGetValue(chunkID, out toAdd)) {
                    Chunk.ConvertToWorldCoords(ref copyChunkPos);
                    Console.WriteLine($"World::currCunkPosition:{copyChunkPos}");
                    toAdd = new(copyChunkPos);
                    allChunks.Add(chunkID, toAdd);
                }
                else
                    Console.WriteLine($"Loaded chunk: {chunkID}");
                if (!forRendering.Contains(toAdd)) {
                    forRendering.Add(toAdd);
                }
            }
        }
     
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
