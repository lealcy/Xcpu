using System;
using System.IO;
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
            string param;
            int[] code;
            Console.WriteLine("Type 'h' for help.");
            Console.WriteLine("CPU integer range is {0} bits.", Xcpu.IntegerRange);
            Console.WriteLine("Installed memory is {0:N0} bytes.", Xcpu.MemorySize);
            while (true) {
                Console.Write("{0,5:D5}: ", cpu.GetR(R.XP));
                cmd = Console.ReadLine().Trim();
                switch (cmd.Split(' ')[0]) {
                    case "h":
                        Console.WriteLine("Available Commands:");
                        Console.WriteLine("\td <address> - Disassemble from zero to address.");
                        Console.WriteLine("\tj <address> - Jump to address.");
                        Console.WriteLine("\tl <file> - Load program into memory.");
                        Console.WriteLine("\tr - Run code.");
                        Console.WriteLine("\ts - Show CPU state.");
                        Console.WriteLine("\tx - Exit.");
                        break;
                    case "d":
                        if (cmd.Length < 3)
                        {
                            Console.WriteLine("Expected memory address.");
                            break;
                        }
                        param = cmd.Substring(2).Trim();
                        int endAddress;
                        if (!int.TryParse(param, out endAddress))
                        {
                            Console.WriteLine("Expected memory address.");
                            break;
                        }
                        Console.WriteLine(Parser.Disassembly(cpu.Read(0, endAddress)));
                        break;
                    case "l":
                        // load from file
                        StreamReader f;
                        try
                        {
                            f = new StreamReader(cmd.Split(' ')[1]);
                        } catch (FileNotFoundException e)
                        {
                            Console.WriteLine(e.Message);
                            break;
                        }
                        code = Parser.Parse(f.ReadToEnd());
                        f.Close();
                        cpu.Write(code, cpu.GetR(R.XP));
                        cpu.SetR(R.XP, cpu.GetR(R.XP) + code.Length);
                        break;
                    case "x":
                        return;
                    case "r":
                        cpu.Run();
                        cpu.Dec(R.XP);
                        break;
                    case "j":
                        if (cmd.Length < 3)
                        {
                            Console.WriteLine("Expected memory address.");
                            break;
                        }
                        param = cmd.Substring(2).Trim();
                        int address;
                        if (!int.TryParse(param, out address)) {
                            Console.WriteLine("Expected memory address.");
                            break;
                        }
                        cpu.SetR(R.XP, address);
                        break;
                    case "s":
                        cpu.PrintState();
                        break;
                    default:
                        try
                        {
                            code = Parser.Parse(cmd);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            break;
                        }
                        cpu.Write(code, cpu.GetR(R.XP));
                        cpu.SetR(R.XP, cpu.GetR(R.XP) + code.Length);
                    break;
                }
            }
        }
    }
}
