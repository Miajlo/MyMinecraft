namespace MyMinecraft;


public class Program {
    public static void Main() {
        using (Window win = new(1920, 1080, "BinomialHeap")) {
            win.Run();
            Console.WriteLine("Drew the window");
        }
        Console.WriteLine("Exiting the program");
    }
}