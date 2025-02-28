namespace MyMinecraft.Models;
public static class Tree {
    public static readonly int[,,] treeBlocks;
    public const byte SIZE = 5;
    public const byte HEIGHT = 7;

    static Tree() {
        treeBlocks = new int[HEIGHT, SIZE, SIZE] // HEIGHT = layers, SIZE x SIZE = grid
        {
            { // Layer 1 (Bottom)
                { 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0 },
                { 0, 0, 6, 0, 0 },
                { 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0 }
            },
            { // Layer 2-3 (Trunk)
                { 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0 },
                { 0, 0, 6, 0, 0 },
                { 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0 }
            },
            {
                { 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0 },
                { 0, 0, 6, 0, 0 },
                { 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0 }
            },
            { // Layer 4 (Trunk with Leaves)
                { 7, 7, 7, 7, 7 },
                { 7, 7, 7, 7, 7 },
                { 7, 7, 6, 7, 7 },
                { 7, 7, 7, 7, 7 },
                { 7, 7, 7, 7, 7 }
            },
            { // Layer 5 (Leaves)
                { 0, 0, 7, 0, 0 },
                { 7, 7, 7, 7, 7 },
                { 7, 7, 6, 7, 7 },
                { 7, 7, 7, 7, 7 },
                { 0, 7, 7, 0, 0 }
            },
            { // Layer 6 (More Leaves)
                { 0, 0, 0, 0, 0 },
                { 0, 0, 7, 0, 0 },
                { 0, 7, 6, 7, 0 },
                { 0, 0, 7, 0, 0 },
                { 0, 0, 0, 0, 0 }
            },
            { // Layer 7 (Top Leaves)
                { 0, 0, 0, 0, 0 },
                { 0, 0, 7, 0, 0 },
                { 0, 7, 7, 7, 0 },
                { 0, 0, 7, 0, 0 },
                { 0, 0, 0, 0, 0 }
            }
        };
    }
}
