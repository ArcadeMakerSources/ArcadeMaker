using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Microsoft.Xna.Framework;
using System;
using System.IO;

namespace ArcadeMaker.Engines.MonoGame.Platforms.Android
{
    [Activity(
        Name = "com.arcademaker.debugger.MainActivity",
        Label = "@string/app_name",
        MainLauncher = true,
        Icon = "@drawable/icon",
        AlwaysRetainTaskState = true,
        LaunchMode = LaunchMode.SingleInstance,
        ScreenOrientation = ScreenOrientation.FullUser,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize
    )]
    public class Activity1 : AndroidGameActivity
    {
        public const string GameDataFilePath = $"/storage/emulated/0/Android/data/com.arcademaker.debugger/gamedata.ampb";

        private ArcadeMaker.Engines.MonoGame.Core.ArcadeMakerMonoGame _game;
        private View _view;
        private MemoryStream _streamRef;

        protected override void OnCreate(Bundle bundle)
        {
            try
            {
                base.OnCreate(bundle);
                MemoryStream memoryStream = new();
                _streamRef = memoryStream;
                using (Stream unseekableStream = File.OpenRead(GameDataFilePath))
                {
                    unseekableStream.CopyTo(memoryStream);
                }
                memoryStream.Position = 0;
                _game = new(memoryStream);
                _view = _game.Services.GetService(typeof(View)) as View;

                SetContentView(_view);
                _game.Run();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("catched exception: " + ex);
            }
        }
    }
}
