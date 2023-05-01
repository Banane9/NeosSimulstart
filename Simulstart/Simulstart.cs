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
using static Microsoft.IO.RecyclableMemoryStreamManager;

namespace Simulstart
{
    public class Simulstart : NeosMod
    {
        public static ModConfiguration Config;

        private const int NoAdministratorConsentError = 1223;

        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<LaunchDetails[]> ProcessesToLaunch = new("ProcessesToLaunch", "Details for each process to launch.", Array.Empty<LaunchDetails>, true);

        private static HashSet<Process> LaunchedProcessesToQuit;
        private static bool shuttingDown = false;
        public override string Author => "Banane9";
        public override string Link => "https://github.com/Banane9/NeosSimulstart";
        public override string Name => "Simulstart";
        public override string Version => "1.1.0";

        public override void OnEngineInit()
        {
            Config = GetConfiguration();
            Config.Save(true);

            LaunchedProcessesToQuit = new(StartProcesses());

            Engine.Current.OnShutdownRequest += _ =>
            {
                shuttingDown = true;
                Msg("Closing remaining processes that should be ended with Neos.");

                foreach (var process in LaunchedProcessesToQuit)
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
                }
            };
        }

        private static Process LaunchProcess(LaunchDetails launchDetails)
        {
            if (string.IsNullOrWhiteSpace(launchDetails.Path))
            {
                Error("Can't launch empty path!");
                return null;
            }

            if (!File.Exists(launchDetails.Path))
            {
                Error("File to launch doesn't exist: " + launchDetails.Path);
                return null;
            }

            Msg("Launching as " + launchDetails);

            var processStartInfo = new ProcessStartInfo(launchDetails.Path, launchDetails.Arguments);
            processStartInfo.UseShellExecute = true;

            if (launchDetails.Administrator)
                processStartInfo.Verb = "runas";

            if (launchDetails.UseWorkingDirectory)
            {
                if (Directory.Exists(launchDetails.WorkingDirectory))
                    processStartInfo.WorkingDirectory = launchDetails.WorkingDirectory;
                else
                {
                    Warn("Override Working Directory doesn't exist, using executable's: " + launchDetails.WorkingDirectory);
                    processStartInfo.WorkingDirectory = Path.GetDirectoryName(launchDetails.Path);
                }
            }
            else
                processStartInfo.WorkingDirectory = Path.GetDirectoryName(launchDetails.Path);

            Process process = null;
            try
            {
                process = Process.Start(processStartInfo);
                process.EnableRaisingEvents = true;

                process.Exited += (sender, evnt) =>
                {
                    LaunchedProcessesToQuit.Remove(process);

                    if (!launchDetails.Restart || shuttingDown)
                        return;

                    var newProcess = LaunchProcess(launchDetails);
                    if (!launchDetails.KeepOpen && newProcess != null && !newProcess.HasExited)
                        LaunchedProcessesToQuit.Add(newProcess);
                };
            }
            catch (Win32Exception ex)
            {
                if (ex.NativeErrorCode == NoAdministratorConsentError)
                    Error("No administrator consent given while launching process!");
                else
                    throw;
            }

            return process;
        }

        private static IEnumerable<Process> StartProcesses()
        {
            foreach (var processToLaunch in Config.GetValue(ProcessesToLaunch))
            {
                var process = LaunchProcess(processToLaunch);

                if (!processToLaunch.KeepOpen && process != null && !process.HasExited)
                    yield return process;
            }
        }
    }
}