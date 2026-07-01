using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.CSharp;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using System.Drawing;
using System.Resources;
using ArcadeMaker.IDE.Items;

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
        public static void GenerateExe(string savePath = null, bool run = false, bool console = true)
        {
            IEnumerable<GameRoom> rooms = project.items.OfType<GameRoom>();
            if (!rooms.Any())
            {
                MessageBox.Show("Game must have at least 1 room.");
                return;
            }

            // validate no errors
            if (!Debugging.Debug.TryBuild())
            {
                MessageBox.Show("Couldn't run the game: Build Failed.\nSee error list for more details.", "Build Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string debugPath = AppDomain.CurrentDomain.BaseDirectory + "\\DEBUG";
            const string debugPname = "debugbuild";
            project.Save(debugPath, successMsg: false, fileName: debugPname);
            Progress = 50;
            isGameRunning = true;

            Engines.MonoGame.Platforms.WindowsDX.Program.Main([debugPath + $@"\{debugPname}\{debugPname}.gsp"]);

            Progress = 100;
            isGameRunning = false;
        }
    }
}