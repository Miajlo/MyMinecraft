namespace MyMinecraft.Models; 
public class World {
    public ConcurrentDictionary<Vector3, Chunk> allChunks = new();
    public ConcurrentQueue<Chunk> forRendering = new();
    
    public int renderDistance = 1;
    public bool readyToRender = false;
    public object locker = new();
    public void AddChunk(Chunk chunk) {
        if (allChunks.TryAdd(chunk.position, chunk))
            Console.WriteLine($"Succesfully saved chunk: {chunk.position}");
        else
            Console.WriteLine($"Error adding chunk: {chunk.position}");
    }

    public void UpdateChunkRanderList(Vector3 pos) {
        //RemoveFarChunks(pos);
        //AddChunksToRender(pos);
        RemoveFarChunks(pos);
        //GC.Collect();
        Task.Run(() => {
            AddChunksToRender(pos);
        });

        //GC.Collect();
        //GC.WaitForFullGCComplete();
    }

    private void AddChunksToRender(Vector3 pos) {
        readyToRender = false;
        var currChunkPos = Camera.GetChunkPos(pos);
        //var chunkID = $"{currChunkPos.X}, {currChunkPos.Z}";
        Chunk.ConvertToWorldCoords(ref currChunkPos);
        //Console.WriteLine($"World::currChunkID: {chunkID}");
        Chunk? toAdd = new();

        int offset = renderDistance * Chunk.SIZE;
        int totalIterations = 0;

        int XbottomBound = (int)(currChunkPos.X) - offset; 
        int ZbottomBound = (int)(currChunkPos.Z) - offset;
        int XtopBound = XbottomBound + 2*offset;
        int ZtopBound = ZbottomBound + 2*offset;


        for (int i = XbottomBound; i <= XtopBound; i+=Chunk.SIZE) {
            for(int j = ZbottomBound; j <= ZtopBound; j+=Chunk.SIZE) {
                //Console.WriteLine($"i = {i}, j = {j}");
                Vector3 copyChunkPos = new(i, 0, j);

                byte chunkNeighbours = 0b_0000;

                if (i != XbottomBound)
                    chunkNeighbours |= 0b_0001;
                if (i != XtopBound)
                    chunkNeighbours |= 0b_0100;

                if (j != ZtopBound)
                    chunkNeighbours |= 0b_1000;
                if (j != ZbottomBound)
                    chunkNeighbours |= 0b_0010;

                if (!allChunks.TryGetValue(copyChunkPos, out toAdd)) {

                    Console.WriteLine($"World::currCunkPosition:{copyChunkPos}");
                    toAdd = new(copyChunkPos, chunkNeighbours);
                    if (allChunks.TryAdd(copyChunkPos, toAdd))
                        Console.WriteLine("Saved chunk succesfully");
                }
                else {
                    Console.WriteLine($"Loaded chunk: {copyChunkPos}");
                    toAdd.neighbours = chunkNeighbours;
                }
                //if (!forRendering.Contains(toAdd)) {
                //    forRendering.Enqueue(toAdd);
                //}

                
                ++totalIterations;
            }
            
        }
        MeshChunks();

        Console.WriteLine($"TotalIterations: {totalIterations}");
    }

