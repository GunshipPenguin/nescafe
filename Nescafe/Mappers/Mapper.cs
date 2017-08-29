namespace Nescafe.Mappers
{
    /// <summary>
    /// Abstract base class for all mappers.
    /// </summary>
    public abstract class Mapper
    {
        /// <summary>
        /// Enumeration representing VRAM mirroring types.
        /// </summary>
        public enum VramMirroring
        {
            /// <summary>
            /// Specifies Horizontal VRAM mirroring (vertical arrangement of the nametables)
            /// </summary>
            Horizontal,
            /// <summary>
            /// Specifies Vertical VRAM mirroring (horizontal arrangement of the nametables)
            /// </summary>
            Vertical,
            /// <summary>
            /// Specifies single screen mirroring of the lower nametable.
            /// </summary>
            SingleLower,
            /// <summary>
            /// Specifies single screen mirroring of the upper nametable.
            /// </summary>
            SingleUpper,
        }

        /// <summary>
        /// Console that this Mapper is a part of.
        /// </summary>
        protected Console _console;

        /// <summary>
        /// The current VRAM mirroring type.
        /// </summary>
        protected VramMirroring _vramMirroringType;

        /// <summary>
        /// Given a address $2000-$3EFFF, returns the index in the VRAM array
        /// that the address points to depending on the current VRAM mirroring
        /// mode.
        /// </summary>
        /// <returns>The address to index.</returns>
        /// <param name="address">Address.</param>
        public int VramAddressToIndex(ushort address)
        {
            // Address in VRAM indexed with 0 at 0x2000
            int index = (address - 0x2000) % 0x1000;
            switch(_vramMirroringType)
            {
                case VramMirroring.Vertical:
                    // If in one of the mirrored regions, subtract 0x800 to get index
                    if (index >= 0x800) index -= 0x800;
                    break;
                case VramMirroring.Horizontal:
                    if (index > 0x800) index = ((index - 0x800) % 0x400) + 0x400; // In the 2 B regions
                    else index %= 0x400; // In one of the 2 A regions
                    break;
                case VramMirroring.SingleLower:
                    index %= 0x400;
                    break;
                case VramMirroring.SingleUpper:
                    index = (index % 400) + 0x400;
                    break;
            }
            return index;
        }

        /// <summary>
        /// Informs the Mapper that a PPU step has just occurred.
        /// </summary>
        /// <remarks>
        /// Some mappers eg. MMC3 make use of this for IRQ timing. If not
        /// overridden it does nothing.
        /// </remarks>
        public virtual void Step()
        {
            
        }

        /// <summary>
        /// Reads a byte of data from the specified cartridge address 
        /// ($4020-$FFFF). This is mapper dependent and should be implemented 
        /// by each mapper.
        /// </summary>
        /// <returns>The byte read from the specified address</returns>
        /// <param name="address">The address to read from</param>
        public abstract byte Read(ushort address);

        /// <summary>
        /// Writes a byte of memory to the specified cartridge address
        /// ($4020-$FFFF). This is mapper dependent and should be implemented
        /// by each mapper.
        /// </summary>
        /// <param name="address">The address to write to</param>
        /// <param name="data">The byte to write to the specified address</param>
        public abstract void Write(ushort address, byte data);
    }

}
