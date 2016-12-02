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
                new Instruction(0, "halt", 0, "", (cpu) => { cpu.SetFlag(F.Halt, true); }),
                new Instruction(1, "int", 1, "i", (cpu) => { cpu.Interrupt(cpu.Next()); }),
                new Instruction(2, "noop", 0, "", (cpu) => { /* do nothing */ }),
                new Instruction(3, "set", 2, "ri", (cpu) => {
                    int reg = cpu.Next();
                    int value = cpu.Next();
                    cpu.SetR(reg, value);
                }),
                new Instruction(4, "load", 2, "ra", (cpu) => {
                    int reg = cpu.Next();
                    int address = cpu.Next();
                    cpu.SetR(reg, cpu.GetAddress(address));
                }),
                new Instruction(5, "store", 2, "ra", (cpu) => {
                    int reg = cpu.Next();
                    int address = cpu.Next();
                    cpu.SetAddress(address, cpu.GetR(reg));
                }),
                new Instruction(6, "move", 2, "rr", (cpu) => {
                    int r1 = cpu.Next();
                    int r2 = cpu.Next();
                    cpu.Move(r1, r2);
                }),
                new Instruction(7, "push", 1, "r", (cpu) => {
                    int r = cpu.Next();
                    cpu.SetAddress(cpu.SP--, cpu.GetR(r));
                }),
                new Instruction(8, "pop", 1, "r", (cpu) => {
                    cpu.SetR(cpu.Next(), cpu.GetAddress(++cpu.SP));
                }),
                new Instruction(12, "inc", 1, "r", (cpu) => {
                    cpu.Inc(cpu.Next());
                }),
                new Instruction(13, "dec", 1, "r", (cpu) => {
                    cpu.Dec(cpu.Next());
                }),
                new Instruction(14, "cstr", 1, "s", (cpu) => {
                    while (cpu.Next() != 0) ;
                }),
                new Instruction(15, "test", 2, "rr", (cpu) => {
                    int r1 = cpu.Next();
                    int r2 = cpu.Next();
                    cpu.SetFlag(F.Zero, cpu.GetR(r1) == 0 ? true : false);
                    cpu.SetFlag(F.Equal, cpu.GetR(r1) == cpu.GetR(r2) ? true : false);
                    cpu.SetFlag(F.Greater, cpu.GetR(r1) > cpu.GetR(r2) ? true : false);
                    cpu.SetFlag(F.Less, cpu.GetR(r1) < cpu.GetR(r2) ? true : false);
                }),
                new Instruction(16, "jump", 1, "a", (cpu) => {
                    cpu.XP = cpu.Next();
                }),
                new Instruction(17, "jz", 1, "a", (cpu) => {
                    int address = cpu.Next();
                    if (cpu.GetFlag(F.Zero))
                    {
                        cpu.XP = address;
                    }
                }),
                new Instruction(18, "jnz", 1, "a", (cpu) => {
                    int address = cpu.Next();
                    if (!cpu.GetFlag(F.Zero))
                    {
                        cpu.XP = address;
                    }
                }),
                new Instruction(19, "je", 1, "a", (cpu) => {
                    int address = cpu.Next();
                    if (cpu.GetFlag(F.Equal))
                    {
                        cpu.XP = address;
                    }
                }),
                new Instruction(20, "jne", 1, "a", (cpu) => {
                    int address = cpu.Next();
                    if (!cpu.GetFlag(F.Equal))
                    {
                        cpu.XP = address;
                    }
                }),
                new Instruction(30, "call", 1, "a", (cpu) => {
                    int address = cpu.Next();
                    cpu.RP = cpu.XP;
                    cpu.XP = address;
                }),
                new Instruction(31, "ret", 0, "", (cpu) => {
                    cpu.XP = cpu.RP;
                }),

                new Instruction(32, "settrap", 1, "a", (cpu) => {
                    cpu.EXP = cpu.Next();
                }),
                new Instruction(33, "raise", 0, "", (cpu) => {
                    cpu.RaiseException();
                }),
                new Instruction(34, "resume", 0, "", (cpu) => {
                    cpu.Resume();
                }),
                //new Instruction(0, "", 0, "", (cpu) => { }),
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
            throw new ArgumentOutOfRangeException("opcode", string.Format("Invalid opcode: {0}", opcode));
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
        public string Format;
        public OperationMethod Execute;

        public Instruction(int opcode, string name, int numOperands, string format, OperationMethod operation)
        {
            Opcode = opcode;
            Name = name;
            NumOperands = numOperands;
            Format = format;
            Execute = operation;
        }
    }
}
