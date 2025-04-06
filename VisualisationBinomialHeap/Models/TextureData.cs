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

    public static List<Vector2> GetUVs(BlockType type, Faces face, Faces rotation=Faces.TOP) {
        if (!blockUVs.TryGetValue(type, out var faceUVs) || !faceUVs.TryGetValue(face, out var topLeft)) {
            throw new KeyNotFoundException($"UV coordinates not found for {type} - {face}");
        }


        if (rotation == Faces.TOP || rotation == Faces.BOTTOM)
            return GetUVsNoRotation(face, topLeft);

        return GetUVsWithRot(face, topLeft, type, rotation);
    }


    private static List<Vector2> GetUVsNoRotation(Faces face, Vector2 topLeft) {
        if (face == Faces.BACK) {
            return new() {
                    new Vector2(topLeft.X, topLeft.Y + 1) / 4,//tl
                    new Vector2(topLeft.X, topLeft.Y) / 4,//bl
                    new Vector2(topLeft.X + 1, topLeft.Y) / 4, //br       // Flip horizontally for BACK/LEFT
                    new Vector2(topLeft.X + 1, topLeft.Y + 1) / 4,//tr
                };
        }

        if (face == Faces.LEFT) {
            return new() {
                new Vector2(topLeft.X + 1, topLeft.Y + 1) / 4,//tr
                new Vector2(topLeft.X + 1, topLeft.Y) / 4, //br       // Flip horizontally for BACK/LEFT
                new Vector2(topLeft.X, topLeft.Y) / 4,//bl
                new Vector2(topLeft.X, topLeft.Y + 1) / 4,//tl
                };
        }

        return new() {
            new Vector2(topLeft.X / 4f, (topLeft.Y + 1f) / 4f),//tl
            new Vector2((topLeft.X + 1f) / 4f, (topLeft.Y + 1f) / 4f), //tr
            new Vector2((topLeft.X + 1f) / 4f, topLeft.Y / 4f),//br
            new Vector2(topLeft.X / 4f, topLeft.Y / 4f),//bl
            };
    }

    public static List<Vector2> GetUVsWithRot(Faces face, Vector2 topLeft, BlockType type, Faces rotation = Faces.TOP) {
        // Start with the default (no rotation) UVs
        List<Vector2> uvs = GetUVsNoRotation(face, topLeft);
        face = MapFaceRotation(face, rotation);

        if (!blockUVs.TryGetValue(type, out var faceUVs) || !faceUVs.TryGetValue(face, out topLeft)) {
            throw new KeyNotFoundException($"UV coordinates not found for {type} - {face}");
        }

        var uvsToRotate = GetUVsNoRotation(face,topLeft);

        if(rotation != Faces.TOP) {
            (uvsToRotate[0], uvsToRotate[2]) = (uvsToRotate[2], uvsToRotate[0]);
            (uvsToRotate[2], uvsToRotate[3]) = (uvsToRotate[3], uvsToRotate[2]);
            (uvsToRotate[0], uvsToRotate[1]) = (uvsToRotate[1], uvsToRotate[0]);
        }

        //if ((rotation == Faces.TOP || rotation == Faces.BOTTOM) || face == Faces.TOP || face == Faces.BOTTOM ) {
        //    // For TOP and BOTTOM faces, no change needed in UVs, they remain as is
        //    return uvsToRotate;
        //}

        //// If rotation is LEFT or RIGHT, apply horizontal flip
        //if (rotation == Faces.LEFT || rotation == Faces.RIGHT) {          

        //    if (rotation == Faces.LEFT) {
        //        // Flip the UVs for LEFT rotation (flip horizontally)
        //        return new List<Vector2>
        //        {
        //        new Vector2(uvsToRotate[1].X, uvsToRotate[1].Y),  // br
        //        new Vector2(uvsToRotate[0].X, uvsToRotate[0].Y),  // tl
        //        new Vector2(uvsToRotate[3].X, uvsToRotate[3].Y),  // tr
        //        new Vector2(uvsToRotate[2].X, uvsToRotate[2].Y)   // bl
        //    };
        //    }
        //    else if (rotation == Faces.RIGHT) {
        //        // Flip the UVs for RIGHT rotation (flip horizontally)
        //        return new List<Vector2>
        //        {
        //        new Vector2(uvsToRotate[2].X, uvsToRotate[2].Y),  // bl
        //        new Vector2(uvsToRotate[3].X, uvsToRotate[3].Y),  // tr
        //        new Vector2(uvsToRotate[0].X, uvsToRotate[0].Y),  // tl
        //        new Vector2(uvsToRotate[1].X, uvsToRotate[1].Y)   // br
        //    };
        //    }
        //}

        //// If rotation is FRONT or BACK, handle flip vertically (swap top/bottom)
        //if (rotation == Faces.FRONT || rotation == Faces.BACK) {
        //    if (face == Faces.TOP || face == Faces.BOTTOM) {
        //        // If it's a top or bottom face, return the default UVs
        //        return uvsToRotate;
        //    }

        //    if (rotation == Faces.BACK) {
        //        // Flip the UVs for BACK rotation (flip vertically)
        //        return new List<Vector2>
        //        {
        //        new Vector2(uvsToRotate[0].X, uvsToRotate[0].Y + 1),  // tl
        //        new Vector2(uvsToRotate[3].X, uvsToRotate[3].Y + 1),  // tr
        //        new Vector2(uvsToRotate[2].X, uvsToRotate[2].Y),      // br
        //        new Vector2(uvsToRotate[1].X, uvsToRotate[1].Y)       // bl
        //    };
        //    }
        //}

        // Default case: return the UVs without any changes
        return uvsToRotate;
    }

    public static Faces MapFaceRotation(Faces face, Faces rotation) {
        // Handle rotation for the FRONT face
        if (rotation == Faces.FRONT) {
            if (face == Faces.FRONT)
                return Faces.TOP;
            if (face == Faces.BACK)
                return Faces.BOTTOM;
            if (face == Faces.TOP)
                return Faces.FRONT;
            if (face == Faces.BOTTOM)
                return Faces.BACK;
        }

        // Handle rotation for the BACK face (reversing FRONT logic)
        if (rotation == Faces.BACK) {
            if (face == Faces.TOP)
                return Faces.BACK;
            if (face == Faces.BOTTOM)
                return Faces.FRONT;
            if (face == Faces.FRONT)
                return Faces.BOTTOM;
            if (face == Faces.BACK)
                return Faces.TOP;
        }

        // Handle rotation for the LEFT face
        if (rotation == Faces.LEFT) {
            if (face == Faces.TOP)
                return Faces.RIGHT;
            if (face == Faces.BOTTOM)
                return Faces.LEFT;
            if (face == Faces.RIGHT)
                return Faces.BOTTOM;
            if (face == Faces.LEFT)
                return Faces.TOP;
        }

        // Handle rotation for the RIGHT face
        if (rotation == Faces.RIGHT) {
            if (face == Faces.TOP)
                return Faces.LEFT;
            if (face == Faces.BOTTOM)
                return Faces.RIGHT;
            if (face == Faces.LEFT)
                return Faces.BOTTOM;
            if (face == Faces.RIGHT)
                return Faces.TOP;
        }

        // If no match (or other rotations), return the face as is
        return face;
    }

    private static void Swap<T>(ref T item1, ref T item2) {        
        var tmp = item1;
        item1 = item2;
        item2 = tmp;
    }

}