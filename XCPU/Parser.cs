using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCPU
{
    class Parser
    {
        static Dictionary<string, int> labels = new Dictionary<string, int>();

        public static int[] Parse(string program)
        {
            var code = new List<int>();
            var lines = program.Split('\n');
            int xp = 0;
            int lineNumber;

            // Search for labels
            for (lineNumber = 0; lineNumber < lines.Length; lineNumber++)
            {
                string line = lines[lineNumber].Trim();
                if (line.Length == 0 || line[0] == '#')
                {
                    continue;
                }
                if (line[0] == '.')
                {
                    // Line starts with a label
                    string label = line.Split(' ')[0].TrimEnd(':');
                    labels[label] = xp;
                    // Strips label from the line
                    line = line.Substring(line.Split(' ')[0].Length).Trim();
                    if (line.Length == 0 || line[0] == '#')
                    {
                        continue;
                    }
                }
                string command = line.Split(' ')[0].ToLower();
                Instruction instr = InstructionSet.GetInstructionByName(command);
                if (instr.Name == "cstr")
                {
                    xp += line.Substring(command.Length + 1).Trim().Trim('\"').Length + 2;
                }
                else
                {
                    xp += instr.NumOperands + 1;
                }
            }

            for (lineNumber = 0; lineNumber < lines.Length; lineNumber++)
            {
                string line = lines[lineNumber].Trim();
                if (line.Length == 0 || line[0] == '#')
                {
                    continue;
                }
                if (line[0] == '.')
                {
                    // Strips label from the line
                    line = line.Substring(line.Split(' ')[0].Length).Trim();
                    if (line.Length == 0 || line[0] == '#')
                    {
                        continue;
                    }
                }
                string command = line.Split(' ')[0].ToLower();
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
                    else if (labels.ContainsKey(oper))
                    {
                        code.Add(labels[oper]);
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
