using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplePkgMgr.Templates
{
    public class TPackage
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string InstallFileUrl { get; set; }
        public string ManuallFileUrl { get; set; }
    }
}
