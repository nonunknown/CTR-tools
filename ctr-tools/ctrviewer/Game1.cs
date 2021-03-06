﻿using CTRFramework;
using CTRFramework.Shared;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace ctrviewer
{
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;

        public static Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();

        public static Dictionary<string, TriList> instTris = new System.Collections.Generic.Dictionary<string, TriList>();
        public static Dictionary<string, QuadList> instmodels = new System.Collections.Generic.Dictionary<string, QuadList>();

        List<InstancedModel> instanced = new List<InstancedModel>();
        List<InstancedModel> paths = new List<InstancedModel>();

        List<InstancedModel> karts = new List<InstancedModel>();

        Menu menu;

        //effects
        BasicEffect effect;
        BasicEffect instanceEffect;

        //cameras
        FirstPersonCamera camera;
        FirstPersonCamera lowcamera;
        FirstPersonCamera skycamera;

        //ctr scenes
        List<Scene> scn = new List<Scene>();

        //hi and low scenes converted to monogame
        List<MGLevel> levels = new List<MGLevel>();
        List<MGLevel> quads_low = new List<MGLevel>();

        //sky
        MGLevel sky;
        Color backColor = Color.Blue;


        public static PlayerIndex activeGamePad = PlayerIndex.One;


        //meh
        public static int currentflag = 1;

        public static string version = Meta.GetVersion();


        public Game1()
        {
            Content.RootDirectory = "Content";
            graphics = new GraphicsDeviceManager(this);
        }

        public void GoFullScreen()
        {
            graphics.PreferredBackBufferWidth = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
            graphics.PreferredBackBufferHeight = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
            graphics.IsFullScreen = true;
            graphics.ApplyChanges();
        }

        public void GoWindowed()
        {
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();
        }



        protected override void Initialize()
        {
            graphics.PreferMultiSampling = true;
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            graphics.SynchronizeWithVerticalRetrace = true;
            IsFixedTimeStep = true;
            graphics.ApplyChanges();
            graphics.GraphicsDevice.PresentationParameters.MultiSampleCount = 4;


            Samplers.Refresh();

            GoWindowed();

            IsMouseVisible = false;

            effect = new BasicEffect(graphics.GraphicsDevice);
            effect.VertexColorEnabled = true;
            effect.TextureEnabled = true;
            effect.DiffuseColor = new Vector3(2f, 2f, 2f);

            instanceEffect = new BasicEffect(graphics.GraphicsDevice);
            instanceEffect.VertexColorEnabled = true;
            instanceEffect.TextureEnabled = false;

            spriteBatch = new SpriteBatch(GraphicsDevice);

            camera = new FirstPersonCamera(this);
            lowcamera = new FirstPersonCamera(this);
            skycamera = new FirstPersonCamera(this);

            DisableLodCamera();

            for (PlayerIndex i = PlayerIndex.One; i <= PlayerIndex.Four; i++)
            {
                GamePadState state = GamePad.GetState(i);
                if (state.IsConnected)
                {
                    activeGamePad = i;
                    break;
                }
            }

            Samplers.InitRasterizers();

            karts.Add(new InstancedModel("bluecone", Vector3.Zero, Vector3.Zero, 3));

            base.Initialize();
        }


        private void EnableLodCamera()
        {
            lodEnabled = true;
            /*
            camera.NearClip = 1f;
            camera.FarClip = 10000f;
            lowcamera.NearClip = 9000f;
            lowcamera.FarClip = 50000f;
            */
            lowcamera.NearClip = 1f;
            lowcamera.FarClip = 100000f;
            camera.NearClip = 1f;
            camera.FarClip = 2f;

            camera.Update(null);
            lowcamera.Update(null);
        }

        private void DisableLodCamera()
        {
            lodEnabled = false;
            camera.NearClip = 1f;
            camera.FarClip = 100000f;
            lowcamera.NearClip = 1f;
            lowcamera.FarClip = 2f;
            camera.Update(null);
            lowcamera.Update(null);
        }



        // int currentCameraPosIndex = 0;

        Texture2D tint;

        protected override void LoadContent()
        {
            textures.Add("test", Content.Load<Texture2D>("test"));
            textures.Add("flag", Content.Load<Texture2D>("flag"));
            textures.Add("logo", Content.Load<Texture2D>("logo"));

            effect.Texture = textures["test"];
            effect.TextureEnabled = true;

            font = Content.Load<SpriteFont>("File");

            tint = new Texture2D(GraphicsDevice, 1, 1);
            tint.SetData(new Color[] { Color.Black });

            menu = new Menu(font);
            //graphics.GraphicsDevice.Viewport.Height / 2));

            AddCone("greencone", Color.Green);
            AddCone("yellowcone", Color.Yellow);
            AddCone("redcone", Color.Red);
            AddCone("purplecone", Color.Purple);
            AddCone("cyancone", Color.Cyan);
            AddCone("bluecone", Color.Blue);
        }



        public void AddCone(string name, Color c)
        {
            QuadList modl = new QuadList();

            List<VertexPositionColorTexture> vptc = new List<VertexPositionColorTexture>();

            vptc.Add(new VertexPositionColorTexture(new Vector3(10, 50, -10), MGConverter.Blend(Color.LightGray, c), new Vector2(0, 0)));
            vptc.Add(new VertexPositionColorTexture(new Vector3(-10, 50, -10), MGConverter.Blend(Color.LightGray, c), new Vector2(0, 0)));
            vptc.Add(new VertexPositionColorTexture(new Vector3(0, 0, 0), MGConverter.Blend(Color.Black, c), new Vector2(0, 0)));
            vptc.Add(new VertexPositionColorTexture(new Vector3(-10, 50, 10), MGConverter.Blend(Color.LightGray, c), new Vector2(0, 0)));
            modl.PushQuad(vptc);

            vptc.Add(new VertexPositionColorTexture(new Vector3(-10, 50, 10), MGConverter.Blend(Color.LightGray, c), new Vector2(0, 0)));
            vptc.Add(new VertexPositionColorTexture(new Vector3(10, 50, 10), MGConverter.Blend(Color.LightGray, c), new Vector2(0, 0)));
            vptc.Add(new VertexPositionColorTexture(new Vector3(0, 0, 0), MGConverter.Blend(Color.Black, c), new Vector2(0, 0)));
            vptc.Add(new VertexPositionColorTexture(new Vector3(10, 50, -10), MGConverter.Blend(Color.LightGray, c), new Vector2(0, 0)));
            modl.PushQuad(vptc);

            vptc.Add(new VertexPositionColorTexture(new Vector3(10, 50, -10), MGConverter.Blend(Color.LightGray, c), new Vector2(0, 0)));
            vptc.Add(new VertexPositionColorTexture(new Vector3(10, 50, 10), MGConverter.Blend(Color.LightGray, c), new Vector2(0, 0)));
            vptc.Add(new VertexPositionColorTexture(new Vector3(-10, 50, -10), MGConverter.Blend(Color.LightGray, c), new Vector2(0, 0)));
            vptc.Add(new VertexPositionColorTexture(new Vector3(-10, 50, 10), MGConverter.Blend(Color.LightGray, c), new Vector2(0, 0)));
            modl.PushQuad(vptc);

            modl.Seal();

            instmodels.Add(name, modl);
        }


        bool gameLoaded = false;

        private void LoadStuff()
        {
            gameLoaded = false;

            LoadLevel();
            ResetCamera();

            gameLoaded = true;
        }


        private void LoadTextures(MGLevel qb)
        {
            foreach (string s in qb.textureList)
            {
                string path = String.Format("levels\\tex\\{0}.png", s);
                string path_new = String.Format("levels\\newtex\\{0}.png", s);

                if (File.Exists(path_new))
                    path = path_new;

                if (File.Exists(path))
                {
                    if (!textures.ContainsKey(s))
                        textures.Add(s, Texture2D.FromStream(graphics.GraphicsDevice, File.OpenRead(path)));
                }
                else Console.WriteLine("Missing texture: " + s);
            }
        }

        private void LoadLevel()
        {
            RenderEnabled = false;

            //wait for the end of frame
            while (IsDrawing) { };

            Stopwatch sw = new Stopwatch();
            sw.Start();

            Console.WriteLine("LoadLevel()");

            scn.Clear();
            levels.Clear();
            quads_low.Clear();
            instanced.Clear();

            string[] files = new string[] { };

            if (Directory.Exists(@"levels\"))
                files = Directory.GetFiles(@"levels\", "*.lev");

            if (files.Length == 0)
            {
                Console.WriteLine("no files");
                return;
            }

            foreach (string s in files)
                scn.Add(Scene.FromFile(s));

            Console.WriteLine("scenes parsed at: " + sw.Elapsed.TotalSeconds);


            foreach (Scene s in scn)
            {
                levels.Add(new MGLevel(s, Detail.Med));
                quads_low.Add(new MGLevel(s, Detail.Low));
            }

            Console.WriteLine("converted scenes to monogame render at: " + sw.Elapsed.TotalSeconds);

            //force 1st scene sky and back color
            if (scn.Count > 0)
            {
                backColor = MGConverter.ToColor(scn[0].header.backColor);
                if (scn[0].skybox != null)
                    sky = new MGLevel(scn[0].skybox);
            }


            foreach (Scene s in scn)
                foreach (LODModel m in s.dynamics)
                {
                    if (!instTris.ContainsKey(m.name))
                    {
                        List<VertexPositionColorTexture> li = new List<VertexPositionColorTexture>();

                        foreach (var x in m.lh[0].verts)
                            li.Add(MGConverter.ToVptc(x, new Vector2b(0, 0)));

                        TriList t = new TriList();
                        t.textureEnabled = false;
                        t.textureName = "test";
                        t.scrollingEnabled = false;
                        t.PushTri(li);
                        t.Seal();

                        instTris.Add(m.name, t);
                    }

                }

            Console.WriteLine("extracted dynamics at: " + sw.Elapsed.TotalSeconds);



            foreach (Scene s in scn)
            {
                foreach (PosAng pa in s.header.startGrid)
                    instanced.Add(new InstancedModel("purplecone", new Vector3(pa.Position.X, pa.Position.Y, pa.Position.Z), Vector3.Zero, 3));

                foreach (PickupHeader ph in s.pickups)
                    instanced.Add(new InstancedModel(
                        ph.ModelName,
                        new Vector3(ph.Position.X, ph.Position.Y, ph.Position.Z),
                        new Vector3((float)(ph.Angle.X / 4094f * Math.PI * 2), (float)(ph.Angle.Y / 4094f * Math.PI * 2), (float)(ph.Angle.Z / 4094f * Math.PI * 2)),
                        1));

                foreach (PosAng n in s.restartPts)
                    paths.Add(new InstancedModel("cyancone", new Vector3(n.Position.X, n.Position.Y, n.Position.Z), Vector3.Zero, 3));

                if (s.nav.paths.Count == 3)
                {
                    foreach (NavFrame n in s.nav.paths[0].frames)
                        paths.Add(new InstancedModel("greencone", new Vector3(n.position.X, n.position.Y, n.position.Z), Vector3.Zero, 3));
                    foreach (NavFrame n in s.nav.paths[1].frames)
                        paths.Add(new InstancedModel("yellowcone", new Vector3(n.position.X, n.position.Y, n.position.Z), Vector3.Zero, 3));
                    foreach (NavFrame n in s.nav.paths[2].frames)
                        paths.Add(new InstancedModel("redcone", new Vector3(n.position.X, n.position.Y, n.position.Z), Vector3.Zero, 3));
                }
            }


            foreach (Scene s in scn)
                s.ExportTextures("levels\\tex");

            Console.WriteLine("textures extracted at: " + sw.Elapsed.TotalSeconds);

            //files = Directory.GetFiles("tex", "*.png");

            foreach (MGLevel q in levels) LoadTextures(q);
            foreach (MGLevel q in quads_low) LoadTextures(q);

            sw.Stop();

            Console.WriteLine("textures loaded. level done: " + sw.Elapsed.TotalSeconds);

            RenderEnabled = true;
        }

        public void ResetCamera()
        {
            if (scn.Count > 0)
            {
                camera.Position = MGConverter.ToVector3(scn[0].header.startGrid[0].Position);
                lowcamera.Position = camera.Position;

                camera.SetRotation((float)(scn[0].header.startGrid[0].Angle.Y / 1024 * Math.PI * 2), scn[0].header.startGrid[0].Angle.X / 1024);
                lowcamera.SetRotation((float)(scn[0].header.startGrid[0].Angle.Y / 1024 * Math.PI * 2), scn[0].header.startGrid[0].Angle.X / 1024);
                skycamera.SetRotation((float)(scn[0].header.startGrid[0].Angle.Y / 1024 * Math.PI * 2), scn[0].header.startGrid[0].Angle.X / 1024);

                Console.WriteLine(scn[0].header.startGrid[0].Angle.ToString());
            }
        }

        protected override void UnloadContent()
        {
        }

        public bool updatemouse = false;
        public bool InMenu = false;
        public static bool HideInvisible = true;
        public static bool RenderEnabled = true;
        public static bool ControlsEnabled = true;
        public static bool IsDrawing = false;
        public bool lodEnabled = false;
        public bool show_inst = false;
        public bool show_paths = false;
        public bool lock_fps = true;

        GamePadState oldstate = GamePad.GetState(activeGamePad);
        GamePadState newstate = GamePad.GetState(activeGamePad);

        KeyboardState oldkb = new KeyboardState();
        KeyboardState newkb = new KeyboardState();

        protected override void Update(GameTime gameTime)
        {
            if (loading == null)
                LoadGame();

            newstate = GamePad.GetState(activeGamePad);
            newkb = Keyboard.GetState();


            //foreach (InstancedModel k in karts)
            //  k.Update();

            //x += 0.01f ;
            //if (x > Math.PI * 2)
            //    x = 0;
            //camera.SetRotation(x, y);
            //Console.WriteLine(x);

            if (newstate.Buttons.Start == ButtonState.Pressed && newstate.Buttons.Back == ButtonState.Pressed)
                Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.RightAlt) && Keyboard.GetState().IsKeyDown(Keys.Enter))
            {
                if (graphics.IsFullScreen) GoWindowed(); else GoFullScreen();
            }


            if (Keyboard.GetState().IsKeyDown(Keys.OemMinus))
            {
                float x = camera.ViewAngle;
                x--;
                if (x < 20) x = 20;

                camera.ViewAngle = x;
                lowcamera.ViewAngle = x;
                skycamera.ViewAngle = x;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.OemPlus))
            {
                float x = camera.ViewAngle;
                x++;
                if (x > 150) x = 150;

                camera.ViewAngle = x;
                lowcamera.ViewAngle = x;
                skycamera.ViewAngle = x;
            }

            if ((newstate.Buttons.Start == ButtonState.Pressed && oldstate.Buttons.Start != newstate.Buttons.Start) ||
                (newkb.IsKeyDown(Keys.Escape) && newkb.IsKeyDown(Keys.Escape) != oldkb.IsKeyDown(Keys.Escape)))
            {
                InMenu = !InMenu;
            }

            if (InMenu)
            {
                menu.Update(oldstate, newstate, oldkb, newkb);

                //currentflag = menu.items.Find(x => x.Title == "current flag: {0}").rangeval;

                if (menu.Exec)
                {
                    switch (menu.SelectedItem.Action)
                    {
                        case "close":
                            InMenu = false;
                            break;
                        case "load":
                            LoadGame();
                            InMenu = false;
                            break;
                        case "link":
                            menu.SetMenu(font);
                            break;
                        case "toggle":
                            switch (menu.SelectedItem.Param)
                            {
                                case "inst": show_inst = !show_inst; break;
                                case "paths": show_paths = !show_paths; break;
                                case "lod": lodEnabled = !lodEnabled; if (lodEnabled) EnableLodCamera(); else DisableLodCamera(); break;
                                case "antialias": graphics.PreferMultiSampling = !graphics.PreferMultiSampling; break;
                                case "invis": HideInvisible = !HideInvisible; break;
                                case "filter": Samplers.EnableBilinear = !Samplers.EnableBilinear; Samplers.Refresh(); break;
                                case "wire": Samplers.EnableWireframe = !Samplers.EnableWireframe; break;
                                case "window": if (graphics.IsFullScreen) GoWindowed(); else GoFullScreen(); break;
                                case "lockfps":
                                    lock_fps = !lock_fps;
                                    graphics.SynchronizeWithVerticalRetrace = lock_fps;
                                    IsFixedTimeStep = lock_fps;
                                    graphics.ApplyChanges();
                                    break;
                                default: Console.WriteLine("unimplemented toggle: " + menu.SelectedItem.Param); break;
                            }
                            break;

                        case "exit":
                            Exit();
                            break;
                    }

                    menu.Exec = !menu.Exec;
                }

                if (newstate.Buttons.B == ButtonState.Pressed && newstate.Buttons.B != oldstate.Buttons.B)
                {
                    bool togglemenu = true;

                    foreach (MenuItem m in menu.items)
                    {
                        Console.WriteLine(m.Action + " " + m.Title);
                        if (m.Action == "link" && m.Title == "BACK")
                        {
                            menu.SetMenu(font, m.Param);
                            togglemenu = false;
                        }
                    }

                    if (togglemenu) InMenu = !InMenu;
                }
            }
            else
            {
                foreach (MGLevel mg in levels)
                    mg.Update(gameTime);

                if (ControlsEnabled)
                    UpdateCameras(gameTime);
            }

            oldstate = newstate;
            oldkb = newkb;

            base.Update(gameTime);
        }

        MouseState oldms = new MouseState();
        MouseState newms = new MouseState();

        private void UpdateCameras(GameTime gameTime)
        {
            oldms = newms;
            newms = Mouse.GetState();

            if (IsActive && newms.X >= 0 && newms.Y >=0 && newms.LeftButton == ButtonState.Pressed)
            {
                IsMouseVisible = false;
                updatemouse = true;
                Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
            }
            else
            {
                IsMouseVisible = true;
                updatemouse = false;
            }

            skycamera.Update(gameTime, updatemouse, false, newms, oldms);
            camera.Update(gameTime, updatemouse, true, newms, oldms);
            lowcamera.Copy(gameTime, camera);

            newms = Mouse.GetState();
        }

        //public static bool twoSided = false;

        private void DrawLevel()
        {
            if (RenderEnabled)
            {
                //if (loading != null && gameLoaded)
                //{
                //if we have a sky
                if (sky != null)
                {
                    effect.View = skycamera.ViewMatrix;
                    effect.Projection = skycamera.ProjectionMatrix;

                    effect.DiffuseColor = new Vector3(1, 1, 1);
                    sky.RenderSky(graphics, effect);
                    effect.DiffuseColor = new Vector3(2.0f, 2.0f, 2.0f);
                }

                //clear z buffer
                GraphicsDevice.Clear(ClearOptions.DepthBuffer, Color.Green, 1, 0);

                //render depending on lod
                if (lodEnabled)
                {
                    effect.View = lowcamera.ViewMatrix;
                    effect.Projection = lowcamera.ProjectionMatrix;

                    if (show_inst)
                    {
                        Samplers.SetToDevice(graphics, EngineRasterizer.DoubleSided);
                        foreach (var v in instanced)
                            v.Render(graphics, instanceEffect, lowcamera);
                    }

                    if (show_paths)
                    {
                        Samplers.SetToDevice(graphics, EngineRasterizer.DoubleSided);
                        foreach (var v in paths)
                            v.Render(graphics, instanceEffect, lowcamera);
                    }

                    Samplers.SetToDevice(graphics, EngineRasterizer.Default);

                    /*
                    foreach (Kart k in karts)
                        k.Render(graphics, effect, lowcamera);
                        */
                    foreach (MGLevel qb in quads_low)
                        qb.Render(graphics, effect);
                }
                else
                {
                    if (show_inst)
                    {
                        Samplers.SetToDevice(graphics, EngineRasterizer.DoubleSided);
                        foreach (var v in instanced)
                            v.Render(graphics, instanceEffect, camera);
                    }

                    if (show_paths)
                    {
                        Samplers.SetToDevice(graphics, EngineRasterizer.DoubleSided);
                        foreach (var v in paths)
                            v.Render(graphics, instanceEffect, camera);
                    }

                    Samplers.SetToDevice(graphics, EngineRasterizer.Default);

                    effect.View = camera.ViewMatrix;
                    effect.Projection = camera.ProjectionMatrix;

                    foreach (MGLevel qb in levels)
                        qb.Render(graphics, effect);
                    /*
                    foreach (InstancedModel k in karts)
                        k.Render(graphics, instanceEffect, camera);
                        */
                }
            }
        }


        Task loading;

        private void LoadGame()
        {
            LoadStuff();
            loading = Task.Run(() => { });
            //loading = Task.Run(() => LoadStuff());
            //loading.Wait();
        }



        protected override void Draw(GameTime gameTime)
        {
            IsDrawing = true;

            GraphicsDevice.Clear(backColor);

            if (RenderEnabled)
                DrawLevel();

            if (InMenu)
                menu.Render(GraphicsDevice, spriteBatch, font, tint);


            spriteBatch.Begin(depthStencilState: DepthStencilState.Default);

            if (InMenu)
            {
                spriteBatch.Draw(textures["logo"], new Vector2(graphics.GraphicsDevice.Viewport.Width / 2 - textures["logo"].Width / 2, 50), Color.White);

                spriteBatch.DrawString(
                    font,
                    version,
                    new Vector2(((graphics.PreferredBackBufferWidth - font.MeasureString(version).X * graphics.GraphicsDevice.Viewport.Height / 1080f) / 2), graphics.PreferredBackBufferHeight - 60),
                    Color.Aquamarine,
                    0,
                    new Vector2(0, 0),
                    graphics.GraphicsDevice.Viewport.Height / 1080f,
                    SpriteEffects.None,
                     0.5f
                    );
            }

            if (!gameLoaded)
                spriteBatch.DrawString(font, "LOADING...", new Vector2(graphics.PreferredBackBufferWidth / 2 - (font.MeasureString("LOADING...").X / 2), graphics.PreferredBackBufferHeight / 2), Color.Yellow);

            if (scn.Count == 0 && gameLoaded)
                spriteBatch.DrawString(font, "No levels loaded. Put LEV files in levels folder.".ToString(), new Vector2(20, 60), Color.Yellow);

            if (Keyboard.GetState().IsKeyDown(Keys.OemMinus) || Keyboard.GetState().IsKeyDown(Keys.OemPlus))
                spriteBatch.DrawString(font, String.Format("FOV {0}", camera.ViewAngle.ToString("0.##")), new Vector2(graphics.PreferredBackBufferWidth - font.MeasureString(String.Format("FOV {0}", camera.ViewAngle.ToString("0.##"))).X - 20, 20), Color.Yellow);

            //spriteBatch.DrawString(font, IsActive ? "Active" : "Not active", new Vector2(20, 20), Color.Yellow);

            spriteBatch.End();


            base.Draw(gameTime);

            IsDrawing = false;
        }


    }
}
