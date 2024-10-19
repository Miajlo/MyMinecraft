namespace MyMinecraft.Models;

public enum BlockType {
    EMPTY,
    DIRT,
    STONE
}

public enum Faces {
    FRONT,
    BACK,
    LEFT,
    RIGHT,
    TOP,
    BOTTOM
}

public struct FaceData {
    public List<Vector3>? vertices;
    public List<Vector2>? uvs;
    public FaceData(List<Vector3> list, List<Vector2> text) {
        vertices = list;
        uvs = text;
    }
    public static readonly Dictionary<Faces, List<Vector3>> rawVertexData = new Dictionary<Faces, List<Vector3>>
{
    { Faces.FRONT, new List<Vector3> {
        new Vector3(0.0f, 1.0f, 1.0f),  // Top-left
        new Vector3(1.0f, 1.0f, 1.0f),  // Top-right
        new Vector3(1.0f, 0.0f, 1.0f),  // Bottom-right
        new Vector3(0.0f, 0.0f, 1.0f)   // Bottom-left
    }},
    { Faces.BACK, new List<Vector3> {
        new Vector3(0.0f, 1.0f, 0.0f),  // Top-left
        new Vector3(1.0f, 1.0f, 0.0f),  // Top-right
        new Vector3(1.0f, 0.0f, 0.0f),  // Bottom-right
        new Vector3(0.0f, 0.0f, 0.0f)   // Bottom-left
    }},
    { Faces.LEFT, new List<Vector3> {
        new Vector3(0.0f, 1.0f, 1.0f),  // Front top-left
        new Vector3(0.0f, 1.0f, 0.0f),  // Back top-left
        new Vector3(0.0f, 0.0f, 0.0f),  // Back bottom-left
        new Vector3(0.0f, 0.0f, 1.0f)   // Front bottom-left
    }},
    { Faces.RIGHT, new List<Vector3> {
        new Vector3(1.0f, 1.0f, 1.0f),  // Front top-right
        new Vector3(1.0f, 1.0f, 0.0f),  // Back top-right
        new Vector3(1.0f, 0.0f, 0.0f),  // Back bottom-right
        new Vector3(1.0f, 0.0f, 1.0f)   // Front bottom-right
    }},
    { Faces.TOP, new List<Vector3> {
        new Vector3(0.0f, 1.0f, 1.0f),  // Front top-left
        new Vector3(1.0f, 1.0f, 1.0f),  // Front top-right
        new Vector3(1.0f, 1.0f, 0.0f),  // Back top-right
        new Vector3(0.0f, 1.0f, 0.0f)   // Back top-left
    }},
    { Faces.BOTTOM, new List<Vector3> {
        new Vector3(0.0f, 0.0f, 1.0f),  // Front bottom-left
        new Vector3(1.0f, 0.0f, 1.0f),  // Front bottom-right
        new Vector3(1.0f, 0.0f, 0.0f),  // Back bottom-right
        new Vector3(0.0f, 0.0f, 0.0f)   // Back bottom-left
    }}
    };
}
