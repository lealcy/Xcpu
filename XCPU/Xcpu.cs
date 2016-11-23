using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Collections;

namespace XCPU
{
    class Xcpu
    {
        public const int
            I_halt = 0,
            I_int = 1,
            I_set = 2,
            I_load = 3,
            I_store = 4,
            I_shift = 100,
            I_unshift = 101,
            I_jump = 200,
            I_jz = 201,
            I_jnz = 202,
            I_test = 300,
            I_noop = 1024,
            F_halt = 0,
            F_exception = 1,
            F_zero = 0,
            F_equal = 0,
            F_greater = 0,
            F_less = 0;

        public static readonly int  MemorySize = 128;
        public static readonly int NumRegs = 16;

        int[] memory = new int[MemorySize];
        int pc = 0;
        BitArray Flags = new BitArray(sizeof(int));
        int[] regs = new int[NumRegs];

        public void Run()
        {
            pc = 0;
            int reg;
            int addr;
            for (pc = 0; pc < memory.Length; pc++)
            {
                switch (memory[pc])
                {
                    case I_halt:
                        Flags[F_halt] = true;
                        return;
                    case I_int:
                        Interrupt();
                        break;
                    case I_noop:
                        continue;
                    case I_set:
                        reg = memory[++pc];
                        int value = memory[++pc];
                        regs[reg] = value;
                        break;
                    case I_load:
                        reg = memory[++pc];
                        addr = memory[++pc];
                        regs[reg] = memory[addr];
                        break;
                    case I_store:
                        reg = memory[++pc];
                        addr = memory[++pc];
                        memory[addr] = regs[reg];
                        break;
                    case I_shift:
                        for (reg = regs.Length - 1; reg > 0; reg--)
                        {
                            regs[reg] = regs[reg - 1];
                        }
                        regs[0] = 0;
                        break;
                    case I_unshift:
                        for (reg = 0; reg < regs.Length - 1; reg++)
                        {
                            regs[reg] = regs[reg + 1];
                        }
                        regs[regs.Length - 1] = 0;
                        break;
                    case I_jump:
                        reg = memory[++pc];
                        pc = regs[reg] - 1; // Conterfeit the pc auto inc in the next cycle.
                        break;
                    case I_jz:
                        reg = memory[++pc];
                        if(Flags[F_zero]) {
                            pc = regs[reg] - 1; 
                        }
                        break;
                    case I_jnz:
                        reg = memory[++pc];
                        if (!Flags[F_zero])
                        {
                            pc = regs[reg] - 1;
                        }
                        break;
                    case I_test:
                        int reg1 = memory[++pc];
                        int reg2 = memory[++pc];
                        Flags[F_zero] = reg1 == 0 ? true : false;
                        Flags[F_equal] = reg1 == reg2 ? true : false;
                        Flags[F_greater] = reg1 > reg2 ? true : false;
                        Flags[F_less] = reg1 < reg2 ? true : false;
                        break;
                    default:
                        Flags[F_exception] = true;
                        break;
                }
            }
        }

        public void Parse(string code)
        {
            string[] lines = code.Split('\n');

            int lineNumber = 1;
            foreach (string line in lines)
            {
                if (!ParseLine(line)) {
                    Console.WriteLine("Parse Error in line {0}: {1}", lineNumber, line);
                }
                lineNumber++;
            }

        }

