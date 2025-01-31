namespace MyMinecraft.Models;

public enum BlockType : byte {
    AIR,
    DIRT,
    STONE,
    GRASS_BLOCK
}

public enum Faces :byte {
    FRONT,
    BACK,
    LEFT,
    RIGHT,
    TOP,
    BOTTOM
}


//public struct TextureData {
//    public const float textureSize = 0.25f;
//    public static readonly Dictionary<BlockType, List<Vector2>> blockUVs = new() {
//        { BlockType.DIRT, new() 
//        {
//            new(0f, 1f),
//            new(1f, 1f),
//            new(1f, 0f),
//            new(0f, 0f),
//        } 
//        },

//    };
//}

public struct FaceData {
    public List<Vector3>? vertices;
    public FaceData(List<Vector3> list) {
        vertices = list;
    }
    public static readonly Dictionary<Faces, List<Vector3>> rawVertexData = new Dictionary<Faces, List<Vector3>>
    {
    { Faces.FRONT, new List<Vector3> {
        new Vector3(0.0f, 1.0f, 1.0f),  // Top-left 1 GOOD
        new Vector3(1.0f, 1.0f, 1.0f),  // Top-right 2
        new Vector3(1.0f, 0.0f, 1.0f),  // Bottom-right 3
        new Vector3(0.0f, 0.0f, 1.0f)   // Bottom-left 4
    }},
    { Faces.BACK, new List<Vector3> {
        new Vector3(0.0f, 1.0f, 0.0f),  // Top-left 1 GOOD
        new Vector3(0.0f, 0.0f, 0.0f),   // Bottom-left 4
        new Vector3(1.0f, 0.0f, 0.0f),  // Bottom-right 3
        new Vector3(1.0f, 1.0f, 0.0f)  // Top-right 2
    }},
    { Faces.LEFT, new List<Vector3> {
        new Vector3(0.0f, 1.0f, 1.0f),  // Front top-left 1
        new Vector3(0.0f, 0.0f, 1.0f),   // Front bottom-left 4
        new Vector3(0.0f, 0.0f, 0.0f),  // Back bottom-left 3
        new Vector3(0.0f, 1.0f, 0.0f)  // Back top-left 2
    }},
    { Faces.RIGHT, new List<Vector3> {
        new Vector3(1.0f, 1.0f, 1.0f),  // Front top-right 1
        new Vector3(1.0f, 1.0f, 0.0f),  // Back top-right 2
        new Vector3(1.0f, 0.0f, 0.0f),  // Back bottom-right 3
        new Vector3(1.0f, 0.0f, 1.0f)   // Front bottom-right 4
    }},
    { Faces.TOP, new List<Vector3> {
        new Vector3(0.0f, 1.0f, 1.0f),  // Front top-left 1 GOOD
        new Vector3(0.0f, 1.0f, 0.0f),   // Back top-left 4
        new Vector3(1.0f, 1.0f, 0.0f),  // Back top-right 3
        new Vector3(1.0f, 1.0f, 1.0f)  // Front top-right 2
    }},
    { Faces.BOTTOM, new List<Vector3> {
        new Vector3(0.0f, 0.0f, 1.0f),  // Front bottom-left 1 GOOD
        new Vector3(1.0f, 0.0f, 1.0f),  // Front bottom-right 2
        new Vector3(1.0f, 0.0f, 0.0f),  // Back bottom-right 3
        new Vector3(0.0f, 0.0f, 0.0f)   // Back bottom-left 4
    }}
    };
}
