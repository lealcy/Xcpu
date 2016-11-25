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
    }
}
