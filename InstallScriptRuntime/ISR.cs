using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleLogs4Net;
using SimplePkgMgr.InstallScriptRuntime.IntegratedFunctions;

namespace SimplePkgMgr.InstallScriptRuntime
{
    public  class ISR
    {
        public static List<Cmd> cmds;
        public static string InstallDir;
        public ISR() 
        {
            cmds = new List<Cmd> 
            {
                new CmdGet("get"),
                new CmdMV("mv"),
                new CmdMkDir("mkdir"),
            };
        }
        public static void RunInstallScript(string path)
        {
            foreach (string item in File.ReadAllLines(path))
            {
                if (!item.StartsWith("//"))
                {
                    string[] args = Program.RemoveStrings(item.Split("\"".ToArray(), StringSplitOptions.RemoveEmptyEntries).Skip(1).ToArray());
                    foreach (Cmd cmd in cmds)
                    {
                        if (item.ToLower().StartsWith(cmd.Name))
                        {
                            if (!cmd.Execute(args))
                            {
                                Log.Write("Instalation Encounterd an Error at:", EType.Error);
                                for (int i = 0; i < args.Length; i++)
                                {
                                    Log.Write(args[i],EType.Informtion);
                                }
                                Log.Write(cmd.ToString(), EType.Informtion);
                                return;
                            }
                        }
                    }
                }
            }
        }
    }
}
