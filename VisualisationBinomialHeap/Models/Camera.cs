namespace MyMinecraft.Models; 
public class Camera {
    #region GameRules
    public bool doCollisionChecks = false;
    public bool generateChunks = true;
    public bool showChunkBorders = false;
    #endregion
    #region Flags
    private bool collisionChecksFlag = false;
    private bool f3Pressed = false;
    private bool genChunksFlag = false;
    private bool chunkBordersflag = false;
    #endregion
    public float Speed = 15f;
    private float Width;
    private float Height;
    private float Sensitivity = 180f;

    private float pitch;
    private float yawn = -90.0f;

    private bool isFirstMove = true;
    public Vector2 lastPosition;

    WeakReference<World> worldRef;
    public Vector3 position;
    public Vector3 lastChunkPos = (-500, 0, -500);
    Vector3 up = Vector3.UnitY;
    Vector3 front = - Vector3.UnitZ;
    Vector3 right = Vector3.UnitX;
    public Camera(float width, float height, Vector3 pos, WeakReference<World> worldd) {
        Width = width;
        Height = height;
        position = pos;
        worldRef = worldd;
    }
    public Matrix4 GetViewMatrix() {
        return Matrix4.LookAt(position, position + front, up);
    }
    public Matrix4 GetProjectionMatrix() {
        return Matrix4.CreatePerspectiveFieldOfView(
            MathHelper.DegreesToRadians(GameConfig.FoV),
            Width / Height, GameConfig.NearPlane, GameConfig.FarPlane);
    }
    
    public Matrix4 GetFrustumProjectionMatrix() {
        return Matrix4.CreatePerspectiveFieldOfView(
           MathHelper.DegreesToRadians(GameConfig.FoV*1.2f),
           Width / Height, GameConfig.NearPlane, GameConfig.FarPlane);
    }

    private void UpdateVectors() {
        if (pitch > 89.0f)
            pitch = 89.0f;
        if (pitch < -89.0f)
            pitch = -89.0f;


        front.X = MathF.Cos(MathHelper.DegreesToRadians(yawn)) 
                * MathF.Cos(MathHelper.DegreesToRadians(pitch));
        front.Y = MathF.Sin(MathHelper.DegreesToRadians(pitch));
        front.Z = MathF.Sin(MathHelper.DegreesToRadians(yawn)) 
                * MathF.Cos(MathHelper.DegreesToRadians(pitch));

        front = Vector3.Normalize(front);

        right = Vector3.Normalize(Vector3.Cross(front, Vector3.UnitY));
        up = Vector3.Normalize(Vector3.Cross(right, front));
    }

