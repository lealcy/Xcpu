using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCPU
{
    struct CPU {
        int r0;
        int pc;
    };

    class Program
    {
        static void Main(string[] args)
        {
            Xcpu cpu = new Xcpu();
            string cmd;
            while (true) {
                Console.Write(">> ");
                cmd = Console.ReadLine();
                switch (cmd.Trim().ToLower()) {
                    case "exit":
                        return;
                        break;
                    case "run":
                        cpu.Run();
                        break;
                    default:
                        cpu.ParseLine(cmd);
                        break;
                }
            }

        }
    }
}
