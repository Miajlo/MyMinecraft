namespace MyMinecraft.Models_r; 
public struct NoiseSettings {
    public FastNoiseLite.NoiseType Type { get; set; }
    public float Frequency { get; set; }

    public int Seed { get; set; }
}
