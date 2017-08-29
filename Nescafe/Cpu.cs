using System;

namespace Nescafe
{
    /// <summary>
    /// Represents a MOS Technologies 6502 CPU with Decimal Mode disabled (as
    /// is the case with the NES).
    /// </summary>
    public class Cpu
    {
        readonly CpuMemory _memory;

        enum AddressMode
        {
            Absolute = 1,    // 1
            AbsoluteX,       // 2
            AbsoluteY,       // 3
            Accumulator,     // 4
            Immediate,       // 5
            Implied,         // 6
            IndexedIndirect, // 7
            Indirect,        // 8
            IndirectIndexed, // 9
            Relative,        // 10
            ZeroPage,        // 11
            ZeroPageX,       // 12
            ZeroPageY        // 13
        };

        int[] _addressModes = {
            6, 7, 6, 7, 11, 11, 11, 11, 6, 5, 4, 5, 1, 1, 1, 1,
            10, 9, 6, 9, 12, 12, 12, 12, 6, 3, 6, 3, 2, 2, 2, 2,
            1, 7, 6, 7, 11, 11, 11, 11, 6, 5, 4, 5, 1, 1, 1, 1,
            10, 9, 6, 9, 12, 12, 12, 12, 6, 3, 6, 3, 2, 2, 2, 2,
            6, 7, 6, 7, 11, 11, 11, 11, 6, 5, 4, 5, 1, 1, 1, 1,
            10, 9, 6, 9, 12, 12, 12, 12, 6, 3, 6, 3, 2, 2, 2, 2,
            6, 7, 6, 7, 11, 11, 11, 11, 6, 5, 4, 5, 8, 1, 1, 1,
            10, 9, 6, 9, 12, 12, 12, 12, 6, 3, 6, 3, 2, 2, 2, 2,
            5, 7, 5, 7, 11, 11, 11, 11, 6, 5, 6, 5, 1, 1, 1, 1,
            10, 9, 6, 9, 12, 12, 13, 13, 6, 3, 6, 3, 2, 2, 3, 3,
            5, 7, 5, 7, 11, 11, 11, 11, 6, 5, 6, 5, 1, 1, 1, 1,
            10, 9, 6, 9, 12, 12, 13, 13, 6, 3, 6, 3, 2, 2, 3, 3,
            5, 7, 5, 7, 11, 11, 11, 11, 6, 5, 6, 5, 1, 1, 1, 1,
            10, 9, 6, 9, 12, 12, 12, 12, 6, 3, 6, 3, 2, 2, 2, 2,
            5, 7, 5, 7, 11, 11, 11, 11, 6, 5, 6, 5, 1, 1, 1, 1,
            10, 9, 6, 9, 12, 12, 12, 12, 6, 3, 6, 3, 2, 2, 2, 2,
        };

        int[] _instructionSizes = {
            1, 2, 0, 0, 2, 2, 2, 0, 1, 2, 1, 0, 3, 3, 3, 0,
            2, 2, 0, 0, 2, 2, 2, 0, 1, 3, 1, 0, 3, 3, 3, 0,
            3, 2, 0, 0, 2, 2, 2, 0, 1, 2, 1, 0, 3, 3, 3, 0,
            2, 2, 0, 0, 2, 2, 2, 0, 1, 3, 1, 0, 3, 3, 3, 0,
            1, 2, 0, 0, 2, 2, 2, 0, 1, 2, 1, 0, 3, 3, 3, 0,
            2, 2, 0, 0, 2, 2, 2, 0, 1, 3, 1, 0, 3, 3, 3, 0,
            1, 2, 0, 0, 2, 2, 2, 0, 1, 2, 1, 0, 3, 3, 3, 0,
            2, 2, 0, 0, 2, 2, 2, 0, 1, 3, 1, 0, 3, 3, 3, 0,
            2, 2, 0, 0, 2, 2, 2, 0, 1, 0, 1, 0, 3, 3, 3, 0,
            2, 2, 0, 0, 2, 2, 2, 0, 1, 3, 1, 0, 0, 3, 0, 0,
            2, 2, 2, 0, 2, 2, 2, 0, 1, 2, 1, 0, 3, 3, 3, 0,
            2, 2, 0, 0, 2, 2, 2, 0, 1, 3, 1, 0, 3, 3, 3, 0,
            2, 2, 0, 0, 2, 2, 2, 0, 1, 2, 1, 0, 3, 3, 3, 0,
            2, 2, 0, 0, 2, 2, 2, 0, 1, 3, 1, 0, 3, 3, 3, 0,
            2, 2, 0, 0, 2, 2, 2, 0, 1, 2, 1, 0, 3, 3, 3, 0,
            2, 2, 0, 0, 2, 2, 2, 0, 1, 3, 1, 0, 3, 3, 3, 0,
        };

