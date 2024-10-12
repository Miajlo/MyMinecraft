namespace MyMinecraft.Models; 
public class World {
    public Dictionary<string, Chunk> allChunks = new();

    public void AddChunk(Chunk chunk) {
        if (allChunks.TryAdd(chunk.ID, chunk))
            Console.WriteLine($"Succesfully saved chunk: {chunk.ID}");
        else
            Console.WriteLine($"Error adding chunk: {chunk.ID}");
    }
}
