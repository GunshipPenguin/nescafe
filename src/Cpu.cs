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
      ___, ora, ___, ___, ___, ora, asl, ___, php, ora, asl, ___, ___, ora, asl, ___, // 0
      bpl, ora, ___, ___, ___, ora, asl, ___, clc, ora, ___, ___, ___, ora, asl, ___, // 1
      jsr, and, ___, ___, bit, and, rol, ___, plp, and, rol, ___, bit, and, rol, ___, // 2
      bmi, and, ___, ___, ___, and, rol, ___, sec, and, ___, ___, ___, and, rol, ___, // 3
      ___, eor, ___, ___, ___, eor, lsr, ___, pha, eor, lsr, ___, jmp, eor, lsr, ___, // 4
      bvc, eor, ___, ___, ___, eor, lsr, ___, ___, eor, ___, ___, ___, eor, lsr, ___, // 5
      rts, adc, ___, ___, ___, adc, ___, ___, pla, adc, ___, ___, jmp, adc, ___, ___, // 6
      bvs, adc, ___, ___, ___, adc, ___, ___, sei, adc, ___, ___, ___, adc, ___, ___, // 7
      ___, sta, ___, ___, sty, sta, stx, ___, dey, ___, ___, ___, sty, sta, stx, ___, // 8
      bcc, sta, ___, ___, sty, sta, stx, ___, ___, sta, ___, ___, ___, sta, ___, ___, // 9
      ldy, lda, ldx, ___, ldy, lda, ldx, ___, ___, lda, ___, ___, ldy, lda, ldx, ___, // A
      bcs, lda, ___, ___, ldy, lda, ldx, ___, clv, lda, ___, ___, ldy, lda, ldx, ___, // B
      cpy, cmp, ___, ___, cpy, cmp, ___, ___, iny, cmp, dex, ___, cpy, cmp, ___, ___, // C
      bne, cmp, ___, ___, ___, cmp, ___, ___, cld, cmp, ___, ___, ___, cmp, ___, ___, // D
      cpx, sbc, ___, ___, cpx, sbc, inc, ___, inx, sbc, nop, ___, cpx, sbc, inc, ___, // E
      beq, sbc, ___, ___, ___, sbc, inc, ___, sed, sbc, ___, ___, ___, sbc, inc, ___  // F
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

  private byte getStatusFlags() {
    byte flags = 0;

    if (C) { // Carry flag, bit 0
      flags |= (byte) (1 << 0);
    }
    if (Z) { // Zero flag, bit 1
      flags |= (byte) (1 << 1);
    }
    if (I) { // Interrupt disable flag, bit 2
      flags |= (byte) (1 << 2);
    }
    if (D) { // Decimal mode flag, bit 3
      flags |= (byte) (1 << 3);
    }
    if (B) { // Break mode, bit 4
      flags |= (byte) (1 << 4);
    }
    
    flags |= (byte) (1 << 5); // Bit 5, always set

    if (V) { // Overflow flag, bit 6
      flags |= (byte) (1 << 6);
    }
    if (N) { // Negative flag, bit 7
      flags |= (byte) (1 << 7);
    }

    return flags;
  }

  private void setProcessorFlags(byte flags) {
    C = isBitSet(flags, 0);
    Z = isBitSet(flags, 1);
    I = isBitSet(flags, 2);
    D = isBitSet(flags, 3);
    B = isBitSet(flags, 4);
    V = isBitSet(flags, 6);
    N = isBitSet(flags, 7);
  }

  // INSTRUCTIONS FOLLOW
  void ___(AddressMode mode, ushort address) {
    throw new Exception("OpCode is not implemented");
  }

  void dex(AddressMode mode, ushort address) {
    X--;
  }
  
  void dey(AddressMode mode, ushort address) {
    Y--;
  }

  void inx(AddressMode mode, ushort address) {
    X++;
  }

  void iny(AddressMode mode, ushort address) {
    Y++;
  }

  void sty(AddressMode mode, ushort address) {
    _memory.write(address, Y);
  }

  void cpx(AddressMode mode, ushort address) {
    byte data = _memory.read(address);
    setZn((byte) (X - data));
    C = X >= data;
  }

  void cpy(AddressMode mode, ushort address) {
    byte data = _memory.read(address);
    setZn((byte) (Y - data));
    C = Y >= data;
  }

  void sbc(AddressMode mode, ushort address) {
    byte data = _memory.read(address);
    int notCarry = (!C ? 1 : 0);

    byte result = (byte) (A - data - notCarry);
    setZn(result);

    // If an overflow occurs (result actually less than 0)
    // the carry flag is cleared
    C = (A - data - notCarry) >= 0 ? true : false;

    V = isBitSet((byte) ~(A ^ data), 7) && !isBitSet((byte) (A ^ result), 7);

    A = result;
  }

  void adc(AddressMode mode, ushort address) {
    byte data = _memory.read(address);
    int carry = (C ? 1 : 0);

    byte sum = (byte) (A + data + carry);
    setZn(A);

    C = (A + data + carry) > 0xFF;

    // Sign bit is wrong if sign bit of operands is same
    // and sign bit of result is different
    // if <A and data> differ in sign and <A and sum> have the same sign
    // https://stackoverflow.com/questions/29193303/6502-emulation-proper-way-to-implement-adc-and-sbc
    V = isBitSet((byte) ~(A ^ data), 7) && !isBitSet((byte) (A ^ sum), 7);

    A = sum;
  }

  void eor(AddressMode mode, ushort address) {
    byte data = _memory.read(address);
    A |= data;
  }

  void clv(AddressMode mode, ushort address) {
    V = false;
  }

  void bmi(AddressMode mode, ushort address) {
    PC = Z ? address : PC;
  }

  void plp(AddressMode mode, ushort address) {
    setProcessorFlags(pullStack());
  }

  void cld(AddressMode mode, ushort address) {
    D = false;
  }

  void cmp(AddressMode mode, ushort address) {
    byte data = _memory.read(address);
    C = A > data;
    setZn((byte) (A - data));
  }

  void and(AddressMode mode, ushort address) {
    byte data = _memory.read(address);
    A &= data;
    setZn(A);
  }

  void pla(AddressMode mode, ushort address) {
    A = pullStack();
  }

  void php(AddressMode mode, ushort address) {
    pushStack(getStatusFlags());
  }

  void sed(AddressMode mode, ushort address) {
    D = true;
  }
  
  void sei(AddressMode mode, ushort address) {
    I = true;
  }

  void inc(AddressMode mode, ushort address) {
    byte data = _memory.read(address);
    byte newData = (byte) (data + 1);
    _memory.write(address, newData);
    setZn(newData);
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

  void ldy(AddressMode mode, ushort address) {
    Y = _memory.read(address);
    setZn(Y);
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
