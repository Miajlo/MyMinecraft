﻿using MyMinecraft.Models;

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
        //if (!IsGenerated(chunk.position))
        //    generatedChunks.Add(chunk.position);

        return loadedChunks.TryAdd(chunk.position, chunk);
    }

    public Chunk_r? RemoveChunk(Vector3 position) {

        if (!loadedChunks.TryRemove(position, out var chunk))
           throw new ArgumentException($"[ERROR]: Chunk not loaded: {position}");

        if (chunk.Dirty)
            chunk.SaveToFile();

        return chunk;
    }

    public bool GetChunk(Vector3i pos, out Chunk_r? chunk) {
        return loadedChunks.TryGetValue(pos, out chunk);
    }

    public BlockType GetBlockAt(Vector3 chunkPos, Vector3 blockPos) {
        if (!loadedChunks.TryGetValue(chunkPos, out var chunk))
            return BlockType.AIR;

        return chunk.chunkBlocks[(int)blockPos.X, (int)blockPos.Y, (int)blockPos.Z].type;
    }
    /// <summary>
    /// Get the block at the specified global position
    /// </summary>
    /// <param name="blockPos"></param>
    /// <returns>Block type at position, or AIR if chunk not loaded</returns>
    public BlockType GetBlockAt(Vector3 blockPos) {
        var chunkPos = Chunk_r.ConvertToChunkCoords(blockPos);
        
        if(!loadedChunks.TryGetValue(chunkPos, out var chunk))
            return BlockType.AIR;

        var chunkBlockPos = Chunk_r.ConvertToChunkBlockCoord(blockPos);

        return GetBlockAt(chunkPos, chunkBlockPos);
    }


    public bool IsGenerated(Vector3 pos) {
        return generatedChunks.Contains(pos);
    }

    public bool IsLoadedChunk(Vector3 position) {
        return loadedChunks.ContainsKey(position);
    }

    internal void AddTreesToLoadedChunks() {
        throw new NotImplementedException();
    }

    public bool IsSolidBlock(Vector3 blockPosX) {
        Vector3i blockPos = Chunk_r.ConvertToChunkBlockCoord(blockPosX);
        Vector3 chunkPos = Chunk_r.ConvertToChunkCoords(blockPosX);
        BlockType block = GetBlockAt(chunkPos, blockPosX);

        return block != BlockType.AIR;
    }
    #endregion

}
