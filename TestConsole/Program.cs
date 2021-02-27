using Null.ArgsParser;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;

namespace TestConsole
{
    class Program
    {
        class StartupArgs
        {
            public bool List;
            public bool Help;
            public bool Create;
            public bool Delete;
            public string Filename;
            public string Path;

            public bool F;
            public bool Q;

            public string[] ExtraContent;
        }
        public static List<string> Split(string cmdline)
        {
            List<string> rstBulder = new List<string>();
            StringBuilder temp = new StringBuilder();
            bool escape = false, quote = false;

            foreach (char i in cmdline)
            {
                if (escape)
                {
                    escape = false;
                    switch (i)
                    {
                        case 'a':
                            temp.Append('\a');
                            break;
                        case 'b':
                            temp.Append('\b');
                            break;
                        case 'f':
                            temp.Append('\f');
                            break;
                        case 'n':
                            temp.Append('\n');
                            break;
                        case 'r':
                            temp.Append('\r');
                            break;
                        case 't':
                            temp.Append('\t');
                            break;
                        case 'v':
                            temp.Append('\v');
                            break;
                        default:
                            temp.Append(i);
                            break;
                    }
                }
                else
                {
                    if (i == '\\')
                    {
                        escape = true;
                    }
                    else
                    {
                        if (quote)
                        {
                            if (i == '"')
                            {
                                rstBulder.Add(temp.ToString());
                                temp.Clear();
                                quote = false;
                            }
                            else
                            {
                                temp.Append(i);
                            }
                        }
                        else
                        {
                            if (i == '"')
                            {
                                rstBulder.Add(temp.ToString());
                                temp.Clear();
                                quote = true;
                            }
                            else if (i == ' ')
                            {
                                if (temp.Length > 0)
                                {
                                    rstBulder.Add(temp.ToString());
                                    temp.Clear();
                                }
                            }
                            else
                            {
                                temp.Append(i);
                            }
                        }
                    }
                }
            }
            if (temp.Length > 0)
            {
                rstBulder.Add(temp.ToString());
            }

            return rstBulder;
        }
        static StartupArgs Initialize()
        {
            string[] args = Environment.GetCommandLineArgs();

            Arguments basic = new Arguments(          // 最基础的命令行参数分析器
                new CommandLine("Help"),                 // 包含Help指令
                new CommandLine("List",                  // 包含List指令
                    new StringArgument("Path", @".\"))         // List指令中需要Path参数, 并且指定了默认值为 ".\"
                        { ElementsIgnoreCase = true },        
                new CommandLine("Create",                // 包含Create指令
                    new StringArgument("Filename"))            // Create指令中需要FileName参数
                        { ElementsIgnoreCase = true},         
                new CommandLine("Delete",                // 包含Delete指令
                    new StringArgument("Filename"))          // Delete指令中需要FileName参数
                        { ElementsIgnoreCase = true}
                )
            { IgnoreCase = true };                      // 忽略大小写

            basic.Parse(args);               // 进行分析

            return basic.ToObject<StartupArgs>();       // 返回类的实例
        }
        class temp
        {
            public string Atlas = "";
            public string Source = "";
            public string Output = "";
            public string Format = "*directory*/*name**extension*";    // name extension size width height year month day hour minute second
            public bool Strict = false;
            public bool Rename = false;

            public string InfoSource = "";
        }
        static void Main(string[] sysargs)
        {
            Arguments qwq = new Arguments(
                new FieldArgument("Atlas"),
                new FieldArgument("Source"),
                new FieldArgument("Output"),
                new SwitchArgument("Strict"),
                new SwitchArgument("Rename"),
                new StringArgument("InfoSource"))
            { IgnoreCase = true, };

            qwq.Parse(sysargs);
            var qwqObj = qwq.ToObject<temp>();


            StartupArgs args = Initialize();

            if (args.Help)               // 表示参数中想要调用 Help 指令
            {
                if (args.ExtraContent.Length == 1)
                {
                    switch(args.ExtraContent[0].ToUpper())
                    {
                        case "LIST":
                            Console.WriteLine(@"列举某个目录下的成员: List Path[=.\]");
                            break;
                        case "HELP":
                            Console.WriteLine("显示帮助手册");
                            break;
                        case "CREATE":
                            Console.WriteLine("创建一个文件: Create Filename=文件名");
                            break;
                        case "DELETE":
                            Console.WriteLine("删除一个文件: Delete Filename=文件名");
                            break;
                        default:
                            Console.WriteLine($"未知指令: {args.ExtraContent[0]}");
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("这是一个示例程序, 用于演示 Null.ArgsParser 的功能" +
                        "\n    支持 List, Help, Create, Delete 指令, 使用 'Help 指令' 以查看详细内容");
                }
            }
            else if (args.List)                    // 表示调用 List
            {
                if (args.Path != null)
                {
                    Console.WriteLine("Files:");
                    foreach (string i in Directory.GetFiles(args.Path))
                        Console.WriteLine($"  {i}");

                    Console.WriteLine("Directories");
                    foreach (string i in Directory.GetDirectories(args.Path))
                        Console.WriteLine($"  {i}");
                }
            }
            else if (args.Create)        // 表示参数中想要调用 Create 指令
            {
                if (args.Filename != null)
                {
                    File.Create(args.Filename);
                }
                else
                {
                    Console.WriteLine("你必须指定一个文件名");
                }
            }
            else if (args.Delete)             // 调用 Delete 
            {
                if (args.Filename != null)
                {
                    File.Delete(args.Filename);
                }
                else
                {
                    Console.WriteLine("你必须指定一个文件名");
                }
            }
            else
            {
                Console.WriteLine("没有进行任何支持的操作? 使用 help 查看帮助手册");
            }


            //Arguments nargs = new Arguments(new PropertyArgument("Fuck"), new FieldArgument("Method"), new SwitchArgument("Quick"));
            //nargs.IgnoreCase = true;
            //nargs.Parse(args);
            //StartupArgs qwq = nargs.ToObject<StartupArgs>();

            //Console.WriteLine(
            //    $"StartupArgs: \n" +
            //    $"    String Fuck: {qwq.Fuck}\n" +
            //    $"    String Method: {qwq.Method}\n" +
            //    $"    bool Quick: {qwq.Quick}\n" +
            //    $"    Special - ExtraContent: {qwq.ExtraContent}\n");
#if DEBUG
            //Console.ReadLine();
#endif
        }
    }
}
