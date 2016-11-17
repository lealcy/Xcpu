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
            InstrNoop = 1,
            InstrSet = 2,
            FlagHalt = 0,
            FlagException = 1;

        int[] memory = new int[128];
        int pc = 0;
        BitArray flags = new BitArray(sizeof(int));
        int[] regs = new int[16];

        public Run()
        {
            pc = 0;
            for (pc = 0; pc < memory.Length; pc++)
            {
                switch (memory[pc])
                {
                    case InstrHalt:
                        flags[FlagHalt] = true;
                        return;
                    case InstrNoop:
                        continue;
                    case InstrSet:
                        regs[memory[++pc]] = memory[++pc];
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
                    default:
                        Console.WriteLine("Invalid opcode '{0}'", opcode);
                        return false;
                        break;
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
    }
}