        public bool ParseLine(string line)
        {
            line = line.Trim();
            if (line.Length == 0 || line[0] == '#')
            {
                return true;
            }
            Match m = Regex.Match(line, @"^([a-zA-Z]\w*)");
            if (m.Success)
            {
                string opcode = m.Value.ToLower();
                int cpc = pc; // Saves the current pc for unwind in case of error.
                switch (opcode)
                {
                    case "halt":
                        memory[pc++] = I_halt;
                        break;
                    case "int":
                        memory[pc++] = I_int;
                        break;
                    case "noop":
                        memory[pc++] = I_noop;
                        break;
                    case "set":
                        memory[pc++] = I_set;
                        if (ParseParam(opcode.Length, line, 2) == false) {
                            pc = cpc;
                            return false;
                        }
                        break;
                    case "load":
                        memory[pc++] = I_load;
                        if (ParseParam(opcode.Length, line, 2) == false) {
                            pc = cpc;
                            return false;
                        }
                        break;
                    case "store":
                        memory[pc++] = I_store;
                        if (ParseParam(opcode.Length, line, 2) == false)
                        {
                            pc = cpc;
                            return false;
                        }
                        break;
                    case "shift":
                        memory[pc++] = I_shift;
                        break;
                    case "unshift":
                        memory[pc++] = I_unshift;
                        break;
                    case "jump":
                        memory[pc++] = I_jump;
                        if (ParseParam(opcode.Length, line, 1) == false) {
                            pc = cpc;
                            return false;
                        }
                        break;
                    case "jz":
                        memory[pc++] = I_jz;
                        if (ParseParam(opcode.Length, line, 1) == false)
                        {
                            pc = cpc;
                            return false;
                        }
                        break;
                    case "test":
                        memory[pc++] = I_test;
                        if (ParseParam(opcode.Length, line, 2) == false) {
                            pc = cpc;
                            return false;
                        }
                        break;
                    default:
                        Console.WriteLine("Invalid opcode '{0}'", opcode);
                        return false;
                }
                return true;
            }
            Console.WriteLine("No match for '{0}'!", line);
            return false;
        }

        public bool ParseParam(int start, string line, int n) {
            string[] parms = line.Substring(start).Split(',');
            if (parms.Length != n) {
                Console.WriteLine("Invalid number of parameters.");
                return false;
            }
            foreach (string param in parms) {
                string p = param.Trim();
                if (p.Length == 0) {
                    Console.WriteLine("Empty parameter.");
                    return false;
                }
                int result;
                if (int.TryParse(p, out result)) {
                    memory[pc++] = result;
                } else if (p[0] == '\'') {
                    if (p.Length != 2) {
                        Console.WriteLine("Invalid char conversion parameter.");
                        return false;
                    }
                    memory[pc++] = (int)p[1];
                } else {
                    Console.WriteLine("Invalid parameter '{0}'", p);
                    return false;
                }
            }
            return true;
        }

        public void Interrupt()
        {
            switch(regs[1]) {
                case 10: // Print integer
                    Console.Write(regs[2]);
                    break;
                case 11: // Print char
                    Console.Write((char)regs[2]);
                    break;
                case 12: // Print chars from memory until found '0'
                    while (regs[2] < memory.Length && memory[regs[2]] != 0) {
                        Console.Write((char)memory[regs[2]]);
                        regs[2]++;
                    }
                    if (regs[2] >= memory.Length) {
                        Flags[F_exception] = true;
                    }
                    break;
            }
        }

        public void PrintState()
        {
            Console.WriteLine("PC: {0}; REGS: {1}", pc, string.Join(", ", regs));
        }

        public int GetReg(int reg)
        {
            return regs[reg];
        }

        public void SetReg(int reg, int value)
        {
            regs[reg] = value;
        }

        public void SetFlag(Flags flag, bool value)
        {
            Flags[(int)flag] = value;
        }

        public bool GetFlag(Flags flag)
        {
            return Flags[(int)flag];
        }

        public int GetAddress(int address)
        {
            return memory[address];
        }

        public void SetAddress(int address, int value)
        {
            memory[address] = value;
        }

        public int Next()
        {
            return memory[pc++];
        }

        public void SetPC(int address)
        {
            pc = address;
        }
    }

    enum Flags
    {
        Halt = 0,
        Exception = 1,
        Zero = 2,
        Equal = 3,
        Greater = 4,
        Less = 5,
    }
}
