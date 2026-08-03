using System.IO;
using System.Windows;

namespace GameBoyEmulator;
public class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("A Boot ROM filepath must be given as a parameter");
            return;
        }
        
        var app = new Application();
        var window = new Views.MainWindow(args);
        app.Run(window);
    }
}
