namespace MyMinecraft.MajorRefactoring; 
public class WorldOptions {
    internal long Seed { get; set; }
    internal string Name { get; set; }
    internal NoiseOptions ErosionNoise { get; set; }
    internal NoiseOptions PeaksAndValleys { get; set; }
    internal NoiseOptions Continentalness { get; set; }
    internal NoiseOptions Trees { get; set; }
    internal bool GenerateStructures { get; set; }
    internal ushort SubchunkHeight { get; set; }
    internal byte ChunkWidth { get; set; }
    internal uint ChunkHeight { get; set; }
}
