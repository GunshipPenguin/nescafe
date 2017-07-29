abstract class Mapper {
  protected Cartridge _cartridge;

  public abstract byte readAddress(ushort address);
  public abstract void writeAddress(ushort address, byte data);
}