    public void MeshChunks() {
        
        foreach(var chunk in allChunks.Values) {
            if (chunk.AddedFaces || !chunk.ReDraw)
                continue;

            for(var x=0;x<Chunk.SIZE;++x) {                
                for(var z=0; z<Chunk.SIZE; ++z) {
                    for(var y=0;y<Chunk.HEIGHT; ++y) {
                        if (chunk.chunkBlocks[x, y, z] != BlockType.AIR) {
                            uint addedFaces = 0; // Reset for each block
                            Vector3 blockPos = new(x, y, z);
                            // Left face
                            Vector3 neighbourBlockPos = new(Chunk.SIZE-1,y,z);
                            Vector3 neighbourChunkPos = new(chunk.position.X-16, chunk.position.Y, chunk.position.Z);
                            BlockType neighbourBlock = GetBlockAt(neighbourChunkPos, neighbourBlockPos);

                            if ((neighbourBlock==BlockType.AIR && x==0) || (x!=0 && chunk.chunkBlocks[x - 1, y, z] == BlockType.AIR)) {
                                chunk.IntegrateFace(chunk.chunkBlocks[x, y, z], Faces.LEFT, blockPos);
                                addedFaces++;
                            }
                            neighbourBlockPos = new(0, y, z);
                            neighbourChunkPos = new(chunk.position.X+16, chunk.position.Y, chunk.position.Z);
                            neighbourBlock = GetBlockAt(neighbourChunkPos, neighbourBlockPos);
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
                            neighbourBlock = GetBlockAt(neighbourChunkPos, neighbourBlockPos);
                            // Back face
                            if ((neighbourBlock == BlockType.AIR && z == 0)|| (z != 0 && chunk.chunkBlocks[x, y, z - 1] == BlockType.AIR)) {
                                chunk.IntegrateFace(chunk.chunkBlocks[x, y, z], Faces.BACK, blockPos);
                                addedFaces++;
                            }
                            neighbourBlockPos = new(x, y, 0);
                            neighbourChunkPos = new(chunk.position.X, chunk.position.Y, chunk.position.Z+16);
                            neighbourBlock = GetBlockAt(neighbourChunkPos, neighbourBlockPos);
                            // Front face
                            if ((neighbourBlock == BlockType.AIR && z == Chunk.SIZE - 1 ) || (z != Chunk.SIZE-1 && chunk.chunkBlocks[x, y, z + 1] == BlockType.AIR)) {
                                chunk.IntegrateFace(chunk.chunkBlocks[x, y, z], Faces.FRONT, blockPos);
                                addedFaces++;
                            }

                            // Add indices for the added faces
                            chunk.AddInceces(addedFaces);
                            //chunkBlocks[x, y, z].ClearFaceData();
                            chunk.AddedFaces = true;
                            chunk.ReDraw = false;
                        }
                    }
                }
            }
            if (!forRendering.Contains(chunk)) {
                forRendering.Enqueue(chunk);
            }
        }
    }

    public void RenderChunks(ShaderProgram program, Matrix4 projection) {
        Frustum frustum = new();
        frustum.ExtractFrustumPlanes(projection);

        foreach (var chunk in forRendering) {
            Vector3 leftCase = new(chunk.position.X+16, chunk.position.Y, chunk.position.Z+16);
            Vector3 rightCase = new(chunk.position.X-16, chunk.position.Y, chunk.position.Z-16);
            if (!IsInsideFrustum(chunk.position, frustum) && !IsInsideFrustum(leftCase,frustum) &&
                !IsInsideFrustum(rightCase,frustum))
                continue;


            //if (chunk.ReDraw)
            //    continue;
            if (!chunk.Built)
                chunk.BuildChunk();
            chunk.Render(program);
            chunk.AddedFaces = true;
        }      
    }

    public void DrawChunkBorders(ShaderProgram program, Vector3 pos) {
        
        program.Bind();


        var currChunkPos = Camera.GetChunkPos(pos);
        Chunk.ConvertToWorldCoords(ref currChunkPos);

        float HEIGHT = 128.0f;   // Example height, can be replaced by your parameter
        float chunkSize = Chunk.SIZE; // Use your Chunk.SIZE value

        // Define vertices for vertical lines
        List<Vector3> verts = new List<Vector3> {
            new Vector3(0.0f, 0.0f, 0.0f),          // Line 1 start
            new Vector3(0.0f, HEIGHT, 0.0f),        // Line 1 end
            new Vector3(chunkSize, 0.0f, 0.0f),     // Line 2 start
            new Vector3(chunkSize, HEIGHT, 0.0f),   // Line 2 end
            new Vector3(0.0f, 0.0f, chunkSize),     // Line 3 start
            new Vector3(0.0f, HEIGHT, chunkSize),   // Line 3 end
            new Vector3(chunkSize, 0.0f, chunkSize),// Line 4 start
            new Vector3(chunkSize, HEIGHT, chunkSize)// Line 4 end
        };

        // Define indices for the lines
        List<uint> ind = new List<uint> {
            0, 1, // Line 1
            2, 3, // Line 2
            4, 5, // Line 3
            6, 7,  // Line 4
            1, 3,            
            1, 5,
            7, 5,
            7, 3
        };

        Matrix4 model = Matrix4.CreateTranslation(currChunkPos);
        int modelLocation = GL.GetUniformLocation(program.ID, "model");
        model = model * Matrix4.Identity;
        GL.UniformMatrix4(modelLocation, true, ref model);

        // Create VAO for the line
        VAO lineVao = new VAO();
        lineVao.Bind();

        // Create VBO for the vertex data
        VBO lineVBO = new VBO(verts);
        lineVBO.Bind();
        lineVao.LinkToVAO(0, 3, lineVBO);  // 0 is the attribute index, 3 components per vertex (x, y, z)

        // Create IBO for the index data
        IBO lineIBO = new IBO(ind);
        lineIBO.Bind();
        GL.LineWidth(5.0f);

        lineVao.Bind();
        lineVBO.Bind();
        lineIBO.Bind();

        GL.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);
        
