using MyMinecraft.Entities;

namespace MyMinecraft.Models;
public class Client : GameWindow {
    //TODO: implement it ?????
    #region CLIENT_DATA
    public int RenderDistance;
    public Player player;
    #endregion
    #region THREAD_CONTEXT
    private Thread meshThread;
    private SemaphoreSlim meshSem;
    private volatile bool runMeshThread;

    private ConcurrentQueue<Chunk> chunksToMesh;
    private ConcurrentDictionary<Vector3, Chunk> chunksToRender;
    #endregion
    public Client(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings) {
    }
}
