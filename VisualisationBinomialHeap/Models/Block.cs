namespace MyMinecraft.Models;

public struct Block {
    public Vector3i position;
    public BlockType Type;
    public byte lightLevel;

    public Block(Vector3i pos, BlockType type = BlockType.AIR) {
        position = pos;

        Type = type;
    }

    public List<Vector3> GetTransformedVertices(List<Vector3> verteces) {
        List<Vector3> transformedVerteces = new();

        foreach (var v in verteces)
            transformedVerteces.Add(v + position);

        return transformedVerteces;
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
