using MyMinecraft.Entities;

namespace MyMinecraft.Models_r; 
public class Client {
    //TODO: implement it ?????
    #region CLIENT_DATA
    public Player player;
    public ConcurrentDictionary<Vector3, Chunk_r> localChunks;
    #endregion
}
