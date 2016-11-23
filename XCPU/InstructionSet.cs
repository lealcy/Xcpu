using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCPU
{
    class InstructionSet
    {
        public static Instruction[] Instructions = new Instruction[] {
                new Instruction(0, "halt", 0, (cpu) => { cpu.SetFlag(Flags.Halt, true); }),
                new Instruction(1, "int", 0, (cpu) => { cpu.Interrupt(); }),
                new Instruction(2, "noop", 0, (cpu) => { /* do nothing */ }),
                new Instruction(3, "set", 2, (cpu) => {
                    int reg = cpu.Next();
                    int value = cpu.Next();
                    cpu.SetReg(reg, value);
                }),
                new Instruction(4, "load", 2, (cpu) => {
                    int reg = cpu.Next();
                    int address = cpu.Next();
                    cpu.SetReg(reg, cpu.GetAddress(address));
                }),
                new Instruction(5, "store", 2, (cpu) => {
                    int reg = cpu.Next();
                    int address = cpu.Next();
                    cpu.SetAddress(address, cpu.GetReg(reg));
                }),
                new Instruction(5, "", 0, (cpu) => { }),
                new Instruction(6, "shift", 0, (cpu) => {
                    for (int i = Xcpu.NumRegs - 1; i > 0; i--)
                    {
                        cpu.SetReg(i, cpu.GetReg(i - 1));
                    }
                }),
                new Instruction(7, "unshift", 0, (cpu) => {
                    for (int i = 0; i < Xcpu.NumRegs - 1; i++)
                    {
                        cpu.SetReg(i, cpu.GetReg(i + 1));
                    }
                    cpu.SetReg(Xcpu.NumRegs - 1, 0);
                }),
                new Instruction(8, "test", 2, (cpu) => {
                    int reg1 = cpu.Next();
                    int reg2 = cpu.Next();
                    cpu.SetFlag(Flags.Zero, reg1 == 0 ? true : false);
                    cpu.SetFlag(Flags.Equal, reg1 == reg2 ? true : false);
                    cpu.SetFlag(Flags.Greater, reg1 > reg2 ? true : false);
                    cpu.SetFlag(Flags.Less, reg1 < reg2 ? true : false);
                }),
                new Instruction(16, "jump", 1, (cpu) => {
                    int address = cpu.Next();
                    cpu.SetPC(address);
                }),
                new Instruction(17, "jz", 1, (cpu) => {
                    int address = cpu.Next();
                    if (cpu.GetFlag(Flags.Zero))
                    {
                        cpu.SetPC(address);
                    }
                }),
                new Instruction(18, "jnz", 1, (cpu) => {
                    int address = cpu.Next();
                    if (!cpu.GetFlag(Flags.Zero))
                    {
                        cpu.SetPC(address);
                    }
                }),
                //new Instruction(0, "", 0, (cpu) => { }),
            };


        public static Instruction Get(int opcode)
        {
            foreach (Instruction i in Instructions)
            {
                if (i.Opcode == opcode)
                {
                    return i;
                }
            }
            throw new ArgumentOutOfRangeException("opcode", "Invalid opcode");
        }

        public static bool HasName(string name)
        {
            foreach (Instruction i in Instructions)
            {
                if (i.Name == name)
                {
                    return true;
                }
            }
            return false;
        }

        public static Instruction GetInstructionByName(string name)
        {
            foreach (Instruction i in Instructions)
            {
                if (i.Name == name)
                {
                    return i;
                }
            }
            throw new KeyNotFoundException(string.Format("Instruction '{0}' not found.", name));
        }
    }

    class Instruction
    {
        public delegate void OperationMethod(Xcpu cpu);
        public int Opcode;
        public string Name;
        public int NumOperands;
        public OperationMethod Execute;

        public Instruction(int opcode, string name, int numOperands, OperationMethod operation)
        {
            Opcode = opcode;
            Name = name;
            NumOperands = numOperands;
            Execute = operation;
        }
    }
}
