using System;
using System.Windows.Forms;

namespace Nescafe
{
    class Emulator
    {
        [STAThread]
        public static void Main()
        {
            Application.Run(new Ui());
        }
    }
}
