using MyMinecraft.Models_r;

namespace MyMinecraft.Models; 
public class Camera {
    #region GameRules
    public GameRules gameRules = new();
    #endregion
    #region Flags
    private bool collisionChecksFlag = false;
    private bool f3Pressed = false;
    private bool genChunksFlag = false;
    private bool chunkBordersflag = false;
    #endregion
    public BlockType selectedBlock;
    private float Width;
    private float Height;
    private float Sensitivity = 180f;

    private float pitch;
    private float yawn = -90.0f;

    public float range = 5.0f;

    private bool isFirstMove = true;
    public Vector2 lastPosition;

    WeakReference<World_r> worldRef;
    Server_r server;
    public Vector3 position;
    public Vector3 lastChunkPos = (-500, 0, -500);
    Vector3 up = Vector3.UnitY;
    public Vector3 front = - Vector3.UnitZ;
    Vector3 right = Vector3.UnitX;
    public Camera(float width, float height, Vector3 pos, WeakReference<World_r> worldd, Server_r server) {
        Width = width;
        Height = height;
        position = pos;
        worldRef = worldd;
        selectedBlock = BlockType.DIRT;
        this.server = server;
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

    public List<ServerPacket> InputController(KeyboardState input, MouseState mouse, FrameEventArgs e, Window window) {
        List<ServerPacket> packetsToSend = [];
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

        HandleNumericalInput(input);

        if (input.IsKeyPressed(Keys.F3))
            PrintCurrentPosition();


        if (input.IsKeyPressed(Keys.N) ) {
            gameRules.doCollisionChecks = !gameRules.doCollisionChecks;
            Console.WriteLine($"Collision checks set to: {gameRules.doCollisionChecks}");
        }
        


        if (input.IsKeyPressed(Keys.M)) {
            gameRules.generateChunks = !gameRules.generateChunks;
            Console.WriteLine($"Generate chunks set to: {gameRules.generateChunks}");
        }

        if (input.IsKeyPressed(Keys.G)) {
            gameRules.showChunkBorders = !gameRules.showChunkBorders;
            Console.WriteLine($"Show Chunk Borders: {gameRules.showChunkBorders}");
            
        }



        if(mouse.IsButtonPressed(MouseButton.Left)) {
            //DestryBlock();
            packetsToSend.Add(new DestroyBlockPacket(position, front, range));
        }

        if (mouse.IsButtonPressed(MouseButton.Right))
            packetsToSend.Add(new PlaceBlockPacket(position, front, range, selectedBlock));

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

        return packetsToSend;
    }

    private void HandleNumericalInput(KeyboardState input) {
        for (int i = 1; i <= 9; i++) { // Loop through number keys
            Keys key = Keys.D0 + i;       // Regular number keys (0-9)           

            if (input.IsKeyPressed(key)) {
                byte blockId = (byte)i; // Convert int to byte first

                if (!Enum.IsDefined(typeof(BlockType), blockId))
                    continue;

                selectedBlock = (BlockType)blockId;
                Console.WriteLine($"Switched to block: {selectedBlock}");
            }
        }
    }

    private void DPressedHandle(FrameEventArgs e) {
        Vector3 desiredPositon = position + right * gameRules.movementSpeed * (float)e.Time;
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
        Vector3 desiredPositon = position - right * gameRules.movementSpeed * (float)e.Time;
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
        Vector3 desiredPositon = position - front * gameRules.movementSpeed * (float)e.Time;
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
            position += (0, gameRules.movementSpeed* (float)e.Time, 0);
    }

    private void ShiftPressedHandle(FrameEventArgs e) {
        Vector3 nextPos = position + (0, -2f, 0);
        if (!CheckForCollisions(nextPos))
            position += (0, -gameRules.movementSpeed * (float)e.Time, 0);

    }

    private void WPressedHandle(FrameEventArgs e) {
        Vector3 desiredPositon = position + front * gameRules.movementSpeed * (float)e.Time;
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
        if (!gameRules.doCollisionChecks)
            return false;
        if (nextPosition.Y > Chunk.HEIGHT - 1 || (int)nextPosition.Y < 2)
            return false;

        Chunk_r? forChekin = new();

        Vector3i chunkPos = GetChunkPos(nextPosition);
        Chunk.ConvertToWorldCoords(ref chunkPos);

        Vector3 blockPos = Chunk.ConvertToChunkBlockCoord(nextPosition);

        var world = GetWorld();

        if (world == null) {
            Console.WriteLine("Camera.CheckForCollision: world was null");
            return false;
        }
        if (!world!.GetChunk((Vector3i)chunkPos, out forChekin)) {
            Console.WriteLine($"Specified chunk not yer generated, ID: {chunkPos}");
            return false;
        }

        if (!world!.IsLoadedChunk(forChekin.position)) {
            Console.WriteLine($"Current chunk not loaded, ID: {chunkPos}");
            return false;
        }
        //Console.WriteLine($"Checking in chunk: {chunkID}");
        //Console.WriteLine($"Checking block: {x}, {0}, {z}");

        if (forChekin.GetBlockAt(blockPos) != BlockType.AIR)
            return true;

        return false;
    }

    public World_r? GetWorld() {
        if (worldRef.TryGetTarget(out var world))
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

    public static Vector3i GetChunkPos(Vector3 pos) {
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

    public List<ServerPacket> Update(KeyboardState input, MouseState mouse, FrameEventArgs e, Window window) {
        return InputController(input, mouse, e, window);
    } 
}
