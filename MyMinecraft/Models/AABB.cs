namespace MyMinecraft.Models;

public struct Collision {
    public bool CollidedX, CollidedY, CollidedZ;

    public Collision(bool x, bool y, bool z) {
        CollidedX = x;
        CollidedY = y;
        CollidedZ = z;
    }

    public bool Any() => CollidedX || CollidedY || CollidedZ;
}

public enum PlayerStates {
    ON_GROUND = 0,
    IN_AIR = 1
}


public struct AABB {
    public Vector3 Min { get; }
    public Vector3 Max { get; }

    public AABB(Vector3 min, Vector3 max) {
        Min = min;
        Max = max;
    }

    // Check if two AABBs are overlapping (colliding)
    public bool Intersects(AABB other) {
        return (Min.X <= other.Max.X && Max.X >= other.Min.X) &&
               (Min.Y <= other.Max.Y && Max.Y >= other.Min.Y) &&
               (Min.Z <= other.Max.Z && Max.Z >= other.Min.Z);
    }

    // Returns true if a point is inside the AABB
    public bool Contains(Vector3 point) {
        return (point.X >= Min.X && point.X <= Max.X) &&
               (point.Y >= Min.Y && point.Y <= Max.Y) &&
               (point.Z >= Min.Z && point.Z <= Max.Z);
    }
}