namespace MyMinecraft.Models_r; 
public class Server_r {
    #region THREAD_CONTEXT
    private Thread genThread;
    private Thread meshThread;

    private volatile bool runGenThread;
    private volatile bool runMeshThread;

    private AutoResetEvent genEvent;
    private AutoResetEvent meshEvent;

    private const string tMeshName = "MeshThread";
    private const string tGenName = "GenThread";

    private ConcurrentQueue<Vector3> chunksToGen;
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

        chunksToGen = [];
        chunksToMesh = [];
        chunksToRender = [];
    }
    public Server_r(World_r world, Camera camera, int renderDistance) : base() {
        this.world = world;
        this.camera = camera;
    }
    #endregion

    #region GEN_METHODS
    private void GenChunks() {
        while(runGenThread) {
            genEvent.WaitOne();

            while(chunksToGen.TryDequeue(out Vector3 chunkPos)) {
                Chunk_r? toAddChunk;
                try {
                    if (world.IsGenerated(chunkPos)) { // try to load chunk from file
                        toAddChunk = world.GetChunk(chunkPos);
                        chunksToMesh.Enqueue(toAddChunk);
                    }
                    else { // if not generated generate it
                        toAddChunk = new Chunk_r(chunkPos);
                        toAddChunk.GenChunk();
                        chunksToMesh.Enqueue(toAddChunk);
                    }
                }
                catch(IOException argEx) { // if loading from file fails regenrate the chunk, this is a worst case scenario
                    Console.WriteLine(argEx);
                    Console.WriteLine($"Regenerating chunk: {chunkPos}");
                    toAddChunk = new Chunk_r(chunkPos);
                    toAddChunk.GenChunk();
                    chunksToMesh.Enqueue(toAddChunk);
                }


            }
        }
    }

    private void AddChunksToGen(Vector3 currChunkPos) {
        Queue<Vector3> processOrder = new();


    }

    private void RemoveFarChunks(Vector3 currChunkPos) {
 


        foreach (var chunk in chunksToRender.Values) {
            var chunkPos = chunk.position;
            if (Math.Abs(currChunkPos.X - chunkPos.X)/Chunk.SIZE >= renderDistance + 1 ||
                Math.Abs(currChunkPos.Z - chunkPos.Z)/Chunk.SIZE >= renderDistance + 1) {
                //chunk.Unload();
                chunk.Delete();
                _ = chunksToRender.TryRemove(chunk.position, out _);
            }
        }
    }

    public void UpdateQueues() {
        var currChunkPos = Camera.GetChunkPos(camera.position);
        Chunk.ConvertToWorldCoords(ref currChunkPos);
        if (camera.lastChunkPos != currChunkPos && camera.gameRules.generateChunks) {

            RemoveFarChunks(currChunkPos);
            AddChunksToGen(currChunkPos);
            genEvent.Set();
            camera.lastChunkPos = currChunkPos;
        }

    }

    #endregion

    #region MESH_METHODS
    private void MeshChunks() {

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

        genThread.Join();
        meshThread.Join();

        genEvent.Dispose();
        meshEvent.Dispose();
    }

    #endregion


}
