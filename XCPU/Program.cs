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
            Console.WriteLine("Type 'h' for help.");
            Console.WriteLine("CPU integer range is {0} bits.", Xcpu.IntegerRange);
            Console.WriteLine("Installed memory is {0:N0} bytes.", Xcpu.MemorySize);
            while (true) {
                Console.Write("{0,5:D5}: ", cpu.GetPC());
                cmd = Console.ReadLine().Trim();
                switch (cmd.Split(' ')[0]) {
                    case "h":
                        Console.WriteLine("Available Commands:");
                        Console.WriteLine("\tj <address> - Jump to address.");
                        Console.WriteLine("\tr - Run code.");
                        Console.WriteLine("\ts - Show CPU state.");
                        Console.WriteLine("\tx - Exit.");
                        break;
                    case "x":
                        return;
                    case "r":
                        cpu.Run();
                        break;
                    case "j":
                        string param = cmd.Substring(2).Trim();
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
                        int[] code;
                        try
                        {
                            code = Parser.Parse(cmd);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            break;
                        }
                        cpu.Write(code, cpu.GetPC());
                        cpu.SetPC(cpu.GetPC() + code.Length);
                    break;
                }
            }
        }
    }
}
