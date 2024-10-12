using MyMinecraft.Unused;
using OpenTK.Compute.OpenCL;
using VisualisationBinomialHeap.Graphics;

namespace VisualisationBinomialHeap;

public class Window : GameWindow {
    private int Width;
    private int Height;

    Chunk chunk, c2;
    public int renderDistance = 2;
    public static World world = new();
    
    Camera? camera;

    ShaderProgram shaderProgram;
    
    public Window(int width, int height, string title):base(GameWindowSettings.Default, NativeWindowSettings.Default) {
        this.CenterWindow(new Vector2i(width, height));
        Height = height;
        Width = width;
        this.Title = title;

        //model = Matrix4.Identity;
        //view = Matrix4.Identity;
        //projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(60.0f),
        //                                                          Width/ Height, 0.1f, 100.0f);
        //model = Matrix4.CreateTranslation(0f, 0f, -3f);
    }

    protected override void OnLoad() {
        base.OnLoad();

               
        shaderProgram = new();

        for(int i=0; i < renderDistance;++i) {
            for(int j=0; j < renderDistance;++j) {
                world.AddChunk(new(new(i*16, 0, j*16)));
            }
        }
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Less);
        camera = new(Width, Height, (-5,-5,-5));
        CursorState = CursorState.Grabbed;
    }

    protected override void OnUnload() {
        base.OnUnload();

        shaderProgram.Delete();
        foreach (var c in world.allChunks)
            c.Value.Delete();
    }

    protected override void OnResize(ResizeEventArgs e) {
        base.OnResize(e);
        GL.Viewport(0, 0, e.Width, e.Height);
        this.Height = e.Height;
        this.Width = e.Width;
    }

    protected override void OnRenderFrame(FrameEventArgs args) {
        GL.ClearColor(0f, 0f, 1f, 1f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

       

        //int colorLocation = GL.GetUniformLocation(shaderProgram.ID, "uColor");

        Matrix4 model = Matrix4.Identity;
        Matrix4 view = camera!.GetViewMatrix();
        Matrix4 projection = camera!.GetProjectionMatrix();

        int modelLocation = GL.GetUniformLocation(shaderProgram.ID, "model");
        int viewLocation = GL.GetUniformLocation(shaderProgram.ID, "view");
        int projectionLocation = GL.GetUniformLocation(shaderProgram.ID, "projection");

        GL.UniformMatrix4(modelLocation, true, ref model);
        GL.UniformMatrix4(viewLocation, true, ref view);
        GL.UniformMatrix4(projectionLocation, true, ref projection);

        //chunk.Render(shaderProgram);
        //c2.Render(shaderProgram);

        foreach (var chank in world.allChunks)
            chank.Value.Render(shaderProgram);

        Context.SwapBuffers();
        base.OnRenderFrame(args);
    }

    private bool CheckShapeToColor(Shape shape) {
        return shape is NumberOne ||
               shape is Rectangle ||
               shape is Parallelogram;
    }

    protected override void OnUpdateFrame(FrameEventArgs args) {
        MouseState mouseState = MouseState;
        KeyboardState keyboardState = KeyboardState;

        base.OnUpdateFrame(args);

        camera.Update(keyboardState, MouseState, args, this);

        //CheckForCollision();
    }

    //private void CheckForCollision() {
    //    int X = (int)camera.position.X;
    //    int Y = (int)camera.position.Y;
    //    int Z = (int)camera.position.Z;

    //    if (Y - 2 < 16 && Y + 2 > 0)
    //        Console.WriteLine("Detected horizontal collision: " + Y);
    //    if (X - 1 < 16 && X + 1 > 0)
    //        Console.WriteLine("Detected vertical X collision: " + X);
    //    if (Z - 1 < 16 && Z + 1 > 0)
    //        Console.WriteLine("Detected vertical Z collision: " + Z);
    //}

    public static string LoadShaderSource(string v) {
        string shaderSource = "";

        try {
            using (StreamReader sr = new("../../../Shaders/" + v)) {
                shaderSource = sr.ReadToEnd();
            }
        }
        catch (Exception ex) {
            Console.WriteLine($"Error:{ex.Message}");
        }
        return shaderSource;
    }


}
