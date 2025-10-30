namespace MyMinecraft.Models;
public struct Block {
    public BlockType type;
    public byte lightLevel;
    public Faces rotation { get; set; } = Faces.TOP;

    public Block() {
        type = BlockType.AIR;
        lightLevel = 0;
        rotation = Faces.TOP;
    }

    public Block(BlockType type): this() {
        this.type = type;
    }

    public override string ToString() {
        return $"{type};{lightLevel};{rotation}";
    }

    public static Block ParseString(string s) {
        var parts = s.Split(';');
        if (parts.Length != 3)
            throw new FormatException("Invalid block string format, expected 3 parts separated by ';'");

        // Parse BlockType
        if (!Enum.TryParse<BlockType>(parts[0], out var parsedType))
            throw new FormatException($"Invalid BlockType: '{parts[0]}'");

        // Parse lightLevel
        if (!byte.TryParse(parts[1], out var parsedLightLevel))
            throw new FormatException($"Invalid lightLevel: '{parts[1]}'");

        // Parse Faces rotation
        if (!Enum.TryParse<Faces>(parts[2], out var parsedRotation))
            throw new FormatException($"Invalid Faces rotation: '{parts[2]}'");

        return new Block {
            type = parsedType,
            lightLevel = parsedLightLevel,
            rotation = parsedRotation
        };
    }

    private static List<Vector3> GetTransformedVertices(Faces face, Vector3 position) {
        List<Vector3> transformedVerteces = new();

        foreach (var v in FaceData.rawVertexData[face])
            transformedVerteces.Add(v + position);

        return transformedVerteces;
    }

    public static List<Vector3> GetFaceData(Faces face, Vector3 position) {
        return GetTransformedVertices(face, position);
    }
}
