using System.Windows.Forms;

class Emulator
{
    public static void Main()
    {
        Cartridge cartridge = new Cartridge("/home/rhys/Downloads/donkeykong.nes");
        Console console = new Console(cartridge);

        Application.Run(new Display(console));
    }
}