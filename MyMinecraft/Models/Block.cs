namespace MyMinecraft.Models;


public class Block {
    public Vector3 position;
    public BlockType Type;
    private Dictionary<Faces, FaceData> faces;

    List<Vector2> dirtUV = new() {
        new(0f, 1f),
        new(1f, 1f),
        new(1f, 0f),
        new(0f, 0f),
    };

    public Block(Vector3 pos, BlockType type = BlockType.EMPTY) {
        position = pos;

        Type = type;

        faces = new() {
            {
                Faces.FRONT, 
                new(AddTransformedVertices(FaceData.rawVertexData[Faces.FRONT]),
                   dirtUV)
            },
            {
                Faces.BACK, 
                new(AddTransformedVertices(FaceData.rawVertexData[Faces.BACK]),
                    dirtUV)
            },
            {
                Faces.LEFT,
                new(AddTransformedVertices(FaceData.rawVertexData[Faces.LEFT])
                , dirtUV)
            },
            {
                Faces.RIGHT,
                new(AddTransformedVertices(FaceData.rawVertexData[Faces.RIGHT])
                , dirtUV)
            },
            {
                Faces.TOP,
                new(AddTransformedVertices(FaceData.rawVertexData[Faces.TOP]),
                 dirtUV)
            },
            {
                Faces.BOTTOM,
                new(AddTransformedVertices(FaceData.rawVertexData[Faces.BOTTOM]),
                 dirtUV)
            }
        };
    }

    public List<Vector3> AddTransformedVertices(List<Vector3> verteces) {
        List<Vector3> transformedVerteces = new();

        foreach (var v in verteces)
            transformedVerteces.Add(v + position);

        return transformedVerteces;
    }

    public FaceData GetFace(Faces face) {
        return faces[face];
    }
}
