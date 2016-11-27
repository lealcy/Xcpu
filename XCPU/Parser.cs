using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCPU
{
    class Parser
    {
        public static int[] Parse(string program)
        {
            List<int> code = new List<int>();
            int lineNumber = 0;

            foreach (string rawLine in program.Split('\n'))
            {
                lineNumber++;
                string line = rawLine.Trim();
                if (line.Length == 0 || line[0] == '#')
                {
                    continue;
                }
                string command = line.Split(' ')[0];
                Instruction instr = InstructionSet.GetInstructionByName(command);
                code.Add(instr.Opcode);
                if (instr.NumOperands == 0)
                {
                    continue;
                }
                if (command.Length == line.Length)
                {
                    throw new FormatException(string.Format("Line {0}: Invalid number of operands for {1}; expected {2}, received zero.", lineNumber, command, instr.NumOperands));
                }
                if (instr.Name == "cstr")
                {
                    string operand = line.Substring(command.Length).Trim();
                    if (!operand.StartsWith("\"") || !operand.EndsWith("\""))
                    {
                        throw new FormatException(string.Format("Line {0}: Invalid operand format; expected C string, received '{1}'.", lineNumber, operand));
                    }
                    foreach (int c in operand.Trim('"'))
                    {
                        code.Add(c);
                    }
                    code.Add(0);
                    continue;
                }
                string[] operands = line.Substring(command.Length + 1).Split(',');
                if (operands.Length != instr.NumOperands)
                {
                    throw new FormatException(string.Format("Line {0}: Invalid number of operands for {1}; expected {2}, received {3}", line, command, instr.NumOperands, operands.Length));
                }
                foreach (string operand in operands)
                {
                    string oper = operand.Trim();
                    if (oper.Length == 0)
                    {
                        throw new ArgumentException(string.Format("Line {0}: Empty operand for {1}", lineNumber, command));
                    }
                    int value;
                    if (int.TryParse(oper, out value))
                    {
                        code.Add(value);
                    }
                    else if (oper[0] == '\'')
                    {
                        if (oper.Length != 2)
                        {
                            throw new ArgumentException(string.Format("Line {0}: Invalid char to int conversion for operand {1}", lineNumber, oper));
                        }
                        code.Add(oper[1]);
                    }
                    else
                    {
                        throw new ArgumentException(string.Format("Line {0}: Invalid argument for {1}", lineNumber, command));
                    }
                }
            }
            return code.ToArray();
        }

        public static string Disassembly(int[] code)
        {
            string program = "";
            Instruction instr;
            int i = 0;
            while (i < code.Length)
            {
                instr = InstructionSet.Get(code[i]);
                program += instr.Name + " ";
                if (instr.NumOperands > 0)
                {
                    if (instr.Name == "cstr")
                    {
                        string cstr = string.Join("", code.Skip(i + 1).TakeWhile((ch, index) => ch != 0).Select(ch => (char)ch).ToArray());
                        program += "\"" + cstr + "\"";
                        i += cstr.Length + 1;
                    }
                    else
                    {
                        program += string.Join(", ", code.Skip(i + 1).Take(instr.NumOperands).ToArray());
                        i += instr.NumOperands;
                    }
                }
                i++;
                program += "\n";
            }

            return program;
        }
    }
}