    public void InputController(KeyboardState input, MouseState mouse, FrameEventArgs e, Window window) {

        if (input.IsKeyDown(Keys.W))
            WPressedHandle(e);
        //position = position + front * Speed * (float)e.Time;
        if (input.IsKeyDown(Keys.S))
            SPressedHandle(e);
            //position -= front * Speed * (float)e.Time;
        if (input.IsKeyDown(Keys.A))
            APressedHandle(e);
        //position -= right * Speed * (float)e.Time;
        if (input.IsKeyDown(Keys.D))
            DPressedHandle(e);
            //position += right * Speed * (float)e.Time;
        if (input.IsKeyDown(Keys.Space))
            SpacePressedHandle(e);
        if (input.IsKeyDown(Keys.LeftShift))
            ShiftPressedHandle(e);

        if (input.IsKeyDown(Keys.Escape))
            window.Close();


        if (input.IsKeyDown(Keys.F3) && !f3Pressed) {
            PrintCurrentPosition();
            f3Pressed = true;
        }

        if (input.IsKeyDown(Keys.F3) && input.IsKeyDown(Keys.N) && !collisionChecksFlag) {
            doCollisionChecks = !doCollisionChecks;
            Console.WriteLine($"Collision checks set to: {doCollisionChecks}");
            collisionChecksFlag = true;
        }
        else if (input.IsKeyReleased(Keys.F3) && input.IsKeyReleased(Keys.N)) {
            collisionChecksFlag = false;
        }
        
        else if (input.IsKeyReleased(Keys.F3))
            f3Pressed = false;

        if (input.IsKeyDown(Keys.F3) && input.IsKeyDown(Keys.M) && !genChunksFlag) {
            generateChunks = !generateChunks;
            Console.WriteLine($"Generate chunks set to: {generateChunks}");
            genChunksFlag = true;
        }
        else if (input.IsKeyReleased(Keys.F3) && input.IsKeyReleased(Keys.M))
            genChunksFlag = false;
        else if (input.IsKeyReleased(Keys.F3))
            f3Pressed = false;

        if (input.IsKeyDown(Keys.F3) && input.IsKeyDown(Keys.G) && !chunkBordersflag) {
            showChunkBorders = !showChunkBorders;
            Console.WriteLine($"Show Chunk Borders: {showChunkBorders}");
            chunkBordersflag = true;
        }
        else if (input.IsKeyReleased(Keys.F3) && input.IsKeyReleased(Keys.G)) {
            chunkBordersflag = false;
        }
        else if (input.IsKeyReleased(Keys.F3))
            f3Pressed = false;


        if(mouse.IsButtonPressed(MouseButton.Left)) {
            DestryBlock();
        }

        if (isFirstMove) {
            lastPosition = new(mouse.X, mouse.Y);
            isFirstMove = false;
        }
        else {
            var dX = mouse.X - lastPosition.X;
            var dY = mouse.Y - lastPosition.Y;

            lastPosition = new(mouse.X, mouse.Y);

            yawn += dX * Sensitivity * (float)e.Time;
            pitch -= dY * Sensitivity * (float)e.Time;
        }
       UpdateVectors();
    }

    private void DestryBlock() {
        World? world = GetWorld();
        if (world == null)
            return;

        Vector3 hitNormal = new();
      
        int x = (int)Math.Floor(position.X);
        int y = (int)Math.Floor(position.Y);
        int z = (int)Math.Floor(position.Z);

        int stepX = front.X < 0 ? -1 : 1;
        int stepY = front.Y < 0 ? -1 : 1;
        int stepZ = front.Z < 0 ? -1 : 1;

        float tMaxX = ((x + (stepX > 0 ? 1 : 0)) - position.X) / front.X;
        float tMaxY = ((y + (stepY > 0 ? 1 : 0)) - position.Y) / front.Y;
        float tMaxZ = ((z + (stepZ > 0 ? 1 : 0)) - position.Z) / front.Z;

        float tDeltaX = Math.Abs(1 / front.X);
        float tDeltaY = Math.Abs(1 / front.Y);
        float tDeltaZ = Math.Abs(1 / front.Z);

        float maxDistance = 5.0f; // Limit to 5 blocks
        float traveledDistance = 0.0f;

        for(int i=0; i< 100; ++i) {
            Vector3 chunkPos = GetChunkPos(position);
            Chunk.ConvertToWorldCoords(ref chunkPos);
            
            Vector3 chunkBlockPos = Chunk.ConvertToChunkBlockCoord(new Vector3(x, y, z));


            // Check if the chunk exists in the dictionary
            if (world.allChunks.TryGetValue(chunkPos, out Chunk chunk)) {
                // Retrieve the block type at the calculated block position
                BlockType block = chunk.GetBlockAt(chunkBlockPos);

                // If the block is solid (not air), stop the raycast and register the hit
                 if (block != BlockType.AIR) {
                    Console.WriteLine($"Hit block: {chunkBlockPos} {chunkPos}");
                    chunk.SetBlockAt(chunkBlockPos, BlockType.AIR);
                    chunk.ReDraw = true;
                    chunk.AddedFaces = false;
                    chunk.Delete();
                    world.MeshChunks();
                    break;
                }
            }

            // Determine next voxel step and distance
            if (tMaxX < tMaxY && tMaxX < tMaxZ) {
                traveledDistance = tMaxX;
                tMaxX += tDeltaX;
                x += stepX;
                hitNormal = new Vector3(-stepX, 0, 0);
            }
            else if (tMaxY < tMaxZ) {
                traveledDistance = tMaxY;
                tMaxY += tDeltaY;
                y += stepY;
                hitNormal = new Vector3(0, -stepY, 0);
            }
            else {
                traveledDistance = tMaxZ;
                tMaxZ += tDeltaZ;
                z += stepZ;
                hitNormal = new Vector3(0, 0, -stepZ);
            }

            // Stop if we exceed the max distance (5 blocks)
            if (traveledDistance > maxDistance)
                break;
        }

       

    }

