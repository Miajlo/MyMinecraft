using static System.Net.Mime.MediaTypeNames;

namespace MyMinecraft.Models;
public class Server {

    public World world;
    Thread genThread;
    Thread meshThread;
    public Camera camera;

    ConcurrentQueue<Vector3> genQueue;
    ConcurrentQueue<Chunk> meshQueue;
    ConcurrentQueue<Chunk> renderDict;

    AutoResetEvent genEvent;
    AutoResetEvent meshEvent;

    static volatile bool genRunning = true;
    static volatile bool meshRunning = true;

    public void Init(World worldd, Camera cam) {
        world = worldd;
        camera = cam;
        genQueue = new();
        meshQueue = new();
        renderDict = new();
        genEvent = new(true);
        meshEvent = new(false);
        genThread = new(GenChunks);
        meshThread = new(MeshChunks);
        //camera.OnClick += OnCameraClick;
    }

    public void Start() {
        genThread.Start();
        meshThread.Start();

    }

    public void Stop() {

        genRunning = false;
        meshRunning = false;
        genEvent.Set();
        meshEvent.Set();

        genThread.Join();
        meshThread.Join();

        meshEvent.Dispose();
        genEvent.Dispose();
    }

    public void MeshChunks() {
        while (meshRunning) {
            meshEvent.WaitOne();

            if (!meshQueue.IsEmpty) {
                MeshChunkQueue();
            }
        }
    }

