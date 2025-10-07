using System;
using System.IO;
using System.Windows.Forms;

static class Program
{
    static void Main()
    {
        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

        Application.EnableVisualStyles();
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new Form());
    }
}