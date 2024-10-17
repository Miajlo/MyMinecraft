namespace MyMinecraft.Unused;
public class Rectangle : Shape
{
    public float XLeft { get; set; }
    public float YLeft { get; set; }
    public float ZLeft { get; set; }

    public Rectangle(float tRightX, float tRightY, float tRightZ, float leftX, float leftY, float leftZ = 0)
        : base(tRightX, tRightY, tRightZ)
    {
        XLeft = leftX;
        YLeft = leftY;
        ZLeft = leftZ;

        Vertices.AddRange([X, Y, Z]); //top rgiht
        Vertices.AddRange([X, YLeft, Z]); //bottom right
        Vertices.AddRange([XLeft, YLeft, Z]); // bottom left
        Vertices.AddRange([XLeft, YLeft, Z]); // bottom left
        Vertices.AddRange([XLeft, Y, Z]); //top left
        Vertices.AddRange([X, Y, Z]); // top right
    }

    public override void DrawShape(int offset)
    {
        GL.DrawArrays(PrimitiveType.Triangles, offset, 6);
    }

    public override int Offset()
    {
        return 6;
    }
}
