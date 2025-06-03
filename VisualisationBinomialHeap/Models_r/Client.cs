using MyMinecraft.Entities;

namespace MyMinecraft.Models_r; 
public class Client {
    //TODO: implement it ?????
    #region CLIENT_DATA
    public Player player;
    public ConcurrentDictionary<Vector2i, Chunk_r> localChunks;
    #endregion

    #region THEAD_CONTEXT
    private Thread meshThread;
    #endregion

    #region CONSTRUCTORS
    private Client() {
        localChunks = [];
    }
    public Client(Player player):this() {
        this.player = player;
    }
    #endregion

    #region CONTROL_METHODS

    #endregion


}
