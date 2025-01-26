namespace MyMinecraft.Models;


public class Block {
    public Vector3 position;
    public BlockType Type;
    private Dictionary<Faces, FaceData>? faces;

    List<Vector2> dirtUV = new() {
        new(0f, 1f),
        new(1f, 1f),
        new(1f, 0f),
        new(0f, 0f),
    };

    public Block(Vector3 pos, BlockType type = BlockType.EMPTY) {
        position = pos;

        Type = type;

        faces = new();
    }

    public Dictionary<Faces, FaceData> CreateFaces() {
        return new() {
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

    public void AddFace(Faces face) {
        faces ??= new();
        
        if (!faces.ContainsKey(face))
            faces.Add(face, new(AddTransformedVertices(FaceData.rawVertexData[face]), dirtUV));
        else
            Console.WriteLine("Block.AddFace: Face alreadt added");
    }

    public FaceData GetFace(Faces face) {
        //faces ??= CreateFaces();

        return faces![face];
    }

    public void ClearFaceData() {
        faces = null;
    }
}