        int[] _instructionCycles = {
            7, 6, 2, 8, 3, 3, 5, 5, 3, 2, 2, 2, 4, 4, 6, 6,
            2, 5, 2, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7,
            6, 6, 2, 8, 3, 3, 5, 5, 4, 2, 2, 2, 4, 4, 6, 6,
            2, 5, 2, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7,
            6, 6, 2, 8, 3, 3, 5, 5, 3, 2, 2, 2, 3, 4, 6, 6,
            2, 5, 2, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7,
            6, 6, 2, 8, 3, 3, 5, 5, 4, 2, 2, 2, 5, 4, 6, 6,
            2, 5, 2, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7,
            2, 6, 2, 6, 3, 3, 3, 3, 2, 2, 2, 2, 4, 4, 4, 4,
            2, 6, 2, 6, 4, 4, 4, 4, 2, 5, 2, 5, 5, 5, 5, 5,
            2, 6, 2, 6, 3, 3, 3, 3, 2, 2, 2, 2, 4, 4, 4, 4,
            2, 5, 2, 5, 4, 4, 4, 4, 2, 4, 2, 4, 4, 4, 4, 4,
            2, 6, 2, 8, 3, 3, 5, 5, 2, 2, 2, 2, 4, 4, 6, 6,
            2, 5, 2, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7,
            2, 6, 2, 8, 3, 3, 5, 5, 2, 2, 2, 2, 4, 4, 6, 6,
            2, 5, 2, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7,
        };

        int[] _instructionPageCycles = {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            1, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 1, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            1, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 1, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            1, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 1, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            1, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 1, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            1, 1, 0, 1, 0, 0, 0, 0, 0, 1, 0, 1, 1, 1, 1, 1,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            1, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 1, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            1, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 1, 0, 0,
        };

        // Registers
        byte A;    // Accumulator
        byte X;
        byte Y;
        byte S;    // Stack Pointer
        ushort PC; // Program Counter (16 bits)

        // Status flag register (implemented as several booleans)
        bool C; // Carry flag
        bool Z; // Zero flag
        bool I; // Interrpt Disable
        bool D; // Decimal Mode (Not used)
        bool B; // Break command
        bool V; // Overflow flag
        bool N; // Negative flag

        // Interrupts
        bool irqInterrupt;
        bool nmiInterrupt;

        /// <summary>
        /// Gets the current number of cycles executed by the CPU.
        /// </summary>
        /// <value>The current number of cycles executed by the CPU.</value>
        public int Cycles { get; private set; }

        // If positive, idle 1 cycle and deincrement each step
        int _idle;

        delegate void Instruction(AddressMode mode, ushort address);
        Instruction[] _instructions;

