namespace Nescafe
{
    /// <summary>
    /// Abstract base class for memory devices.
    /// </summary>
    public abstract class Memory
    {
        /// <summary>
        /// Read a byte of memory from the specified address.
        /// </summary>
        /// <returns>The byte of memory read from the specified address</returns>
        /// <param name="address">The address to read from</param>
        public abstract byte Read(ushort address);

        /// <summary>
        /// Write a byte of memory to the specified address.
        /// </summary>
        /// <param name="address">The address to write the byte of memory to</param>
        /// <param name="data">The byte to write to the specified address</param>
        public abstract void Write(ushort address, byte data);

        /// <summary>
        /// Reads the specified number of bytes into the buffer starting from 
        /// the specified address.
        /// </summary>
        /// <param name="buffer">Buffer to read into</param>
        /// <param name="address">Address to start reading from</param>
        /// <param name="size">Number of bytes to read</param>
        public void ReadBuf(byte[] buffer, ushort address, ushort size)
        {
            for (int bytesRead = 0; bytesRead < size; bytesRead++)
            {
                ushort ReadAddr = (ushort)(address + bytesRead);
                buffer[bytesRead] = Read(ReadAddr);
            }
        }

        /// <summary>
        /// Reads the specified number of bytes into the buffer starting from 
        /// the specified address and starting at the specified index in the
        /// buffer, wrapping around to index 0 in the buffer if the end is
        /// reached.
        /// </summary>
        /// <param name="buffer">Buffer.</param>
        /// <param name="startIndex">Start index.</param>
        /// <param name="startAddress">Start address.</param>
        /// <param name="size">Size.</param>
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

        /// <summary>
        /// Reads two bytes from the specified address (little endian).
        /// </summary>
        /// <returns>A 16 bit value representing the two bytes read in a little endian fashion.</returns>
        /// <param name="address">The address to read two bytes from</param>
        public ushort Read16(ushort address)
        {
            byte lo = Read(address);
            byte hi = Read((ushort)(address + 1));
            return (ushort)((hi << 8) | lo);
        }

        /// <summary>
        /// Reads two bytes (little endian) wrapping around to the start of
        /// the 256 byte page if the lower byte is at beginning
        /// </summary>
        /// <remarks>
        /// For example, reading from $0AFF Reads $0AFF first and $0A00 second.
        /// </remarks>
        /// <returns>A 16 bit value representing the two bytes read in a little endian fashion</returns>
        /// <param name="address">The address to read two bytes from</param>
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
