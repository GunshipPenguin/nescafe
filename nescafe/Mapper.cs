public abstract class Mapper
{
    protected Cartridge _cartridge;

    public abstract byte ReadAddress(ushort address);
}
