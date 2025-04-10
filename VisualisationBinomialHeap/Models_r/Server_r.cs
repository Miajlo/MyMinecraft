﻿using MyMinecraft.Models;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace MyMinecraft.Models_r; 
public class Server_r {
    #region CONNECTION_CONTEXT
    TcpListener listener; //some day one day
    ushort port;
    IPAddress serverAddress;
    #endregion


    #region THREAD_CONTEXT
    private Thread genThread;
    private Thread meshThread;

    private volatile bool runGenThread;
    private volatile bool runMeshThread;

    private SemaphoreSlim genSem;
    private SemaphoreSlim meshSem;
    private readonly ReaderWriterLockSlim lockChunk;

    private const string tMeshName = "MeshThread";
    private const string tGenName = "GenThread";

    private ConcurrentQueue<Tuple<Vector3i, bool>> chunksToGen;
    private ConcurrentQueue<Chunk_r> chunksToMesh;
    private ConcurrentDictionary<Vector3, Chunk_r> chunksToRender;
    private ConcurrentQueue<ServerPacket> packets;
    private ConcurrentDictionary<Vector3i, CrossChunkData> crossChunkData;
    #endregion

    #region DATA
    private World_r world;
    //private Camera camera;
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
        crossChunkData = [];
        packets = [];

        genSem = new SemaphoreSlim(0);
        meshSem = new SemaphoreSlim(0);
        lockChunk = new ReaderWriterLockSlim();

