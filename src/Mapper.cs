abstract class Mapper {
  protected Cartridge _cartridge;

  public abstract byte readAddress(ushort address);
}
