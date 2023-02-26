using SimpleLogs4Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SimplePkgMgr.InstallScriptRuntime.IntegratedFunctions
{
    public class CmdMkDir : Cmd
    {
        public CmdMkDir(string name) : base(name) { }
        public override bool Execute(string[] args)
        {
            Log.Write("Making directory " + ISR.InstallDir + args[0]);
            try
            {
                Directory.CreateDirectory(ISR.InstallDir + args[0]);
            }
            catch
            {
                Log.Write("Making directory Failed " + ISR.InstallDir + args[0], EType.Error);
                return false;
            }
            Log.Write("Made directory " + ISR.InstallDir + args[0], EType.Informtion);
            return true;
        }
    }
}
