namespace MyMinecraft.Models;


public class Block {
    public Vector3 position;
    public BlockType Type;
    private Dictionary<Faces, FaceData>? faces;


    public Block(Vector3 pos, BlockType type = BlockType.AIR) {
        position = pos;

        Type = type;

        faces = new();
    }

    public Dictionary<Faces, FaceData> CreateFaces() {
        return new() {
            {
                Faces.FRONT,
                new(AddTransformedVertices(FaceData.rawVertexData[Faces.FRONT]))
            },
            {
                Faces.BACK,
                new(AddTransformedVertices(FaceData.rawVertexData[Faces.BACK]))
            },
            {
                Faces.LEFT,
                new(AddTransformedVertices(FaceData.rawVertexData[Faces.LEFT]))
            },
            {
                Faces.RIGHT,
                new(AddTransformedVertices(FaceData.rawVertexData[Faces.RIGHT]))
            },
            {
                Faces.TOP,
                new(AddTransformedVertices(FaceData.rawVertexData[Faces.TOP]))
            },
            {
                Faces.BOTTOM,
                new(AddTransformedVertices(FaceData.rawVertexData[Faces.BOTTOM]))
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
            faces.Add(face, new(AddTransformedVertices(FaceData.rawVertexData[face])));
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
