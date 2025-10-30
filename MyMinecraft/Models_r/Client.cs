using MyMinecraft.Entities;

namespace MyMinecraft.Models_r;
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

    private ConcurrentQueue<Chunk_r> chunksToMesh;
    private ConcurrentDictionary<Vector3, Chunk_r> chunksToRender;
    #endregion
    public Client(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings) {
    }
}