        /// <summary>
        /// Initializes a new <see cref="T:Nescafe.Cpu"/> CPU.
        /// </summary>
        /// <param name="console">The Console that this CPU is a part of</param>
        public Cpu(Console console)
        {
            _memory = console.CpuMemory;

            _instructions = new Instruction[256] {
            //  0    1    2    3    4    5    6    7    8    9    A    B    C    D    E    F
                brk, ora, ___, ___, ___, ora, asl, ___, php, ora, asl, ___, ___, ora, asl, ___, // 0
                bpl, ora, ___, ___, ___, ora, asl, ___, clc, ora, ___, ___, ___, ora, asl, ___, // 1
                jsr, and, ___, ___, bit, and, rol, ___, plp, and, rol, ___, bit, and, rol, ___, // 2
                bmi, and, ___, ___, ___, and, rol, ___, sec, and, ___, ___, ___, and, rol, ___, // 3
                rti, eor, ___, ___, ___, eor, lsr, ___, pha, eor, lsr, ___, jmp, eor, lsr, ___, // 4
                bvc, eor, ___, ___, ___, eor, lsr, ___, cli, eor, ___, ___, ___, eor, lsr, ___, // 5
                rts, adc, ___, ___, ___, adc, ror, ___, pla, adc, ror, ___, jmp, adc, ror, ___, // 6
                bvs, adc, ___, ___, ___, adc, ror, ___, sei, adc, ___, ___, ___, adc, ror, ___, // 7
                ___, sta, ___, ___, sty, sta, stx, ___, dey, ___, txa, ___, sty, sta, stx, ___, // 8
                bcc, sta, ___, ___, sty, sta, stx, ___, tya, sta, txs, ___, ___, sta, ___, ___, // 9
                ldy, lda, ldx, ___, ldy, lda, ldx, ___, tay, lda, tax, ___, ldy, lda, ldx, ___, // A
                bcs, lda, ___, ___, ldy, lda, ldx, ___, clv, lda, tsx, ___, ldy, lda, ldx, ___, // B
                cpy, cmp, ___, ___, cpy, cmp, dec, ___, iny, cmp, dex, ___, cpy, cmp, dec, ___, // C
                bne, cmp, ___, ___, ___, cmp, dec, ___, cld, cmp, ___, ___, ___, cmp, dec, ___, // D
                cpx, sbc, ___, ___, cpx, sbc, inc, ___, inx, sbc, nop, ___, cpx, sbc, inc, ___, // E
                beq, sbc, ___, ___, ___, sbc, inc, ___, sed, sbc, ___, ___, ___, sbc, inc, ___  // F
            };
        }

        /// <summary>
        /// Resets this CPU to its power on state.
        /// </summary>
        public void Reset()
        {
            PC = _memory.Read16(0xFFFC);
            S = 0xFD;
            A = 0;
            X = 0;
            Y = 0;
            SetProcessorFlags((byte)0x24);

            Cycles = 0;
            _idle = 0;

            nmiInterrupt = false;
        }

        /// <summary>
        /// Triggers a non maskable interrupt on this CPU.
        /// </summary>
        public void TriggerNmi()
        {
            nmiInterrupt = true;
        }

        /// <summary>
        /// Triggers an interrupt on this CPU if the interrupt disable flag
        /// is not set.
        /// </summary>
        public void TriggerIrq()
        {
            if (!I) irqInterrupt = true;
        }

        /// <summary>
        /// Instructs the CPU to idle for the specified number of cycles.
        /// </summary>
        /// <param name="idleCycles">Idle cycles.</param>
        public void AddIdleCycles(int idleCycles)
        {
            _idle += idleCycles;
        }

