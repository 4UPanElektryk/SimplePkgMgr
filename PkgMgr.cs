using Newtonsoft.Json;
using SimplePkgMgr.Templates;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SimplePkgMgr
{
    public class PkgMgr
    {
        public static string PkgsFolder { get; set; }
        public static string RepoDBFile { get; set; }
        public static string TempDir { get; set; }
        public static string[] Repolocs { get; set; }
        public static List<TRepo> Repos { get; set; }
        public static void LoadRepoDB()
        {
            if (File.Exists(RepoDBFile))
            {
                Repolocs = File.ReadAllLines(RepoDBFile);
            }
        }
        public bool AddRepo(string uri)
        {
            WebClient client = new WebClient();
            TRepo repo = null;
            try
            {
                repo = JsonConvert.DeserializeObject<TRepo>(client.DownloadString(uri));
            }
            catch
            {
                return false;
            }
            repo.
            return true;
        }
    }
}
