namespace MyMinecraft.Models_r;

public class TerrainEvaluator {
    private TerrainSpline continentalnessSpline;
    private TerrainSpline erosionSpline;
    private TerrainSpline peaksValleysSpline;

    private float maxHeight;

    public TerrainEvaluator(float maxHeight = 100f) {
        this.maxHeight = maxHeight;

        // Initialize splines with fixed control points
        continentalnessSpline = new TerrainSpline(
            new List<float> { -1.0f, -0.5f, 0.0f, 0.5f, 1.0f }, // Added more points
            new List<float> { 0.5f, 0.6f, 0.75f, 0.85f, 1.0f }  // Added more values for smoothness
        );

        erosionSpline = new TerrainSpline(
            new List<float> { -1.0f, 0.0f, 0.5f, 1.0f },
            new List<float> { 0.2f, 0.6f, 0.2f, 0.2f }
        );

        peaksValleysSpline = new TerrainSpline(
            new List<float> { -1.0f, -0.5f, 0.0f, 0.5f, 1.0f }, // Added more points
            new List<float> { 0.0f, 0.1f, 0.2f, 0.5f, 0.5f }  // Modified values for more variation
        );
    }


    public float EvaluateHeight(float continentalnessNoise, float erosionNoise, float peaksValleysNoise) {
        float cont = continentalnessSpline.Evaluate(continentalnessNoise);
        float ero = erosionSpline.Evaluate(erosionNoise);
        float peaks = peaksValleysSpline.Evaluate(peaksValleysNoise);

        // Combine into final height value
        float heightValue = cont * (1f - ero) + peaks;

        // Optional: adjust curve shape
        heightValue = (float)Math.Pow(heightValue, 1.2f);

        return heightValue * maxHeight;
    }

    public void EvaluateAll(float continentalnessNoise, float erosionNoise, float peaksValleysNoise,
                            out float continentalness, out float erosion, out float peaks, out float height) {
        continentalness = continentalnessSpline.Evaluate(continentalnessNoise);
        erosion = erosionSpline.Evaluate(erosionNoise);
        peaks = peaksValleysSpline.Evaluate(peaksValleysNoise);

        float heightValue = continentalness * (1f - erosion) + peaks;
        heightValue = (float)Math.Pow(heightValue, 1.2f);

        height = heightValue * maxHeight;
    }
}
