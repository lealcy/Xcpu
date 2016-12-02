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
        static Dictionary<string, int> regIds = new Dictionary<string, int>();

        public static int[] Parse(string program)
        {
            var code = new List<int>();
            var lines = program.Split('\n');
            int xp = 0;
            int lineNumber;

            // Registers positions in registry memory
            regIds["r0"] = 0;
            regIds["r1"] = 1;
            regIds["r2"] = 2;
            regIds["r3"] = 3;
            regIds["r4"] = 4;
            regIds["r5"] = 5;
            regIds["r6"] = 6;
            regIds["r7"] = 7;
            regIds["xp"] = 15;
            regIds["ip"] = 16;
            regIds["sp"] = 17;
            regIds["rp"] = 25;
            regIds["exp"] = 28;
            regIds["eip"] = 29;

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

            foreach(var kp in labels)
            {
                Console.WriteLine(string.Format("{0} = {1}", kp.Key, kp.Value));
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
                line = line.Substring(command.Length).Trim();
                if (line.Length == 0)
                {
                    if (instr.NumOperands == 0)
                    {
                        continue;
                    }
                    else
                    {
                        throw new FormatException(string.Format("Line {0}: Invalid number of operands for {1}; expected {2}, received zero.", lineNumber + 1, command, instr.NumOperands));
                    }
                }
                else if (instr.NumOperands == 0)
                {
                    throw new FormatException(string.Format("Line {0}: Instruction {1} receives no operands.", lineNumber + 1, command));
                }

                if (instr.Name == "cstr")
                {
                    if (!line.StartsWith("\"") || !line.EndsWith("\""))
                    {
                        throw new FormatException(string.Format("Line {0}: Invalid operand format; expected C string, received '{1}'.", lineNumber + 1, line));
                    }
                    foreach (int c in line.Trim('"'))
                    {
                        code.Add(c);
                    }
                    code.Add(0);
                    continue;
                }
                string[] operands = line.Split(',');
                if (operands.Length != instr.NumOperands)
                {
                    throw new FormatException(string.Format("Line {0}: Invalid number of operands for {1}; expected {2}, received {3}", lineNumber + 1, command, instr.NumOperands, operands.Length));
                }
                for (int oi = 0; oi < instr.NumOperands; oi++)
                {
                    string oper = operands[oi].Trim();
                    if (oper.Length == 0)
                    {
                        throw new ArgumentException(string.Format("Line {0}: Empty operand for {1}", lineNumber + 1, command));
                    }
                    switch(instr.Format[oi])
                    {
                        case 'a':
                            if (oper[0] == '.')
                            {
                                if (labels.ContainsKey(oper))
                                {
                                    code.Add(labels[oper]);
                                }
                                else
                                {
                                    throw new KeyNotFoundException(string.Format("Line {0}: Undefined label '{1}'.", lineNumber + 1, oper));
                                }
                            }
                            else
                            {
                                code.Add(int.Parse(oper));
                            }
                            break;
                        case 'i':
                            if (oper[0] == '\'')
                            {
                                if (oper.Length != 2)
                                {
                                    throw new ArgumentException(string.Format("Line {0}: Invalid char to int conversion for operand {1}.", lineNumber + 1, oi + 1));
                                }
                                code.Add(oper[1]);
                            }
                            else
                            {
                                code.Add(int.Parse(oper));
                            }
                            break;
                        case 'r':
                            string regId = oper.ToLower();
                            if (regIds.ContainsKey(regId))
                            {
                                code.Add(regIds[oper.ToLower()]);
                            }
                            else
                            {
                                throw new KeyNotFoundException(string.Format("Line {0}: Invalid register name.", lineNumber + 1));
                            }
                            break;
                        default:
                            throw new ArgumentException(string.Format("Line {0}: Invalid operand {1}.", lineNumber + 1, oi + 1));
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
