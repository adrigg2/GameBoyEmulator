using System.IO;
using System.Windows;
using GameBoyEmulator.Core;

namespace GameBoyEmulator;
public class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("A ROM and Boot ROM filepath should be given as a parameter");
            return;
        }

        var emulator = new Emulator(args[0], args[1]);
        var app = new Application();
        var window = new MainWindow(emulator);
        app.Run(window);
    }
}
