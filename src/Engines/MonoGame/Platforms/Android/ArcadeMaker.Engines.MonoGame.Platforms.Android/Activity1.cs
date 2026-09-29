using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Java.Nio.FileNio;
using Microsoft.Xna.Framework;
using System;
using System.IO;

namespace ArcadeMaker.Engines.MonoGame.Platforms.Android
{
    [Activity(
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
        private ArcadeMaker.Engines.MonoGame.Core.ArcadeMakerMonoGame _game;
        private View _view;
        private MemoryStream _gameFileStream = new();

        protected override void OnCreate(Bundle bundle)
        {
            try
            {
                base.OnCreate(bundle);

                using (Stream unseekableStream = Application.Context.Assets.Open("game.ampb") ?? throw new Exception("game stream was null"))
                {
                    unseekableStream.CopyTo(_gameFileStream);
                }
                {
                    _gameFileStream.Position = 0;
                    _game = new(_gameFileStream);
                    _view = _game.Services.GetService(typeof(View)) as View;

                    SetContentView(_view);
                    _game.Run();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("catched exception: " + ex);
            }
        }
    }
}
