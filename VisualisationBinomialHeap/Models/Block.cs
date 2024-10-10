namespace VisualisationBinomialHeap.Models;


public class Block {
    public Vector3 position;

    private Dictionary<Faces, FaceData> faces;

    public Block(Vector3 pos) {
        position = pos;

        faces = new() {
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

    public FaceData GetFace(Faces face) {
        return faces[face];
    }
}
