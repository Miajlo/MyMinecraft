using MyMinecraft.Models;

namespace MyMinecraft.Models_r; 
public  class World_r {
    #region CHUNK_DATA
    private HashSet<Vector3> generatedChunks;
    private ConcurrentDictionary<Vector3, Chunk_r> loadedChunks;
    #endregion

    #region WORLD_DATA
    public int seed;
    public NoiseSettings perlinNoise2DSettings;
    #endregion

    #region CONSTRUCTORS
    public World_r() {
        generatedChunks = [];
        loadedChunks = [];
        seed = 0;

        perlinNoise2DSettings = new();
    }

    public World_r(int seed, NoiseSettings settings):base() {
        this.seed = seed;
        perlinNoise2DSettings = settings;
    }
    #endregion

    #region CHUNK_DATA_METHODS
    public bool AddLoadedChunk(Chunk_r chunk) {
        if (!IsGenerated(chunk.position))
            generatedChunks.Add(chunk.position);

        return loadedChunks.TryAdd(chunk.position, chunk);
    }

    public Chunk_r? RemoveChunk(Vector3 position) {

        if (!loadedChunks.TryRemove(position, out var chunk))
            throw new ArgumentException($"Chunk not loaded: {position}");

        if (chunk.Dirty)
            chunk.SaveToFile();

        return chunk;
    }

    public Chunk_r? GetChunk(Vector3 pos) {
        Chunk_r? chunk;
        if (loadedChunks.TryGetValue(pos, out chunk))
            return chunk;       
        else if (generatedChunks.Contains(pos)) {
            chunk ??= new();
            chunk.LoadFromFile();
            return chunk;
        }
        else
            throw new Exception($"Could not get chunk: {pos}");
    }

    public bool IsGenerated(Vector3 pos) {
        return generatedChunks.Contains(pos);
    }
    #endregion

}
