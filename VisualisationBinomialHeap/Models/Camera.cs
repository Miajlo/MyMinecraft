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
    public PlayerStates playerState;
    public float gravitalVelocity = 0;

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
        playerState = PlayerStates.IN_AIR;
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
        Vector3 newPosition = position;

        //if (input.IsKeyDown(Keys.W))
        //    newPosition += front * gameRules.movementSpeed * (float)e.Time;

        //if (input.IsKeyDown(Keys.S))
        //    newPosition -= front * gameRules.movementSpeed * (float)e.Time;

        //if (input.IsKeyDown(Keys.A))
        //    newPosition -= right * gameRules.movementSpeed * (float)e.Time;

        //if (input.IsKeyDown(Keys.D))
        //    newPosition += right * gameRules.movementSpeed * (float)e.Time;

        //if (input.IsKeyDown(Keys.Space))
        //    newPosition += new Vector3(0, gameRules.movementSpeed * (float)e.Time, 0);

        //if (input.IsKeyDown(Keys.LeftShift))
        //    newPosition -= new Vector3(0, gameRules.movementSpeed * (float)e.Time, 0);

        newPosition = gameRules.physics 
                    ? HandleMovement(input, e)
                    : HandleMovementNoPhysics(input,e);


        Vector3 finalPosition = position;
        AABB newHitbox;

        // Check X movement first
        newHitbox = GetPlayerHitbox(new(newPosition.X, position.Y, position.Z));
        if (!CheckForCollisions(newHitbox).CollidedX)
            finalPosition.X = newPosition.X;
        else
            newPosition.X = finalPosition.X; // Revert X movement if colliding

        //if (gameRules.physics && playerState == PlayerStates.IN_AIR) {
        //    gravitalVelocity += gameRules.gravity;
        //    if (gravitalVelocity > GameConfig.terminalVelocity)
        //        gravitalVelocity = GameConfig.terminalVelocity;
        //    newPosition.Y -= gravitalVelocity * (float)e.Time;
        //}

        // Check Y movement (gravity & jumping)
        newHitbox = GetPlayerHitbox(new(finalPosition.X, newPosition.Y, position.Z));
        if (!CheckForCollisions(newHitbox).CollidedY)
            finalPosition.Y = newPosition.Y;
        else {
            newPosition.Y = finalPosition.Y; // Revert Y movement if colliding
            gravitalVelocity = 0;
        }

        // Check Z movement
        newHitbox = GetPlayerHitbox(new(finalPosition.X, finalPosition.Y, newPosition.Z));
        if (!CheckForCollisions(newHitbox).CollidedZ)
            finalPosition.Z = newPosition.Z;
        else
            newPosition.Z = finalPosition.Z; // Revert Z movement if colliding

        // Final check: Ensure player is not fully inside a block
        newHitbox = GetPlayerHitbox(finalPosition);
        if (CheckForCollisions(newHitbox).Any()) {
            finalPosition = position; // Fully blocked, revert to original position
        }

        //if (finalPosition.Y == position.Y && gameRules.physics) {
        //    playerState = PlayerStates.ON_GROUND;
        //    gravitalVelocity = 0;
        //    //Console.WriteLine($"Player state: [{playerState}]");
        //}
        //if (finalPosition.Y != position.Y && playerState == PlayerStates.ON_GROUND) {
        //    playerState = PlayerStates.IN_AIR;
        //    //Console.WriteLine($"Player state: [{playerState}]");
        //}

        // Apply movement
        position = finalPosition;


        if (input.IsKeyDown(Keys.Escape))
            window.Close();

        HandleNumericalInput(input);

        if (input.IsKeyPressed(Keys.F3))
            PrintCurrentPosition();


        if(input.IsKeyPressed(Keys.F)) {
            gameRules.physics = !gameRules.physics;
            Console.WriteLine($"Physics set to: {gameRules.physics}");
        }

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

    private Vector3 HandleMovementNoPhysics(KeyboardState input, FrameEventArgs e) {
        Vector3 newPosition = position;
        if (input.IsKeyDown(Keys.W))
            newPosition += front * gameRules.movementSpeed * (float)e.Time;

        if (input.IsKeyDown(Keys.S))
            newPosition -= front * gameRules.movementSpeed * (float)e.Time;

        if (input.IsKeyDown(Keys.A))
            newPosition -= right * gameRules.movementSpeed * (float)e.Time;

        if (input.IsKeyDown(Keys.D))
            newPosition += right * gameRules.movementSpeed * (float)e.Time;

        if (input.IsKeyDown(Keys.Space))
            newPosition += new Vector3(0, gameRules.movementSpeed * (float)e.Time, 0);

        if (input.IsKeyDown(Keys.LeftShift))
            newPosition -= new Vector3(0, gameRules.movementSpeed * (float)e.Time, 0);

        return newPosition;
    }

    private Vector3 HandleMovement(KeyboardState input, FrameEventArgs e) {
        if (!gameRules.physics)
            return position; // No movement if physics is disabled

        Vector3 newPosition = position;
        float deltaTime = (float)e.Time;

        // Movement direction
        Vector3 moveDirection = Vector3.Zero;

        if (input.IsKeyDown(Keys.W))
            moveDirection += (front - new Vector3(0, front.Y, 0));
        if (input.IsKeyDown(Keys.S))
            moveDirection -= (front - new Vector3(0, front.Y, 0));

        if (input.IsKeyDown(Keys.A))
            moveDirection -= (right - new Vector3(0, right.Y, 0));
        if (input.IsKeyDown(Keys.D))
            moveDirection += (right - new Vector3(0, right.Y, 0));

        if (moveDirection.LengthSquared > 0)
            moveDirection = Vector3.Normalize(moveDirection);

        // Apply gravity
        gravitalVelocity -= gameRules.gravity * deltaTime;
        gravitalVelocity = MathF.Max(gravitalVelocity, -GameConfig.terminalVelocity); // Clamp velocity

        // Jumping logic
        if (input.IsKeyDown(Keys.Space) && playerState == PlayerStates.ON_GROUND) {
            gravitalVelocity = gameRules.jumpForce;
            playerState = PlayerStates.IN_AIR;
        }

        // **Apply vertical movement first**
        newPosition.Y += gravitalVelocity * deltaTime;

        // **Check for ceiling collision (head hit)**
        AABB ceilingHitbox = GetPlayerHitbox(new Vector3(newPosition.X, newPosition.Y + 0.1f, newPosition.Z));
        if (CheckForCollisions(ceilingHitbox).Any()) {
            gravitalVelocity = 0;
            newPosition.Y = position.Y; // Revert to previous Y position
        }

        // **Ground Check BEFORE movement**
        bool onGround = CheckForCollisions(GetPlayerHitbox(new Vector3(newPosition.X, newPosition.Y - 0.1f, newPosition.Z))).Any();
        if (onGround) {
            playerState = PlayerStates.ON_GROUND;
            gravitalVelocity = 0;
        }
        else {
            playerState = PlayerStates.IN_AIR;
        }

        // **Apply horizontal movement AFTER gravity adjustments**
        float airControlFactor = (playerState == PlayerStates.IN_AIR) ? 0.6f : 1.0f;
        Vector3 finalMove = moveDirection * gameRules.movementSpeed * airControlFactor * deltaTime;

        // **Check horizontal collisions separately**
        Vector3 moveX = new Vector3(finalMove.X, 0, 0);
        AABB hitboxX = GetPlayerHitbox(newPosition + moveX);
        if (CheckForCollisions(hitboxX).Any()) {
            // **Step up logic for X**
            AABB stepUpHitbox = GetPlayerHitbox(newPosition + moveX + new Vector3(0, 0.6f, 0)); // Check for step height
            if (!CheckForCollisions(stepUpHitbox).Any()) {
                float stepSpeed = 3.0f * deltaTime; // Smooth step-up speed
                newPosition.Y += MathF.Min(0.3f, stepSpeed); // Move up gradually
                
            }
            else {
                moveX = Vector3.Zero; // Blocked movement
            }
        }

        Vector3 moveZ = new Vector3(0, 0, finalMove.Z);
        AABB hitboxZ = GetPlayerHitbox(newPosition + moveZ);
        if (CheckForCollisions(hitboxZ).Any()) {
            // **Step up logic for Z**
            AABB stepUpHitbox = GetPlayerHitbox(newPosition + moveZ + new Vector3(0, 0.6f, 0));
            if (!CheckForCollisions(stepUpHitbox).Any()) {
                float stepSpeed = 3.0f * deltaTime; // Smooth step-up speed
                newPosition.Y += MathF.Min(0.3f, stepSpeed); // Move up gradually
                
            }
            else {
                moveZ = Vector3.Zero;
            }
        }

        // **Apply movement only in the allowed directions**
        newPosition += moveX + moveZ;

        // **Final Ground Check (for stepping up)**
        if (CheckForCollisions(GetPlayerHitbox(new Vector3(newPosition.X, newPosition.Y - 0.1f, newPosition.Z))).Any()) {
            playerState = PlayerStates.ON_GROUND;
            gravitalVelocity = 0;
        }

        return newPosition;
    }





    private Collision CheckForCollisions(AABB playerAABB) {
        if (!gameRules.doCollisionChecks)
            return new();

        if (playerAABB.Max.Y > Chunk.HEIGHT - 1 || playerAABB.Min.Y < 2)
            return new Collision(false, false, false);

        var world = GetWorld();
        if (world == null) {
            Console.WriteLine("Camera.CheckForCollision: world was null");
            return new Collision(false, false, false);
        }

        bool collidedX = false, collidedY = false, collidedZ = false;

        for (int x = (int)Math.Floor(playerAABB.Min.X); x <= (int)Math.Floor(playerAABB.Max.X); x++) {
            for (int y = (int)Math.Floor(playerAABB.Min.Y); y <= (int)Math.Floor(playerAABB.Max.Y); y++) {
                for (int z = (int)Math.Floor(playerAABB.Min.Z); z <= (int)Math.Floor(playerAABB.Max.Z); z++) {
                    Vector3 blockPos = new Vector3(x, y, z);
                    Vector3i chunkPos = (Vector3i)Chunk_r.ConvertToChunkCoords(blockPos);
                    Vector3 localBlockPos = Chunk_r.ConvertToChunkBlockCoord(blockPos);

                    if (!world.GetChunk(chunkPos, out var chunk))
                        continue;

                    if (!world.IsLoadedChunk(chunk.position))
                        continue;

                    if (chunk.GetBlockAt(localBlockPos) != BlockType.AIR) {
                        // Block hitbox (AABB for the block)
                        AABB blockAABB = new AABB(blockPos, blockPos + Vector3.One);

                        if (playerAABB.Intersects(blockAABB)) {
                            if (playerAABB.Max.X > blockAABB.Min.X && playerAABB.Min.X < blockAABB.Max.X)
                                collidedX = true;

                            if (playerAABB.Max.Y > blockAABB.Min.Y && playerAABB.Min.Y < blockAABB.Max.Y)
                                collidedY = true;

                            if (playerAABB.Max.Z > blockAABB.Min.Z && playerAABB.Min.Z < blockAABB.Max.Z)
                                collidedZ = true;
                        }
                    }
                }
            }
        }

        return new Collision(collidedX, collidedY, collidedZ);
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
        var chunkID = Chunk_r.ConvertToChunkCoords(position);
        Console.WriteLine($"Current chunk: [ {chunkID} ]");
        var world = GetWorld();

        if (world == null)
            return;

        Console.WriteLine($"Gravity velocity: [{gravitalVelocity}]");
        Console.WriteLine($"Player state: [{playerState}]");
        //var collision = CheckForCollisions(GetPlayerHitbox(position));

        //Console.WriteLine($"Collision [x,y,z]: [{collision.CollidedX}, {collision.CollidedY}, {collision.CollidedZ}");
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

    public AABB GetPlayerHitbox(Vector3 eyePos) {
        float minX = eyePos.X - GameConfig.playerWidth / 2;
        float maxX = eyePos.X + GameConfig.playerWidth / 2;

        float minY = eyePos.Y - GameConfig.playerEyeHeight; // Bottom of hitbox (feet)
        float maxY = eyePos.Y + GameConfig.playerTopHeight; // Top of hitbox (head)

        float minZ = eyePos.Z - GameConfig.playerDepth / 2;
        float maxZ = eyePos.Z + GameConfig.playerDepth / 2;

        return new AABB(new Vector3(minX, minY, minZ), new Vector3(maxX, maxY, maxZ));
    }

    public List<ServerPacket> Update(KeyboardState input, MouseState mouse, FrameEventArgs e, Window window) {
        return InputController(input, mouse, e, window);
    } 
}
