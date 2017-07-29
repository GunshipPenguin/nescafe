using System;

namespace nes {
   public class Emulator {
      public static void Main() {
        Cartridge cartridge = new Cartridge("nestest.nes");
        Console console = new Console(cartridge);
        console.start();
      }
   }
}
