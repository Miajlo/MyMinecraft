using OpenTK.Compute.OpenCL;

namespace MyMinecraft.Graphics; 
public class ShaderProgram {
    public int ID;
    public ShaderProgram() {

        ID = GL.CreateProgram();

        int vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader, LoadShaderSource("Default.vert"));
        GL.CompileShader(vertexShader);

        int fragShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragShader, LoadShaderSource("Default.frag"));
        GL.CompileShader(fragShader);

        GL.AttachShader(ID, vertexShader);
        GL.AttachShader(ID, fragShader);

        GL.LinkProgram(ID);

        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragShader);

    }

    public void Bind() {
        GL.UseProgram(ID);
        GL.Uniform1(GL.GetUniformLocation(ID, "texture0"), 0);
    }
    public void Unbind() {
        GL.UseProgram(0);
    }

    public void Delete() {
        GL.DeleteShader(ID);
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
}