    private void DPressedHandle(FrameEventArgs e) {
        Vector3 desiredPositon = position + right * Speed * (float)e.Time;
        Vector3 nextPos = position + (Math.Sign(right.X) * 1f, 0, 0);

        if (!CheckForCollisions(nextPos) && !CheckForCollisions(nextPos + (0,-1f,0)) )
            position.X = (desiredPositon).X;

        nextPos = position + (0, 1f, Math.Sign(right.Z) * 1f);

        if (!CheckForCollisions(nextPos) && !CheckForCollisions(nextPos + (0, -1f, 0)))
            position.Z = desiredPositon.Z;

        if (!CheckForCollisions(nextPos + (0, 1.8f, 0)) && !CheckForCollisions(nextPos + (0, -1.8f, 0)))
            position.Y = desiredPositon.Y;
    }

    private void APressedHandle(FrameEventArgs e) {
        Vector3 desiredPositon = position - right * Speed * (float)e.Time;
        Vector3 nextPos = position + (Math.Sign(-right.X) * 1f, 0, 0);

        if (!CheckForCollisions(nextPos) && !CheckForCollisions(nextPos + (0, -1f, 0)))
            position.X = (desiredPositon).X;

        nextPos = position + (0, 0, Math.Sign(-right.Z) * 1f);

        if (!CheckForCollisions(nextPos) && !CheckForCollisions(nextPos + (0, -1f, 0)))
            position.Z = desiredPositon.Z;

        if (!CheckForCollisions(nextPos + (0, 1.8f, 0)) && !CheckForCollisions(nextPos + (0, -1.8f, 0)))
            position.Y = desiredPositon.Y;
    }

    private void SPressedHandle(FrameEventArgs e) {
        Vector3 desiredPositon = position - front * Speed * (float)e.Time;
        Vector3 nextPos = position + (Math.Sign(front.X) * 1f, 0, 0);

        if (!CheckForCollisions(nextPos) && !CheckForCollisions(nextPos + (0, -1f, 0)))
            position.X = (desiredPositon).X;

        nextPos = position + (0, 0, Math.Sign(front.Z) * 1f);

        if (!CheckForCollisions(nextPos) && !CheckForCollisions(nextPos + (0, -1f, 0)))
            position.Z = desiredPositon.Z;

        if (!CheckForCollisions(nextPos + (0, 1.8f, 0)) && !CheckForCollisions(nextPos + (0, -1.8f, 0)))
            position.Y = desiredPositon.Y;
    }

    private void SpacePressedHandle(FrameEventArgs e) {
        //position.Y += Speed * (float)e.Time;

        Vector3 nextPos = position + (0, 2.2f, 0);
        if (!CheckForCollisions(nextPos))
            position += (0, Speed * (float)e.Time, 0);
    }

    private void ShiftPressedHandle(FrameEventArgs e) {
        Vector3 nextPos = position + (0, -2f, 0);
        if (!CheckForCollisions(nextPos))
            position += (0, -Speed * (float)e.Time, 0);

    }