        // Draw the line
        GL.DrawElements(PrimitiveType.Lines, ind.Count, DrawElementsType.UnsignedInt, 0);



        Matrix4 identityMatrix = Matrix4.Identity;  // Identity matrix (no transformation)

        modelLocation = GL.GetUniformLocation(program.ID, "model");
        GL.UniformMatrix4(modelLocation, false, ref identityMatrix);
        // Unbind the buffers
        lineIBO.Unbind();
        lineVao.Unbind();
        lineVBO.Unbind();
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

        var newForRendering = new ConcurrentQueue<Chunk>(forRendering.Except(toRemove));
        forRendering = newForRendering;

        foreach (var chunk in toRemove) {
            chunk.Delete();
            //MarkNeighboursForReDraw(chunk.position);
            allChunks.TryRemove(chunk.position, out _);
        }
    }

    private void MarkNeighboursForReDraw(Vector3 position) {
        Vector3 checkPos = new(position.X+16, position.Y, position.Z);
        Chunk? neighbour;
        if (allChunks.TryGetValue(checkPos, out neighbour)) {
            neighbour.ReDraw = true;
            neighbour.Delete();
        }
        checkPos = new(position.X, position.Y, position.Z-16);
        if (allChunks.TryGetValue(checkPos, out neighbour)) {
            neighbour.ReDraw = true;
            neighbour.Delete();
        }
        checkPos = new(position.X, position.Y, position.Z+16);
        if (allChunks.TryGetValue(checkPos, out neighbour)) {
            neighbour.ReDraw = true;
            neighbour.Delete();
        }
        checkPos = new(position.X-16, position.Y, position.Z);
        if (allChunks.TryGetValue(checkPos, out neighbour)) {
            neighbour.ReDraw = true;
            neighbour.Delete();
        }
    }

    private void UpdateChunkNeighbourFlag(Vector3 position) {
        Vector3 checkPos = new(position.X+16, position.Y, position.Z);
        Chunk? neighbour;
        if (allChunks.TryGetValue(checkPos, out neighbour)) {
            neighbour.neighbours ^= 0b_0001;
        }
        checkPos = new(position.X, position.Y, position.Z-16);
        if (allChunks.TryGetValue(checkPos, out neighbour)) {
            neighbour.neighbours ^= 0b_1000;
        }
        checkPos = new(position.X, position.Y, position.Z+16);
        if (allChunks.TryGetValue(checkPos, out neighbour)) {
            neighbour.neighbours ^= 0b_0010;
        }
        checkPos = new(position.X-16, position.Y, position.Z);
        if (allChunks.TryGetValue(checkPos, out neighbour)) {
            neighbour.neighbours ^= 0b_01000;
        }
    }
    private void ClearFaceData(ConcurrentBag<Chunk> chunks) {

    }

    private List<byte[,]> GetNeighboursHeightMap(Vector3 chunkPos) {
        List<byte[,]> retVal = new();
        Vector3 neighbourPos = new(chunkPos.X, chunkPos.Y, chunkPos.Z+16);
        if (allChunks.TryGetValue(neighbourPos, out Chunk? neighbour))
            retVal.Add(neighbour.heightMap);

        neighbourPos = new(chunkPos.X+16, chunkPos.Y, chunkPos.Z);
        if (allChunks.TryGetValue(neighbourPos, out neighbour))
            retVal.Add(neighbour.heightMap);

        neighbourPos= new(chunkPos.X, chunkPos.Y, chunkPos.Z-16);
        if (allChunks.TryGetValue(neighbourPos, out neighbour))
            retVal.Add(neighbour.heightMap);

        neighbourPos = new(chunkPos.X-16, chunkPos.Y, chunkPos.Z);
        if (allChunks.TryGetValue(neighbourPos, out neighbour))
            retVal.Add(neighbour.heightMap);


        return retVal;
    }

    public BlockType GetBlockAt(Vector3 chunkPos, Vector3 blockPos) {

        if (!allChunks.TryGetValue(chunkPos, out Chunk? chunk))
            return BlockType.AIR;

        

        return chunk.chunkBlocks[(int)blockPos.X, (int)blockPos.Y, (int)blockPos.Z];
    }


    public bool IsInsideFrustum(Vector3 position, Frustum frustum) {
        foreach (var plane in frustum.GetPlanes()) {
            float distance = Vector3.Dot(plane.Normal, position) + plane.D;
            if (distance < 0) {
                return false; // The point is outside one of the planes
            }
        }
        return true; // The point is inside all planes
    }
}
