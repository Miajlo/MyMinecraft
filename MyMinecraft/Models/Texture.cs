namespace MyMinecraft.Models;
public static class Texture
{
    public static int ID;
    public static string path = "../../../Resources/TextureAtlas.png";
    static Texture()
    {
        Load();
    }
    public static void Load()
    {
        ID = GL.GenTexture();

        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, ID);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
            (int)TextureWrapMode.Repeat);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
            (int)TextureWrapMode.Repeat);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
            (int)TextureMinFilter.Nearest);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
            (int)TextureMagFilter.Nearest);

        StbImage.stbi_set_flip_vertically_on_load(1);
        ImageResult dirtTexture = ImageResult.FromStream(File.OpenRead(path),
            ColorComponents.RedGreenBlueAlpha);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
            dirtTexture.Height, dirtTexture.Width, 0, PixelFormat.Rgba,
            PixelType.UnsignedByte, dirtTexture.Data);

        Unbind();
    }
    public static void Bind()
    {
        GL.BindTexture(TextureTarget.Texture2D, ID);
    }

    public static void Unbind()
    {
        GL.BindTexture(TextureTarget.Texture2D, 0);
    }

    public static void Delete()
    {
        GL.DeleteTexture(ID);
    }
}
