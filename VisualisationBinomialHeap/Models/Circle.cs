namespace VisualisationBinomialHeap.Models; 
public class Circle : Shape {
    public float Radius { get; protected set; }
    public int Segments { get; set; }
    public Circle(float centerX, float centerY, float radius, int segments,float centerZ = 0) :base(centerX,centerY, centerZ) {
        // Center vertex
        Vertices.Add(centerX);
        Vertices.Add(centerY);
        Vertices.Add(0.0f);

        // Circle vertices
        for (int i = 0; i <= segments; i++) {
            float angle = (float)i / (float)segments * 2.0f * MathF.PI;
            float x = radius * MathF.Cos(angle);
            float y = radius * MathF.Sin(angle);

            // Adjust x by the aspect ratio
            Vertices.Add(centerX + x);
            Vertices.Add(centerY + y);
            Vertices.Add(0.0f);
        }
        Radius = radius;
        Segments = segments;
    }

    public override void DrawShape(int offset) {
        GL.DrawArrays(PrimitiveType.TriangleFan, offset, Segments + 2);
    }

    public override int Offset() {
        return Segments + 2;
    }
}
