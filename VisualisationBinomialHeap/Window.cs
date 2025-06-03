using MyMinecraft.Unused;
using OpenTK.Compute.OpenCL;
using MyMinecraft.Graphics;
using System.Diagnostics;
using MyMinecraft.Models_r;

namespace MyMinecraft;

public class Window : GameWindow {
    private int Width;
    private int Height;

    private Stopwatch stopwatch;
    private int frameCount;

    public int renderDistance = 5;
    public World_r world = new();
    
    Camera? camera;

    Server_r server;

    ShaderProgram shaderProgram;
    HudTextures hud;
    public Window(int width, int height, string title):base(GameWindowSettings.Default, NativeWindowSettings.Default) {
        this.CenterWindow(new Vector2i(width, height));
        Height = height;
        Width = width;
        this.Title = title;
        hud = new();
        stopwatch = new();
        stopwatch.Start();
        frameCount = 0;
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
        GL.Enable(EnableCap.CullFace);
        GL.FrontFace(FrontFaceDirection.Cw); // or Cw, depending on your winding order
        GL.CullFace(CullFaceMode.Back); // or Front, depending on which faces should be culled
        GL.DepthFunc(DepthFunction.Less); // or another appropriate depth function

        camera = new(Width, Height, (1 , 70, 1), new WeakReference<World_r>(world), server);
        server = new Server_r(world, renderDistance);
        CursorState = CursorState.Grabbed;
        server.Start();
    }

    protected override void OnUnload() {
        base.OnUnload();

        shaderProgram.Delete();
        server.Stop();
        Texture.Delete();
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
        //if(world.readyToRender)


        server.RenderChunks(shaderProgram);

        //if (camera.gameRules.showChunkBorders)
        //    world.DrawChunkBorders(shaderProgram, camera.position);


        frameCount++;

        // If one second (or more) has passed, calculate FPS and update title
        if (stopwatch.ElapsedMilliseconds >= 1000) {
            double fps = frameCount / (stopwatch.ElapsedMilliseconds / 1000.0);
            // Reset for next calculation:
            Title = $"OpenGL Application - FPS: {fps}";
            frameCount = 0;
            stopwatch.Restart();
        }



        Context.SwapBuffers();
        base.OnRenderFrame(args);
    }

    protected override void OnUpdateFrame(FrameEventArgs args) {
        MouseState mouseState = MouseState;
        KeyboardState keyboardState = KeyboardState;

        base.OnUpdateFrame(args);
        //Console.WriteLine("Update frame!");
        var packets = camera!.Update(keyboardState, MouseState, args, this);
        
        if (packets != null && packets.Count > 0) {
            server.RecievePackets(packets);
            server.ProcessPackets();
        }

        var currChunkPos = Camera.GetChunkPos(camera.position);
        if (currChunkPos != camera.lastChunkPos && camera.gameRules.generateChunks) {
            server.UpdateQueues(currChunkPos);
            camera.lastChunkPos =  Camera.GetChunkPos(camera.position);
        }

 
        //CheckForCollision();
    }
}
