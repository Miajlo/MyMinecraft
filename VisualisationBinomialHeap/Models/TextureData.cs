//using System.Numerics;

namespace MyMinecraft.Models;
public class TextureData {
    public const float txtSize = 0.25f;
    public static Dictionary<BlockType, Dictionary<Faces, List<Vector2>>> blockUVs = new() {
      { BlockType.DIRT, new()
            {
            { Faces.FRONT, new() { new(0, 1.0f), new(0, 0.75f), new(0.25f, 0.75f), new(0.25f, 1.0f) } },
            { Faces.BACK, new() { new(0, 1.0f), new(0, 0.75f), new(0.25f, 0.75f), new(0.25f, 1.0f) } },
            { Faces.LEFT, new() { new(0, 1.0f), new(0, 0.75f), new(0.25f, 0.75f), new(0.25f, 1.0f) } },
            { Faces.RIGHT, new() { new(0, 1.0f), new(0, 0.75f), new(0.25f, 0.75f), new(0.25f, 1.0f) } },
            { Faces.TOP, new() { new(0, 1.0f), new(0, 0.75f), new(0.25f, 0.75f), new(0.25f, 1.0f) } },
            { Faces.BOTTOM, new() { new(0, 1.0f), new(0, 0.75f), new(0.25f, 0.75f), new(0.25f, 1.0f) } }
            }
        },
       { BlockType.GRASS_BLOCK, new()
            {
            { Faces.FRONT, new() { new(0.5f, 1.0f), new(0.5f, 0.75f), new(0.75f, 0.75f), new(0.75f, 1.0f) } },
            { Faces.BACK, new() { new(0.5f, 1.0f), new(0.5f, 0.75f), new(0.75f, 0.75f), new(0.75f, 1.0f) } },
            { Faces.LEFT, new() { new(0.5f, 1.0f), new(0.5f, 0.75f), new(0.75f, 0.75f), new(0.75f, 1.0f) } },
            { Faces.RIGHT, new() { new(0.5f, 1.0f), new(0.5f, 0.75f), new(0.75f, 0.75f), new(0.75f, 1.0f) } },
            { Faces.TOP, new() { new(0.75f, 1.0f), new(0.75f, 0.75f), new(1.0f, 0.75f), new(1.0f, 1.0f) } },
            { Faces.BOTTOM, new() { new(0, 1.0f), new(0, 0.75f), new(0.25f, 0.75f), new(0.25f, 1.0f) } }
            }
        }
    };

}
