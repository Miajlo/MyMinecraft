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
    public float upwardVelocity = 0;

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
            //gravitalVelocity = 0;
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
        // Movement direction (horizontal only)
        Vector3 moveDirection = Vector3.Zero;

        if (input.IsKeyDown(Keys.W))
            moveDirection += (front - new Vector3(0, front.Y, 0));
        if (input.IsKeyDown(Keys.S))
            moveDirection -= (front - new Vector3(0, front.Y, 0));
        if (input.IsKeyDown(Keys.A))
            moveDirection -= (right - new Vector3(0, right.Y, 0));
        if (input.IsKeyDown(Keys.D))
            moveDirection += (right - new Vector3(0, right.Y, 0));
        if (playerState == PlayerStates.IN_AIR) {
            gravitalVelocity += gameRules.gravity * deltaTime;
            gravitalVelocity = MathF.Min(gravitalVelocity, gameRules.terminalVelocity);
            if (gravitalVelocity == 0)
                playerState = PlayerStates.ON_GROUND;

        }
        if (input.IsKeyDown(Keys.Space) && playerState == PlayerStates.ON_GROUND) {
            upwardVelocity = CalculateInitialJumpVelocity();
            playerState = PlayerStates.IN_AIR;
            Console.WriteLine("Jump pressed!");
        }        

        if (moveDirection.LengthSquared > 0)
            moveDirection = Vector3.Normalize(moveDirection);       

        float newY = position.Y;

        newY += upwardVelocity*deltaTime - gravitalVelocity * deltaTime;

        newPosition.X += moveDirection.X * gameRules.movementSpeed * deltaTime;
        if (CheckForCollisions(GetPlayerHitbox(newPosition)).Any())
            newPosition.X = position.X;//revert

        newPosition.Z += moveDirection.Z * gameRules.movementSpeed * deltaTime;
        if (CheckForCollisions(GetPlayerHitbox(newPosition)).Any())
            newPosition.Z = position.Z;//revert

        newPosition.Y = newY;
        if (CheckForCollisions(GetPlayerHitbox(newPosition)).Any()) {
            newPosition.Y = position.Y;//revert
            gravitalVelocity = 0;
            upwardVelocity = 0;
            playerState = PlayerStates.ON_GROUND;
        }        

        var world = GetWorld();

        if (playerState == PlayerStates.IN_AIR && gravitalVelocity == 0 && upwardVelocity == 0)
            playerState = PlayerStates.ON_GROUND;

        if (world != null && playerState == PlayerStates.ON_GROUND) {
            var hitbox = GetPlayerHitbox(newPosition);
            bool fullyAirborne = true; // Assume the player is airborne
            //Console.WriteLine($"FeetY:{hitbox.Min.Y}, Eye height:{position.Y}");
            float minY = MathF.Floor(hitbox.Min.Y)+0.1f;
            var corners = new[] {
                new Vector3(hitbox.Min.X, minY, hitbox.Min.Z), // Front-left corner
                new Vector3(hitbox.Max.X, minY, hitbox.Min.Z), // Front-right corner
                new Vector3(hitbox.Min.X, minY, hitbox.Max.Z), // Back-left corner
                new Vector3(hitbox.Max.X, minY, hitbox.Max.Z)  // Back-right corner
            };

            foreach (var corner in corners) {
                var checkPos = new Vector3(corner.X, corner.Y - 1, corner.Z);
                var chunkPos = Chunk_r.ConvertToChunkCoords(checkPos);
                var blockPos = Chunk_r.ConvertToChunkBlockCoord(checkPos);

                var block = world.GetBlockAt(chunkPos, blockPos);
                if (block != BlockType.AIR) {
                    fullyAirborne = false; 
                    break; 
                }
            }
            if (fullyAirborne) 
                playerState = PlayerStates.IN_AIR;
            
        }

        if (playerState == PlayerStates.ON_GROUND)
            newPosition.Y = MathF.Floor(newPosition.Y)+0.7f;

        return newPosition;
    }






    private Collision CheckForCollisions(AABB playerAABB) {
        if (!gameRules.doCollisionChecks)
            return new();

        if (playerAABB.Max.Y > Chunk_r.HEIGHT - 1 || playerAABB.Min.Y < 2)
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

        Console.WriteLine($"Position eye: [ {position.X}, {position.Y}, {position.Z} ]");
        Console.WriteLine($"Position feet: [ {position.X}, {position.Y-GameConfig.playerEyeHeight}, {position.Z} ]");
        f3Pressed = true;
        var chunkID = Chunk_r.ConvertToChunkCoords(position);
        Console.WriteLine($"Current chunk: [ {chunkID} ]");
        var world = GetWorld();

        if (world == null)
            return;

        Console.WriteLine($"Gravity velocity: [{gravitalVelocity}]");
        Console.WriteLine($"Player state: [{playerState}]");
        Console.WriteLine($"Initial jump velocity: [{CalculateInitialJumpVelocity()}]");
        Console.WriteLine($"Terminal velocity: [{gameRules.terminalVelocity}]");

        var blockPos = Chunk_r.ConvertToChunkBlockCoord(position);

        blockPos.Y-=2;

        var block = world.GetBlockAt(chunkID, blockPos);

        Console.WriteLine($"Standing on block: {block}, block position: {blockPos}");
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

    public float CalculateInitialJumpVelocity() {
        return MathF.Sqrt(2 * gameRules.gravity * gameRules.jumpHeight);
    }



    public List<ServerPacket> Update(KeyboardState input, MouseState mouse, FrameEventArgs e, Window window) {
        return InputController(input, mouse, e, window);
    } 
}
