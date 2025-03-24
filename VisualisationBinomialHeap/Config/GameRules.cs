namespace MyMinecraft.Config; 
public struct GameRules {
    public bool generateChunks;
    public bool doCollisionChecks;
    public bool showChunkBorders;
    public float movementSpeed;
    public bool physics;
    public float gravity;
    public float jumpForce;
    public GameRules() {
        generateChunks = true;
        doCollisionChecks = false;
        showChunkBorders = false;
        movementSpeed = 10f;
        gravity = 14f;
        physics = false;
        jumpForce = 6f;
    }
}
