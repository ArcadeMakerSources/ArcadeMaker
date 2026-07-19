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
        public static GameProject project = null;

        private static string Basecode = null;
        private static bool readBasecodeAgain = false;

        private static string[] _assembliesLocations = null;
        public static string[] AssembliesLocations
        {
            get
            {
                if (_assembliesLocations == null)
                    LoadAssembliesLocations();
                return _assembliesLocations;
            }
        }

        private static void LoadAssembliesLocations()
        {
            List<string> assemblies = new List<string>
            {
                // we only want the assembly, the class does not matter
                System.Reflection.Assembly.GetAssembly(typeof(System.Linq.Expressions.Expression)).Location,
                System.Reflection.Assembly.GetAssembly(typeof(System.Random)).Location,
                System.Reflection.Assembly.GetAssembly(typeof(System.IDisposable)).Location,
                System.Reflection.Assembly.GetAssembly(typeof(System.IO.Stream)).Location,
                System.Reflection.Assembly.GetAssembly(typeof(System.Windows.Forms.Form)).Location,
                System.Reflection.Assembly.GetAssembly(typeof(System.Drawing.Bitmap)).Location,
                System.Reflection.Assembly.GetAssembly(typeof(System.Linq.Enumerable)).Location,
                System.Reflection.Assembly.GetAssembly(typeof(System.ComponentModel.AddingNewEventArgs)).Location,
                //engineDllLocation,
                //System.Reflection.Assembly.Load("netstandard, Version=2.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51").Location
            };

            // load NAudio NuGet package assemblies
            //Type[] NAudio_assemblies_types = new Type[] {
            //    typeof(NAudio.Wave.AudioFileReader),
            //    typeof(NAudio.Wave.WaveStream),
            //    typeof(NAudio.Wave.WaveOut),
            //    typeof(NAudio.Wave.MediaFoundationReader),
            //    typeof(NAudio.Wave.WaveFormatConversionStream)
            //};
            //foreach (Type type in NAudio_assemblies_types)
            //{
            //    string location = System.Reflection.Assembly.GetAssembly(type).Location;
            //    if (!assemblies.Contains(location))
            //        assemblies.Add(location);
            //}

            _assembliesLocations = assemblies.ToArray();
        }

        private static string[] GetNAudioAssembliesLocations()
        {
            List<string> assemblies = new List<string>();
            return assemblies.ToArray();
        }

        public static event EventHandler<int> ProgressUpdated;
        private static int progress = 0;
        public static int Progress
        {
            get
            {
                return progress;
            }
            private set
            {
                progress = value;
                if (ProgressUpdated != null)
                    ProgressUpdated(null, value);
            }
        }

        private const string EngineDllResName = "engine.dll";
        internal static bool isGameRunning = false;
        public static void GenerateExe(string? savePath = null, bool run = false, bool console = true)
        {
            IEnumerable<GameRoom> rooms = project.items.OfType<GameRoom>();
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
            project.Save(debugPath, successMsg: false);
            Progress = 50;
            isGameRunning = true;

            if (run)
                Engines.MonoGame.Platforms.WindowsDX.Program.Main([debugPath]);
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