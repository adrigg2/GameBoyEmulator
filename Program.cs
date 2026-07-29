using System.IO;
using System.Windows;
using GameBoyEmulator.Core;

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
        var window = new MainWindow(args);
        app.Run(window);
    }
}
