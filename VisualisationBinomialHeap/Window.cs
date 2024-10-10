using VisualisationBinomialHeap.Graphics;
using VisualisationBinomialHeap.Models;

namespace VisualisationBinomialHeap;

public class Window : GameWindow {
    private int Width;
    private int Height;
    private Vector3[]? vertices;

    uint[] indeces = [];

    Chunk chunk = new(new(0, 0, 0));

    List<Cube> cubes = new();
    List<int> ind = new();

    Camera? camera;

    private List<Shape> ArrShapes { get; set; } = new();
    VBO vbo;
    VAO vao;
    IBO ibo;
    ShaderProgram shaderProgram;
    //private int vao;
    //private int vbo;
    //private int ebo;
    //private int ShaderProgram;
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


        //Cube cube = new(-1.0f, 0.5f, 0.0f, 1.0f);
        //Cube c2 = new(0.1f, 0.5f, 0.0f, 1.0f);

        //cubes.Add(cube);
        //cubes.Add(c2);

        chunk.GenChunk();

        vao = new VAO();

        vbo = new(chunk.chunkVert);

        vao.LinkToVAO(0, 3, vbo);

        ibo = new(chunk.chunkInd);

        shaderProgram = new();
        camera = new(Width, Height, Vector3.Zero);
        CursorState = CursorState.Grabbed;
    }

    protected override void OnUnload() {
        base.OnUnload();

        shaderProgram.Delete();
        vao.Delete();
        vbo.Delete();
        ibo.Delete();
    }

    protected override void OnResize(ResizeEventArgs e) {
        base.OnResize(e);
        GL.Viewport(0, 0, e.Width, e.Height);
        this.Height = e.Height;
        this.Width = e.Width;
    }

    protected override void OnRenderFrame(FrameEventArgs args) {
        GL.ClearColor(0f, 0f, 1f, 1f);
        GL.Clear(ClearBufferMask.ColorBufferBit);

        //GL.UseProgram(shaderProgram.ID);
        //GL.BindVertexArray(vao.ID);
        //GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);

        shaderProgram.Bind();
        vao.Bind();
        ibo.Bind();

        int offset = 0;

        int colorLocation = GL.GetUniformLocation(shaderProgram.ID, "uColor");

        Matrix4 model = Matrix4.Identity;
        Matrix4 view = camera!.GetViewMatrix();
        Matrix4 projection = camera!.GetProjectionMatrix();

        int modelLocation = GL.GetUniformLocation(shaderProgram.ID, "model");
        int viewLocation = GL.GetUniformLocation(shaderProgram.ID, "view");
        int projectionLocation = GL.GetUniformLocation(shaderProgram.ID, "projection");

        GL.UniformMatrix4(modelLocation, true, ref model);
        GL.UniformMatrix4(viewLocation, true, ref view);
        GL.UniformMatrix4(projectionLocation, true, ref projection);

        GL.Uniform3(colorLocation, 0.486f, 0.294f, 0.165f);

        GL.DrawElements(PrimitiveType.Triangles, chunk.chunkInd.Count , DrawElementsType.UnsignedInt, 0);


        //foreach (var shape in ArrShapes) {
        //    //if (CheckShapeToColor(shape))
        //    //    GL.Uniform3(colorLocation, 0.0f, 0.0f, 0.0f);
        //    shape.DrawShape(offset);
        //    offset += shape.Offset();
        //    GL.Uniform3(colorLocation, 1.0f, 1.0f, 1.0f);
        //}


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
    }

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

    private Vector3[] GetCombinedVertices() {
        List<Vector3> retVal = new();

        foreach (var shape in cubes)
            retVal.AddRange(shape.GetVerteces());

        return retVal.ToArray();
    }

    private uint[] GetCombinedIndeces() {
        List<uint> retVal = new();
        uint offset = 0;

        foreach (var c in cubes) {
            var tmp = c.GetIndeces(offset);
            retVal.AddRange(c.GetIndeces());
            offset += (uint)c.GetVerteces().Length;

        }
        return retVal.ToArray();
    }
}
