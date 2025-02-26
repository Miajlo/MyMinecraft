namespace MyMinecraft.Models_r; 
public class Server_r {
    #region THREAD_CONTEXT
    private Thread genThread;
    private Thread meshThread;

    private volatile bool runGenThread;
    private volatile bool runMeshThread;

    private SemaphoreSlim genSem;
    private SemaphoreSlim meshSem;

    private const string tMeshName = "MeshThread";
    private const string tGenName = "GenThread";

    private ConcurrentQueue<Vector3i> chunksToGen;
    private ConcurrentQueue<Chunk_r> chunksToMesh;
    private ConcurrentDictionary<Vector3, Chunk_r> chunksToRender;
    #endregion

    #region DATA
    private World_r world;
    private Camera camera;
    private int renderDistance;
    #endregion
    #region CONSTRUCTORS
    public Server_r() {
        meshThread = new Thread(MeshChunks);
        genThread = new Thread(GenChunks);

        meshThread.Name = tMeshName;
        genThread.Name = tGenName;

        chunksToGen = new();
        chunksToMesh = new();
        chunksToRender = new();

        genSem = new SemaphoreSlim(0);
        meshSem = new SemaphoreSlim(0);

        runGenThread = runMeshThread = true;
    }
    public Server_r(World_r world, Camera camera, int renderDistance) : this() {
        this.world = world;
        this.camera = camera;
        this.renderDistance = renderDistance;
    }
    #endregion

    #region GEN_METHODS
    private void GenChunks() {
        while(runGenThread) {
            genSem.Wait();

            while(chunksToGen.TryDequeue(out var chunkPos)) {
                Chunk_r? toAddChunk;

                if (world.IsLoadedChunk(chunkPos)) { // if loaded get it
                    world.GetChunk(chunkPos, out toAddChunk);
                }
                else { //if not loaded generate it, it will need a check if not genrated later
                    toAddChunk = new(chunkPos);
                    toAddChunk.GenChunk();
                    world.AddLoadedChunk(toAddChunk);
                }

                if(toAddChunk != null) { //add to meshing queue
                    chunksToMesh.Enqueue(toAddChunk);
                    meshSem.Release();
                }
            }
        }
    }

    private void AddChunksToGen(Vector3 currChunkPos) {
        int offset = renderDistance * Chunk.SIZE;

        int XbottomBound = (int)(currChunkPos.X) - offset;
        int ZbottomBound = (int)(currChunkPos.Z) - offset;
        int XtopBound = XbottomBound + 2*offset;
        int ZtopBound = ZbottomBound + 2*offset;

        for (int i = XbottomBound; i <= XtopBound; i+=Chunk.SIZE) {
            for (int j = ZbottomBound; j <= ZtopBound; j+=Chunk.SIZE) {
                //Console.WriteLine($"i = {i}, j = {j}");
                Vector3i copyChunkPos = new(i, 0, j);

                chunksToGen.Enqueue(copyChunkPos);
                //genSem.Release();
            }

        }

    }

    private void RemoveFarChunks(Vector3 currChunkPos) {
        foreach (var chunk in chunksToRender.Values) {
            var chunkPos = chunk.position;
            if (Math.Abs(currChunkPos.X - chunkPos.X)/Chunk.SIZE >= renderDistance + 1 ||
                Math.Abs(currChunkPos.Z - chunkPos.Z)/Chunk.SIZE >= renderDistance + 1) {
                //chunk.Unload();
                chunk.Delete();
                _ = chunksToRender.TryRemove(chunk.position, out _);
                _ = world.RemoveChunk(chunk.position);
            }
        }
    }

    public void UpdateQueues() {
        var currChunkPos = Camera.GetChunkPos(camera.position);
        Chunk.ConvertToWorldCoords(ref currChunkPos);
        if (camera.lastChunkPos != currChunkPos && camera.gameRules.generateChunks) {

            RemoveFarChunks(currChunkPos);
            AddChunksToGen(currChunkPos);
            genSem.Release();
            camera.lastChunkPos = currChunkPos;
        }

    }

    #endregion

    #region MESH_METHODS
    private void MeshChunks() {
        while(runMeshThread) {
            meshSem.Wait();

            if(chunksToMesh.TryDequeue(out var chunk)) {
                MeshChunk(chunk);
            }
        }
    }

