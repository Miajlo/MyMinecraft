namespace VisualisationBinomialHeap.Models; 
public class Camera {
    public bool doCollisionChecks = false;
    public bool collisionChecksFlag = false;
    public bool f3Pressed = false;
    private float corrFact = 0.0f;
    public float Speed = 8f;
    private float Width;
    private float Height;
    private float Sensitivity = 180f;

    private float pitch;
    private float yawn = -90.0f;

    private bool isFirstMove = true;
    public Vector2 lastPosition;

    public Vector3 position;

    Vector3 up = Vector3.UnitY;
    Vector3 front = - Vector3.UnitZ;
    Vector3 right = Vector3.UnitX;
    public Camera(float width, float height, Vector3 pos) {
        Width = width;
        Height = height;
        position = pos;
    }
    public Matrix4 GetViewMatrix() {
        return Matrix4.LookAt(position, position + front, up);
    }
    public Matrix4 GetProjectionMatrix() {
        return Matrix4.CreatePerspectiveFieldOfView(
            MathHelper.DegreesToRadians(45.0f),
            Width / Height, 0.1f, 100.0f);
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
            position += front * Speed * (float)e.Time;
        if (input.IsKeyDown(Keys.S))
            position -= front * Speed * (float)e.Time;
        if (input.IsKeyDown(Keys.A))
            position -= right * Speed * (float)e.Time;
        if (input.IsKeyDown(Keys.D))
            position += right * Speed * (float)e.Time;
        if (input.IsKeyDown(Keys.Space))
            position.Y += Speed * (float)e.Time;
        if (input.IsKeyDown(Keys.LeftShift))
            position.Y -= Speed * (float)e.Time;
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
            
        if(isFirstMove) {
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

    private void ShiftPressedHandle(FrameEventArgs e) {
        Vector3 nextPos = position + (0, - Speed * (float)e.Time, 0);
        if (!CheckForCollision(nextPos))
            position = nextPos;

    }

    private void WPressedHandle(FrameEventArgs e) {
        Vector3 nextPos = position + front * Speed * (float)e.Time;
        if (!CheckForCollision(nextPos))
            position = nextPos;
    }

    private void PrintCurrentPosition() {
        float fposX = (int)(position.X + position.X > 0 ? corrFact : -corrFact);
        float fposY = (int)(position.Y + position.Y > 0 ? corrFact : -corrFact);
        float fposZ = (int)(position.Y + position.Y > 0 ? corrFact : -corrFact);

        int posX = (int)Math.Floor(fposX / 16);
        int posY = (int)Math.Floor(fposY / 16);
        int posZ = (int)Math.Floor(fposZ / 16);

        Console.WriteLine($"Position: [ {position.X}, {position.Y}, {position.Z} ]");
        f3Pressed = true;
        string chunkID = $"{posX}, {posY}, {posZ}";
        Console.WriteLine($"Current chunk: [ {chunkID} ]");
    }

    public bool CheckForCollision(Vector3 nextPosition) {
        if (!doCollisionChecks)
            return false;

        if (nextPosition.X < 0 || position.Y < 0 || nextPosition.Z < 0)
            return false;

        string chunkID = $"{(int)(nextPosition.X - position.X%16)}," +
                         $" {(int)(nextPosition.Y - nextPosition.Y % 16)}," +
                         $" {(int)(nextPosition.Z - nextPosition.Z % 16)}";
        //Console.WriteLine("Current chunk: " + chunkID);
        int X = (int)nextPosition.X;
        int Y = (int)nextPosition.Y;
        int Z = (int)nextPosition.Z;

        Chunk forCheckin = new();
        if (!Window.world.allChunks.TryGetValue(chunkID, out forCheckin!)) {
            //Console.WriteLine($"Not generated chunk: {chunkID}");
            return false;
        }
        //Console.WriteLine($"Found chunk: {forCheckin.ID}");

        if (X - 1 < forCheckin.position.X + 16 || X + 1 > forCheckin.position.X)
            return true;
        if (Y - 10 < forCheckin.position.Y + 18 || Y + 2 > forCheckin.position.Y)
            return true;
        if (Z - 1 < forCheckin.position.Z + 16 || Z + 1> forCheckin.position.Z)
            return true;

        return false;
    }

    public void Update(KeyboardState input, MouseState mouse, FrameEventArgs e, Window window) {
        InputController(input, mouse, e, window);
    } 
}
