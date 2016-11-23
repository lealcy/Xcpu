using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCPU
{

    class Program
    {
        static void Main(string[] args)
        {
            Xcpu cpu = new Xcpu();
            string cmd;
            while (true) {
                Console.Write("{0,5:D5}: ", cpu.GetPC());
                cmd = Console.ReadLine();
                string command = cmd.Split(' ')[0].Trim().ToLower();
                if (command.Length == 1) {
                    switch (command) {
                        case "x":
                            return;
                        case "r":
                            cpu.Run();
                            break;
                        case "j":
                            string param = cmd.Substring(6).Trim();
                            int pc;
                            if (!int.TryParse(param, out pc)) {
                                Console.WriteLine("Expected memory address.");
                                break;
                            }
                            cpu.SetPC(pc);
                            break;
                        case "s":
                            cpu.PrintState();
                            break;
                        default:
                            Console.WriteLine("Invalid command.");
                            break;
                    }
                } else {
                    int[] code = Parser.Parse(cmd);
                    cpu.Write(code, cpu.GetPC());
                }
            }
        }
    }
}
