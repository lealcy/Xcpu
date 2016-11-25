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
        public static readonly int IntegerRange = 8 * sizeof(int);
        public static readonly int MemorySize = (int)Math.Pow(2, IntegerRange > 22 ? 22 : IntegerRange); // Up to 4MB.
        public static readonly int NumRegs = IntegerRange;

        int[] memory = new int[MemorySize];
        int ip = 0; // Instruction Address
        int pc = 0; // Program Counter
        int xt = 0; // Exception Trap Address
        int xs = 0; // Exception Source Address
        BitArray flags = new BitArray(IntegerRange);
        int[] regs = new int[NumRegs];

        public void Run()
        {
            pc = 0;
            while (pc < memory.Length)
            {
                ip = pc;
                try
                {
                    InstructionSet.Get(Next()).Execute(this);
                } catch (ArgumentOutOfRangeException e)
                {
                    Console.WriteLine("[PC: {0}, IP: {1}: {2}", pc, ip, e.Message);
                }
                if (GetFlag(Flags.Interrupt))
                {
                    Interrupt();
                }
            }
        }

        public void Write(int[] source, int startAddress)
        {
            source.CopyTo(memory, startAddress);
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
                    while (GetAddress(regs[2]) != 0) {
                        Console.Write((char)GetAddress(regs[2]));
                        regs[2]++;
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
            flags[(int)flag] = value;
        }

        public bool GetFlag(Flags flag)
        {
            return flags[(int)flag];
        }

        public int GetAddress(int address)
        {
            if (address > memory.Length - 1)
            {
                RaiseException();
            }
            return memory[address];
        }

        public void SetAddress(int address, int value)
        {
            if (address > memory.Length - 1)
            {
                RaiseException();
            }
            memory[address] = value;
        }

        public int Next()
        {
            if (pc > memory.Length - 1)
            {
                RaiseException();
            }
            return memory[pc++];
        }

        public int GetPC()
        {
            return pc;
        }

        public void SetPC(int address)
        {
            pc = address;
        }

        public void RaiseException()
        {
            xs = ip;
            SetPC(xt);
        }

        public void SetExceptionTrap(int address)
        {
            xt = address;
        }

        public int GetExceptionTrap()
        {
            return xt;
        }

        public void ResumeFromException()
        {
            SetPC(xs);
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
        Interrupt = 6,
    }
}
