using System.Windows.Forms;

class Emulator {
    public static void Main() {
        Cartridge cartridge = new Cartridge("/home/rhys/Downloads/full_nes_palette.nes");
        Console console = new Console(cartridge);

        Application.Run(new Display(console));
    }
}