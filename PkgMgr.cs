using Newtonsoft.Json;
using SimplePkgMgr.Templates;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using SimplePkgMgr.InstallScriptRuntime;

namespace SimplePkgMgr
{
    public class PkgMgr
    {
        public static string PkgsFolder;
        public static string EtcFolder;
        public static string RepoDBFile;
        public static string TempDir;
        public static List<string> Repolocs;
        public static List<TRepo> Repos;
        public static void LoadRepoDB()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;
            if (File.Exists(RepoDBFile))
            {
                Repolocs = new List<string>(File.ReadAllLines(RepoDBFile));
            }
            else
            {
                Repolocs = new List<string>
                {
                    "https://raw.githubusercontent.com/4UPanElektryk/MySpmPkgs/main/list.json",
                };
            }
        }
        public static void SaveRepoDB()
        {
            File.WriteAllLines(RepoDBFile,Repolocs);
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
        public void Install(string pkgname)
        {
            foreach (TRepo repo in Repos)
            {
                foreach (TPackage package in repo.Packages)
                {
                    if (package.Name == pkgname)
                    {
                        Directory.CreateDirectory(PkgsFolder + repo.Name);
                        Directory.CreateDirectory(PkgsFolder + repo.Name + "\\" + package.Name);
                        new ISR();
                        ISR.InstallDir = PkgsFolder + repo.Name + "\\" + package.Name;
                        new WebClient().DownloadFile(package.InstallFileUrl,TempDir + package.Name+".is");
                        ISR.RunInstallScript(TempDir + package.Name + ".is");
                        File.Delete(TempDir + package.Name + ".is");
                    }
                }
            }
        }
        public void Uninstall(string pkgname)
        {
            foreach (TRepo repo in Repos)
            {
                foreach (TPackage package in repo.Packages)
                {
                    if (package.Name == pkgname)
                    {
                        foreach (string item in Directory.GetFiles(PkgsFolder + repo.Name + "\\" + package.Name))
                        {
                            File.Delete(item);
                        }
                        Directory.Delete(PkgsFolder + repo.Name + "\\" + package.Name);
                    }
                }
            }
        }
    }
}
