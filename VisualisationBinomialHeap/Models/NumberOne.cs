namespace VisualisationBinomialHeap.Models;
public class NumberOne : Shape {
    public NumberOne(float x, float y, float z, float height, float width) : base(x, y, z) {
        Vertices.AddRange([
            x + width / 2, y + height / 2, z, //top right
            x + width / 2, y - height / 2, z,
            x - width / 2, y - height / 2, z,
            x - width / 2, y - height / 2, z,
            x - width / 2, y + height / 2, z, //top left
            x + width / 2, y + height / 2, z,
            // Rectangle
            x - width / 2, y + height / 2, z, //top left
            x - height / 4, y + width, z,
            x - height / 4, y - width, z,

            x - height / 4, y - width, z,
            x - width / 2, y + height / 2, z,
            x - width / 2, y + height / 2 - width * 2, z,     
        ]);  
    }

    public override void DrawShape(int offset) {
        GL.DrawArrays(PrimitiveType.Triangles, offset, 12);
    }

    public override int Offset() {
        return 12;
    }
}
