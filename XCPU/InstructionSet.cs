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
                new Instruction(0, "halt", 0, (cpu) => { cpu.SetFlag(F.Halt, true); }),
                new Instruction(1, "int", 0, (cpu) => { cpu.SetFlag(F.Interrupt, true); }),
                new Instruction(2, "noop", 0, (cpu) => { /* do nothing */ }),
                new Instruction(3, "set", 2, (cpu) => {
                    int reg = cpu.Next();
                    int value = cpu.Next();
                    cpu.SetR((R)reg, value);
                }),
                new Instruction(4, "load", 2, (cpu) => {
                    int reg = cpu.Next();
                    int address = cpu.Next();
                    cpu.SetR((R)reg, cpu.GetAddress(address));
                }),
                new Instruction(5, "store", 2, (cpu) => {
                    int reg = cpu.Next();
                    int address = cpu.Next();
                    cpu.SetAddress(address, cpu.GetR((R)reg));
                }),
                new Instruction(8, "test", 2, (cpu) => {
                    R r1 = (R)cpu.Next();
                    R r2 = (R)cpu.Next();
                    cpu.SetFlag(F.Zero, cpu.GetR(r1) == 0 ? true : false);
                    cpu.SetFlag(F.Equal, cpu.GetR(r1) == cpu.GetR(r2) ? true : false);
                    cpu.SetFlag(F.Greater, cpu.GetR(r1) > cpu.GetR(r2) ? true : false);
                    cpu.SetFlag(F.Less, cpu.GetR(r1) < cpu.GetR(r2) ? true : false);
                }),
                new Instruction(16, "jump", 1, (cpu) => {
                    int address = cpu.Next();
                    cpu.SetR(R.XP, address);
                }),
                new Instruction(17, "jz", 1, (cpu) => {
                    int address = cpu.Next();
                    if (cpu.GetFlag(F.Zero))
                    {
                        cpu.SetR(R.XP, address);
                    }
                }),
                new Instruction(18, "jnz", 1, (cpu) => {
                    int address = cpu.Next();
                    if (!cpu.GetFlag(F.Zero))
                    {
                        cpu.SetR(R.XP, address);
                    }
                }),
                new Instruction(32, "settrap", 1, (cpu) => {
                    int address = cpu.Next();
                    cpu.SetR(R.EXP, address);
                }),
                new Instruction(33, "raise", 0, (cpu) => {
                    cpu.RaiseException();
                }),
                new Instruction(34, "resume", 0, (cpu) => {
                    cpu.Resume();
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
