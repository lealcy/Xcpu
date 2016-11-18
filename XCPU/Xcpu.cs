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
            InstrHalt = 0,
            InstrInt = 1,
            InstrSet = 2,
            InstrLoad = 3,
            InstrStore = 4,
            InstrShift = 100,
            InstrUnshift = 101,
            InstrJump = 200,
            InstrJz = 201,
            InstrJnz = 202,
            InstrTest = 300,
            InstrNoop = 1024,
            FlagHalt = 0,
            FlagException = 1,
            FlagZero = 0,
            FlagEqual = 0,
            FlagGreater = 0,
            FlagLess = 0;

        int[] memory = new int[128];
        public int pc = 0;
        BitArray flags = new BitArray(sizeof(int));
        int[] regs = new int[16];

        public void Run()
        {
            pc = 0;
            int reg;
            int addr;
            for (pc = 0; pc < memory.Length; pc++)
            {
                switch (memory[pc])
                {
                    case InstrHalt:
                        flags[FlagHalt] = true;
                        return;
                    case InstrInt:
                        Interrupt();
                        break;
                    case InstrNoop:
                        continue;
                    case InstrSet:
                        reg = memory[++pc];
                        int value = memory[++pc];
                        regs[reg] = value;
                        break;
                    case InstrLoad:
                        reg = memory[++pc];
                        addr = memory[++pc];
                        regs[reg] = memory[addr];
                        break;
                    case InstrStore:
                        reg = memory[++pc];
                        addr = memory[++pc];
                        memory[addr] = regs[reg];
                        break;
                    case InstrShift:
                        for (reg = regs.Length - 1; reg > 0; reg--)
                        {
                            regs[reg] = regs[reg - 1];
                        }
                        regs[0] = 0;
                        break;
                    case InstrUnshift:
                        for (reg = 0; reg < regs.Length - 1; reg++)
                        {
                            regs[reg] = regs[reg + 1];
                        }
                        regs[regs.Length - 1] = 0;
                        break;
                    case InstrJump:
                        reg = memory[++pc];
                        pc = regs[reg] - 1; // Conterfeit the pc auto inc in the next cycle.
                        break;
                    case InstrJz:
                        reg = memory[++pc];
                        if(flags[FlagZero]) {
                            pc = regs[reg] - 1; 
                        }
                        break;
                    case InstrJnz:
                        reg = memory[++pc];
                        if (!flags[FlagZero])
                        {
                            pc = regs[reg] - 1;
                        }
                        break;
                    case InstrTest:
                        int reg1 = memory[++pc];
                        int reg2 = memory[++pc];
                        flags[FlagZero] = reg1 == 0 ? true : false;
                        flags[FlagEqual] = reg1 == reg2 ? true : false;
                        flags[FlagGreater] = reg1 > reg2 ? true : false;
                        flags[FlagLess] = reg1 < reg2 ? true : false;
                        break;
                    default:
                        flags[FlagException] = true;
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
                        memory[pc++] = InstrHalt;
                        break;
                    case "int":
                        memory[pc++] = InstrInt;
                        break;
                    case "noop":
                        memory[pc++] = InstrNoop;
                        break;
                    case "set":
                        memory[pc++] = InstrSet;
                        if (ParseParam(opcode.Length, line, 2) == false) {
                            pc = cpc;
                            return false;
                        }
                        break;
                    case "load":
                        memory[pc++] = InstrLoad;
                        if (ParseParam(opcode.Length, line, 2) == false) {
                            pc = cpc;
                            return false;
                        }
                        break;
                    case "store":
                        memory[pc++] = InstrStore;
                        if (ParseParam(opcode.Length, line, 2) == false)
                        {
                            pc = cpc;
                            return false;
                        }
                        break;
                    case "shift":
                        memory[pc++] = InstrShift;
                        break;
                    case "unshift":
                        memory[pc++] = InstrUnshift;
                        break;
                    case "jump":
                        memory[pc++] = InstrJump;
                        if (ParseParam(opcode.Length, line, 1) == false) {
                            pc = cpc;
                            return false;
                        }
                        break;
                    case "jz":
                        memory[pc++] = InstrJz;
                        if (ParseParam(opcode.Length, line, 1) == false)
                        {
                            pc = cpc;
                            return false;
                        }
                        break;
                    case "test":
                        memory[pc++] = InstrTest;
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
                case 1: // Print
                    switch(regs[2]) {
                        case 0: // Print number
                            Console.Write(regs[3]);
                            break;
                        case 1: // Print char
                            Console.Write((char)regs[3]);
                            break;
                        case 2: // Print chars from memory until found '0'
                            while (regs[3] < memory.Length && memory[regs[3]] != 0) {
                                Console.Write((char)memory[regs[3]]);
                                regs[3]++;
                            }
                            if (regs[3] >= memory.Length) {
                                flags[FlagException] = true;
                            }
                            break;
                    }
                    break;
            }
        }

        public void PrintState()
        {
            Console.WriteLine("PC: {0}; REGS: {1}", pc, string.Join(", ", regs));
        }
    }
}
