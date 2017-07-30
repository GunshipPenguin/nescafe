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

  String[] instructionNames = new String[256] {
    "BRK", "ORA", "KIL", "SLO", "NOP", "ORA", "ASL", "SLO",
    "PHP", "ORA", "ASL", "ANC", "NOP", "ORA", "ASL", "SLO",
    "BPL", "ORA", "KIL", "SLO", "NOP", "ORA", "ASL", "SLO",
    "CLC", "ORA", "NOP", "SLO", "NOP", "ORA", "ASL", "SLO",
    "JSR", "AND", "KIL", "RLA", "BIT", "AND", "ROL", "RLA",
    "PLP", "AND", "ROL", "ANC", "BIT", "AND", "ROL", "RLA",
    "BMI", "AND", "KIL", "RLA", "NOP", "AND", "ROL", "RLA",
    "SEC", "AND", "NOP", "RLA", "NOP", "AND", "ROL", "RLA",
    "RTI", "EOR", "KIL", "SRE", "NOP", "EOR", "LSR", "SRE",
    "PHA", "EOR", "LSR", "ALR", "JMP", "EOR", "LSR", "SRE",
    "BVC", "EOR", "KIL", "SRE", "NOP", "EOR", "LSR", "SRE",
    "CLI", "EOR", "NOP", "SRE", "NOP", "EOR", "LSR", "SRE",
    "RTS", "ADC", "KIL", "RRA", "NOP", "ADC", "ROR", "RRA",
    "PLA", "ADC", "ROR", "ARR", "JMP", "ADC", "ROR", "RRA",
    "BVS", "ADC", "KIL", "RRA", "NOP", "ADC", "ROR", "RRA",
    "SEI", "ADC", "NOP", "RRA", "NOP", "ADC", "ROR", "RRA",
    "NOP", "STA", "NOP", "SAX", "STY", "STA", "STX", "SAX",
    "DEY", "NOP", "TXA", "XAA", "STY", "STA", "STX", "SAX",
    "BCC", "STA", "KIL", "AHX", "STY", "STA", "STX", "SAX",
    "TYA", "STA", "TXS", "TAS", "SHY", "STA", "SHX", "AHX",
    "LDY", "LDA", "LDX", "LAX", "LDY", "LDA", "LDX", "LAX",
    "TAY", "LDA", "TAX", "LAX", "LDY", "LDA", "LDX", "LAX",
    "BCS", "LDA", "KIL", "LAX", "LDY", "LDA", "LDX", "LAX",
    "CLV", "LDA", "TSX", "LAS", "LDY", "LDA", "LDX", "LAX",
    "CPY", "CMP", "NOP", "DCP", "CPY", "CMP", "DEC", "DCP",
    "INY", "CMP", "DEX", "AXS", "CPY", "CMP", "DEC", "DCP",
    "BNE", "CMP", "KIL", "DCP", "NOP", "CMP", "DEC", "DCP",
    "CLD", "CMP", "NOP", "DCP", "NOP", "CMP", "DEC", "DCP",
    "CPX", "SBC", "NOP", "ISC", "CPX", "SBC", "INC", "ISC",
    "INX", "SBC", "NOP", "SBC", "CPX", "SBC", "INC", "ISC",
    "BEQ", "SBC", "KIL", "ISC", "NOP", "SBC", "INC", "ISC",
    "SED", "SBC", "NOP", "ISC", "NOP", "SBC", "INC", "ISC",
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

    S = 0xFF;

    instructions = new Instruction[256] {
  //  0    1    2    3    4    5    6    7    8    9    A    B    C    D    E    F
      ___, ora, ___, ___, ___, ora, asl, ___, ___, ora, asl, ___, ___, ora, asl, ___, // 0
      bpl, ora, ___, ___, ___, ora, asl, ___, clc, ora, ___, ___, ___, ora, asl, ___, // 1
      jsr, ___, ___, ___, bit, ___, rol, ___, ___, ___, rol, ___, bit, ___, rol, ___, // 2
      ___, ___, ___, ___, ___, ___, rol, ___, sec, ___, ___, ___, ___, ___, rol, ___, // 3
      ___, ___, ___, ___, ___, ___, lsr, ___, pha, ___, lsr, ___, jmp, ___, lsr, ___, // 4
      bvc, ___, ___, ___, ___, ___, lsr, ___, ___, ___, ___, ___, ___, ___, lsr, ___, // 5
      rts, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, jmp, ___, ___, ___, // 6
      bvs, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, // 7
      ___, sta, ___, ___, ___, sta, stx, ___, ___, ___, ___, ___, ___, sta, stx, ___, // 8
      bcc, sta, ___, ___, ___, sta, stx, ___, ___, sta, ___, ___, ___, sta, ___, ___, // 9
      ___, lda, ldx, ___, ___, lda, ldx, ___, ___, lda, ___, ___, ___, lda, ldx, ___, // A
      bcs, lda, ___, ___, ___, lda, ldx, ___, ___, lda, ___, ___, ___, lda, ldx, ___, // B
      ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, // C
      bne, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, // D
      ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, nop, ___, ___, ___, ___, ___, // E
      beq, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___, ___  // F
    };
  }

  public void start() {
    while (true) {
      next();
    }
  }

  private void next() {
    byte opCode = _memory.read(PC);

    System.Console.Write("( ");
    System.Console.Write("A: " + A.ToString("X2") + " ");
    System.Console.Write("X: " + X.ToString("X2") + " ");
    System.Console.Write("Y: " + Y.ToString("X2") + " ");
    System.Console.Write("SP: " + S.ToString("X2") + " ");
    System.Console.Write(")    ");

    System.Console.Write(PC.ToString("X4"));
    System.Console.Write("  ");
    System.Console.Write(instructionNames[opCode]);
    System.Console.Write("  ");

    AddressMode mode = (AddressMode) addressModes[opCode];

    // Get address to operate on
    ushort address;
    switch (mode) {
      case AddressMode.Implied:
        address = 0;
        break;
      case AddressMode.Immediate:
        address = (ushort) (PC + 1);
        System.Console.Write("#" + address.ToString("X2"));
        break;
      case AddressMode.Absolute:
        address = _memory.read16((ushort) (PC + 1));
        System.Console.Write("$" + address.ToString("X4"));
        break;
      case AddressMode.AbsoluteX:
        address = (ushort) (_memory.read16((ushort) (PC + 1)) + X);
        System.Console.Write("$" + address.ToString("X4") + " + X");
        break;
      case AddressMode.AbsoluteY:
        address = (ushort) (_memory.read16((ushort) (PC + 1)) + Y);
        System.Console.Write("$" + address.ToString("X4") + " + Y");
        break;
      case AddressMode.Accumulator:
        address = 0;
        System.Console.Write("A");
        break;
      case AddressMode.Relative:
        address = (ushort) (PC + (sbyte) _memory.read((ushort) (PC + 1)) + 2);
        break;
      case AddressMode.ZeroPage:
        address = _memory.read((ushort) (PC + 1));
        System.Console.Write("$" + address.ToString("X2"));
        break;
      case AddressMode.ZeroPageY:
        address = (ushort) (_memory.read((ushort) (PC+1)) + Y);
        System.Console.Write("$" + address.ToString("X2") + " + Y");
        break;
      case AddressMode.ZeroPageX:
        address = (ushort) (_memory.read((ushort) (PC+1)) + X);
        System.Console.Write("$" + address.ToString("X2") + " + X");
        break;
      default:
        throw new Exception("Address mode not implemented for 0x" + opCode.ToString("X2"));
    }
    
    System.Console.Write("\n");
    
    PC += (ushort) instructionSizes[opCode];
    
    instructions[opCode](mode, address);
  }

  private void setZn(byte value) {
    Z = value == 0;
    N = ((value>>7) & 1) == 1;
  }

  private bool isBitSet(byte value, int index) {
    return (value & (1 << index)) != 0;
  }
  
  private byte pullStack() {
    S++;
    byte data = _memory.read((ushort) (0x0100 | S));
    return data;
  }

  private void pushStack(byte data) {
    _memory.write((ushort) (0x100 | S), data);
    S--;
  }

  private ushort pullStack16() {
    byte lo = pullStack();
    byte hi = pullStack();
    return (ushort) ((hi << 8) | lo);
  }

  private void pushStack16(ushort data) {
    byte lo = (byte) (data & 0xFF);
    byte hi = (byte) ((data >> 8) & 0xFF);

    pushStack(hi);
    pushStack(lo);
  }

  // INSTRUCTIONS FOLLOW
  void ___(AddressMode mode, ushort address) {
    throw new Exception("OpCode is not implemented");
  }
  
  void rts(AddressMode mode, ushort address) {
    PC = (ushort) (pullStack16() + 1);
  }

  void jsr(AddressMode mode, ushort address) {
    pushStack16((ushort) (PC - 1));
    PC = address;
  }


  void bpl(AddressMode mode, ushort address) {
    PC = !N ? address : PC;
  }

  void bvc(AddressMode mode, ushort address) {
      PC = !V ? address : PC;
  }

  void bvs(AddressMode mode, ushort address) {
    PC = V ? address : PC;
  }

  void bit(AddressMode mode, ushort address) {
    byte data = _memory.read(address);
    N = isBitSet(data, 7);
    V = isBitSet(data, 6);
    Z = (data & A) == 0;
  }

  void bne(AddressMode mode, ushort address) {
    PC = !Z ? address : PC;
  }

  void beq(AddressMode mode, ushort address) {
    PC = Z ? address : PC;
  }

  void clc(AddressMode mode, ushort address) {
    C = false;
  }

  void bcc(AddressMode mode, ushort address) {
      PC = !C ? address : PC;
  }

  void bcs(AddressMode mode, ushort address) {
    PC = C ? address : PC;
  }

  void sec(AddressMode mode, ushort address) {
    C = true;
  }

  void nop(AddressMode mode, ushort address) {

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
    pushStack(A);
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
