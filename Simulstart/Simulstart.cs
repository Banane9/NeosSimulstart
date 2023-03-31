using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BaseX;
using CodeX;
using FrooxEngine;
using FrooxEngine.LogiX;
using FrooxEngine.LogiX.Data;
using FrooxEngine.LogiX.ProgramFlow;
using FrooxEngine.UIX;
using HarmonyLib;
using NeosModLoader;

namespace Simulstart
{
    public class Simulstart : NeosMod
    {
        public static ModConfiguration Config;

        private const int NoAdministratorConsentError = 1223;

        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<LaunchDetails[]> ProcessesToLaunch = new("ProcessesToLaunch", "Details for each process to launch.", Array.Empty<LaunchDetails>, true);

        private static Process[] LaunchedProcessesToQuit;
        public override string Author => "Banane9";
        public override string Link => "https://github.com/Banane9/NeosSimulstart";
        public override string Name => "Simulstart";
        public override string Version => "1.0.0";

        public override void OnEngineInit()
        {
            Config = GetConfiguration();
            Config.Save(true);

            LaunchedProcessesToQuit = StartProcesses().ToArray();

            Engine.Current.OnShutdownRequest += _ =>
            {
                Msg("Closing remaining processes that should be ended with Neos.");

                LaunchedProcessesToQuit.Where(process => !process.HasExited).Do(process =>
                {
                    try
                    {
                        Msg("Closing: " + process.ProcessName);
                        process.Kill();
                    }
                    catch (InvalidOperationException)
                    {
                        // Need an Administrator privileged process to kill another
                        Process.Start(new ProcessStartInfo("cmd.exe", "/C taskkill /F /T /PID " + process.Id) { Verb = "runas", UseShellExecute = true, WindowStyle = ProcessWindowStyle.Hidden });
                    }
                    catch (Exception e)
                    {
                        Error("Failed to close process.");
                        Error(e.ToString());
                    }
                });
            };
        }

        private static IEnumerable<Process> StartProcesses()
        {
            foreach (var processToLaunch in Config.GetValue(ProcessesToLaunch))
            {
                if (string.IsNullOrWhiteSpace(processToLaunch.Path))
                {
                    Error("Can't launch empty path!");
                    continue;
                }

                if (!File.Exists(processToLaunch.Path))
                {
                    Error("File to launch doesn't exist: " + processToLaunch.Path);
                    continue;
                }

                Msg("Launching as " + processToLaunch);

                var processStartInfo = new ProcessStartInfo(processToLaunch.Path, processToLaunch.Arguments);
                processStartInfo.UseShellExecute = true;

                if (processToLaunch.Administrator)
                    processStartInfo.Verb = "runas";

                if (processToLaunch.UseWorkingDirectory)
                {
                    if (Directory.Exists(processToLaunch.WorkingDirectory))
                        processStartInfo.WorkingDirectory = processToLaunch.WorkingDirectory;
                    else
                    {
                        Warn("Override Working Directory doesn't exist, using executable's: " + processToLaunch.WorkingDirectory);
                        processStartInfo.WorkingDirectory = Path.GetDirectoryName(processToLaunch.Path);
                    }
                }
                else
                    processStartInfo.WorkingDirectory = Path.GetDirectoryName(processToLaunch.Path);

                Process process = null;
                try
                {
                    process = Process.Start(processStartInfo);
                }
                catch (Win32Exception ex)
                {
                    if (ex.NativeErrorCode == NoAdministratorConsentError)
                        Error("No administrator consent given while launching process!");
                    else
                        throw;
                }

                if (!processToLaunch.KeepOpen && process != null)
                    yield return process;
            }
        }
    }
}