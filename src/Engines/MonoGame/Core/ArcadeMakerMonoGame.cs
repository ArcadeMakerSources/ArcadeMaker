using ArcadeMaker.Engines.MonoGame.Core.Localization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Globalization;
using static System.Net.Mime.MediaTypeNames;
using ArcadeMaker.Core;
using ArcadeMaker.Core.Models;
using ArcadeMaker.Core.Runtime;
using Microsoft.Xna.Framework.Content;
using Exp;
using ArcadeMaker.Core.Resources;
using ArcadeMaker.Engines.MonoGame.Core.Graphics;
using ArcadeMaker.Core.ExpSrc;
using ArcadeMaker.Core.Resources.Serializeables;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System.Linq;
using Exp.Spans;
using System.Reflection;
using MonoGame.Extended;
using MonoGame.Extended.ViewportAdapters;
using System.Runtime.CompilerServices;
using System.IO;

namespace ArcadeMaker.Engines.MonoGame.Core
{
    /// <summary>
    /// The main class for the game, responsible for managing game components, settings, 
    /// and platform-specific configurations.
    /// </summary>
    public sealed partial class ArcadeMakerMonoGame : Game, IGame
    {
        public event EventHandler<RuntimeException>? OnExpRuntimeError;
        public event EventHandler<Exception>? OnCsError;

        // resources
        private readonly GraphicsDeviceManager graphicsDeviceManager;
        private SpriteBatch SpriteBatch { get; set; } = null!;
        public List<Sprite> Sprites { get; } = [];
        private Dictionary<Background, Texture2D?> BackgroundTextures { get; } = [];
        public List<Background> Backgrounds { get; } = [];
        public List<Sound> Sounds { get; } = [];
        public List<ArcadeMaker.Core.Resources.Path> Paths { get; } = [];
        public List<ObjectModel> Objects { get; } = [];
        public List<GameFont> FontsData { get; } = [];
        public List<ScriptDocument> Scripts { get; } = [];
        public List<RoomModel> Rooms { get; } = [];
        private List<(Viewport port, OrthographicCamera camera)> Cameras { get; } = [];
        public RoomInstance? CurrentRoom
        {
            get;
            set
            {
                field = value;

                // load cameras
                Cameras.Clear();

                if (value != null)
                {
                    foreach (var view in value.Model.Views)
                    {
                        BoxingViewportAdapter viewportAdapter = new(Window, GraphicsDevice, (int)view.Width, (int)view.Height);
                        var camera = new OrthographicCamera(viewportAdapter);
                        view.PositionChanged += (s, e) =>
                        {
                            camera.Position = new Vector2((float)view.X, (float)view.Y);
                        };
                        Cameras.Add((new(view.PortX, view.PortY, view.PortWidth, view.PortHeight), camera));
                    }
                }
            }
        }
        public TextureAtlasMap MainTextureAtlasMap { get; set; }
        public TextureAtlas MainTextureAtlas { get; private set; }

        public StringWriter Debug { get; } = new StringWriter();

        // runtime private data
        private GameRunner<ArcadeMakerMonoGame> GameRunner { get; set; }

        /// <summary>
        /// Indicates if the game is running on a mobile platform.
        /// </summary>
        public readonly static bool IsMobile = OperatingSystem.IsAndroid() || OperatingSystem.IsIOS();

        /// <summary>
        /// Indicates if the game is running on a desktop platform.
        /// </summary>
        public readonly static bool IsDesktop = OperatingSystem.IsMacOS() || OperatingSystem.IsLinux() || OperatingSystem.IsWindows();

        /// <summary>
        /// Initializes a new instance of the game. Configures platform-specific settings, 
        /// initializes services like settings and leaderboard managers, and sets up the 
        /// screen manager for screen transitions.
        /// </summary>
        public ArcadeMakerMonoGame(string projectFile)
        {
            graphicsDeviceManager = new GraphicsDeviceManager(this);

            // share GraphicsDeviceManager as a service.
            Services.AddService(typeof(GraphicsDeviceManager), graphicsDeviceManager);

            Content.RootDirectory = "Content";

            // configure screen orientations.
            graphicsDeviceManager.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;

            // load game data
            ((IGame)this).LoadFromProjectFile(projectFile);
        }

        /// <summary>
        /// Initializes the game, including setting up localization and adding the 
        /// initial screens to the ScreenManager.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            IsMouseVisible = true;

