
namespace MyMinecraft.Config; 
public struct GameRules {
    public bool generateChunks;
    public bool doCollisionChecks;
    public bool showChunkBorders;
    public float movementSpeed;
    public bool physics;
    public float gravity;
    public float jumpHeight;
    public float weight;
    public float terminalVelocity;
    public const float dragCoefitient = 1.05f;
    public GameRules() {
        generateChunks = true;
        doCollisionChecks = false;
        showChunkBorders = false;
        movementSpeed = 10f;
        gravity = 9.80527f;
        physics = false;
        jumpHeight = 1.25f;
        weight = 100;
        terminalVelocity = CalculateTerminalVelocity(1.225f);
    }

    public float CalculateCrossSection() {
        return GameConfig.playerDepth*GameConfig.playerWidth;
    }

    public float CalculateTerminalVelocity(float fluidDensity) {
        float mass = weight*gravity;

        return MathF.Sqrt(2*mass /
            (fluidDensity * GameRules.dragCoefitient * CalculateCrossSection()));
    }
}
