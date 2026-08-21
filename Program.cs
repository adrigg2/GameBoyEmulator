using System.IO;
using System.Windows;

namespace GameBoyEmulator;
public class Program
{
    [STAThread]
    public static void Main()
    {   
        var app = new Application();
        var window = new Views.MainWindow();
        app.Run(window);
    }
}
