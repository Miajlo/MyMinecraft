namespace MyMinecraft.MajorRefactoring;
public struct NoiseOptions {
    public float Frequency;
    public int Seed;
    public int Octaves;
    public float Amplitude;
    public FastNoiseLite.NoiseType NoiseType;

    public NoiseOptions(float frequency, int seed, int octaves, float amplitude, FastNoiseLite.NoiseType noiseType) {
        Frequency = frequency;
        Seed = seed;
        Octaves = octaves;
        Amplitude = amplitude;
        NoiseType = noiseType;
    }
}

