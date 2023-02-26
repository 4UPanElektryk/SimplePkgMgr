using SimpleLogs4Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SimplePkgMgr.InstallScriptRuntime.IntegratedFunctions
{
    public class CmdMV : Cmd
    {
        public CmdMV(string name) : base(name) { }
        public override bool Execute(string[] args)
        {
            Log.Write("Moving " + args[0] + " to " + args[1]);
            try
            {
                File.Copy(ISR.InstallDir + args[0], ISR.InstallDir + args[1]);
                File.Delete(ISR.InstallDir + args[0]);
            }
            catch
            {
                Log.Write("Moving Failed " + ISR.InstallDir + args[0] + " to " + ISR.InstallDir + args[1], EType.Error);
                return false;
            }
            Log.Write("Moved " + ISR.InstallDir + args[0] + " to " + ISR.InstallDir + args[1], EType.Informtion);
            return true;
        }
    }
}