        /// <summary>
        /// Executes the next CPU instruction specified by the Program Counter.
        /// </summary>
        /// <returns>The number of CPU cycles excuted</returns>
        public int Step()
        {
            if (_idle > 0)
            {
                _idle--;
                return 1;
            }

            if (irqInterrupt) Irq();
            irqInterrupt = false;

            if (nmiInterrupt) Nmi();
            nmiInterrupt = false;

            int cyclesOrig = Cycles;
            byte opCode = _memory.Read(PC);

            AddressMode mode = (AddressMode)_addressModes[opCode];

            // Get address to operate on
            ushort address = 0;
            bool pageCrossed = false;
            switch (mode)
            {
                case AddressMode.Implied:
                    break;
                case AddressMode.Immediate:
                    address = (ushort)(PC + 1);
                    break;
                case AddressMode.Absolute:
                    address = _memory.Read16((ushort)(PC + 1));
                    break;
                case AddressMode.AbsoluteX:
                    address = (ushort)(_memory.Read16((ushort)(PC + 1)) + X);
                    pageCrossed = IsPageCross((ushort)(address - X), (ushort)X);
                    break;
                case AddressMode.AbsoluteY:
                    address = (ushort)(_memory.Read16((ushort)(PC + 1)) + Y);
                    pageCrossed = IsPageCross((ushort)(address - Y), (ushort)Y);
                    break;
                case AddressMode.Accumulator:
                    break;
                case AddressMode.Relative:
                    address = (ushort)(PC + (sbyte)_memory.Read((ushort)(PC + 1)) + 2);
                    break;
                case AddressMode.ZeroPage:
                    address = _memory.Read((ushort)(PC + 1));
                    break;
                case AddressMode.ZeroPageY:
                    address = (ushort)((_memory.Read((ushort)(PC + 1)) + Y) & 0xFF);
                    break;
                case AddressMode.ZeroPageX:
                    address = (ushort)((_memory.Read((ushort)(PC + 1)) + X) & 0xFF);
                    break;
                case AddressMode.Indirect:
                    // Must wrap if at the end of a page to emulate a 6502 bug present in the JMP instruction
                    address = (ushort)_memory.Read16WrapPage((ushort)_memory.Read16((ushort)(PC + 1)));
                    break;
                case AddressMode.IndexedIndirect:
                    // Zeropage address of lower nibble of target address (& 0xFF to wrap at 255)
                    ushort lowerNibbleAddress = (ushort)((_memory.Read((ushort)(PC + 1)) + X) & 0xFF);

                    // Target address (Must wrap to 0x00 if at 0xFF)
                    address = (ushort)_memory.Read16WrapPage((ushort)(lowerNibbleAddress));
                    break;
                case AddressMode.IndirectIndexed:
                    // Zeropage address of the value to add the Y register to to get the target address
                    ushort valueAddress = (ushort)_memory.Read((ushort)(PC + 1));

                    // Target address (Must wrap to 0x00 if at 0xFF)
                    address = (ushort)(_memory.Read16WrapPage(valueAddress) + Y);
                    pageCrossed = IsPageCross((ushort)(address - Y), address);
                    break;
            }

            PC += (ushort)_instructionSizes[opCode];
            Cycles += _instructionCycles[opCode];

            if (pageCrossed) Cycles += _instructionPageCycles[opCode];
            _instructions[opCode](mode, address);

            return Cycles - cyclesOrig;
        }

        void SetZn(byte value)
        {
            Z = value == 0;
            N = ((value >> 7) & 1) == 1;
        }

        bool IsBitSet(byte value, int index)
        {
            return (value & (1 << index)) != 0;
        }

        byte PullStack()
        {
            S++;
            byte data = _memory.Read((ushort)(0x0100 | S));
            return data;
        }

        void PushStack(byte data)
        {
            _memory.Write((ushort)(0x100 | S), data);
            S--;
        }

        ushort PullStack16()
        {
            byte lo = PullStack();
            byte hi = PullStack();
            return (ushort)((hi << 8) | lo);
        }

        void PushStack16(ushort data)
        {
            byte lo = (byte)(data & 0xFF);
            byte hi = (byte)((data >> 8) & 0xFF);

            PushStack(hi);
            PushStack(lo);
        }

        byte GetStatusFlags()
        {
            byte flags = 0;

            if (C) flags |= (byte)(1 << 0); // Carry flag, bit 0
            if (Z) flags |= (byte)(1 << 1); // Zero flag, bit 1
            if (I) flags |= (byte)(1 << 2); // Interrupt disable flag, bit 2
            if (D) flags |= (byte)(1 << 3); // Decimal mode flag, bit 3
            if (B) flags |= (byte)(1 << 4); // Break mode, bit 4
            flags |= (byte)(1 << 5); // Bit 5, always set
            if (V) flags |= (byte)(1 << 6); // Overflow flag, bit 6
            if (N) flags |= (byte)(1 << 7); // Negative flag, bit 7

            return flags;
        }

