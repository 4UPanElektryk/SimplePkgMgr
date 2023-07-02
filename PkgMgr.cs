using Newtonsoft.Json;
using SimplePkgMgr.Templates;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Diagnostics;
using SimplePkgMgr.InstallScriptRuntime;
using SimpleLogs4Net;
using System.Linq;

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
		public static List<string> Installed;
		public static void LoadRepoDB()
		{
			if (!Directory.Exists(PkgsFolder))
			{
				Directory.CreateDirectory(PkgsFolder);
			}
			if (!Directory.Exists(TempDir))
			{
				Directory.CreateDirectory(TempDir);
			}
			if (!Directory.Exists(EtcFolder))
			{
				Directory.CreateDirectory(EtcFolder);
			}
			if (File.Exists(EtcFolder + "sources.db"))
			{
				Repolocs = new List<string>(File.ReadAllLines(EtcFolder + "sources.db"));
			}
			else
			{
				Repolocs = new List<string>
				{
					"https://raw.githubusercontent.com/4UPanElektryk/MySpmPkgs/main/list.json",
				};
				File.WriteAllLines(EtcFolder + "sources.db", Repolocs);
			}
			if (File.Exists(RepoDBFile))
			{
				Repos = JsonConvert.DeserializeObject<List<TRepo>>(File.ReadAllText(RepoDBFile));
			}
			Repos = new List<TRepo>();
			if (File.Exists(PkgsFolder + "installed.db"))
			{
				Installed = new List<string>(File.ReadAllLines(PkgsFolder + "installed.db"));
			}
			else
			{
				Installed = new List<string>();
			}
		}
		public static void SaveRepoDB()
		{
			File.WriteAllLines(EtcFolder + "sources.db", Repolocs);
			File.WriteAllLines(PkgsFolder + "installed.db", Installed);
			File.WriteAllText(RepoDBFile, JsonConvert.SerializeObject(Repos));
		}
		public static bool AddRepo(string uri)
		{
			WebClient client = new WebClient();
			try
			{
				Repos.Add(JsonConvert.DeserializeObject<TRepo>(client.DownloadString(uri)));
				Log.DebugMsg("Reached: " + uri, EType.Normal);
			}
			catch (Exception)
			{
				Log.DebugMsg("Failed to reach: " + uri, EType.Warning);
				return false;
			}
			return true;
		}
		public static void Install(string pkgname)
		{
			if (IsQuerry(pkgname))
			{
				string pkgs = string.Join(" ", GetQuerriedPackages(pkgname));
				Log.DebugMsg("Installing packeges: " + pkgs);
				RunQuerry(pkgname, (string x) => InstallSingle(x));
			}
			else
			{
				if (pkgname.Contains("/"))
				{
					InstallSingle(pkgname);
				}
				else
				{
					List<string> packages = GetPackages(pkgname);
					if (packages.Count == 1)
					{
						InstallSingle(packages.First());
					}
					else if (packages.Count > 1)
					{
						Console.WriteLine("Multiple packages found. Please select one:");
						for (int i = 0; i < packages.Count; i++)
						{
							Console.WriteLine(i + " " + packages[i]);
						}
						if (int.TryParse(Console.ReadLine(), out int sel))
						{
							InstallSingle(packages[sel]);
						}
					}
				}
			}

		}
		public static void Uninstall(string pkgname)
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
		public static void InstallSingle(string pkgname)
		{
			TPackage package = null;
			string pacrepo = pkgname.Split('/')[0];
			foreach (var item in Repos)
			{
				foreach (var items in item.Packages)
				{
					if (item.Name + "/" + items.Name == pkgname)
					{
						package = items;
					}
				}
			}
			Directory.CreateDirectory(PkgsFolder + pacrepo);
			Directory.CreateDirectory(PkgsFolder + pacrepo + "\\" + package.Name);
			Log.DebugMsg("Created Directories", EType.Informtion);
			new ISR();
			ISR.InstallDir = PkgsFolder + pacrepo + "\\" + package.Name + "\\";
			try
			{
				new WebClient().DownloadFile(package.InstallFileUrl, TempDir + package.Name + ".is");
				Log.DebugMsg("Reached: " + package.InstallFileUrl, EType.Normal);
			}
			catch (Exception)
			{
				Log.DebugMsg("Failed to reach: " + package.InstallFileUrl, EType.Warning);
				return;
			}
			ISR.RunInstallScript(TempDir + package.Name + ".is");
			Log.DebugMsg("Installed", EType.Informtion);
			Installed.Add(pacrepo +"/"+ package.Name+"|" + package.Version);
			SaveRepoDB();
			File.Delete(TempDir + package.Name + ".is");
		}
		#region Querries
		public static bool IsQuerry(string text)
		{
			return text.StartsWith("*") || text.EndsWith("*");
		}
		public static bool ChechIfQuerryMatches(string querry, string texttomatch)
		{
			if (querry.StartsWith("*") && querry.EndsWith("*"))
			{
				string qri = querry.Substring(1, querry.Length - 2);
				if (texttomatch.Contains(qri))
				{
					return true;
				}
			}
			else if (querry.StartsWith("*") && !querry.EndsWith("*"))
			{
				string qri = querry.Substring(1);
				if (texttomatch.StartsWith(qri))
				{
					return true;
				}
			}
			else if (!querry.StartsWith("*") && querry.EndsWith("*"))
			{
				string qri = querry.Substring(querry.Length - 1);
				if (texttomatch.EndsWith(qri))
				{
					return true;
				}
			}
			return false;
		}
		public static void RunQuerry(string query, Action<string> action)
		{
			foreach (string item in GetQuerriedPackages(query))
			{
				action.Invoke(item);
			}
		}
		public static List<string> GetPackages(string pkgname)
		{
			List<string> packages = new List<string>();
			foreach (TRepo repo in Repos)
			{
				foreach (TPackage package in repo.Packages)
				{
					if (pkgname == "")
					{
						packages.Add(repo.Name + "/" + package.Name);
					}
					if (package.Name == pkgname)
					{
						packages.Add(repo.Name + "/" + package.Name);
					}
				}
			}
			return packages;
		}
		public static List<string> GetInstalledPackages(bool upgradable)
		{
			return Installed;
		}
		public static List<TRepo> GetReposFromWeb()
		{
			List<TRepo> ret = new List<TRepo>();
			foreach (string item in Repolocs)
			{
				try
				{
					ret.Add(JsonConvert.DeserializeObject<TRepo>(new WebClient().DownloadString(item)));
					Log.DebugMsg("Reached: " + item, EType.Normal);
				}
				catch (Exception)
				{
					Log.DebugMsg("Failed to reach: " + item, EType.Warning);
				}
			}
			return ret;
		}
		public static List<string> GetQuerriedPackages(string querry)
		{
			List<string> packages = new List<string>();
			foreach (TRepo repo in Repos)
			{
				foreach (TPackage package in repo.Packages)
				{
					string pkg;
					if (querry.Contains('/'))
					{
						pkg = repo.Name + "/" + package.Name;
					}
					else
					{
						pkg = package.Name;
					}
					if (ChechIfQuerryMatches(querry, pkg))
					{
						packages.Add(repo.Name + "/" + package.Name);
					}
				}
			}
			return packages;
		}
		private static bool IsNewer(string ver, string nver)
		{
			int[] nverr = { int.Parse(nver.Split('.')[0]), int.Parse(nver.Split('.')[1]), int.Parse(nver.Split('.')[2]) };
			int[] verr = { int.Parse(ver.Split('.')[0]), int.Parse(ver.Split('.')[1]), int.Parse(ver.Split('.')[2]) };
			return nver[0] > ver[0] || (nver[0] == ver[0] && nver[1] > ver[1]) || (nver[0] == ver[0] && nver[1] == ver[1] && nver[2] > ver[2]);
		}
		#endregion
	}
}
