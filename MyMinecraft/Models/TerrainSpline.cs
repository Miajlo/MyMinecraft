namespace MyMinecraft.Models;

public class TerrainSpline {
    private List<float> inputValues;
    private List<float> outputValues;

    public TerrainSpline(List<float> input, List<float> output) {
        if (input.Count != output.Count)
            throw new ArgumentException("Input and output lists must be the same length.");

        inputValues = input;
        outputValues = output;

        // Make cliffs steeper by adjusting output values
        AdjustForCliffs();
    }

    private void AdjustForCliffs() {
        // Example: Increase the steepness of output values where you want cliffs.
        // We will multiply the output difference to create a sharper transition.
        for (int i = 0; i < outputValues.Count - 1; i++) {
            // For example, if the difference between adjacent values is greater than a threshold, make the transition steeper
            if (Math.Abs(outputValues[i + 1] - outputValues[i]) > 0.2f) {
                // Sharpen transition between the values (this factor could be adjusted to control the steepness)
                outputValues[i] = outputValues[i] * 1.5f;  // Increase height at cliffs
                outputValues[i + 1] = outputValues[i + 1] * 1.5f;  // Increase height after cliff
            }
        }
    }

    public float Evaluate(float x) {
        if (x <= inputValues[0]) return outputValues[0];
        if (x >= inputValues[^1]) return outputValues[^1];

        for (int i = 0; i < inputValues.Count - 1; i++) {
            float x0 = inputValues[i];
            float x1 = inputValues[i + 1];

            if (x >= x0 && x <= x1) {
                float t = (x - x0) / (x1 - x0);
                float y0 = outputValues[i];
                float y1 = outputValues[i + 1];
                return Lerp(y0, y1, t);
            }
        }

        return 0f;
    }

    private float Lerp(float a, float b, float t) {
        return a + (b - a) * t;
    }
}
