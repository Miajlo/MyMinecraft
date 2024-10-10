using System;
using System.Collections.Generic;
namespace VisualisationBinomialHeap.Models;
public class Parallelogram : Shape {
    public float X2 { get; set; }
    public float Y2 { get; set; }
    public float Z2 { get; set; }

    public float X3 { get; set; }
    public float Y3 { get; set; }
    public float Z3 { get; set; }

    public float X4 { get; set; }
    public float Y4 { get; set; }
    public float Z4 { get; set; }

    public Parallelogram(float x1, float y1, float z1, float x2, float y2, float z2, float x3, float y3, float z3, float x4, float y4, float z4)
        : base(x1, y1, z1) {
        X2 = x2;
        Y2 = y2;
        Z2 = z2;
        X3 = x3;
        Y3 = y3;
        Z3 = z3;
        X4 = x4;
        Y4 = y4;
        Z4 = z4;

        Vertices.AddRange(
        [
                X, Y, Z,  // Vertex 1
                X2, Y2, Z2,  // Vertex 2
                X3, Y3, Z3,  // Vertex 3

                X3, Y3, Z3,  // Vertex 3
                X4, Y4, Z4,  // Vertex 4
                X2, Y2, Z2   // Vertex 1
        ]);
    }

    public override void DrawShape(int offset) {
        GL.DrawArrays(PrimitiveType.Triangles, offset, 6);
    }

    public override int Offset() {
        return 6;
    }
}