        runGenThread = runMeshThread = true;
    }
    public Server_r(World_r world, int renderDistance) : this() {
        this.world = world;
    
        this.renderDistance = renderDistance;
    }
    #endregion

    #region GEN_METHODS
    private void GenChunks() {
        while(runGenThread) {
            genSem.Wait();

            int releaseCount = 0;
            List<Chunk_r> chunksToAdd = [];
            while (chunksToGen.TryDequeue(out var chunkGenInfo)) {
                ++releaseCount;

                var toAddChunk = GenChunk(chunkGenInfo.Item1, chunkGenInfo.Item2);
                toAddChunk.UsedForStructGen = chunkGenInfo.Item2;
                chunksToAdd.Add(toAddChunk);
                //chunksToMesh.Enqueue(GenChunk(chunkGenInfo.Item1, chunkGenInfo.Item2));
                //meshSem.Release();
            }

            foreach (var chunk in chunksToAdd) {
                if (this.crossChunkData.TryGetValue(chunk.position, out var blockData)) {
                    foreach (var block in blockData.crossChunkBlocks) {
                        chunk.SetBlockAt(block.Key, block.Value.type);
                    }
                    // Remove data after applying it to avoid reapplying on reload
                    crossChunkData.Remove(chunk.position, out _);
                }

                if (chunk.UsedForStructGen || !chunk.Generated) {
                    chunk.GenChunk();  // Force regeneration
                    chunk.Generated = true;
                    chunk.UsedForStructGen = false;
                    int remeshCount = MarkNeighboursForRedraw(chunk.position);

                    if (remeshCount > 0)
                        meshSem.Release(remeshCount);
                }
                this.crossChunkData.TryRemove(chunk.position, out _);
                chunksToMesh.Enqueue(chunk);                
            }
            if (releaseCount > 0)
                meshSem.Release(releaseCount);
        }
    }

    private Chunk_r? GenChunk(Vector3i chunkPos, bool usedForGen) {
        //if (state is not Vector3i)
        //    return;

        //Vector3i chunkPos = (Vector3i)state;

        Chunk_r? toAddChunk;

        if (world.IsLoadedChunk(chunkPos)) { // if loaded get it
            world.GetChunk(chunkPos, out toAddChunk);            
        }
        else { //if not loaded generate it, it will need a check if not genrated later
            toAddChunk = new(chunkPos);
            if (!usedForGen)
                toAddChunk.GenChunk();
            var chunkCrossData = toAddChunk.GenTrees();
            world.AddLoadedChunk(toAddChunk);

            foreach (var pair in chunkCrossData) {
                if (!this.crossChunkData.TryGetValue(pair.Key, out var blockData)) {
                    blockData = new(); // Create new CrossChunkData if it doesn't exist
                    this.crossChunkData[pair.Key] = blockData;
                }

                blockData.AddAllBlocks(pair.Value.crossChunkBlocks); // Always add blocks
            }
            chunkCrossData.Clear();
        }
        return toAddChunk;
    }

    private void AddChunksToGen(Vector3 currChunkPos) {
        int offset = (renderDistance)* Chunk_r.SIZE;

        int XbottomBound = (int)(currChunkPos.X) - offset;
        int ZbottomBound = (int)(currChunkPos.Z) - offset;
        int XtopBound = XbottomBound + 2*offset;
        int ZtopBound = ZbottomBound + 2*offset;

        List<Tuple<Vector3i, bool>> chunkGenInfo = [];

        for (int i = XbottomBound; i <= XtopBound; i+=Chunk_r.SIZE) {
            for (int j = ZbottomBound; j <= ZtopBound; j+=Chunk_r.SIZE) {
                //Console.WriteLine($"i = {i}, j = {j}");
                Vector3i copyChunkPos = new(i, 0, j);
                //if (world.IsLoadedChunk(copyChunkPos))
                //    continue;
                bool usedForGen = false;
                chunksToGen.Enqueue(new(copyChunkPos, usedForGen));
                //genSem.Release();
            }

        }

        //chunkGenInfo = chunkGenInfo.OrderBy(x =>
        //Vector3.Distance(x.Item1, currChunkPos)).ToList();

        //foreach (var info in chunkGenInfo)
        //    chunksToGen.Enqueue(info);

        //genSem.Release();
    }

    private void RemoveFarChunks(Vector3 currChunkPos) {
        //int remeshCount = 0;
        foreach (var chunk in chunksToRender.Values) {
            var chunkPos = chunk.position;
            if (Math.Abs(currChunkPos.X - chunkPos.X)/Chunk_r.SIZE >= renderDistance + 1 ||
                Math.Abs(currChunkPos.Z - chunkPos.Z)/Chunk_r.SIZE >= renderDistance + 1) {
                
                chunk.Delete();
                
                
                _ = chunksToRender.TryRemove(chunk.position, out _);
                try {
                    _ = world.RemoveChunk(chunk.position);
                }
                catch(ArgumentException argEx) {
                    Console.WriteLine(argEx.Message);
                }
            }
        }
        //GC.Collect();
        //GC.WaitForPendingFinalizers();
        //if (remeshCount>0)
        //    meshSem.Release(remeshCount);
    }

    private int MarkNeighboursForRedraw(Vector3i position) {
        int remeshCount = 0;
        Vector3i checkPos = new(position.X+16, position.Y, position.Z);
        Chunk_r? neighbour;
        if (world.GetChunk(checkPos, out neighbour)) {
            neighbour!.Redraw = true;
            neighbour.AddedFaces = false;
            if (!chunksToMesh.Contains(neighbour)) {
                chunksToMesh.Enqueue(neighbour);
                ++remeshCount;
            }
        }
        checkPos = new(position.X, position.Y, position.Z-16);
        if (world.GetChunk(checkPos, out neighbour)) {
            neighbour!.Redraw = true;
            neighbour.AddedFaces = false;
            if (!chunksToMesh.Contains(neighbour)) {
                chunksToMesh.Enqueue(neighbour);
                ++remeshCount;
            }
        }
        checkPos = new(position.X, position.Y, position.Z+16);
        if (world.GetChunk(checkPos, out neighbour)) {
            neighbour!.Redraw = true;
            neighbour.AddedFaces = false;
            if (!chunksToMesh.Contains(neighbour)) {
                chunksToMesh.Enqueue(neighbour);
                ++remeshCount;
            }
        }
        checkPos = new(position.X-16, position.Y, position.Z);
        if (world.GetChunk(checkPos, out neighbour)) {
            neighbour!.Redraw = true;
            neighbour.AddedFaces = false;
            if (!chunksToMesh.Contains(neighbour)) {
                chunksToMesh.Enqueue(neighbour);
                ++remeshCount;
            }
        }
        return remeshCount;
    }

    public void UpdateQueues(Vector3i currChunkPos) {
        Chunk_r.ConvertToWorldCoords(ref currChunkPos);
       

        RemoveFarChunks(currChunkPos);
        AddChunksToGen(currChunkPos);
        genSem.Release();
    }
    #endregion

    #region PACKET_PROCESSING
    public void ProcessPackets() {
        while(packets.TryDequeue(out var packet)) {
            switch(packet.Type) {
                case PacketType.DESTROY_BLOCK:
                    TryDestroyBlock(packet as DestroyBlockPacket);
                    break;
                case PacketType.PLACE_BLOCK:
                    TryPlaceBlock(packet as PlaceBlockPacket);
                    break;
                default:
                    break;
            }
        }
    }

    private void TryPlaceBlock(PlaceBlockPacket? packet) {
        if (packet == null)
            return;

        int x = (int)Math.Floor(packet.position.X);
        int y = (int)Math.Floor(packet.position.Y);
        int z = (int)Math.Floor(packet.position.Z);

        int stepX = (packet.front.X > 0) ? 1 : -1;
        int stepY = (packet.front.Y > 0) ? 1 : -1;
        int stepZ = (packet.front.Z > 0) ? 1 : -1;

        float tMaxX = (stepX > 0 ? (x + 1 - packet.position.X)
                                 : (packet.position.X - x)) / Math.Abs(packet.front.X);
        float tMaxY = (stepY > 0 ? (y + 1 - packet.position.Y)
                                 : (packet.position.Y - y)) / Math.Abs(packet.front.Y);
        float tMaxZ = (stepZ > 0 ? (z + 1 - packet.position.Z)
        : (packet.position.Z - z)) / Math.Abs(packet.front.Z);

        float tDeltaX = Math.Abs(1 / packet.front.X);
        float tDeltaY = Math.Abs(1 / packet.front.Y);
        float tDeltaZ = Math.Abs(1 / packet.front.Z);

        float maxDistance = packet.playerRange; // Limit to 5 blocks
        float traveledDistance = 0.0f;

        Vector3i prevBlockPos = new(), prevChunkPos = new();
        bool prevValSet = false;
        BlockType prevBlock;


        for (int i = 0; i < 100; ++i) {
            Vector3i chunkPos = (Vector3i)Chunk_r.ConvertToChunkCoords(new(x, y, z));

            Vector3i chunkBlockPos = (Vector3i)Chunk_r.ConvertToChunkBlockCoord(new Vector3(x, y, z));

            if (y<Chunk_r.HEIGHT && world.GetChunk(chunkPos, out Chunk_r? chunk)) {
                BlockType block = chunk.GetBlockAt(chunkBlockPos);

                if (prevValSet && (prevChunkPos == chunkPos || !world.IsLoadedChunk(prevChunkPos)))
                    prevBlock = chunk.GetBlockAt(prevBlockPos);
                else if (prevValSet && prevChunkPos != chunkPos) {
                    if (!world.GetChunk(prevChunkPos, out chunk))
                        return;
                    prevBlock = chunk.GetBlockAt(prevBlockPos);
                }
                else
                    prevBlock = BlockType.AIR;


                if (block != BlockType.AIR && prevValSet && prevBlock == BlockType.AIR) {
                    lockChunk.EnterWriteLock();
                    int remeshCount = 0;
                    

                    try {

                        if (!world.GetChunk(prevChunkPos, out chunk))
                            return;

                        float hitPositionX = packet.position.X + packet.front.X * tMaxX;
                        float hitPositionY = packet.position.Y + packet.front.Y * tMaxY;
                        float hitPositionZ = packet.position.Z + packet.front.Z * tMaxZ;

                        // The hit position will be the current position of the ray when it hits a block
                        Vector3 hitPosition = new Vector3(hitPositionX, hitPositionY, hitPositionZ);


                        Console.WriteLine($"Hit block: {chunkBlockPos} {chunkPos}");
                        

                        var faceHit = DetermineHitFace(chunkBlockPos+chunkPos, prevBlockPos+prevChunkPos);
                        Console.WriteLine($"Hit the face: {faceHit}");

                        chunk.PlaceBlockAt(prevBlockPos, packet.selectedBlock, faceHit);
                        chunk.Redraw = true;
                        chunk.AddedFaces = false;

                        chunksToMesh.Enqueue(chunk);

                        remeshCount = MarkNeighboursForRedraw(chunk.position, chunkBlockPos) + 1;


                    }
                    finally {
                        lockChunk.ExitWriteLock();
                    }
                    meshSem.Release(remeshCount);
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

            prevValSet = true;
            prevBlockPos = chunkBlockPos;
            prevChunkPos = chunkPos;
        }
    }

    private void TryDestroyBlock(DestroyBlockPacket? packet) {
        if (packet == null)
            return;     

        int x = (int)Math.Floor(packet.position.X);
        int y = (int)Math.Floor(packet.position.Y);
        int z = (int)Math.Floor(packet.position.Z);

        int stepX = (packet.front.X > 0) ? 1 : -1;
        int stepY = (packet.front.Y > 0) ? 1 : -1;
        int stepZ = (packet.front.Z > 0) ? 1 : -1;

        float tMaxX = (stepX > 0 ? (x + 1 - packet.position.X) 
                                 : (packet.position.X - x)) / Math.Abs(packet.front.X);
        float tMaxY = (stepY > 0 ? (y + 1 - packet.position.Y) 
                                 : (packet.position.Y - y)) / Math.Abs(packet.front.Y);
        float tMaxZ = (stepZ > 0 ? (z + 1 - packet.position.Z) 
                                 : (packet.position.Z - z)) / Math.Abs(packet.front.Z);

        float tDeltaX = Math.Abs(1 / packet.front.X);
        float tDeltaY = Math.Abs(1 / packet.front.Y);
        float tDeltaZ = Math.Abs(1 / packet.front.Z);

        float maxDistance = packet.playerRange; // Limit to 5 blocks
        float traveledDistance = 0.0f;

        for (int i = 0; i < 100; ++i) {
            Vector3i chunkPos = (Vector3i)Chunk_r.ConvertToChunkCoords(new(x, y, z));

            Vector3i chunkBlockPos = (Vector3i)Chunk_r.ConvertToChunkBlockCoord(new Vector3(x, y, z));

            if (y<Chunk_r.HEIGHT && world.GetChunk(chunkPos, out Chunk_r? chunk)) {
                BlockType block = chunk.GetBlockAt(chunkBlockPos);

                if (block != BlockType.AIR) {
                    lockChunk.EnterWriteLock();
                    int remeshCount = 0;
                    try {
                        Console.WriteLine($"Hit block: {chunkBlockPos} {chunkPos}");
                        chunk.SetBlockAt(chunkBlockPos, BlockType.AIR);
                        chunk.Redraw = true;
                        chunk.AddedFaces = false;
                       
                        chunksToMesh.Enqueue(chunk);

                        remeshCount = MarkNeighboursForRedraw(chunk.position, chunkBlockPos) + 1; 

                       
                    }
                    finally {
                        lockChunk.ExitWriteLock();
                    }
                    meshSem.Release(remeshCount);
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

    private Faces DetermineHitFace(Vector3i chunkBlockPos, Vector3i prevBlockPos) {
        // Calculate the direction of the ray relative to the block
        Vector3i deltaBlockPos = chunkBlockPos - prevBlockPos; // No abs
        Console.WriteLine($"Delta blockPos: {deltaBlockPos}");
        if (deltaBlockPos.X == -1)
            return Faces.LEFT;
        else if (deltaBlockPos.X == 1)
            return Faces.RIGHT;
        if (deltaBlockPos.Z == -1)
            return Faces.BACK;
        else if (deltaBlockPos.Z == 1)
            return Faces.FRONT;
        if (deltaBlockPos.Y == -1)
            return Faces.TOP; // Fixed this
        else
            return Faces.BOTTOM; // Fixed this
    }

    private Vector3i VectorIAbs(Vector3i position) {
        int x, y, z;
        position.Deconstruct(out x, out y, out z);

        return new(Math.Abs(x), Math.Abs(x), Math.Abs(x));
    }

    private int MarkNeighboursForRedraw(Vector3i position, Vector3i chunkBlockPos) {
        int remeshCount = 0;
        Vector3i checkPos = new(position.X+16, position.Y, position.Z);
        Chunk_r? neighbour;
        if (chunkBlockPos.X == Chunk_r.SIZE-1 && world.GetChunk(checkPos, out neighbour)) {
            neighbour.Redraw = true;
            neighbour.AddedFaces = false;
            if (!chunksToMesh.Contains(neighbour))
                chunksToMesh.Enqueue(neighbour);
            ++remeshCount;
        }
        checkPos = new(position.X, position.Y, position.Z-16);
        if (chunkBlockPos.Z == 0 && world.GetChunk(checkPos, out neighbour)) {
            neighbour.Redraw = true;
            neighbour.AddedFaces = false;
            if (!chunksToMesh.Contains(neighbour))
                chunksToMesh.Enqueue(neighbour);
            ++remeshCount;
        }
        checkPos = new(position.X, position.Y, position.Z+16);
        if (chunkBlockPos.Z == Chunk_r.SIZE-1 && world.GetChunk(checkPos, out neighbour)) {
            neighbour.Redraw = true;
            neighbour.AddedFaces = false;
            if (!chunksToMesh.Contains(neighbour))
                chunksToMesh.Enqueue(neighbour);
            ++remeshCount;
        }
        checkPos = new(position.X-16, position.Y, position.Z);
        if (chunkBlockPos.X == 0 && world.GetChunk(checkPos, out neighbour)) {
            neighbour.Redraw = true;
            neighbour.AddedFaces = false;
            if(!chunksToMesh.Contains(neighbour))
                chunksToMesh.Enqueue(neighbour);
            ++remeshCount;
        }
        return remeshCount;
    }

    public void RecievePackets(List<ServerPacket> sPackets) {
        foreach (var packet in sPackets)
            packets.Enqueue(packet);
    }

    #endregion

    #region MESH_METHODS
    private void MeshChunks() {
        while(runMeshThread) {
            meshSem.Wait();

            if(chunksToMesh.TryDequeue(out var chunk)) {
                //if (chunk.UsedForStructGen)
                //    chunksToMesh.Enqueue(chunk);
                
                MeshChunk(ref chunk);
            }
        }
    }

    private void MeshChunk(ref Chunk_r chunk) {
        lockChunk.EnterReadLock();
        try {
            if (chunk.AddedFaces || !chunk.Redraw || chunk.UsedForStructGen)
                return;
        }
        finally {
            lockChunk.ExitReadLock();
        }
        List<Vector3> chunkVert = [];
        List<Vector2> chunkUVs = [];
        List<uint> indeces = [];
        uint indexCount = 0;


        for (var x = 0; x<Chunk_r.SIZE; ++x) {
            for (var z = 0; z<Chunk_r.SIZE; ++z) {
                for (var y = 0; y<Chunk_r.HEIGHT; ++y) {
                    if (chunk.chunkBlocks[x, y, z].type != BlockType.AIR) {
                        uint addedFaces = 0; // Reset for each block
                        Vector3 blockPos = new(x, y, z);
                        var currBlock = chunk.chunkBlocks[x, y, z];
                        // Left face
                        Vector3 neighbourBlockPos = new(Chunk_r.SIZE-1, y, z);
                        Vector3 neighbourChunkPos = new(chunk.position.X-16, chunk.position.Y, chunk.position.Z);
                        BlockType neighbourBlock = world.GetBlockAt(neighbourChunkPos, neighbourBlockPos);

                        if ((neighbourBlock==BlockType.AIR && x==0) || (x!=0 && chunk.chunkBlocks[x - 1, y, z].type == BlockType.AIR)) {
                            var faceData = Block.GetFaceData(Faces.LEFT, blockPos);
                            chunkVert.AddRange(faceData);
                            chunkUVs.AddRange(TextureData.GetUVs(currBlock.type, Faces.LEFT, currBlock.rotation));
                            addedFaces++;
                        }
                        neighbourBlockPos = new(0, y, z);
                        neighbourChunkPos = new(chunk.position.X+16, chunk.position.Y, chunk.position.Z);
                        neighbourBlock = world.GetBlockAt(neighbourChunkPos, neighbourBlockPos);
                        // Right face
                        if ((neighbourBlock==BlockType.AIR && x == Chunk_r.SIZE - 1) || (x != Chunk_r.SIZE-1 && chunk.chunkBlocks[x + 1, y, z].type == BlockType.AIR)) {
                            var faceData = Block.GetFaceData(Faces.RIGHT, blockPos);
                            chunkVert.AddRange(faceData);
                            chunkUVs.AddRange(TextureData.GetUVs(currBlock.type, Faces.RIGHT, currBlock.rotation));
                            addedFaces++;
                        }

                        // Bottom face
                        if (y == 0 || chunk.chunkBlocks[x, y - 1, z].type == BlockType.AIR) {
                            var faceData = Block.GetFaceData(Faces.BOTTOM, blockPos);
                            chunkVert.AddRange(faceData);
                            chunkUVs.AddRange(TextureData.GetUVs(currBlock.type, Faces.BOTTOM, currBlock.rotation));
                            addedFaces++;
                        }

                        // Top face
                        if (y == Chunk_r.HEIGHT - 1 || chunk.chunkBlocks[x, y + 1, z].type == BlockType.AIR) {
                            var faceData = Block.GetFaceData(Faces.TOP, blockPos);
                            chunkVert.AddRange(faceData);
                            chunkUVs.AddRange(TextureData.GetUVs(currBlock.type, Faces.TOP, currBlock.rotation));
                            addedFaces++;
                        }
                        neighbourBlockPos = new(x, y, Chunk_r.SIZE-1);
                        neighbourChunkPos = new(chunk.position.X, chunk.position.Y, chunk.position.Z-16);
                        neighbourBlock = world.GetBlockAt(neighbourChunkPos, neighbourBlockPos);
                        // Back face
                        if ((neighbourBlock == BlockType.AIR && z == 0)|| (z != 0 && chunk.chunkBlocks[x, y, z - 1].type == BlockType.AIR)) {
                            var faceData = Block.GetFaceData(Faces.BACK, blockPos);
                            chunkVert.AddRange(faceData);
                            chunkUVs.AddRange(TextureData.GetUVs(currBlock.type, Faces.BACK, currBlock.rotation));
                            addedFaces++;
                        }
                        neighbourBlockPos = new(x, y, 0);
                        neighbourChunkPos = new(chunk.position.X, chunk.position.Y, chunk.position.Z+16);
                        neighbourBlock = world.GetBlockAt(neighbourChunkPos, neighbourBlockPos);
                        // Front face
                        if ((neighbourBlock == BlockType.AIR && z == Chunk_r.SIZE - 1) || (z != Chunk_r.SIZE-1 && chunk.chunkBlocks[x, y, z + 1].type == BlockType.AIR)) {
                            var faceData = Block.GetFaceData(Faces.FRONT, blockPos);
                            chunkVert.AddRange(faceData);
                            chunkUVs.AddRange(TextureData.GetUVs(currBlock.type, Faces.FRONT, currBlock.rotation));
                            addedFaces++;
                        }

                        // Add indices for the added faces
                        for (int i = 0; i < addedFaces; ++i) {
                            indeces.Add(0 + indexCount);
                            indeces.Add(1 + indexCount);
                            indeces.Add(2 + indexCount);
                            indeces.Add(2 + indexCount);
                            indeces.Add(3 + indexCount);
                            indeces.Add(0 + indexCount);

                            indexCount += 4;
                        }
                        //chunkBlocks[x, y, z].ClearFaceData();
                       
                    }
                }
            }
        }


        lockChunk.EnterWriteLock();
        try {
            chunk.chunkVert = chunkVert;
            chunk.chunkUVs = chunkUVs;
            chunk.chunkInd = indeces;
            chunk.AddedFaces = true;
            chunk.Redraw = false;
            chunk.Built = false;
            chunk.ShouldClearData = chunk.Redraw && !chunk.FirstDrawing;
            
        }
        finally {
            lockChunk.ExitWriteLock();
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
            if (!chunk.UsedForStructGen) {
                lockChunk.EnterReadLock();
                try {
                    if (chunk.ShouldClearData) {
                        chunk.DeleteGL();
                        chunk.ShouldClearData = false;
                    }
                    if (!chunk.Built)
                        chunk.BuildChunk();
                    chunk.Render(program);
                    //chunk.AddedFaces = true;
                }
                finally {
                    lockChunk.ExitReadLock();
                }
            }
        }
    }

    private void DeleteChunkRenderData() {
        foreach (var chunk in chunksToRender.Values)
            chunk.Delete();
    }
}