    private void MeshChunk(Chunk_r chunk) {
        if (chunk.AddedFaces || !chunk.Redraw)
            return;

        for (var x = 0; x<Chunk_r.SIZE; ++x) {
            for (var z = 0; z<Chunk_r.SIZE; ++z) {
                for (var y = 0; y<Chunk_r.HEIGHT; ++y) {
                    if (chunk.chunkBlocks[x, y, z] != BlockType.AIR) {
                        uint addedFaces = 0; // Reset for each block
                        Vector3 blockPos = new(x, y, z);
                        // Left face
                        Vector3 neighbourBlockPos = new(Chunk_r.SIZE-1, y, z);
                        Vector3 neighbourChunkPos = new(chunk.position.X-16, chunk.position.Y, chunk.position.Z);
                        BlockType neighbourBlock = world.GetBlockAt(neighbourChunkPos, neighbourBlockPos);

                        if ((neighbourBlock==BlockType.AIR && x==0) || (x!=0 && chunk.chunkBlocks[x - 1, y, z] == BlockType.AIR)) {
                            chunk.IntegrateFace(chunk.chunkBlocks[x, y, z], Faces.LEFT, blockPos);
                            addedFaces++;
                        }
                        neighbourBlockPos = new(0, y, z);
                        neighbourChunkPos = new(chunk.position.X+16, chunk.position.Y, chunk.position.Z);
                        neighbourBlock = world.GetBlockAt(neighbourChunkPos, neighbourBlockPos);
                        // Right face
                        if ((neighbourBlock==BlockType.AIR && x == Chunk_r.SIZE - 1) || (x != Chunk_r.SIZE-1 && chunk.chunkBlocks[x + 1, y, z] == BlockType.AIR)) {
                            chunk.IntegrateFace(chunk.chunkBlocks[x, y, z], Faces.RIGHT, blockPos);
                            addedFaces++;
                        }

                        // Bottom face
                        if (y == 0 || chunk.chunkBlocks[x, y - 1, z] == BlockType.AIR) {
                            chunk.IntegrateFace(chunk.chunkBlocks[x, y, z], Faces.BOTTOM, blockPos);
                            addedFaces++;
                        }

                        // Top face
                        if (y == Chunk_r.HEIGHT - 1 || chunk.chunkBlocks[x, y + 1, z] == BlockType.AIR) {
                            chunk.IntegrateFace(chunk.chunkBlocks[x, y, z], Faces.TOP, blockPos);
                            addedFaces++;
                        }
                        neighbourBlockPos = new(x, y, Chunk_r.SIZE-1);
                        neighbourChunkPos = new(chunk.position.X, chunk.position.Y, chunk.position.Z-16);
                        neighbourBlock = world.GetBlockAt(neighbourChunkPos, neighbourBlockPos);
                        // Back face
                        if ((neighbourBlock == BlockType.AIR && z == 0)|| (z != 0 && chunk.chunkBlocks[x, y, z - 1] == BlockType.AIR)) {
                            chunk.IntegrateFace(chunk.chunkBlocks[x, y, z], Faces.BACK, blockPos);
                            addedFaces++;
                        }
                        neighbourBlockPos = new(x, y, 0);
                        neighbourChunkPos = new(chunk.position.X, chunk.position.Y, chunk.position.Z+16);
                        neighbourBlock = world.GetBlockAt(neighbourChunkPos, neighbourBlockPos);
                        // Front face
                        if ((neighbourBlock == BlockType.AIR && z == Chunk_r.SIZE - 1) || (z != Chunk_r.SIZE-1 && chunk.chunkBlocks[x, y, z + 1] == BlockType.AIR)) {
                            chunk.IntegrateFace(chunk.chunkBlocks[x, y, z], Faces.FRONT, blockPos);
                            addedFaces++;
                        }

                        // Add indices for the added faces
                        chunk.AddIndeces(addedFaces);
                        //chunkBlocks[x, y, z].ClearFaceData();
                        chunk.AddedFaces = true;
                        chunk.Redraw = false;
                    }
                }
            }
        }
        if (!chunksToRender.ContainsKey(chunk.position)) {
            chunksToRender.TryAdd(chunk.position, chunk);
        }
    }

    public void AddChunkToMesh(Chunk_r chunk) {
        chunksToMesh.Enqueue(chunk);
    }

    public void ReleaseMeshSem() {
        meshSem.Release();
    }
    #endregion

    #region SERVER_CONTROL_METHODS
    public void Start() {
        genThread.Start();
        meshThread.Start();

        Console.WriteLine("Succesfuly stared server....");
    }

    public void Stop() {
        runGenThread = false;
        runMeshThread = false;

        genSem.Release();
        meshSem.Release();

        genThread.Join();
        meshThread.Join();

        genSem.Dispose();
        meshSem.Dispose();

        DeleteChunkRenderData();
    }

    #endregion

    public void RenderChunks(ShaderProgram program) {
        foreach(var chunk in chunksToRender.Values) {
            if(!chunk.Built)
                chunk.BuildChunk();
            chunk.Render(program);
            chunk.AddedFaces = true;
        }
    }

    private void DeleteChunkRenderData() {
        foreach (var chunk in chunksToRender.Values)
            chunk.Delete();
    }
}
