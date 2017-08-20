namespace Nescafe.Mappers
{
    public abstract class Mapper
    {
        protected Cartridge _cartridge;

        public abstract byte Read(ushort address);
        public abstract void Write(ushort address, byte data);
    }

}
