using ArcadeMaker.IDE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ArcadeMaker.IDE
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            string projectPath = null;
            if (args != null && args.Length > 0 && args[0] != null)
                projectPath = args[0];
            object[] structs = null;
            if (projectPath != null)
            {
                Environment.Project = GameProject.Open(projectPath, out structs);
                Global.PushRecentProject(projectPath);
            }
            if (Environment.Project == null)
                Environment.Project = new GameProject("New Project" /*+ DateTime.Today.ToString("dd-MM-yy")*/);
            Application.Run(new Form1(structs));
        }
    }
}
