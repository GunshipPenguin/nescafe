namespace Nescafe
{
    public abstract class Memory
    {
        public abstract byte Read(ushort address);
        public abstract void Write(ushort address, byte data);

        public void ReadBuf(byte[] buffer, ushort address, ushort size)
        {
            for (int bytesRead = 0; bytesRead < size; bytesRead++)
            {
                ushort ReadAddr = (ushort)(address + bytesRead);
                buffer[bytesRead] = Read(ReadAddr);
            }
        }

        public void ReadBufWrapping(byte[] buffer, int startIndex, ushort startAddress, int size)
        {
            int index = startIndex;
            int bytesRead = 0;
            ushort address = startAddress;
            while (bytesRead < size)
            {
                if (index >= buffer.Length) index = 0;
                buffer[index] = Read(address);

                address++;
                bytesRead++;
                index++;
            }
        }

        public ushort Read16(ushort address)
        {
            byte lo = Read(address);
            byte hi = Read((ushort)(address + 1));
            return (ushort)((hi << 8) | lo);
        }

        // Reads 2 bytes, wrapping around to the start of the page if lower byte is at beginning
        // Eg Reading from 0x0AFF Reads 0x0AFF first and 0x0A00 second
        public ushort Read16WrapPage(ushort address)
        {
            ushort data;
            if ((address & 0xFF) == 0xFF)
            {
                byte lo = Read(address);
                byte hi = Read((ushort)(address & (~0xFF))); // Wrap around to start of page eg. 0x02FF becomes 0x0200
                data = (ushort)((hi << 8) | lo);
            }
            else
            {
                data = Read16(address);
            }
            return data;
        }
    }
}
