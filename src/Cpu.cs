using System;

public class Cpu {
  Memory _memory;

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
  ushort PC; // Program Counter (16 bits)

  // Status flag register (implemented as several booleans)
  bool C; // Carry flag
  bool Z; // Zero flag
  bool I; // Interrpt Disable
  bool D; // Decimal Mode (Not used)
  bool B; // Break command
  bool V; // Overflow flag
  bool N; // Negative flag


  delegate void Instruction(AddressMode mode, ushort address);
  Instruction[] instructions;

  public Cpu(Memory memory) {
    _memory = memory;
    PC = 0xC000;

    instructions = new Instruction[256] {
  //  0    1    2    3    4    5    6    7    8    9    A    B    C    D    E    F
      ___, ora, ___, ___, ___, ora, asl, ___, ___, ora, asl, ___, ___, ora, asl, ___, // 0
      ___, ora, ___, ___, ___, ora, asl, ___, ___, ora, ___, ___, ___, ora, asl, ___, // 1
      jsr, ___, ___, ___, ___, ___, rol, ___, ___, ___, rol, ___, ___, ___, rol, ___, // 2
      ___, ___, ___, ___, ___, ___, rol, ___, ___, ___, ___, ___, ___, ___, rol, ___, // 3
      ___, ___, ___, ___, ___, ___, lsr, ___, pha, ___, lsr, ___, jmp, ___, lsr, ___, // 4
      ___, ___, ___, ___, ___, ___, lsr, ___, ___, ___, ___, ___, ___, ___, lsr, ___, // 5
      ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, jmp, ___, ___, ___, // 6
      ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, // 7
      ___, sta, ___, ___, ___, sta, stx, ___, ___, ___, ___, ___, ___, sta, stx, ___, // 8
      ___, sta, ___, ___, ___, sta, stx, ___, ___, sta, ___, ___, ___, sta, ___, ___, // 9
      ___, lda, ldx, ___, ___, lda, ldx, ___, ___, lda, ___, ___, ___, lda, ldx, ___, // A
      ___, lda, ___, ___, ___, lda, ldx, ___, ___, lda, ___, ___, ___, lda, ldx, ___, // B
      ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, // C
      ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, // D
      ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, nop, ___, ___, ___, ___, ___, // E
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
    ushort address;
    switch (mode) {
      case AddressMode.Implied:
        address = 0;
        break;
      case AddressMode.Immediate:
        address = (ushort) (PC + 1);
        break;
      case AddressMode.Absolute:
        address = _memory.read16((ushort) (PC + 1));
        break;
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

  private void setZn(byte value) {
    Z = value == 0;
    N = ((value>>7) & 1) == 1;
  }

  private bool isBitSet(byte value, int index) {
    return (value & (1 << index)) != 0;
  }

  // INSTRUCTIONS FOLLOW
  void ___(AddressMode mode, ushort address) {
    throw new Exception("OpCode is not implemented");
  }

  void nop(AddressMode mode, ushort address) {

  }
  
  void jsr(AddressMode mode, ushort address) {
    _memory.write16((ushort) (S+1), (ushort) (address-1));
    S += 1;
    PC = address;
  }

  void stx(AddressMode mode, ushort address) {
    _memory.write(address, X);
  }

  void ldx(AddressMode mode, ushort address) {
    X = _memory.read(address);
    setZn(X);
  }

  void jmp(AddressMode mode, ushort address) {
    PC = address;
  }

  void sta(AddressMode mode, ushort address) {
    System.Console.WriteLine(address);
    _memory.write(address, A);
  }

  void ora(AddressMode mode, ushort address) {
    A |= _memory.read(address);
    setZn(A);
  }

  void lda(AddressMode mode, ushort address) {
    A = _memory.read(address);
    setZn(A);
  }

  void pha(AddressMode mode, ushort address) {
    _memory.write((ushort) (S+1), A);
    S += 1;
  }

  void asl(AddressMode mode, ushort address) {
    if (mode == AddressMode.Accumulator) {
      C = isBitSet(A, 7);
      A <<= 1;
      setZn(A);
    } else {
      byte data = _memory.read(address);
      C = isBitSet(data, 7);
      byte dataUpdated = (byte) (data << 1);
      _memory.write(address, dataUpdated);
      setZn(dataUpdated);
    }
  }

  void rol(AddressMode mode, ushort address) {
    bool carrySetOrig = C;

    if (mode == AddressMode.Accumulator) {
      // Set carry flag to old msb
      int msb = (A >> 7) & 1;
      C = msb == 1;

      // Shift A left 1
      A <<= 1;

      // Set lsb of A to old carry flag value
      A |= (byte) (carrySetOrig ? 1 : 0);

      setZn(A);
    } else {
      byte data = _memory.read(address);

      int msb = (data >> 7) & 1;
      C = msb == 1;

      // Shift data left 1 and set lsb to old carry flag
      data <<= 1;
      data |= (byte) (carrySetOrig ? 1 : 0);

      _memory.write(address, data);

      setZn(A);
    }
  }

  void lsr(AddressMode mode, ushort address) {
    if (mode == AddressMode.Accumulator) {
      C = (A & 1) == 1;
      A >>= 1;

      setZn(A);
    } else {
      byte value = _memory.read(address);
      C = (value & 1) == 1;

      byte updatedValue = (byte) (value >> 1);

      _memory.write(address, updatedValue);

      setZn(updatedValue);
    }
  }
}
