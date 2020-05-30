﻿// Copy from original Playnite repository
//
//    https://github.com/JosefNemec/Playnite/tree/master/source
//
// Any changes should be commited to original repository and backported here.

using Microsoft.Win32;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Playnite.Common
{
    public class Program
    {
        public string Path { get; set; }
        public string Arguments { get; set; }
        public string Icon { get; set; }
        public string WorkDir { get; set; }
        public string Name { get; set; }
        public string AppId { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    public class UninstallProgram
    {
        public string DisplayIcon { get; set; }
        public string DisplayName { get; set; }
        public string DisplayVersion { get; set; }
        public string InstallLocation { get; set; }
        public string Publisher { get; set; }
        public string UninstallString { get; set; }
        public string URLInfoAbout { get; set; }
        public string RegistryKeyName { get; set; }
        public string Path { get; set; }

        public override string ToString()
        {
            return DisplayName ?? RegistryKeyName;
        }
    }

    public class Programs
    {
        private static readonly string[] uninstallerMasks = new string[]
        {
            "uninst",
            "setup",
            @"unins\d+"
        };

        private static ILogger logger = LogManager.GetLogger();

        public static bool IsPathUninstaller(string path)
        {
            return uninstallerMasks.Any(a => Regex.IsMatch(path, a, RegexOptions.IgnoreCase));
        }

        private static List<UninstallProgram> GetUninstallProgsFromView(RegistryView view)
        {
            var rootString = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\";
            void SearchRoot(RegistryHive hive, List<UninstallProgram> programs)
            {
                var root = RegistryKey.OpenBaseKey(hive, view);
                var keyList = root.OpenSubKey(rootString);
                if (keyList == null)
                {
                    return;
                }

                foreach (var key in keyList.GetSubKeyNames())
                {
                    var prog = root.OpenSubKey(rootString + key);
                    if (prog == null)
                    {
                        continue;
                    }

                    var program = new UninstallProgram()
                    {
                        DisplayIcon = prog.GetValue("DisplayIcon")?.ToString(),
                        DisplayVersion = prog.GetValue("DisplayVersion")?.ToString(),
                        DisplayName = prog.GetValue("DisplayName")?.ToString(),
                        InstallLocation = prog.GetValue("InstallLocation")?.ToString(),
                        Publisher = prog.GetValue("Publisher")?.ToString(),
                        UninstallString = prog.GetValue("UninstallString")?.ToString(),
                        URLInfoAbout = prog.GetValue("URLInfoAbout")?.ToString(),
                        Path = prog.GetValue("Path")?.ToString(),
                        RegistryKeyName = key
                    };

                    programs.Add(program);
                }
            }

            var progs = new List<UninstallProgram>();
            SearchRoot(RegistryHive.LocalMachine, progs);
            SearchRoot(RegistryHive.CurrentUser, progs);
            return progs;
        }

        public static List<UninstallProgram> GetUnistallProgramsList()
        {
            var progs = new List<UninstallProgram>();

            if (Environment.Is64BitOperatingSystem)
            {
                progs.AddRange(GetUninstallProgsFromView(RegistryView.Registry64));
            }

            progs.AddRange(GetUninstallProgsFromView(RegistryView.Registry32));
            return progs;
        }
    }
}