        void SetProcessorFlags(byte flags)
        {
            C = IsBitSet(flags, 0);
            Z = IsBitSet(flags, 1);
            I = IsBitSet(flags, 2);
            D = IsBitSet(flags, 3);
            B = IsBitSet(flags, 4);
            V = IsBitSet(flags, 6);
            N = IsBitSet(flags, 7);
        }

        bool IsPageCross(ushort a, ushort b)
        {
            return (a & 0xFF) != (b & 0xFF);
        }

        void HandleBranchCycles(ushort origPc, ushort branchPc)
        {
            Cycles++;
            Cycles += IsPageCross(origPc, branchPc) ? 1 : 0;
        }

        void Nmi()
        {
            PushStack16(PC);
            PushStack(GetStatusFlags());
            PC = _memory.Read16(0xFFFA);
            I = true;
        }

        void Irq()
        {
            PushStack16(PC);
            PushStack(GetStatusFlags());
            PC = _memory.Read16(0xFFFE);
            I = true;
        }

        // Illegal opcode, throw exception
        void ___(AddressMode mode, ushort address)
        {
            throw new Exception("Illegal Opcode");
        }

        // INSTRUCTIONS FOLLOW
        // BRK - Force Interrupt
        void brk(AddressMode mode, ushort address)
        {
            PushStack16(PC);
            PushStack(GetStatusFlags());
            B = true;
            PC = _memory.Read16((ushort)0xFFFE);
        }

        // ROR - Rotate Right
        void ror(AddressMode mode, ushort address)
        {
            bool Corig = C;
            if (mode == AddressMode.Accumulator)
            {
                C = IsBitSet(A, 0);
                A >>= 1;
                A |= (byte)(Corig ? 0x80 : 0);

                SetZn(A);
            }
            else
            {
                byte data = _memory.Read(address);
                C = IsBitSet(data, 0);

                data >>= 1;
                data |= (byte)(Corig ? 0x80 : 0);

                _memory.Write(address, data);

                SetZn(data);
            }
        }

        // RTI - Return from Interrupt
        void rti(AddressMode mode, ushort address)
        {
            SetProcessorFlags(PullStack());
            PC = PullStack16();
        }

        // TXS - Transfer X to Stack Pointer
        void txs(AddressMode mode, ushort address)
        {
            S = X;
        }

        // TSX - Transfer Stack Pointer to X
        void tsx(AddressMode mode, ushort address)
        {
            X = S;
            SetZn(X);
        }

        // TXA - Transfer X to Accumulator
        void txa(AddressMode mode, ushort address)
        {
            A = X;
            SetZn(A);
        }

        // TYA - Transfer Y to Accumulator
        void tya(AddressMode mode, ushort address)
        {
            A = Y;
            SetZn(A);
        }

        // TAY - Transfer Accumulator to Y
        void tay(AddressMode mode, ushort address)
        {
            Y = A;
            SetZn(Y);
        }

        // TAX  - Transfer Accumulator to X
        void tax(AddressMode mode, ushort address)
        {
            X = A;
            SetZn(X);
        }

        // DEX - Deincrement X
        void dex(AddressMode mode, ushort address)
        {
            X--;
            SetZn(X);
        }

        // DEY - Deincrement Y
        void dey(AddressMode mode, ushort address)
        {
            Y--;
            SetZn(Y);
        }

        // INX - Increment X
        void inx(AddressMode mode, ushort address)
        {
            X++;
            SetZn(X);
        }

        // INY - Increment Y
        void iny(AddressMode mode, ushort address)
        {
            Y++;
            SetZn(Y);
        }

        // STY - Store Y Register
        void sty(AddressMode mode, ushort address)
        {
            _memory.Write(address, Y);
        }

        // CPX - Compare X Register
        void cpx(AddressMode mode, ushort address)
        {
            byte data = _memory.Read(address);
            SetZn((byte)(X - data));
            C = X >= data;
        }

        // CPX - Compare Y Register
        void cpy(AddressMode mode, ushort address)
        {
            byte data = _memory.Read(address);
            SetZn((byte)(Y - data));
            C = Y >= data;
        }

