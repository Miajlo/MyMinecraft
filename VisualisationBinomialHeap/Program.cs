﻿namespace VisualisationBinomialHeap;


public class Program {
    public static void Main() {
        using (Window win = new(700, 700, "BinomialHeap")) {
            win.Run();
            Console.WriteLine("Drew the window");
        }
        Console.WriteLine("Exiting the program");
    }
}