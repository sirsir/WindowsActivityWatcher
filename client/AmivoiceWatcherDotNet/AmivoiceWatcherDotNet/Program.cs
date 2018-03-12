using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AmivoiceWatcher
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                Console.WriteLine("Hello World" +args[0]);
            }
            //else
            {
                Console.WriteLine("Hello World" );
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //Application.Run(new FormTemp());
            Application.Run(new AmivoiceWatcher());
        }
    }
}
