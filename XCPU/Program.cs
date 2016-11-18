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
            Console.WriteLine("Commands starts with '/'.");
            while (true) {
                Console.Write("{0,5:D5}: ", cpu.pc);
                cmd = Console.ReadLine();
                string command = cmd.Split(' ')[0].Trim().ToLower();
                if (command.StartsWith("/")) {
                    switch (command) {
                        case "/exit":
                            return;
                        case "/run":
                            cpu.Run();
                            break;
                        case "/reset":
                            cpu.pc = 0;
                            break;
                        case "/jump":
                            string param = cmd.Substring(6).Trim();
                            if (!int.TryParse(param, out cpu.pc)) {
                                Console.WriteLine("Expected memory address.");
                            }
                            break;
                        case "/state":
                            cpu.PrintState();
                            break;
                        default:
                            Console.WriteLine("Invalid command.");
                            break;
                    }
                } else {
                    cpu.ParseLine(cmd);
                }
            }
        }
    }
}
