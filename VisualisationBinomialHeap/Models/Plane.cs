namespace MyMinecraft.Models;

public class Frustum {
    public struct Plane {
        public Vector3 Normal; // (A, B, C)
        public float D;        // Distance from origin

        public Plane(Vector3 normal, float d) {
            Normal = normal;
            D = d;
        }
    }

    private Plane[] planes = new Plane[6];

    public void ExtractFrustumPlanes(Matrix4 viewProjection) {
        // Left plane
        planes[0] = new Plane(
            new Vector3(viewProjection.M14 + viewProjection.M11,
                        viewProjection.M24 + viewProjection.M21,
                        viewProjection.M34 + viewProjection.M31),
            viewProjection.M44 + viewProjection.M41);

        // Right plane
        planes[1] = new Plane(
            new Vector3(viewProjection.M14 - viewProjection.M11,
                        viewProjection.M24 - viewProjection.M21,
                        viewProjection.M34 - viewProjection.M31),
            viewProjection.M44 - viewProjection.M41);

        // Bottom plane (Fixed)
        planes[2] = new Plane(
            new Vector3(viewProjection.M14 + viewProjection.M13,
                        viewProjection.M24 + viewProjection.M23,
                        viewProjection.M34 + viewProjection.M33),
            viewProjection.M44 + viewProjection.M43);

        // Top plane (Fixed)
        planes[3] = new Plane(
            new Vector3(viewProjection.M14 - viewProjection.M13,
                        viewProjection.M24 - viewProjection.M23,
                        viewProjection.M34 - viewProjection.M33),
            viewProjection.M44 - viewProjection.M43);

        // Near plane (Fixed)
        planes[4] = new Plane(
            new Vector3(viewProjection.M13,
                        viewProjection.M23,
                        viewProjection.M33),
            viewProjection.M43);

        // Far plane (Fixed)
        planes[5] = new Plane(
            new Vector3(viewProjection.M14 - viewProjection.M12,
                        viewProjection.M24 - viewProjection.M22,
                        viewProjection.M34 - viewProjection.M32),
            viewProjection.M44 - viewProjection.M42);

        // Normalize the planes
        for (int i = 0; i < 6; i++) {
            float length = planes[i].Normal.Length;
            if (length > 0) {
                planes[i].Normal /= length;
                planes[i].D /= length;
            }
        }
    }

    public Plane[] GetPlanes() {
        return planes;
    }
}