    private void WPressedHandle(FrameEventArgs e) {
        Vector3 desiredPositon = position + front * Speed * (float)e.Time;
        Vector3 nextPos = position + (Math.Sign(front.X) * 1f, 0, 0);

        if (!CheckForCollisions(nextPos) && !CheckForCollisions(nextPos + (0, -1f, 0)) && !CheckForCollisions(nextPos + (0, 1.8f, 0)))
            position.X = (desiredPositon).X;

        nextPos = position + (0, 0, Math.Sign(front.Z) * 1f);

        if (!CheckForCollisions(nextPos) && !CheckForCollisions(nextPos + (0,-1f,0)))
            position.Z = desiredPositon.Z;

        if (!CheckForCollisions(nextPos + (0,1.8f, 0)) && !CheckForCollisions(nextPos + (0, -1.8f, 0)) && !CheckForCollisions(nextPos + (0, -1f, 0)))
            position.Y = desiredPositon.Y;
    }
    //position = nextPos + (Math.Sign(position.X) * 0.5f, 0 , 0);
    private bool CheckForCollisions(Vector3 nextPosition) {
        if (!doCollisionChecks)
            return false;
        if (nextPosition.Y > Chunk.HEIGHT - 1 || (int)nextPosition.Y < 2)
            return false;

        Chunk? forChekin = new();

        int posX, posY, posZ;

        int x = (int)nextPosition.X * Math.Sign(nextPosition.X),
            z = (int)nextPosition.Z * Math.Sign(nextPosition.Z);
        x  = x % 16;
        z = z % 16;

        posX = (int)((nextPosition.X -  nextPosition.X % 16)/ 16 + 1 * Math.Sign(nextPosition.X));
        posY = 0;
        posZ = (int)((nextPosition.Z -  nextPosition.Z % 16) / 16 + 1 * Math.Sign(nextPosition.Z));


        string chunkID = $"{Chunk.ConvertPosToChunkID(position)}";
        Vector3 chunkPos = new(
                             (position.X - position.X % 16),
                             0,
                             (position.Z - position.Z % 16)
                         );
        var world = GetWorld();

        if (world == null) {
            Console.WriteLine("Camera.CheckForCollision: world was null");
            return false;
        }
        if (!world!.allChunks.TryGetValue(chunkPos, out forChekin)) {
            Console.WriteLine($"Specified chunk not yer generated, ID: {chunkPos}");
            return false;
        }

        if (!world!.forRendering.Contains(forChekin)) {
            Console.WriteLine($"Current chunk not loaded, ID: {chunkID}");
            return false;
        }
        //Console.WriteLine($"Checking in chunk: {chunkID}");
        //Console.WriteLine($"Checking block: {x}, {0}, {z}");

        if (forChekin.chunkBlocks[x,(int)nextPosition.Y * Math.Sign(nextPosition.Y), z] != BlockType.AIR)
            return true;

        return false;
    }

    public World? GetWorld() {
        if (worldRef.TryGetTarget(out World? world))
            return world;
        else
            return null;
    }

    private void PrintCurrentPosition() {
        int posX, posY, posZ;

        posX = (int)((position.X -  position.X % 16 )/ 16 + 1 * Math.Sign(position.X));
        posY = 0;
        posZ = (int)((position.Z -  position.Z % 16) / 16 + 1 * Math.Sign(position.Z));

        Console.WriteLine($"Position: [ {position.X}, {position.Y}, {position.Z} ]");
        f3Pressed = true;
        string chunkID = GetChunkID(position);
        Console.WriteLine($"Current chunk: [ {chunkID} ]");
    }

    public static Vector3 GetChunkPos(Vector3 pos) {
        int posX, posY, posZ;

        posX = (int)((pos.X -  pos.X % 16) / 16 + 1 * Math.Sign(pos.X));
        posY = 0;
        posZ = (int)((pos.Z -  pos.Z % 16) / 16 + 1 * Math.Sign(pos.Z));
        return new(posX, posY, posZ);
    }

    public static string GetChunkID(Vector3 pos) {
        int posX, posY, posZ;

        posX = (int)((pos.X -  pos.X % 16) / 16 + 1 * Math.Sign(pos.X));
        posY = 0;
        posZ = (int)((pos.Z -  pos.Z % 16) / 16 + 1 * Math.Sign(pos.Z));
        return $"{posX},{posY},{posZ}";
    }

    public void Update(KeyboardState input, MouseState mouse, FrameEventArgs e, Window window) {
        InputController(input, mouse, e, window);
    } 
}