        // SBC - Subtract with Carry
        void sbc(AddressMode mode, ushort address)
        {
            byte data = _memory.Read(address);
            int notCarry = (!C ? 1 : 0);

            byte result = (byte)(A - data - notCarry);
            SetZn(result);

            // If an overflow occurs (result actually less than 0)
            // the carry flag is cleared
            C = (A - data - notCarry) >= 0 ? true : false;

            V = ((A ^ data) & (A ^ result) & 0x80) != 0;

            A = result;
        }

        // ADC - Add with Carry
        void adc(AddressMode mode, ushort address)
        {
            byte data = _memory.Read(address);
            int carry = (C ? 1 : 0);

            byte sum = (byte)(A + data + carry);
            SetZn(sum);

            C = (A + data + carry) > 0xFF;

            // Sign bit is wrong if sign bit of operands is same
            // and sign bit of result is different
            // if <A and data> differ in sign and <A and sum> have the same sign, set the overflow flag
            // https://stackoverflow.com/questions/29193303/6502-emulation-proper-way-to-implement-adc-and-sbc
            V = (~(A ^ data) & (A ^ sum) & 0x80) != 0;

            A = sum;
        }

        // EOR - Exclusive OR
        void eor(AddressMode mode, ushort address)
        {
            byte data = _memory.Read(address);
            A ^= data;
            SetZn(A);
        }

        // CLV - Clear Overflow Flag
        void clv(AddressMode mode, ushort address)
        {
            V = false;
        }

        // BMI - Branch if Minus
        void bmi(AddressMode mode, ushort address)
        {
            PC = N ? address : PC;
        }

        // PLP - Pull Processor Status
        void plp(AddressMode mode, ushort address)
        {
            SetProcessorFlags((byte)(PullStack() & ~(0x10)));
        }

        // CLD - Clear Decimal Mode
        void cld(AddressMode mode, ushort address)
        {
            D = false;
        }

        // CMP - Compare
        void cmp(AddressMode mode, ushort address)
        {
            byte data = _memory.Read(address);
            C = A >= data;
            SetZn((byte)(A - data));
        }

        // AND - Logical AND
        void and(AddressMode mode, ushort address)
        {
            byte data = _memory.Read(address);
            A &= data;
            SetZn(A);
        }

        // PLA - Pull Accumulator
        void pla(AddressMode mode, ushort address)
        {
            A = PullStack();
            SetZn(A);
        }

        // PHP - Push Processor Status
        void php(AddressMode mode, ushort address)
        {
            PushStack((byte)(GetStatusFlags() | 0x10));
        }

        // SED - Set Decimal Flag
        void sed(AddressMode mode, ushort address)
        {
            D = true;
        }

        // CLI - Clear Interrupt Disable
        void cli(AddressMode mode, ushort address)
        {
            I = false;
        }

        // SEI - Set Interrupt Disable
        void sei(AddressMode mode, ushort address)
        {
            I = true;
        }

        // DEC - Deincrement Memory
        void dec(AddressMode mode, ushort address)
        {
            byte data = _memory.Read(address);
            data--;
            _memory.Write(address, data);
            SetZn(data);
        }

        // INC - Increment Memory
        void inc(AddressMode mode, ushort address)
        {
            byte data = _memory.Read(address);
            data++;
            _memory.Write(address, data);
            SetZn(data);
        }

        // RTS - Return from Subroutine
        void rts(AddressMode mode, ushort address)
        {
            PC = (ushort)(PullStack16() + 1);
        }

        // JSR - Jump to Subroutine
        void jsr(AddressMode mode, ushort address)
        {
            PushStack16((ushort)(PC - 1));
            PC = address;
        }

        // BPL - Branch if Positive
        void bpl(AddressMode mode, ushort address)
        {
            if (!N)
            {
                HandleBranchCycles(PC, address);
                PC = address;
            }
        }

        // BVC - Branch if Overflow Clear
        void bvc(AddressMode mode, ushort address)
        {
            if (!V)
            {
                HandleBranchCycles(PC, address);
                PC = address;
            }
        }

        // BVS - Branch if Overflow Set
        void bvs(AddressMode mode, ushort address)
        {
            if (V)
            {
                HandleBranchCycles(PC, address);
                PC = address;
            }
        }

