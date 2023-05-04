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
		public static List<TPackage> Installed;
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
			if (File.Exists(PkgsFolder + "installed.json"))
			{
				Installed = (List<TPackage>)JsonConvert.DeserializeObject(File.ReadAllText(PkgsFolder + "installed.json"));
			}
			else
			{
				Installed = new List<TPackage>();
			}
		}
		public static void SaveRepoDB()
		{
			File.WriteAllLines(RepoDBFile, Repolocs);
			File.WriteAllText(PkgsFolder + "installed.json", JsonConvert.SerializeObject(Installed));
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
			string pacrepo = pkgname.Split('/')[0];
			string pacname = pkgname.Split('/')[1].Split('|')[0];
			string link = pkgname.Substring(pkgname.Split('|')[0].Length + 1);
			Directory.CreateDirectory(PkgsFolder + pacrepo);
			Directory.CreateDirectory(PkgsFolder + pacrepo + "\\" + pacname);
			Log.DebugMsg("Created Directories", EType.Informtion);
			new ISR();
			ISR.InstallDir = PkgsFolder + pacrepo + "\\" + pacname + "\\";
			try
			{
				new WebClient().DownloadFile(link, TempDir + pacname + ".is");
				Log.DebugMsg("Reached: " + link, EType.Normal);
			}
			catch (Exception)
			{
				Log.DebugMsg("Failed to reach: " + link, EType.Warning);
				return;
			}
			ISR.RunInstallScript(TempDir + pacname + ".is");
			Log.DebugMsg("Installed", EType.Informtion);
			File.Delete(TempDir + pacname + ".is");
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
			List<string> packages = new List<string>();
			if (upgradable)
			{
				List<TPackage> online = new List<TPackage> ();
				foreach (TRepo repo in GetReposFromWeb())

				foreach (TPackage item in Installed)
				{
					packages.Add(item.Name);
				}
			}
			else
			{
				foreach (TPackage item in Installed)
				{
					packages.Add(item.Name);
				}
			}
			return packages;
		}
		public static List<TRepo> GetReposFromWeb()
		{
			List<TRepo> ret = new List<TRepo>();
			foreach (string item in Repolocs)
			{
				try
				{
					ret.Add((TRepo)JsonConvert.DeserializeObject(new WebClient().DownloadString(item)));
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
