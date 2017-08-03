using System.Windows.Forms;

class Emulator {
    public static void Main() {
        Cartridge cartridge = new Cartridge("nestest.nes");
        Console console = new Console(cartridge);

        Application.Run(new Display(console));
    }
}