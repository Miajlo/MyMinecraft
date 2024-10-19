using MyMinecraft.Unused;
using OpenTK.Compute.OpenCL;
using MyMinecraft.Graphics;

namespace MyMinecraft;

public class Window : GameWindow {
    private int Width;
    private int Height;

    public int renderDistance = 4;
    public static World world = new();
    
    Camera? camera;

    ShaderProgram shaderProgram;
    
    public Window(int width, int height, string title):base(GameWindowSettings.Default, NativeWindowSettings.Default) {
        this.CenterWindow(new Vector2i(width, height));
        Height = height;
        Width = width;
        this.Title = title;
    }

    protected override void OnLoad() {
        base.OnLoad();

               
        shaderProgram = new();

        //for(int i=0; i < renderDistance;++i) {
        //    for(int j=0; j < renderDistance;++j) {
        //        world.AddChunk(new(new(i*16, 0, j*16)));
        //    }
        //}
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Less);
        camera = new(Width, Height, (1 , 66, 1));
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

        Matrix4 model = Matrix4.Identity;
        Matrix4 view = camera!.GetViewMatrix();
        Matrix4 projection = camera!.GetProjectionMatrix();

        int modelLocation = GL.GetUniformLocation(shaderProgram.ID, "model");
        int viewLocation = GL.GetUniformLocation(shaderProgram.ID, "view");
        int projectionLocation = GL.GetUniformLocation(shaderProgram.ID, "projection");

        GL.UniformMatrix4(modelLocation, true, ref model);
        GL.UniformMatrix4(viewLocation, true, ref view);
        GL.UniformMatrix4(projectionLocation, true, ref projection);
        //Console.WriteLine($"New Thread ID: {Thread.CurrentThread.ManagedThreadId}");
        world.RenderChunks(shaderProgram);

        Context.SwapBuffers();
        base.OnRenderFrame(args);
    }

    protected override void OnUpdateFrame(FrameEventArgs args) {
        MouseState mouseState = MouseState;
        KeyboardState keyboardState = KeyboardState;

        base.OnUpdateFrame(args);
        //Console.WriteLine("Update frame!");
        camera!.Update(keyboardState, MouseState, args, this);

        var currChunkPos = Camera.GetChunkPos(camera.position);
        if (currChunkPos != camera.lastChunkPos && camera.generateChunks) {
            world.UpdateChunkRanderList(camera.position);
            camera.lastChunkPos =  Camera.GetChunkPos(camera.position);
        }
       
        //CheckForCollision();
    }
}
