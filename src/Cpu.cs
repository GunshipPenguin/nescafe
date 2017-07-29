using System;

public class Cpu {
  Memory _memory;

  enum StatusFlag {
    Carry = 1<<0,
    Zero = 1<<1,
    InteruptDisable = 1<<2,
    DecimalMode = 1<<0,
    BreakCommand = 1<<4,
    Bit5 = 1<<5, // Always Set
    Overflow = 1<<6,
    Negative = 1<<7,
  }

  enum AddressMode {
    Absolute=1,      // 1
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

  int[] addressModes = new int[256] {
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

  int[] instructionSizes = new int[256] {
  	1, 2, 0, 0, 2, 2, 2, 0, 1, 2, 1, 0, 0, 0, 0, 0,
  	2, 2, 0, 0, 2, 2, 2, 0, 1, 0, 1, 0, 0, 0, 0, 0,
  	0, 2, 0, 0, 2, 2, 2, 0, 1, 2, 1, 0, 0, 0, 0, 0,
  	2, 2, 0, 0, 2, 2, 2, 0, 1, 0, 1, 0, 0, 0, 0, 0,
  	1, 2, 0, 0, 2, 2, 2, 0, 1, 2, 1, 0, 0, 0, 0, 0,
  	2, 2, 0, 0, 2, 2, 2, 0, 1, 0, 1, 0, 0, 0, 0, 0,
  	1, 2, 0, 0, 2, 2, 2, 0, 1, 2, 1, 0, 0, 0, 0, 0,
  	2, 2, 0, 0, 2, 2, 2, 0, 1, 0, 1, 0, 0, 0, 0, 0,
  	2, 2, 0, 0, 2, 2, 2, 0, 1, 0, 1, 0, 0, 0, 0, 0,
  	2, 2, 0, 0, 2, 2, 2, 0, 1, 0, 1, 0, 0, 0, 0, 0,
  	2, 2, 2, 0, 2, 2, 2, 0, 1, 2, 1, 0, 0, 0, 0, 0,
  	2, 2, 0, 0, 2, 2, 2, 0, 1, 0, 1, 0, 0, 0, 0, 0,
  	2, 2, 0, 0, 2, 2, 2, 0, 1, 2, 1, 0, 0, 0, 0, 0,
  	2, 2, 0, 0, 2, 2, 2, 0, 1, 0, 1, 0, 0, 0, 0, 0,
  	2, 2, 0, 0, 2, 2, 2, 0, 1, 2, 1, 0, 0, 0, 0, 0,
  	2, 2, 0, 0, 2, 2, 2, 0, 1, 0, 1, 0, 0, 0, 0, 0
  };

  // Registers
  byte A; // Accumulator
  byte X;
  byte Y;
  byte S;
  byte P; // Status flags register
  ushort PC; // Program Counter (16 bits)

  delegate void Instruction(AddressMode mode, ushort address);
  Instruction[] instructions;

  public Cpu(Memory memory) {
    _memory = memory;
    PC = 0xC000;

    instructions = new Instruction[256] {
  //  0    1    2    3    4    5    6    7    8    9    A    B    C    D    E    F
      ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, // 0
      ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, // 1
      ___, ___, ___, ___, ___, ___, rol, ___, ___, ___, rol, ___, ___, ___, rol, ___, // 2
      ___, ___, ___, ___, ___, ___, rol, ___, ___, ___, ___, ___, ___, ___, rol, ___, // 3
      ___, ___, ___, ___, ___, ___, lsr, ___, ___, ___, lsr, ___, ___, ___, lsr, ___, // 4
      ___, ___, ___, ___, ___, ___, lsr, ___, ___, ___, ___, ___, ___, ___, lsr, ___, // 5
      ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, // 6
      ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, // 7
      ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, // 8
      ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, // 9
      ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, // A
      ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, // B
      ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, // C
      ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, // D
      ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, // E
      ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___  // F
    };
  }

  public void start() {
    while (true) {
      next();
    }
  }

  private void next() {
    byte opCode = _memory.read(PC);
    System.Console.WriteLine("Executing: 0x" + opCode.ToString("X2"));

    AddressMode mode = (AddressMode) addressModes[opCode];
    System.Console.WriteLine(mode);

    // Get address to operate on
    byte address;
    switch (mode) {
      case AddressMode.Accumulator:
        address = 0;
        break;
      case AddressMode.ZeroPage:
        address = _memory.read((ushort) (PC + 1));
        break;
      default:
        throw new Exception("Address mode not implemented for 0x" + opCode.ToString("X2"));
    }
    PC += (ushort) instructionSizes[opCode];
    System.Console.WriteLine(PC.ToString("X2"));

    instructions[opCode](mode, address);
  }

  private void setFlag(StatusFlag flag, bool value) {
      switch (value) {
        case true:
          P &= (byte) flag;
          break;
        case false:
          P &= (byte) (~((byte) ~flag));
          break;
      }
  }

  private bool flagIsSet(StatusFlag flag) {
    return (P & (byte) flag) == 1;
  }

  // INSTRUCTIONS FOLLOW
  void ___(AddressMode mode, ushort address) {
    throw new Exception("OpCode is not implemented");
  }

  void asl(AddressMode mode, ushort address) {
    
  }

  void rol(AddressMode mode, ushort address) {
    bool carrySetOrig = flagIsSet(StatusFlag.Carry);

    if (mode == AddressMode.Accumulator) {
      // Set carry flag to old msb
      int msb = A >> 7;
      setFlag(StatusFlag.Carry, msb == 1);

      // Shift A left 1
      A <<= 1;

      // Set lsb of A to old carry flag value
      A |= (byte) (carrySetOrig ? 1 << 7 : 0);

      // Set zero and carry flags
      setFlag(StatusFlag.Zero, A == 0);
      setFlag(StatusFlag.Carry, (A >> 7) == 1);
    } else {
      byte data = _memory.read(address);

      // Set carry flag to old msb
      int msb = data >> 7;
      setFlag(StatusFlag.Carry, msb == 1);

      // Shift data left 1
      data <<= 1;

      // Set lsb of data to old carry flag value
      data |= (byte) (carrySetOrig ? 1 << 7 : 0);

      _memory.write(address, data);

      // Set zero and carry flags
      setFlag(StatusFlag.Zero, data == 0);
      setFlag(StatusFlag.Carry, (data >> 7) == 1);
    }
  }

  void lsr(AddressMode mode, ushort address) {
    if (mode == AddressMode.Accumulator) {
      setFlag(StatusFlag.Carry, (A & 1) == 1);
      A >>= 1;
      setFlag(StatusFlag.Zero, A == 0);
    } else {
      byte value = _memory.read(address);
      setFlag(StatusFlag.Carry, (value & 1) == 1);
      byte valueUpdated = (byte) (value >> 1);
      setFlag(StatusFlag.Zero, valueUpdated==0);
    }
  }
}
