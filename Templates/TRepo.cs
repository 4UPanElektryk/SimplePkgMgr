using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplePkgMgr.Templates
{
    public class TRepo
    {
        public string Name { get; set; }
        public TPackage[] Packages { get; set; }
    }
}
