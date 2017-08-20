using System;

namespace Nescafe.Mappers
{
    public class Mmc1Mapper : Mapper
    {
        public Mmc1Mapper()
        {
            
        }

        public override byte Read(ushort address)
        {
            return 0x01;
        }

        public override void Write(ushort address, byte data)
        {
            throw new Exception();
        }
    }
}
