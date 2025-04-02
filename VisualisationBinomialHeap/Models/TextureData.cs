namespace MyMinecraft.Models; 
public readonly struct TextureData {
    public const float txtSize = 1.0f;
    public static Dictionary<BlockType, Dictionary<Faces, Vector2>> blockUVs = new() {
    { BlockType.DIRT, new() {
        { Faces.FRONT, new(0.0f, 3.0f) },
        { Faces.BACK, new(0.0f, 3.0f) },
        { Faces.LEFT, new(0.0f, 3.0f) },
        { Faces.RIGHT, new(0.0f, 3.0f) },
        { Faces.TOP, new(0.0f, 3.0f) },
        { Faces.BOTTOM, new(0.0f, 3.0f) }
    }},
    { BlockType.GRASS_BLOCK, new() {
        { Faces.FRONT, new(2.0f, 3.0f) },
        { Faces.BACK, new(2.0f, 3.0f) },
        { Faces.LEFT, new(2.0f, 3.0f) },
        { Faces.RIGHT, new(2.0f, 3.0f) },
        { Faces.TOP, new(3.0f, 3.0f) },
        { Faces.BOTTOM, new(0.0f, 3.0f) }
    }},
      { BlockType.STONE, new() {
        { Faces.FRONT, new(1.0f, 3.0f) },
        { Faces.BACK,  new(1.0f, 3.0f) },
        { Faces.LEFT,  new(1.0f, 3.0f) },
        { Faces.RIGHT,  new(1.0f, 3.0f) },
        { Faces.TOP,  new(1.0f, 3.0f) },
        { Faces.BOTTOM,  new(1.0f, 3.0f) }
    }},
      { BlockType.STONE_BRICKS, new() {
        { Faces.FRONT, new(0.0f, 2.0f) },
        { Faces.BACK,  new(0.0f, 2.0f) },
        { Faces.LEFT,  new(0.0f, 2.0f) },
        { Faces.RIGHT,  new(0.0f, 2.0f) },
        { Faces.TOP,  new(0.0f, 2.0f) },
        { Faces.BOTTOM,  new(0.0f, 2.0f) }
    }},
       { BlockType.BRICKS, new() {
        { Faces.FRONT, new(1.0f, 2.0f) },
        { Faces.BACK,  new(1.0f, 2.0f) },
        { Faces.LEFT,  new(1.0f, 2.0f) },
        { Faces.RIGHT,  new(1.0f, 2.0f) },
        { Faces.TOP,  new(1.0f, 2.0f) },
        { Faces.BOTTOM,  new(1.0f, 2.0f) }
    }},
       { BlockType.OAK_LOG, new() {
        { Faces.FRONT, new(2.0f, 2.0f) },
        { Faces.BACK,  new(2.0f, 2.0f) },
        { Faces.LEFT,  new(2.0f, 2.0f) },
        { Faces.RIGHT,  new(2.0f, 2.0f) },
        { Faces.TOP,  new(3.0f, 2.0f) },
        { Faces.BOTTOM,  new(3.0f, 2.0f) }
    }},
       { BlockType.OAK_LEAVES, new() {
        { Faces.FRONT, new(0.0f, 1.0f) },
        { Faces.BACK,  new(0.0f, 1.0f) },
        { Faces.LEFT,  new(0.0f, 1.0f) },
        { Faces.RIGHT,  new(0.0f, 1.0f) },
        { Faces.TOP,  new(0.0f, 1.0f) },
        { Faces.BOTTOM,  new(0.0f, 1.0f) }
    }},
       { BlockType.COBBLESTONE, new() {
        { Faces.FRONT, new(1.0f, 1.0f) },
        { Faces.BACK,  new(1.0f, 1.0f) },
        { Faces.LEFT,  new(1.0f, 1.0f) },
        { Faces.RIGHT,  new(1.0f, 1.0f) },
        { Faces.TOP,  new(1.0f, 1.0f) },
        { Faces.BOTTOM,  new(1.0f, 1.0f) }
    }},
       { BlockType.MOSSY_COBBLESTONE, new() {
        { Faces.FRONT, new(2.0f, 1.0f) },
        { Faces.BACK,  new(2.0f, 1.0f) },
        { Faces.LEFT,  new(2.0f, 1.0f) },
        { Faces.RIGHT,  new(2.0f, 1.0f) },
        { Faces.TOP,  new(2.0f, 1.0f) },
        { Faces.BOTTOM,  new(2.0f, 1.0f) }
    }}


};

    //public static List<Vector2> GetUVs(BlockType type, Faces face, Faces rotation=Faces.TOP) {
    //    if (!blockUVs.TryGetValue(type, out var faceUVs) || !faceUVs.TryGetValue(face, out var topLeft)) {
    //        throw new KeyNotFoundException($"UV coordinates not found for {type} - {face}");
    //    }


    //    if (face == Faces.BACK) {
    //        return new() {
    //            new Vector2(topLeft.X, topLeft.Y + 1) / 4,//tl
    //            new Vector2(topLeft.X, topLeft.Y) / 4,//bl
    //            new Vector2(topLeft.X + 1, topLeft.Y) / 4, //br       // Flip horizontally for BACK/LEFT
    //            new Vector2(topLeft.X + 1, topLeft.Y + 1) / 4,//tr
    //        };
    //    }

    //    if (face == Faces.LEFT) {
    //        return new() {
    //        new Vector2(topLeft.X + 1, topLeft.Y + 1) / 4,//tr
    //        new Vector2(topLeft.X + 1, topLeft.Y) / 4, //br       // Flip horizontally for BACK/LEFT
    //        new Vector2(topLeft.X, topLeft.Y) / 4,//bl
    //        new Vector2(topLeft.X, topLeft.Y + 1) / 4,//tl
    //        };
    //    }

    //    return new() {
    //    new Vector2(topLeft.X / 4f, (topLeft.Y + 1f) / 4f),//tl
    //    new Vector2((topLeft.X + 1f) / 4f, (topLeft.Y + 1f) / 4f), //tr
    //    new Vector2((topLeft.X + 1f) / 4f, topLeft.Y / 4f),//br
    //    new Vector2(topLeft.X / 4f, topLeft.Y / 4f),//bl
    //    };
    //}

    public static List<Vector2> GetUVs(BlockType type, Faces face, Faces rotation = Faces.TOP) {
        if (!blockUVs.TryGetValue(type, out var faceUVs) || !faceUVs.TryGetValue(face, out var topLeft)) {
            throw new KeyNotFoundException($"UV coordinates not found for {type} - {face}");
        }

        // Case for rotation being TOP or BOTTOM
        if (rotation == Faces.TOP || rotation == Faces.BOTTOM) {
            // Use the original logic for TOP/BOTTOM (no face swap)
            if (face == Faces.BACK) {
                return new() {
                new Vector2(topLeft.X, topLeft.Y + 1) / 4, // tl
                new Vector2(topLeft.X, topLeft.Y) / 4,     // bl
                new Vector2(topLeft.X + 1, topLeft.Y) / 4, // br
                new Vector2(topLeft.X + 1, topLeft.Y + 1) / 4 // tr
            };
            }

            if (face == Faces.LEFT) {
                return new() {
                new Vector2(topLeft.X + 1, topLeft.Y + 1) / 4, // tr
                new Vector2(topLeft.X + 1, topLeft.Y) / 4,     // br
                new Vector2(topLeft.X, topLeft.Y) / 4,         // bl
                new Vector2(topLeft.X, topLeft.Y + 1) / 4      // tl
            };
            }

            // Default for other faces
            return new() {
            new Vector2(topLeft.X / 4f, (topLeft.Y + 1f) / 4f), // tl
            new Vector2((topLeft.X + 1f) / 4f, (topLeft.Y + 1f) / 4f), // tr
            new Vector2((topLeft.X + 1f) / 4f, topLeft.Y / 4f), // br
            new Vector2(topLeft.X / 4f, topLeft.Y / 4f) // bl
        };
        }

        // Case for other rotations (LEFT, RIGHT, FRONT, BACK)
        if (rotation == Faces.FRONT || rotation == Faces.BACK) {
            // Swap top and bottom for FRONT/BACK rotation
            if (face == Faces.TOP) {
                face = Faces.FRONT;
            }
            else if (face == Faces.BOTTOM) {
                face = Faces.BACK;
            }
        }
        else if (rotation == Faces.LEFT || rotation == Faces.RIGHT) {
            // Swap top and bottom for LEFT/RIGHT rotation
            if (face == Faces.TOP) {
                face = Faces.LEFT;
            }
            else if (face == Faces.BOTTOM) {
                face = Faces.RIGHT;
            }
        }

        // Now that face has been remapped, get the corresponding UVs
        if (!faceUVs.TryGetValue(face, out topLeft)) {
            throw new KeyNotFoundException($"UV coordinates not found for {type} - {face} (after rotation adjustment)");
        }

        // Handle face flipping for BACK and LEFT
        if (face == Faces.BACK) {
            return new() {
            new Vector2(topLeft.X, topLeft.Y + 1) / 4, // tl
            new Vector2(topLeft.X, topLeft.Y) / 4,     // bl
            new Vector2(topLeft.X + 1, topLeft.Y) / 4, // br
            new Vector2(topLeft.X + 1, topLeft.Y + 1) / 4 // tr
        };
        }

        if (face == Faces.LEFT) {
            return new() {
            new Vector2(topLeft.X + 1, topLeft.Y + 1) / 4, // tr
            new Vector2(topLeft.X + 1, topLeft.Y) / 4,     // br
            new Vector2(topLeft.X, topLeft.Y) / 4,         // bl
            new Vector2(topLeft.X, topLeft.Y + 1) / 4      // tl
        };
        }

        // Default UV mapping for other faces
        return new() {
        new Vector2(topLeft.X / 4f, (topLeft.Y + 1f) / 4f), // tl
        new Vector2((topLeft.X + 1f) / 4f, (topLeft.Y + 1f) / 4f), // tr
        new Vector2((topLeft.X + 1f) / 4f, topLeft.Y / 4f), // br
        new Vector2(topLeft.X / 4f, topLeft.Y / 4f) // bl
    };
    }

}