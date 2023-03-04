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
using SimpleLogs4Net;

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
		#region Repo DB
		public static void LoadRepoDB()
		{
			Assembly assembly = Assembly.GetExecutingAssembly();
			FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
			string version = fvi.FileVersion;
			if (File.Exists(RepoDBFile))
			{
				Repolocs = new List<string>(File.ReadAllLines(RepoDBFile));
				Log.Write("Loaded Repo DB",EType.Informtion);
			}
			else
			{
                Log.Write("Repo DB Not Found", EType.Warning);
				Repolocs = new List<string>
				{
					"https://raw.githubusercontent.com/4UPanElektryk/MySpmPkgs/main/list.json",
				};
				SaveRepoDB();
                Log.Write("Repo DB Recreated", EType.Informtion);
            }
			LoadRepoDBFromRepolocs();
		}
		public static void SaveRepoDB()
		{
			File.WriteAllLines(RepoDBFile,Repolocs);
		}
		public static void LoadRepoDBFromRepolocs() 
		{
            WebClient client = new WebClient();
            foreach (string repo in Repolocs)
            {
                try
                {
                    Repos.Add(JsonConvert.DeserializeObject<TRepo>(client.DownloadString(repo)));
                    Log.Write("Got: " + repo, EType.Informtion);
                }
                catch (Exception ex)
                {
					if (ex.InnerException is WebException)
					{
						Log.Write("Could not get repo at: " + repo, EType.Warning);
					}
					else
					{
						Log.Write("Unknown Error while adding repo: " + repo,EType.Error);
                        Log.Write("Message: " + ex.Message, EType.Error);
                        Log.Write("Trace: " + ex.StackTrace, EType.Error);
						return;
					}
                }
            }
        }

		#endregion
		#region repos
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
			Repolocs.Add(uri);
			SaveRepoDB();
			LoadRepoDB();
			return true;
		}
		public void RemoveRepo(string uri)
		{
			Repolocs.Remove(uri);
			SaveRepoDB();
			LoadRepoDB();
		}
		#endregion
		#region PKGs
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
		#endregion
	}
}
