namespace MyMinecraft.Config; 
public struct GameRules {
    public bool generateChunks;
    public bool doCollisionChecks;
    public bool showChunkBorders;
    public float movementSpeed;
    public GameRules() {
        generateChunks = true;
        doCollisionChecks = false;
        showChunkBorders = false;
        movementSpeed = 15.0f;
    }
}
