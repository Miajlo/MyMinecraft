namespace VisualisationBinomialHeap.Models; 
public class Camera {
    public bool doChecks = false;
    public bool f3Pressed = false;
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
       
        if (input.IsKeyDown(Keys.W) && !CheckForCollision())
            position += front * Speed * (float)e.Time;
        if (input.IsKeyDown(Keys.S) && !CheckForCollision())
            position -= front * Speed * (float)e.Time;
        if (input.IsKeyDown(Keys.A) && !CheckForCollision())
            position -= right * Speed * (float)e.Time;
        if (input.IsKeyDown(Keys.D) && !CheckForCollision())
            position += right * Speed * (float)e.Time;
        if (input.IsKeyDown(Keys.Space) && !CheckForCollision())
            position.Y += Speed * (float)e.Time;
        if (input.IsKeyDown(Keys.LeftShift) && !CheckForCollision())
            position.Y -= Speed * (float)e.Time;
        if (input.IsKeyDown(Keys.Escape))
            window.Close();
        if (input.IsKeyDown(Keys.F3) && !f3Pressed) {
            Console.WriteLine($"Position: [ {position.X}, {position.Y}, {position.Z} ]");
            f3Pressed = true;
            string chunkID = $"{(int)position.X / 16}," +
            $" {(int)position.Y / 16}, {(int)position.Z / 16}";
            Console.WriteLine($"Current chunk: [ {chunkID} ]");
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

    public bool CheckForCollision() {
        if (!doChecks)
            return false;

        if (position.X < 0 || position.Y < 0 || position.Z < 0)
            return false;

        string chunkID = $"{position.X - position.X%16}," +
            $" {position.Y - position.Y % 16}, {position.Z - position.Z % 16}";
        Console.WriteLine("Current chunk: " + chunkID);
        int X = (int)position.X;
        int Y = (int)position.Y;
        int Z = (int)position.Z;

        Chunk forCheckin = null;
        
        foreach (var c in Window.chunks)
            if (c.ID == chunkID)
                forCheckin = c;

        if (forCheckin == null)
            return false;

        if (X - 1 < forCheckin.position.X + 16 || X + 1 > forCheckin.position.X)
            return true;
        if (Y - 2 < forCheckin.position.Y + 16 || Y + 2 > forCheckin.position.Y)
            return true;
        if (Z - 1 < forCheckin.position.Z + 16 || Z + 1> forCheckin.position.Z)
            return true;

        return false;
    }

    public void Update(KeyboardState input, MouseState mouse, FrameEventArgs e, Window window) {
        InputController(input, mouse, e, window);
    } 
}
