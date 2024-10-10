namespace VisualisationBinomialHeap.Models; 
public abstract class Shape {
    public List<float> Vertices { get; protected set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }

    public int Size => Vertices.Count() * sizeof(float);
    public abstract int Offset();
    public Shape(float x, float y, float z) {
        Vertices = new();
        X = x;
        Y = y;
        Z = z;
    }

    public abstract void DrawShape(int offset);
}
