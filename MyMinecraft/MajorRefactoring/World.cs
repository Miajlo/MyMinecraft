namespace MyMinecraft.MajorRefactoring; 
public class World {
    internal static WorldOptions? Options { get; set; }
    internal ConcurrentDictionary<Vector3i, Chunk> loadedChunks;
    internal HashSet<Vector3i> generatedChunks; 
}
