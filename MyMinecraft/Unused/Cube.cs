namespace MyMinecraft.Unused;
public class Cube
{

    public Cube(float v1, float v2, float v3, float v4)
    {
        tlX = v1;
        tlY = v2;
        tlZ = v3;
        EdgeLength = v4;
    }

    public float tlX { get; set; }
    public float tlY { get; set; }
    public float tlZ { get; set; }
    public float EdgeLength { get; set; }

    public Vector3[] GetVerteces()
    {
        return [
            new Vector3(tlX, tlY, tlZ),
            new Vector3(tlX + EdgeLength, tlY, tlZ),
            new Vector3(tlX + EdgeLength, tlY - EdgeLength, tlZ),
            new Vector3(tlX, tlY - EdgeLength, tlZ),
            new Vector3(tlX, tlY, tlZ + EdgeLength),
            new Vector3(tlX + EdgeLength, tlY, tlZ + EdgeLength),
            new Vector3(tlX + EdgeLength, tlY - EdgeLength, tlZ + EdgeLength),
            new Vector3(tlX, tlY - EdgeLength, tlZ + EdgeLength)
        ];
    }

    public uint[] GetIndeces(uint offset = 0)
    {

        uint[] arr = [
            0 , 1, 2, 2, 3, 0,
            // Front face
            4, 5, 6, 6, 7, 4,
            // Left face
            0, 3, 7, 7, 4, 0,
            // Right face
            1, 5, 6, 6, 2, 1,
            // Top face
            3, 2, 6, 6, 7, 3,
            // Bottom face
            0, 1, 5, 5, 4, 0
        ];

        for (int i = 0; i < arr.Length; ++i)
            arr[i] += offset;

        return arr;
    }

}
