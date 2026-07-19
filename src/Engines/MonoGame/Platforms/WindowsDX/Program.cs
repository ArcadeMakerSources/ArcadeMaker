using ArcadeMaker.Engines.MonoGame.Core;
using Microsoft.Xna.Framework;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace ArcadeMaker.Engines.MonoGame.Platforms.WindowsDX;

public class Program
{
    public const string RESOURCES_FILE_NAME = "ArcadeMaker.Engines.MonoGame.Platforms.WindowsDX.GameData.ampb";

    /// <summary>
    /// The main entry point for the application on Windows.
    /// Configures the application for high DPI awareness.
    /// It also creates an instance of your game and calls it's Run() method 
    /// </summary>
    /// <param name="args">Command-line arguments passed to the application.</param>
    public static void Main(string[] args)
    {
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            File.WriteAllText(@"C:\Users\איתן\Desktop\windxlog.txt", e.ExceptionObject.ToString());
        };

        // find game resources file
        string setupErr = null;
        Stream gameData = null;
        if (args is not { Length: > 0 })
        {
            // read embedded resource
            string[] embeddedRes = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            gameData = Assembly.GetExecutingAssembly().GetManifestResourceStream(RESOURCES_FILE_NAME);
            if (gameData == null)
                setupErr = $"No project file was attached.\nargs: [{string.Join(", ", args)}].\nEmbedded Resources: [{string.Join(", ", embeddedRes)}]";
        }

        // configure the application to be DPI-aware for better display scaling.
        Application.SetHighDpiMode(HighDpiMode.SystemAware);

        if (setupErr == null)
        {
            // create an instance of the game and start the game loop.
            using ArcadeMakerMonoGame game = gameData is null ? new(args[0]) : new(gameData);

            Program.game = game;
            game.OnExpRuntimeError += (s, e) => { if (!exceptionViewerIsDisplayed) ShowExceptionViewer(new ExceptionViewerDialog(e)); };
            game.OnCsError += (s, e) => ShowExceptionViewer(new ExceptionViewerDialog(e));

            game.Run();
        }
        else
        {
            System.Windows.Forms.Application.Run(new ExceptionViewerDialog(new Exception(setupErr)));
        }
    }

    private static ArcadeMakerMonoGame game;
    private static bool exceptionViewerIsDisplayed;
    private static void ShowExceptionViewer(ExceptionViewerDialog dialog)
    {
        exceptionViewerIsDisplayed = true;
        var result = dialog.ShowDialog();
        if (result == DialogResult.Abort)
            game.Exit();
        exceptionViewerIsDisplayed = false;
    }
}