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
        public static readonly int  MemorySize = 128;
        public static readonly int NumRegs = 16;

        int[] memory = new int[MemorySize];
        int pc = 0;
        BitArray flags = new BitArray(sizeof(int));

        int[] regs = new int[NumRegs];

        public void Run()
        {
            pc = 0;
            while (pc < memory.Length)
            {
                InstructionSet.Get(Next()).Execute(this);
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
                    while (regs[2] < memory.Length && memory[regs[2]] != 0) {
                        Console.Write((char)memory[regs[2]]);
                        regs[2]++;
                    }
                    if (regs[2] >= memory.Length) {
                        SetFlag(Flags.Exception, true);
                    }
                    break;
            }
        }

        private void SetFlag(object exception, bool v)
        {
            throw new NotImplementedException();
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

        public int GetPC()
        {
            return pc;
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