        // BIT - Bit Test
        void bit(AddressMode mode, ushort address)
        {
            byte data = _memory.Read(address);
            N = IsBitSet(data, 7);
            V = IsBitSet(data, 6);
            Z = (data & A) == 0;
        }

        // BNE - Branch if Not Equal
        void bne(AddressMode mode, ushort address)
        {
            if (!Z)
            {
                HandleBranchCycles(PC, address);
                PC = address;
            }
        }

        // BEQ - Branch if Equal
        void beq(AddressMode mode, ushort address)
        {
            if (Z)
            {
                HandleBranchCycles(PC, address);
                PC = address;
            }
        }

        // CLC - Clear Carry Flag
        void clc(AddressMode mode, ushort address)
        {
            C = false;
        }

        // BCC - Branch if Carry Clear
        void bcc(AddressMode mode, ushort address)
        {
            if (!C)
            {
                HandleBranchCycles(PC, address);
                PC = address;
            }
        }

        // BCs - Branch if Carry Set
        void bcs(AddressMode mode, ushort address)
        {
            if (C)
            {
                HandleBranchCycles(PC, address);
                PC = address;
            }
        }

        // SEC - Set Carry Flag
        void sec(AddressMode mode, ushort address)
        {
            C = true;
        }

        // NOP - No Operation
        void nop(AddressMode mode, ushort address)
        {

        }

        // STX - Store X Register
        void stx(AddressMode mode, ushort address)
        {
            _memory.Write(address, X);
        }

        // LDY - Load Y Register
        void ldy(AddressMode mode, ushort address)
        {
            Y = _memory.Read(address);
            SetZn(Y);
        }

        // LDX - Load X Register
        void ldx(AddressMode mode, ushort address)
        {
            X = _memory.Read(address);
            SetZn(X);
        }

        // JMP - Jump
        void jmp(AddressMode mode, ushort address)
        {
            PC = address;
        }

        // STA - Store Accumulator
        void sta(AddressMode mode, ushort address)
        {
            _memory.Write(address, A);
        }

        // ORA - Logical Inclusive OR
        void ora(AddressMode mode, ushort address)
        {
            A |= _memory.Read(address);
            SetZn(A);
        }

        // LDA - Load A Register
        void lda(AddressMode mode, ushort address)
        {
            A = _memory.Read(address);
            SetZn(A);
        }

        // PHA - Push Accumulator
        void pha(AddressMode mode, ushort address)
        {
            PushStack(A);
        }

        // ASL - Arithmetic Shift Left
        void asl(AddressMode mode, ushort address)
        {
            if (mode == AddressMode.Accumulator)
            {
                C = IsBitSet(A, 7);
                A <<= 1;
                SetZn(A);
            }
            else
            {
                byte data = _memory.Read(address);
                C = IsBitSet(data, 7);
                byte dataUpdated = (byte)(data << 1);
                _memory.Write(address, dataUpdated);
                SetZn(dataUpdated);
            }
        }

        // ROL - Rotate Left
        void rol(AddressMode mode, ushort address)
        {
            bool Corig = C;
            if (mode == AddressMode.Accumulator)
            {
                C = IsBitSet(A, 7);
                A <<= 1;
                A |= (byte)(Corig ? 1 : 0);

                SetZn(A);
            }
            else
            {
                byte data = _memory.Read(address);
                C = IsBitSet(data, 7);

                data <<= 1;
                data |= (byte)(Corig ? 1 : 0);

                _memory.Write(address, data);

                SetZn(data);
            }
        }

        // LSR - Logical Shift Right
        void lsr(AddressMode mode, ushort address)
        {
            if (mode == AddressMode.Accumulator)
            {
                C = (A & 1) == 1;
                A >>= 1;

                SetZn(A);
            }
            else
            {
                byte value = _memory.Read(address);
                C = (value & 1) == 1;

                byte updatedValue = (byte)(value >> 1);

                _memory.Write(address, updatedValue);

                SetZn(updatedValue);
            }
        }
    }
}
