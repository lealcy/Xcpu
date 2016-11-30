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
        int[] regs = new int[32];

        public void Run()
        {
            SetR(R.XP, 0);
            SetR(R.IP, 0);
            SetR(R.SP, MemorySize - 1);
            Continue();
        }

        public void Continue()
        {
            while (GetFlag(F.Halt) == false)
            {
                Move(R.IP, R.XP);
                try
                {
                    InstructionSet.Get(Next()).Execute(this);
                }
                catch (ArgumentOutOfRangeException e)
                {
                    Console.WriteLine("[XP: {0,5:D5}, IP: {1,5:D5}: {2}", GetR(R.XP), GetR(R.IP), e.Message);
                }
                if (GetFlag(F.Interrupt))
                {
                    Interrupt();
                }
            }
            SetFlag(F.Halt, false);
        }

        public void Write(int[] source, int startAddress)
        {
            source.CopyTo(memory, startAddress);
        }

        public void Interrupt()
        {
            switch(GetR(R.R1)) {
                case 10: // Print integer
                    Console.Write(GetR(R.R2));
                    break;
                case 11: // Print char
                    Console.Write((char)GetR(R.R2));
                    break;
                case 12: // Print chars from memory until found '0'
                    char ch;
                    Inc(R.R2); // Jump the 'cstr' opcode.
                    while ((ch = (char)GetAddress(Inc(R.R2))) != 0) {
                        Console.Write(ch);
                    }
                    break;
            }
            SetFlag(F.Interrupt, false);
        }

        public int[] Read(int start, int end)
        {
            return memory.Skip(start).Take(end - start).ToArray();
        }

        public void PrintState()
        {
            Console.WriteLine("[XP: {0,5:D5}, IP: {1,5:D5}]", GetR(R.XP), GetR(R.IP));
        }

        public int GetR(R r)
        {
            return regs[(int)r];
        }

        public void SetR(R r, int value)
        {
            regs[(int)r] = value;
        }

        public void Move(R rd, R rs)
        {
            SetR(rd, GetR(rs));
        }

        public int Inc(R r)
        {
            return regs[(int)r]++;
        }

        public int Dec(R r)
        {
            return regs[(int)r]--;
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
            if (GetR(R.XP) >= MemorySize)
            {
                SetFlag(F.InvalidAddress, true);
                RaiseException();
            }
            return memory[Inc(R.XP)];
        }

        public void RaiseException()
        {
            Move(R.EIP, R.IP);
            Move(R.XP, R.EXP);
        }

        public void Resume()
        {
            Move(R.XP, R.EIP);
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

    // Registers
    enum R
    {
        R0 = 0, R1 = 1, R2 = 2, R3 = 3, R4 = 4, R5 = 5, R6 = 6, R7 = 7, R8 = 8, R9 = 9, // General purpose registers.
        XP = 16, // eXecution Pointer - Current memory address read by the CPU.
        IP = 17, // Instruction Pointer - Current instruction memory address been executed by the CPU.
        SP = 18, // Stack Pointer
        EXP = 29, // Exception eXecution pointer - Memory address to jump in case of exception.
        EIP = 30, // Exception Instruction pointer - Memory address of the Instruction been executed when the exception raise.
    }
}
