namespace MyMinecraft.Unused;
public class CircleOutline : Circle
{
    public CircleOutline(float centerX, float centerY, float radius, int segments, float z = 0)
        : base(centerX, centerX, radius, segments)
    {

    }

    public override void DrawShape(int offset)
    {
        GL.DrawArrays(PrimitiveType.LineLoop, offset, 102);
    }

    public override int Offset()
    {
        return 104;
    }
}
