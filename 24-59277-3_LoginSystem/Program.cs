using System;
using System.Windows.Forms;

namespace _24_59277_3_LoginSystem
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// The app starts on LoginForm. HomeForm is opened FROM LoginForm after a
        /// successful login, and logging out closes HomeForm and shows LoginForm
        /// again - the application only exits when LoginForm itself is closed.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LoginForm());
        }
    }
}
