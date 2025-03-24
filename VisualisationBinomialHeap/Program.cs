namespace MyMinecraft;

public class Program {
    public static void Main() {
        using (Window win = new(900, 900, "BinomialHeap")) {
            //win.VSync = VSyncMode.On;
            win.Run();
            Console.WriteLine("Drew the window");
        }
        Console.WriteLine("Exiting the program");
    }
}