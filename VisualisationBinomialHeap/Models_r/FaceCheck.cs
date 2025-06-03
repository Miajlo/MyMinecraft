

namespace MyMinecraft.Models_r; 
public class FaceCheckData {
    public static Faces[] faces = {
    Faces.LEFT, Faces.RIGHT, Faces.BOTTOM, Faces.TOP, Faces.BACK, Faces.FRONT
    };

    public static Vector3[] offsets = {
        new(-1, 0, 0), new(1, 0, 0),
        new(0, -1, 0), new(0, 1, 0),
        new(0, 0, -1), new(0, 0, 1)
    };
}
