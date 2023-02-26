using SimpleLogs4Net;
using SimplePkgMgr.InstallScriptRuntime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplePkgMgr
{
    internal class Program
    {
        static void Main(string[] args)
        {
            new Log("Logs\\", OutputStream.Both);
            new ISR();
            ISR.RunInstallScript(args[0]);
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
