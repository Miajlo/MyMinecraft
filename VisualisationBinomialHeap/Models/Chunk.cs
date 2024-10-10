namespace VisualisationBinomialHeap.Models; 
public class Chunk {
    public List<Vector3> chunkVert = new();
    public List<uint> chunkInd = new();

    private const int SIZE = 16;
    private const int HEIGHT = 16;

    public Vector3 position;

    public uint indexCount;
    public Chunk(Vector3 pos) {
        position = pos;
    }

    public void GenChunk() {
        for (int y = 0; y < HEIGHT; ++y) {
            for (int i = 0; i < SIZE; ++i) {
                for (int j = 0; j < SIZE; ++j) {
                    Block block = new(new(i, y, j));
                    if (j == 0) {
                        var frontData = block.GetFace(Faces.FRONT);
                        chunkVert.AddRange(frontData.vertices!);
                        AddInceces(1);
                    }
                    if (j == SIZE-1) {
                        var backData = block.GetFace(Faces.BACK);
                        chunkVert.AddRange(backData.vertices!);
                        AddInceces(1);
                    }
                    if (i == 0) {
                        var leftData = block.GetFace(Faces.LEFT);
                        chunkVert.AddRange(leftData.vertices!);
                        AddInceces(1);
                    }
                    if (i == SIZE - 1) {
                        var rightData = block.GetFace(Faces.RIGHT);
                        chunkVert.AddRange(rightData.vertices!);
                        AddInceces(1);
                    }
                    if (y == SIZE - 1) {
                        var topData = block.GetFace(Faces.TOP);
                        chunkVert.AddRange(topData.vertices!);
                        AddInceces(1);
                    }
                    if (y == 0) {
                        var bottomData = block.GetFace(Faces.BOTTOM);
                        chunkVert.AddRange(bottomData.vertices!);
                        AddInceces(1);
                    }
                }
            }
        }
    }

    public void AddInceces(uint indCount) {
        for(int i=0; i < indCount; ++i) {
            chunkInd.Add(0 + indexCount);
            chunkInd.Add(1 + indexCount);
            chunkInd.Add(2 + indexCount);
            chunkInd.Add(2 + indexCount);
            chunkInd.Add(3 + indexCount);
            chunkInd.Add(0 + indexCount);

            indexCount += 4;
        }
    }

    public void BuildChunk() {

    }

    public void Render() {

    }
}
