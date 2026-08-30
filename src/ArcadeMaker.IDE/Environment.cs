using ArcadeMaker.Engines.MonoGame.Core;
using ArcadeMaker.IDE.Items;
using Microsoft.CSharp;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ArcadeMaker.IDE
{
    public static class Environment
    {
        public static GameProject? Project { get; set; } = null;

        public static event EventHandler<int>? ProgressUpdated;
        public static int Progress
        {
            get;
            private set
            {
                field = value;
                ProgressUpdated?.Invoke(null, value);
            }
        }

        internal static bool isGameRunning = false;
        public static void GenerateExe(string? savePath = null, bool run = false, bool console = true)
        {
            if (Project == null)
            {
                MessageBox.Show("No project is opened.");
                return;
            }

            IEnumerable<GameRoom> rooms = Project.items.OfType<GameRoom>();
            if (!rooms.Any())
            {
                MessageBox.Show("Game must have at least 1 room.");
                return;
            }

            if (!run && savePath == null)
            {
#if DEBUG
                ArgumentNullException.ThrowIfNull(savePath);
#endif
                return;
            }

            // validate no errors
            if (!Debugging.Debug.TryBuild())
            {
                MessageBox.Show("Couldn't run the game: Build Failed.\nSee error list for more details.", "Build Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string debugPath = AppDomain.CurrentDomain.BaseDirectory + $"\\DEBUG";
            Directory.CreateDirectory(debugPath);
            debugPath += "\\debugbuild" + GameProject.FileFormats.ArcadeMakerBundledProject;
            Project.Save(debugPath, successMsg: false);
            Progress = 50;
            isGameRunning = true;

            if (run)
            {
                Engines.MonoGame.Platforms.WindowsDX.Program.Main([debugPath]);
            }
            else
            {
                string dll_winDX = typeof(Engines.MonoGame.Platforms.WindowsDX.Program).Assembly.Location;
                using FileStream projectFileStream = File.OpenRead(debugPath);
                EmbedResourceFileToExe(dll_winDX, projectFileStream, savePath!);
            }

            Progress = 100;
            isGameRunning = false;
        }

        private static void EmbedResourceFileToExe(string exePath, Stream data, string saveAs)
        {
            // TODO: after adding the embedded resource - generate a standalone .exe
            using var assembly = AssemblyDefinition.ReadAssembly(exePath);

            // create the EmbeddedResource
            var resource = new EmbeddedResource(
                Engines.MonoGame.Platforms.WindowsDX.Program.RESOURCES_FILE_NAME,
                Mono.Cecil.ManifestResourceAttributes.Public,
                data
            );

            // add the resource to the module and save
            assembly.MainModule.Resources.Add(resource);
            string newDll = saveAs.Substring(0, saveAs.Length - 4) + ".dll";
            assembly.Write(newDll);
        }
    }
}