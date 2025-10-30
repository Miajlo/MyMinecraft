namespace MyMinecraft.Models; 
public class HudTextures {

    private string path = "../../../Resources/crosshair.png";
    private int ID;

    void LoadTexture() {
        using var stream = File.OpenRead(path);
        var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

        ID = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, ID);

        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
            image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
    }

    public void Bind() {
        GL.BindTexture(TextureTarget.Texture2D, ID);
    }

    public void Unbind() {
        GL.BindTexture(TextureTarget.Texture2D, 0);
    }

    public void Delete() {
        GL.DeleteTexture(ID);
    }
}
