namespace MyMinecraft;

public class Program {
    public static void Main() {
        using (Window win = new(1600, 1000, "BinomialHeap")) {
            //win.VSync = VSyncMode.On;
            win.Run();
            Console.WriteLine("Drew the window");
        }
        Console.WriteLine("Exiting the program");
    }
}