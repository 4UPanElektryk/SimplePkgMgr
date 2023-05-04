using SimplePkgMgr.InstallScriptRuntime;
using System.Collections.Generic;
using SimpleLogs4Net;
using System.Runtime.InteropServices;
using System;

namespace SimplePkgMgr
{
    internal class Program
    {
        static void Main(string[] args)
        {
            new LogConfiguration("Logs\\", OutputStream.Both);
            new ISR();
            //ISR.RunInstallScript(args[0]);
            PkgMgr.LoadRepoDB();
            //return;
            if (args.Length == 1)
            {
                if (args[0] == "list")
                {

                }
                else if (args[0] == "update")
                {

                }
                else if (args[0] == "upgrade")
                {

                }
            }
            else if (args.Length == 2)
            {
                if (args[0] == "install")
                {
                    PkgMgr.Install(args[1]);
                }
                else if (args[0] == "remove")
                {

                }
                else if (args[0] == "list")
                {
                    if (args[1] == "installed")
                    {
                        
                    }
                    else if (args[1] == "upgradable")
                    {

                    }
                    else
                    {
                        PkgMgr.RunQuerry(args[1], (string x) => Console.WriteLine(x));
                    }
                }
				else if (args[0] == "repo")
				{

				}
			}
        }

        public static string[] RemoveStrings(string[] input)
        {
            List<string> output = new List<string>();
            foreach (string s in input) 
            {
                if (s != " ")
                {
                    output.Add(s);
                }
            }
            return output.ToArray();
        }
    }
}