            // Load supported languages and set the default language.
            List<CultureInfo> cultures = LocalizationManager.GetSupportedCultures();
            var languages = new List<CultureInfo>();
            for (int i = 0; i < cultures.Count; i++)
            {
                languages.Add(cultures[i]);
            }

            // TODO You should load this from a settings file or similar,
            // based on what the user or operating system selected.
            var selectedLanguage = LocalizationManager.DEFAULT_CULTURE_CODE;
            LocalizationManager.SetCulture(selectedLanguage);

            SpriteBatch = new SpriteBatch(GraphicsDevice);

            Try(() =>
            {
                GameRunner = new GameRunner<ArcadeMakerMonoGame>(this);
                GameRunner.Run(invokeInit: false);
            });
        }

        public void Init() => Initialize();

        /// <summary>
        /// Loads game content, such as textures and particle systems.
        /// </summary>
        protected override void LoadContent()
        {
            Try(() =>
            {
                base.LoadContent();

                // load background textures
                Backgrounds.ForEach(bg => { if (bg.FilePath != null) BackgroundTextures.Add(bg, Texture2D.FromFile(GraphicsDevice, bg.FilePath)); });

                // load fonts
                Fonts.All.Clear();
                Fonts.Current = null;
                foreach (var fontd in FontsData)
                {
                    var spriteFont = Fonts.FromGameFont(fontd, GraphicsDevice);
                    Fonts.All.Add(fontd, spriteFont);
                }
                if (Fonts.All.Count > 0)
                    Fonts.Current = Fonts.All.Values.First();

                // load main texture atlas
                var mainAtlasTexture = Texture2D.FromFile(GraphicsDevice, MainTextureAtlasMap.AtlasFilePath);
                MainTextureAtlas = new(mainAtlasTexture);
                foreach (var item in MainTextureAtlasMap.Items)
                    MainTextureAtlas.AddRegion(Sprites.First(sprite => sprite.Name == item.SpriteName), item.ImageIndex, item.X, item.Y, item.W, item.H);

                // load sounds
                foreach (var sound in Sounds)
                {
                    try
                    {
                        if (sound.Type == Sound.Types.SoundEffect)
                        {
                            var effect = SoundEffect.FromFile(sound.FilePath);
                            soundEffects.Add(sound, effect);
                        }
                        else if (sound.Type == Sound.Types.BackgroundMusic)
                        {
                            var song = Song.FromUri(sound.Name, new Uri(sound.FilePath, UriKind.Absolute));
                            backgroundMusics.Add(sound, song);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new ArgumentException($"Error loading sound {sound.Name}: {ex.Message}.");
                    }
                }
            });
        }

        private bool isOnError;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Try(Action action) // TODO: consider using a more efficient way to handle this, as this might cause performance issues when used in Update and Draw methods.
        {
            try
            {
                action();
            }
            catch (RuntimeException ex)
            {
                isOnError = true;
                OnExpRuntimeError?.Invoke(this, ex);
                isOnError = false;
            }
            catch (Exception ex) when (true)
            {
                isOnError = true;
                OnCsError?.Invoke(this, ex);
                isOnError = false;
            }
        }

        /// <summary>
        /// Updates the game's logic, called once per frame.
        /// </summary>
        /// <param name="gameTime">
        /// Provides a snapshot of timing values used for game updates.
        /// </param>
        protected override void Update(GameTime gameTime)
        {
            if (isOnError)
                return;

            // get input state
            KeyboardState = Keyboard.GetState();
            Gamepad1State = GamePad.GetState(PlayerIndex.One);
            Gamepad2State = GamePad.GetState(PlayerIndex.Two);
            Gamepad3State = GamePad.GetState(PlayerIndex.Three);
            Gamepad4State = GamePad.GetState(PlayerIndex.Four);
            MouseState    = Mouse.GetState();

            Try(GameRunner.FireStep);

            // save input state
            PrevKeyboardState = KeyboardState;
            PrevGamepad1State = Gamepad1State;
            PrevGamepad2State = Gamepad2State;
            PrevGamepad3State = Gamepad3State;
            PrevGamepad4State = Gamepad4State;
            PrevMouseState    = MouseState;

            base.Update(gameTime);
        }

        public int CurrentViewIndex { get; private set; } = -1;

        private Color backColor;
        public System.Drawing.Color BackColor
        {
            get => System.Drawing.Color.FromArgb(backColor.A, backColor.R, backColor.G, backColor.B);
            set => backColor = new(value.R, value.G, value.B, value.A);
        }

        /// <summary>
        /// Draws the game's graphics, called once per frame.
        /// </summary>
        /// <param name="gameTime">
        /// Provides a snapshot of timing values used for rendering.
        /// </param>
        protected override void Draw(GameTime gameTime)
        {
            if (isOnError || CurrentRoom == null)
                return;

            GraphicsDevice.Clear(backColor);

            // if views are defined, we need to draw the room for each view, applying the corresponding camera transformations.
            // otherwise, we can just draw the room once without any transformations
            if (CurrentRoom.Model.Views.Count > 0) // views are defined
            {
                CurrentViewIndex = 0;
                try
                {
                    foreach (var view in CurrentRoom.Model.Views)
                    {
                        if (!view.Visible)
                            continue;

                        GraphicsDevice.Viewport = Cameras[CurrentViewIndex].port;

                        Matrix transformMatrix = Cameras[CurrentViewIndex].camera.GetViewMatrix();

                        DrawBackgrounds(view.PortWidth, view.PortHeight, transformMatrix);

                        SpriteBatch.Begin(transformMatrix: transformMatrix);

                        Try(GameRunner.FireDraw);

                        SpriteBatch.End();

                        CurrentViewIndex++;
                    }
                }
                finally
                {
                    CurrentViewIndex = -1;
                }
            }
            else // no views defined, just draw the room once with default view
            {
                DrawBackgrounds(Window.ClientBounds.Width, Window.ClientBounds.Height, Matrix.Identity);

                SpriteBatch.Begin();

                Try(GameRunner.FireDraw);

                SpriteBatch.End();
            }

            base.Draw(gameTime);
        }

        public void DrawBackgrounds(int w, int h, Matrix transformMatrix)
        {
            // rooms can have multiple backgrounds, so we need to draw all of them
            RoomInstance room = CurrentRoom!;
            foreach (var background in room.Backgrounds)
            {
                if (!background.Visible)
                    continue;

                // get the texture for the background, if it exists. if the background doesn't have a texture, we skip drawing it
                if (BackgroundTextures.TryGetValue(background.Background, out var texture) && texture != null)
                {
                    SpriteBatch.Begin(transformMatrix: transformMatrix, samplerState: SamplerState.PointWrap); // point wrap = repeat (tile) when texture coordinates (in source rectangle) are outside of 0-1 range, which is what we need for drawing tiled backgrounds

                    // draw with the following logic for source and destination rectangles:
                    SpriteBatch.Draw(
                        texture,
                        destinationRectangle: new Rectangle(0, 0, background.TileHor || background.Stretch ? room.Model.Width : texture.Width, background.TileVer || background.Stretch ? room.Model.Height : texture.Height),
                        sourceRectangle:      new Rectangle((int)background.X, (int)background.Y, background.TileHor && !background.Stretch ? w : texture.Width, background.TileVer && !background.Stretch ? h : texture.Height),
                        Color.White
                    ); 
                    SpriteBatch.End();
                }
            }
        }

        public Exp.Void DrawInstance(ArcadeMaker.Core.Runtime.Instance inst)
        {
            if (inst.Model.Sprite == null)
                return Exp.Void.Return;

            Vector2 position = new((float)inst.X.Value!.Number, (float)inst.Y.Value!.Number);
            Vector2 origin = new(inst.Model.Sprite.OriginX, inst.Model.Sprite.OriginY);
            Vector2 scale = new((float)inst.ImageXScale.Value.Number, (float)inst.ImageYScale.Value.Number);
            
            MainTextureAtlas.GetRegion(inst.Model.Sprite, (int)inst.ImageIndex.Value.Number)?.Draw(
                SpriteBatch,
                position,
                Color.White,
                (float)ArcadeMaker.Core.Math.Formulas.DegreesToRadians(inst.ImageAngle.Value.Number),
                origin,
                scale,
                SpriteEffects.None,
                0
            );

            return Exp.Void.Return;
        }

        public void SetWindowsSize(int w, int h)
        {
            graphicsDeviceManager.PreferredBackBufferWidth = w;
            graphicsDeviceManager.PreferredBackBufferHeight = h;
            graphicsDeviceManager.ApplyChanges();
        }

        public void SetCaption(string caption)
        {
            Window.Title = caption;
        }

        public Exp.Void ShowMessage(Exp.Instance? _, IValue?[] args)
        {
            MessageBox.Show("Message", ("".ToExpString() + args[0]?.Object?.ToString()).ToString(), ["OK"]);
            return Exp.Void.Return;
        }

        private KeyboardState KeyboardState { get; set; }
        private GamePadState Gamepad1State { get; set; }
        private GamePadState Gamepad2State { get; set; }
        private GamePadState Gamepad3State { get; set; }
        private GamePadState Gamepad4State { get; set; }
        private MouseState MouseState { get; set; }
        private KeyboardState PrevKeyboardState { get; set; }
        private GamePadState PrevGamepad1State { get; set; }
        private GamePadState PrevGamepad2State { get; set; }
        private GamePadState PrevGamepad3State { get; set; }
        private GamePadState PrevGamepad4State { get; set; }
        private MouseState PrevMouseState { get; set; }

        public BoolValue KeyDown(Exp.Instance? _, IValue?[] args)
        {
            // check if the specified key is currently down
            return KeyboardState.IsKeyDown((Keys)args[0].ThrowIfNull().Number);
        }

        public BoolValue KeyPress(Exp.Instance? _, IValue?[] args)
        {
            // check if the specified key was got pressed in this frame
            Keys key = (Keys)args[0].ThrowIfNull().Number;
            return KeyboardState.IsKeyDown(key) && PrevKeyboardState.IsKeyUp(key);
        }

        public BoolValue KeyRelease(Exp.Instance? _, IValue?[] args)
        {
            // check if the specified key was released in this frame
            Keys key = (Keys)args[0].ThrowIfNull().Number;
            return KeyboardState.IsKeyUp(key) && PrevKeyboardState.IsKeyDown(key);
        }

        public BoolValue GamepadButtonDown(Exp.Instance? _, IValue?[] args)
        {
            // check if the specified key is currently pressed
            GamePadState? gamepad = args[0].ThrowIfNull().Number switch
            {
                1 => Gamepad1State,
                2 => Gamepad2State,
                3 => Gamepad3State,
                4 => Gamepad4State,
                _ => throw new ArgumentException("Valid inputs for argument playerIndex is a number in range of 1-4.")
            };

            return gamepad.Value.IsButtonDown((Buttons)args[1].Number);
        }

        public BoolValue MouseButtonDown(Exp.Instance? _, IValue?[] args)
        {
            // check if the specified key is currently pressed
            return args[0].ThrowIfNull().Number switch
            {
                0d => MouseState.LeftButton   == ButtonState.Pressed,
                1d => MouseState.MiddleButton == ButtonState.Pressed,
                2d => MouseState.RightButton  == ButtonState.Pressed,
                _ => throw new ArgumentException($"{args[0]!.Number} is not a valid mouse button input. Use '{ExpSrc.EngineNamespace}{Exp.Spans.NamespaceSpecificationSpan.Symbol}MouseButton' enum to pass valid values.")
            };
        }

        public BoolValue MouseButtonPress(Exp.Instance? _, IValue?[] args)
        {
            // check if the specified key is currently pressed
            return args[0].ThrowIfNull().Number switch
            {
                0d => MouseState.LeftButton   == ButtonState.Pressed && PrevMouseState.LeftButton   == ButtonState.Released,
                1d => MouseState.MiddleButton == ButtonState.Pressed && PrevMouseState.MiddleButton == ButtonState.Released,
                2d => MouseState.RightButton  == ButtonState.Pressed && PrevMouseState.RightButton  == ButtonState.Released,
                _ => throw new ArgumentException($"{args[0]!.Number} is not a valid mouse button input. Use '{ExpSrc.EngineNamespace}{Exp.Spans.NamespaceSpecificationSpan.Symbol}MouseButton' enum to pass valid values.")
            };
        }

        public BoolValue MouseButtonRelease(Exp.Instance? _, IValue?[] args)
        {
            // check if the specified key is currently pressed
            return args[0].ThrowIfNull().Number switch
            {
                0d => MouseState.LeftButton   == ButtonState.Released && PrevMouseState.LeftButton   == ButtonState.Pressed,
                1d => MouseState.MiddleButton == ButtonState.Released && PrevMouseState.MiddleButton == ButtonState.Pressed,
                2d => MouseState.RightButton  == ButtonState.Released && PrevMouseState.RightButton  == ButtonState.Pressed,
                _ => throw new ArgumentException($"{args[0]!.Number} is not a valid mouse button input. Use '{ExpSrc.EngineNamespace}{Exp.Spans.NamespaceSpecificationSpan.Symbol}MouseButton' enum to pass valid values.")
            };
        }

        public Exp.Void DrawSprite(Exp.Instance? _, IValue?[] args)
        {
            // parameters
            Vector2 pos = new((float)args[0].ThrowIfNull().Number, (float)args[1].ThrowIfNull().Number);
            Sprite sprite = Sprites.FirstOrDefault(s => s.ID == args[2].ThrowIfNull().Number) ?? throw new ArgumentException($"No sprite with ID '{args[2]}' found.");
            double imageIndex = args[3].ThrowIfNull().Number;

            int angle = 0;
            Color alpha = Color.White;
            if (args.Length > 5)
            {
                angle = (int)args[4].ThrowIfNull().Number;
                alpha = new Color((uint)args[5].ThrowIfNull().Number);
            }

            // get the texture region for the sprite and draw it
            MainTextureAtlas.GetRegion(sprite, (int)imageIndex)?.Draw(SpriteBatch, pos, alpha, (float)ArcadeMaker.Core.Math.Formulas.DegreesToRadians(angle), new(sprite.OriginX, sprite.OriginY), 1f, SpriteEffects.None, 0);

            return Exp.Void.Return;
        }

        public Exp.Void DrawText(Exp.Instance? inst, IValue?[] args)
        {
            args.ValidateArgsNumber(3);
            args[0].ThrowIfNull();
            args[1].ThrowIfNull();

            if (Fonts.Current == null)
                throw new InvalidOperationException("Game must have at least 1 font to draw text.");

            SpriteBatch.DrawString(Fonts.Current, args[2]?.ToString() ?? "NULL", new((float)args[0]!.Number, (float)args[1]!.Number), drawColor);
            return Exp.Void.Return;
        }

        public Exp.Void SetFont(Exp.Instance? _, IValue?[] args)
        {
            Fonts.Current = Fonts.All.FirstOrDefault(f => f.Key.ID == args[0].ThrowIfNull().Number).Value ?? throw new ArgumentException($"No font with ID {args[0]?.Number} found.");
            return Exp.Void.Return;
        }

        private Color drawColor = Color.White;
        public Exp.Void SetColor(Exp.Instance? _, IValue?[] args)
        {
            drawColor = new Color((uint)args[0].ThrowIfNull().Number);
            return Exp.Void.Return;
        }

        public void DrawLine(double x1, double y1, double x2, double y2, double thickness = 1f)
        {
            SpriteBatch.DrawLine(new Vector2((float)x1, (float)y1), new Vector2((float)x2, (float)y2), drawColor, (float)thickness);
        }

        public IValue GetMouseX(Exp.Instance? _, IValue?[] args) => MouseState.X.ToExp();
        public IValue GetMouseY(Exp.Instance? _, IValue?[] args) => MouseState.Y.ToExp();

        protected override void Dispose(bool disposing)
        {
            // dispose sounds
            MediaPlayer.Queue.ActiveSong?.Dispose();
            MediaPlayer.IsRepeating = false;
            MediaPlayer.Stop();
            backgroundMusics.ForEach(bm => { if (!bm.Value.IsDisposed) bm.Value.Dispose(); });
            soundEffectInstances.Values.ForEach(ls => ls.ForEach(sei => { if (!sei.IsDisposed) sei.Dispose(); }));
            soundEffects.ForEach(se => { if (!se.Value.IsDisposed) se.Value.Dispose(); });

            // dispose textures
            BackgroundTextures.ForEach(tex => { if (!tex.Value.IsDisposed) tex.Value.Dispose(); });
            if (MainTextureAtlas?.Texture?.IsDisposed == false)
                MainTextureAtlas.Texture.Dispose();
            Fonts.All.ForEach(f => { if (!f.Value.Texture.IsDisposed) f.Value.Texture.Dispose(); });

            Debug?.Dispose();

            base.Dispose(disposing);
        }
    }
}