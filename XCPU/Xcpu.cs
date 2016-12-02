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
        public const int IntegerRange = 8 * sizeof(int);
        public const int MemorySize = 0x400000 / sizeof(int);
        public const int NumRegs = IntegerRange;

        int[] memory = new int[MemorySize];
        BitArray flags = new BitArray(32);

        // Set up registers
        int[] r = new int[32];

        // General purpose registers.
        public int R0 { get { return r[0]; } set { r[0] = value; } }
        public int R1 { get { return r[1]; } set { r[1] = value; } }
        public int R2 { get { return r[2]; } set { r[2] = value; } }
        public int R3 { get { return r[3]; } set { r[3] = value; } }
        public int R4 { get { return r[4]; } set { r[4] = value; } }
        public int R5 { get { return r[5]; } set { r[5] = value; } }
        public int R6 { get { return r[6]; } set { r[6] = value; } }
        public int R7 { get { return r[7]; } set { r[7] = value; } }

        public int XP { get { return r[15]; } set { r[15] = value; } } // eXecution Pointer - Current memory address read by the CPU.
        public int IP { get { return r[16]; } set { r[16] = value; } } // Instruction Pointer - Current instruction memory address been executed by the CPU.
        public int SP { get { return r[17]; } set { r[17] = value; } } // Stack Pointer
        public int RP { get { return r[25]; } set { r[25] = value; } } // Return Pointer for calls
        public int EIP { get { return r[28]; } set { r[28] = value; } } // Exception Instruction pointer - Memory address of the Instruction been executed when the exception raise.*/
        public int EXP { get { return r[29]; } set { r[29] = value; } } // Exception eXecution pointer - Memory address to jump in case of exception.

        public void Run()
        {
            XP = 0;
            IP = 0;
            SP = MemorySize - 1;
            RP = 0;
            Continue();
        }

        public void Continue()
        {
            while (GetFlag(F.Halt) == false)
            {
                IP = XP;
                try
                {
                    InstructionSet.Get(Next()).Execute(this);
                }
                catch (ArgumentOutOfRangeException e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            SetFlag(F.Halt, false);
        }

        public void Write(int[] source, int startAddress)
        {
            source.CopyTo(memory, startAddress);
        }

        public void Interrupt(int value)
        {
            switch(value) {
                case 10: // Print integer
                    Console.Write(R1);
                    break;
                case 11: // Print char
                    Console.Write((char)R1);
                    break;
                case 12: // Print chars from memory until found '0'
                    char ch;
                    R1++; ; // Jump the 'cstr' opcode.
                    while ((ch = (char)GetAddress(R1++)) != 0) {
                        Console.Write(ch);
                    }
                    break;
            }
        }

        public int[] Read(int start, int end)
        {
            return memory.Skip(start).Take(end - start).ToArray();
        }

        public int GetR(int reg)
        {
            return r[reg];
        }

        public void SetR(int reg, int value)
        {
            r[reg] = value;
        }

        public void Move(int rd, int rs)
        {
            SetR(rd, GetR(rs));
        }

        public int Inc(int reg)
        {
            return r[reg]++;
        }

        public int Dec(int reg)
        {
            return r[reg]--;
        }

        public void SetFlag(F f, bool value)
        {
            flags[(int)f] = value;
        }

        public bool GetFlag(F f)
        {
            return flags[(int)f];
        }

        public int GetAddress(int address)
        {
            if (address >= MemorySize)
            {
                SetFlag(F.InvalidAddress, true);
                RaiseException();
            }
            return memory[address];
        }

        public void SetAddress(int address, int value)
        {
            if (address >= MemorySize)
            {
                SetFlag(F.InvalidAddress, true);
                RaiseException();
            }
            memory[address] = value;
        }

        public int Next()
        {
            if (XP >= MemorySize)
            {
                SetFlag(F.InvalidAddress, true);
                RaiseException();
            }
            return memory[XP++];
        }

        public void RaiseException()
        {
            EIP = IP;
            XP = EXP;
        }

        public void Resume()
        {
            XP = EIP;
        }
    }

    // Flags
    enum F
    {
        Halt = 0,
        Exception = 1,
        Zero = 2,
        Equal = 3,
        Greater = 4,
        Less = 5,
        Interrupt = 6,
        InvalidAddress = 7,
    }
}
