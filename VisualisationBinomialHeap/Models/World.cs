namespace MyMinecraft.Models; 
public class World {
    public Dictionary<string, Chunk> allChunks = new();
    public List<Chunk> forRendering = new();
    public int renderDistance = -1;
    public void AddChunk(Chunk chunk) {
        if (allChunks.TryAdd(chunk.ID, chunk))
            Console.WriteLine($"Succesfully saved chunk: {chunk.ID}");
        else
            Console.WriteLine($"Error adding chunk: {chunk.ID}");
    }

    public void UpdateChunkRanderList(Vector3 pos) {
        RemoveFarChunks(pos);
        AddChunksToRender(pos);
    }
   
    private void AddChunksToRender(Vector3 pos) {
        var currChunkPos = Camera.GetChunkPos(pos);
        var chunkID = $"{currChunkPos.X}, {currChunkPos.Y}, {currChunkPos.Z}";
        //Console.WriteLine($"World::currChunkID: {chunkID}");
        Chunk toAdd = new();

        if (!allChunks.TryGetValue(chunkID, out toAdd)) {
            Chunk.ConvertToWorldCoords(ref currChunkPos);
            Console.WriteLine($"World::currCunkPosition:{currChunkPos}");
            toAdd = new(currChunkPos);
            allChunks.Add(chunkID, toAdd);
        }
        else
            Console.WriteLine($"Loaded chunk: {chunkID}");
        if (!forRendering.Contains(toAdd)) {
            forRendering.Add(toAdd);
        }
    }

    public void RenderChunks(ShaderProgram program) {
        foreach(var chunk in forRendering) {
            if (!chunk.Built)
                chunk.BuildChunk();
            chunk.Render(program);
        }
    }

    private void RemoveFarChunks(Vector3 pos) {
        for (int i = 0; i< forRendering.Count; ++i) {
            var playerPos = Camera.GetChunkPos(pos);
            var chunkPos = forRendering[i].NormalizedChunkPos;
            if (Math.Abs(playerPos.X - chunkPos.X) > renderDistance) {
                //forRendering[i].Unload();
                forRendering.RemoveAt(i);
            }
        }
    }
}