    public void GenChunks() {
        while (genRunning) {
            genEvent.WaitOne();

            while (genQueue.TryDequeue(out Vector3 position)) {
                if (!camera.gameRules.generateChunks)
                    break;
                var chunk = new Chunk(position);
                meshQueue.Enqueue(chunk);
                world.allChunks.TryAdd(chunk.position, chunk);
                meshEvent.Set();
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

    private void RemoveFarChunks(Vector3 currChunkPos) {
        List<Chunk> toRemove = new();


        foreach (var chunk in renderDict) {
            var chunkPos = chunk.position;
            //Chunk.ConvertToWorldCoords(ref chunkPos);
            if (Math.Abs(currChunkPos.X - chunkPos.X)/Chunk.SIZE >= world.renderDistance + 1 ||
                Math.Abs(currChunkPos.Z - chunkPos.Z)/Chunk.SIZE >= world.renderDistance + 1) {
                //chunk.Unload();
                chunk.Delete();
                toRemove.Add(chunk);

                //toRemove.Add(chunk);
            }
        }

        var newForRendering = new ConcurrentQueue<Chunk>(renderDict.Except(toRemove));
        renderDict = newForRendering;

        //foreach (var chunk in toRemove) {
        //    chunk.Delete();
        //    //MarkNeighboursForReDraw(chunk.position);
        //    allChunks.TryRemove(chunk.position, out _);
        //}
    }

    private void AddChunksToGen(Vector3 currChunkPos) {

        //Console.WriteLine($"World::currChunkID: {chunkID}");
        Chunk? toAdd = new();


        int offset = world.renderDistance * Chunk.SIZE;
        int totalIterations = 0;

        int XbottomBound = (int)(currChunkPos.X) - offset;
        int ZbottomBound = (int)(currChunkPos.Z) - offset;
        int XtopBound = XbottomBound + 2*offset;
        int ZtopBound = ZbottomBound + 2*offset;


        for (int i = XbottomBound; i <= XtopBound; i+=Chunk.SIZE) {
            for (int j = ZbottomBound; j <= ZtopBound; j+=Chunk.SIZE) {
                //Console.WriteLine($"i = {i}, j = {j}");
                Vector3 copyChunkPos = new(i, 0, j);

                if (!world.allChunks.ContainsKey(copyChunkPos))
                    genQueue.Enqueue(copyChunkPos);

                ++totalIterations;
            }

        }
    }

    public void RenderChunks(ShaderProgram program, Matrix4 projection) {
        Frustum frustum = new();
        frustum.ExtractFrustumPlanes(projection);

        foreach (var chunk in renderDict) {
            Vector3 leftCase = new(chunk.position.X+16, chunk.position.Y, chunk.position.Z+16);
            Vector3 rightCase = new(chunk.position.X-16, chunk.position.Y, chunk.position.Z-16);
            if (GameConfig.DoFrustumCulling &&
                !world.IsInsideFrustum(chunk.position, frustum) && !world.IsInsideFrustum(leftCase, frustum) &&
                !world.IsInsideFrustum(rightCase, frustum))
                continue;


            //if (chunk.ReDraw)
            //    continue;
            if (!chunk.Built)
                chunk.BuildChunk();
            chunk.Render(program);
            chunk.AddedFaces = true;
        }
        // meshEvent.Set();
    }

    private void PlaceBlock(Vector3 position, Vector3 front) {
        if (world == null)
            return;

        int x = (int)Math.Floor(position.X);
        int y = (int)Math.Floor(position.Y);
        int z = (int)Math.Floor(position.Z);

        int stepX = camera.front.X < 0 ? -1 : 1;
        int stepY = camera.front.Y < 0 ? -1 : 1;
        int stepZ = camera.front.Z < 0 ? -1 : 1;

        float tMaxX = ((x + (stepX > 0 ? 1 : 0)) - camera.position.X) / camera.front.X;
        float tMaxY = ((y + (stepY > 0 ? 1 : 0)) - camera.position.Y) / camera.front.Y;
        float tMaxZ = ((z + (stepZ > 0 ? 1 : 0)) - camera.position.Z) / camera.front.Z;

        float tDeltaX = Math.Abs(1 / camera.front.X);
        float tDeltaY = Math.Abs(1 / camera.front.Y);
        float tDeltaZ = Math.Abs(1 / camera.front.Z);

        float maxDistance = 5.0f; // Limit to 5 blocks
        float traveledDistance = 0.0f;

        Vector3 prevBlockPos = new(), prevChunkPos = new();
        bool prevValSet = false;

        for (int i = 0; i < 100; ++i) {
            Vector3 chunkPos = Chunk.ConvertToChunkCoords(new(x, y, z));

            Vector3 chunkBlockPos = Chunk.ConvertToChunkBlockCoord(new Vector3(x, y, z));


            // Check if the chunk exists in the dictionary
            if (y<Chunk.HEIGHT && world.allChunks.TryGetValue(chunkPos, out Chunk? chunk)) {
                // Retrieve the block type at the calculated block position
                BlockType block = chunk.GetBlockAt(chunkBlockPos);
                BlockType prevBlock = chunk.GetBlockAt(prevBlockPos);
                // If the block is solid (not air), stop the raycast and register the hit
                if (block != BlockType.AIR && prevValSet && prevBlock == BlockType.AIR) {
                    if (!world.allChunks.TryGetValue(prevChunkPos, out chunk))
                        return;
                    Console.WriteLine($"Hit block: {prevBlockPos} {prevChunkPos}");
                    chunk.SetBlockAt(prevBlockPos, camera.selectedBlock);
                    chunk.ReDraw = true;
                    chunk.AddedFaces = false;
                    chunk.Delete();
                    //renderDict.TryRemove(chunk.position, out _);
                    meshQueue.Enqueue(chunk);
                    AddNeighboursToRemesh(chunk.position);
                    meshEvent.Set();
                    break;

                }
            }

            // Determine next voxel step and distance
            if (tMaxX < tMaxY && tMaxX < tMaxZ) {
                traveledDistance = tMaxX;
                tMaxX += tDeltaX;
                x += stepX;

            }
            else if (tMaxY < tMaxZ) {
                traveledDistance = tMaxY;
                tMaxY += tDeltaY;
                y += stepY;

            }
            else {
                traveledDistance = tMaxZ;
                tMaxZ += tDeltaZ;
                z += stepZ;

            }

            // Stop if we exceed the max distance (5 blocks)
            if (traveledDistance > maxDistance)
                break;

            prevValSet = true;
            prevBlockPos = chunkBlockPos;
            prevChunkPos = chunkPos;
        }
    }

    private void DestroyBlock(Vector3 position, Vector3 front) {
        if (world == null)
            return;

        int x = (int)Math.Floor(position.X);
        int y = (int)Math.Floor(position.Y);
        int z = (int)Math.Floor(position.Z);

        int stepX = (front.X > 0) ? 1 : -1;
        int stepY = (front.Y > 0) ? 1 : -1;
        int stepZ = (front.Z > 0) ? 1 : -1;

        float tMaxX = (stepX > 0 ? (x + 1 - position.X) : (position.X - x)) / Math.Abs(front.X);
        float tMaxY = (stepY > 0 ? (y + 1 - position.Y) : (position.Y - y)) / Math.Abs(front.Y);
        float tMaxZ = (stepZ > 0 ? (z + 1 - position.Z) : (position.Z - z)) / Math.Abs(front.Z);

        float tDeltaX = Math.Abs(1 / front.X);
        float tDeltaY = Math.Abs(1 / front.Y);
        float tDeltaZ = Math.Abs(1 / front.Z);

        float maxDistance = 5.0f; // Limit to 5 blocks
        float traveledDistance = 0.0f;

        for (int i = 0; i < 100; ++i) {
            Vector3 chunkPos = Chunk.ConvertToChunkCoords(new(x, y, z));

            Vector3 chunkBlockPos = Chunk.ConvertToChunkBlockCoord(new Vector3(x, y, z));

            if (y<Chunk.HEIGHT && world.allChunks.TryGetValue(chunkPos, out Chunk? chunk)) {
                BlockType block = chunk.GetBlockAt(chunkBlockPos);

                if (block != BlockType.AIR) {
                    Console.WriteLine($"Hit block: {chunkBlockPos} {chunkPos}");
                    chunk.SetBlockAt(chunkBlockPos, BlockType.AIR);
                    chunk.ReDraw = true;
                    chunk.AddedFaces = false;
                    chunk.Delete();
                    meshQueue.Enqueue(chunk);
                    AddNeighboursToRemesh(chunk.position);
                    meshEvent.Set();
                    break;
                }
            }

            // Step to the next voxel
            if (tMaxX < tMaxY && tMaxX < tMaxZ) {
                traveledDistance = tMaxX;
                tMaxX += tDeltaX;
                x += stepX;
            }
            else if (tMaxY < tMaxZ) {
                traveledDistance = tMaxY;
                tMaxY += tDeltaY;
                y += stepY;
            }
            else {
                traveledDistance = tMaxZ;
                tMaxZ += tDeltaZ;
                z += stepZ;
            }

            if (traveledDistance > maxDistance)
                break;
        }
    }

    private void AddNeighboursToRemesh(Vector3 position) {
        Vector3[] directions =
        {
            new(16, 0, 0),  // +X
            new(-16, 0, 0), // -X
            new(0, 0, 16),  // +Z
            new(0, 0, -16)  // -Z
        };

        foreach (var offset in directions) {
            Vector3 checkPos = position + offset;

            if (renderDict.TryDequeue(out Chunk? neighbour)) {
                neighbour.ReDraw = true;
                neighbour.AddedFaces = false;
                neighbour.Delete();
                meshQueue.Enqueue(neighbour);
                meshEvent.Set();
            }
        }
    }

    public void OnCameraClick(Vector3 positoion, Vector3 front, bool isLeft) {
        if (isLeft)
            DestroyBlock(positoion, front);
        else
            PlaceBlock(positoion, front);
    }

    private void MeshChunkQueue() {
        while (meshQueue.TryDequeue(out Chunk? chunk)) {
            if (chunk.AddedFaces || !chunk.ReDraw)
                continue;

            for (var x = 0; x<Chunk.SIZE; ++x) {
                for (var z = 0; z<Chunk.SIZE; ++z) {
                    for (var y = 0; y<Chunk.HEIGHT; ++y) {
                        if (chunk.chunkBlocks[x, y, z] != BlockType.AIR) {
                            uint addedFaces = 0; // Reset for each block
                            Vector3 blockPos = new(x, y, z);
                            // Left face
                            Vector3 neighbourBlockPos = new(Chunk.SIZE-1, y, z);
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
                            if ((neighbourBlock==BlockType.AIR && x == Chunk.SIZE - 1) || (x != Chunk.SIZE-1 && chunk.chunkBlocks[x + 1, y, z] == BlockType.AIR)) {
                                chunk.IntegrateFace(chunk.chunkBlocks[x, y, z], Faces.RIGHT, blockPos);
                                addedFaces++;
                            }

                            // Bottom face
                            if (y == 0 || chunk.chunkBlocks[x, y - 1, z] == BlockType.AIR) {
                                chunk.IntegrateFace(chunk.chunkBlocks[x, y, z], Faces.BOTTOM, blockPos);
                                addedFaces++;
                            }

                            // Top face
                            if (y == Chunk.HEIGHT - 1 || chunk.chunkBlocks[x, y + 1, z] == BlockType.AIR) {
                                chunk.IntegrateFace(chunk.chunkBlocks[x, y, z], Faces.TOP, blockPos);
                                addedFaces++;
                            }
                            neighbourBlockPos = new(x, y, Chunk.SIZE-1);
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
                            if ((neighbourBlock == BlockType.AIR && z == Chunk.SIZE - 1) || (z != Chunk.SIZE-1 && chunk.chunkBlocks[x, y, z + 1] == BlockType.AIR)) {
                                chunk.IntegrateFace(chunk.chunkBlocks[x, y, z], Faces.FRONT, blockPos);
                                addedFaces++;
                            }

                            // Add indices for the added faces
                            chunk.AddInceces(addedFaces);
                            //chunkBlocks[x, y, z].ClearFaceData();
                            chunk.AddedFaces = true;
                            chunk.ReDraw = chunk.firstDrawing;
                        }
                    }
                }
            }
            renderDict.Enqueue(chunk);

        }
    }
}
