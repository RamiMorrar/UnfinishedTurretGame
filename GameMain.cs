using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace TurretGame
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
     public struct PlayerData
    {
        
        public Vector2 Position;
    public bool IsAlive;
    public Color Color;
    public float Angle;
    public float Power;
}
public class GameMain : Game
    {
        private Color[,] rocketColorArray;
        private Color[,] foregroundColorArray;
        private Color[,] carriageColorArray;
        private Color[,] cannonColorArray;
        //Properties
        private int[] terrainContour;
        private List<Vector2> smokeList = new List<Vector2>();
        private Random randomizer = new Random();
        private int currentPlayer = 0;
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private GraphicsDevice Device;
        private PlayerData[] Players;
        private int numofPlayers = 4;
        private Texture2D bgTexture;
        private Texture2D fgTexture;
        private Texture2D rocketTexture;
        private bool rocketFlying = false;
        private Vector2 rocketPosition;
        private Vector2 rocketDirection;
        private float rocketAngle;
        private float rocketScaling = 0.1f;
        private SoundEffect hitCannon, hitterrain, launch;
        private Texture2D smoke, explosion, ground;
        private float playerScaling;// Scales player to appropriate position
        private Texture2D carriageTexture;
        private Texture2D cannonTexture;
        private int screenWidth;
        private int screenHeight;
        private SpriteFont Font;
        //Array for colors
        private Color[] Colors = new Color[10]
        {
            Color.Red,
            Color.Green,
            Color.Blue,
            Color.Yellow,
            Color.Purple,
            Color.Pink,
            Color.Brown,
            Color.OrangeRed,
            Color.Aquamarine,
            Color.Cyan,
        };

        public GameMain()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        private void ProcessKeyboard()
        {
            KeyboardState keybState = Keyboard.GetState();

            if (keybState.IsKeyDown(Keys.Left))
            {
                Players[currentPlayer].Angle -= 0.01f;
            }
            if (keybState.IsKeyDown(Keys.Right))
            {
                Players[currentPlayer].Angle += 0.01f;
            }
            //Rotation validation(do not hit the floor)
            if (Players[currentPlayer].Angle > MathHelper.PiOver2)
            {
                Players[currentPlayer].Angle = -MathHelper.PiOver2;
            }
            if (Players[currentPlayer].Angle < -MathHelper.PiOver2)
            {
                Players[currentPlayer].Angle = MathHelper.PiOver2;
            }

            if (keybState.IsKeyDown(Keys.Down))
            {
                 Players[currentPlayer].Power -= 1;
            }
            if (keybState.IsKeyDown(Keys.Up))
            {
                Players[currentPlayer].Power += 1;
            }
            if (keybState.IsKeyDown(Keys.PageDown))
            {
                Players[currentPlayer].Power -= 20;
            }
            if (keybState.IsKeyDown(Keys.PageUp))
            {
                Players[currentPlayer].Power += 20;
            }

            if (Players[currentPlayer].Power > 1000)
            {
                Players[currentPlayer].Power = 1000;
            }
            if (Players[currentPlayer].Power < 0)
            {
                Players[currentPlayer].Power = 0;
            }

            if (keybState.IsKeyDown(Keys.Enter) || keybState.IsKeyDown(Keys.Space))
            {
                rocketFlying = true;
                rocketPosition = Players[currentPlayer].Position;
                rocketPosition.X += 20;
                rocketPosition.Y -= 10;
                rocketAngle = Players[currentPlayer].Angle;
                Vector2 up = new Vector2(0, -1);
                Matrix rotMatrix = Matrix.CreateRotationZ(rocketAngle); // Rotates rocket
                rocketDirection = Vector2.Transform(up, rotMatrix); // rotates rocket to direction
                rocketDirection *= Players[currentPlayer].Power / 50.0f; // Calculates the direction and speed of the rocket
            }
        }
        //Unconventional way to do Collision?
        private Vector2 TexturesCollide(Color[,] tex1, Matrix mat1, Color[,] tex2, Matrix mat2)
        {
            Matrix mat1to2 = mat1 * Matrix.Invert(mat2);
            int width1 = tex1.GetLength(0);
            int height1 = tex1.GetLength(1);
            int width2 = tex2.GetLength(0);
            int height2 = tex2.GetLength(1);

            for (int x1 = 0; x1 < width1; x1++)
            {
                for (int y1 = 0; y1 < height1; y1++)
                {
                    Vector2 pos1 = new Vector2(x1, y1);
                    Vector2 pos2 = Vector2.Transform(pos1, mat1);

                    int x2 = (int)pos2.X;
                    int y2 = (int)pos2.Y;
                    if ((x2 >= 0) && (x2 < width2))
                    {
                        if ((y2 >= 0) && (y2 < height2))
                        {
                            if (tex1[x1, y1].A > 0)
                            {
                                if (tex2[x2, y2].A > 0)
                                {
                                    Vector2 screenCoord = Vector2.Transform(pos1, mat1);
                                    return screenCoord;
                                }
                            }
                        }
                    }
                }
            }

            return new Vector2(-1, -1);
        }
        private void CreateForeGround()
        {
            Color[] foregroundColors = new Color[screenHeight * screenWidth];
            Color[,] groundColors = TextureTo2DArray(ground);
            for (int x = 0; x < screenWidth; x++)
            {
                for (int y = 0; y < screenHeight; y++)
                {
                    if (y > terrainContour[x])
                    {
                        //This atlases a texture to a color and puts it on the randomly generated terrain
                                                                             // The % sign is the Modulo of the function, making sure that the ground texture's height and width
                        foregroundColors[x + y * screenWidth] = groundColors[x % ground.Width, y % ground.Height];
                    }
                    else
                    {
                        foregroundColors[x + y * screenWidth] = Color.Transparent;
                    }
                }
            }
            fgTexture = new Texture2D(Device, screenWidth, screenHeight, false, SurfaceFormat.Color);
            fgTexture.SetData(foregroundColors);

            foregroundColorArray = TextureTo2DArray(ground);
        }
       
        private void DrawRocket()
        {
            if (rocketFlying)
            {
                spriteBatch.Draw(rocketTexture, rocketPosition, null, Players[currentPlayer].Color, rocketAngle, new Vector2(42, 240), rocketScaling, SpriteEffects.None, 1);
            }
        }
        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = 500;
            graphics.PreferredBackBufferHeight = 500;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();
            Window.Title = "Turret Game";
            
        
            base.Initialize();
        }
        private void UpdateRocket()
        {
            if (rocketFlying)
            {
                Vector2 gravity = new Vector2(0, 1);
                rocketDirection += gravity / 10.0f;
                rocketPosition += rocketDirection;
                rocketAngle = (float)Math.Atan2(rocketDirection.X, -rocketDirection.Y);
                Vector2 smokePos = rocketPosition;
                smokeList.Add(smokePos);
            }
        }
        private void DrawSmoke()
        {
            for (int i = 0; i < smokeList.Count; i++)
            {
                spriteBatch.Draw(smoke, smokeList[i], null, Color.White, 0, new Vector2(40, 35), 0.2f, SpriteEffects.None, 1);
            }
        }
        private void SetupPlayers()
        {
            Players = new PlayerData[numofPlayers];
            for (int i = 0; i < numofPlayers; i++)
            {
                Players[i].IsAlive = true;
                Players[i].Color = Colors[i];
                Players[i].Angle = MathHelper.ToRadians(90);
                Players[i].Power = 100;
                Players[i].Position = new Vector2();
                Players[i].Position.X = screenWidth / (numofPlayers + 1) * (i + 1);
                Players[i].Position.Y = terrainContour[(int)Players[i].Position.X];
            }

            
        }
        private void FlattenTerrainBelowPlayers()
        {
            foreach (PlayerData player in Players)
            {
                if (player.IsAlive)
                {
                    for (int x = 0; x < 40; x++)
                    {
                        terrainContour[(int)player.Position.X + x] = terrainContour[(int)player.Position.X];
                    }
                }
            }
        }
        private void DrawPlayers()
        {
            for (int i = 0; i < Players.Length; i++)
            {
                if (Players[i].IsAlive)
                {
                    int xPos = (int)Players[i].Position.X;
                    int yPos = (int)Players[i].Position.Y;
                    Vector2 cannonOrigin = new Vector2(11, 50);
                    spriteBatch.Draw(carriageTexture, Players[i].Position, null, Players[i].Color, 0, new Vector2(0, carriageTexture.Height), playerScaling, SpriteEffects.FlipHorizontally, 0);
                    spriteBatch.Draw(cannonTexture, new Vector2(xPos + 20, yPos - 10), null, Players[i].Color, Players[i].Angle, cannonOrigin, playerScaling, SpriteEffects.None, 1); //This sets the cannon position
                }
            }
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Device = graphics.GraphicsDevice;
            cannonTexture = Content.Load<Texture2D>("Content/cannon");
            bgTexture = Content.Load<Texture2D>("Content/background");
            ground = Content.Load<Texture2D>("Content/ground");
            carriageTexture = Content.Load<Texture2D>("Content/carriage");
            screenHeight = Device.PresentationParameters.BackBufferWidth;
            screenWidth = Device.PresentationParameters.BackBufferHeight;
            Font = Content.Load<SpriteFont>("Content/myFont");
            rocketTexture = Content.Load<Texture2D>("Content/rocket");
            smoke = Content.Load<Texture2D>("Content/smoke");
            playerScaling = 40.0f / (float)carriageTexture.Width;

            rocketColorArray = TextureTo2DArray(rocketTexture);
            cannonColorArray = TextureTo2DArray(cannonTexture);
            carriageColorArray = TextureTo2DArray(carriageTexture);

            GenerateTerrainContour();
            SetupPlayers();
            FlattenTerrainBelowPlayers();
            CreateForeGround();
            //Apparently, you can set positions and other stuff in here from the get go. Nice to know.
            

           
        }
        //Function made for turning a texture2D to a color.
        private Color[,] TextureTo2DArray(Texture2D texture)
        {
            Color[] colors1D = new Color[texture.Width * texture.Height];
            texture.GetData(colors1D);
            Color[,] colors2D = new Color[texture.Width, texture.Height];
            for (int x = 0; x < texture.Width; x++)
            {
                for (int y = 0; y < texture.Height; y++)
                {
                    colors2D[x, y] = colors1D[x + y * texture.Width];
                }
            }

            return colors2D;
        }

        private void GenerateTerrainContour()
        {
            terrainContour = new int[screenWidth];
            float offset = screenHeight / 2;
            float peakheight = 100;
            float flatness = 70;
            //Parameters for randomizing Terrain
            double rand1 = randomizer.NextDouble() + 1;
            double rand2 = randomizer.NextDouble() + 2;
            double rand3 = randomizer.NextDouble() + 3;
            for (int x = 0; x < screenWidth; x++)
            {
                double height = peakheight / rand1 * Math.Sin((float)x / flatness * rand1 + rand1);
                height += peakheight / rand2 * Math.Sin((float)x / flatness * rand2 + rand2);
                height += peakheight / rand3 * Math.Sin((float)x / flatness * rand3 + rand3);
                height += offset;
                terrainContour[x] = (int)height;
            }
        }
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            ProcessKeyboard();
            UpdateRocket();

            base.Update(gameTime);
        }

        private void DrawText()
        {
            PlayerData singleplayer = Players[currentPlayer];
            int currentAngle = (int)MathHelper.ToDegrees(singleplayer.Angle);
            spriteBatch.DrawString(Font, "Cannon angle: " + currentAngle.ToString(), new Vector2(20, 20), singleplayer.Color);
            spriteBatch.DrawString(Font, "Cannon power: " + singleplayer.Power.ToString(), new Vector2(20, 45), singleplayer.Color);
            spriteBatch.DrawString(Font, "Player " + (currentPlayer +1) + "'s turn", new Vector2(20, 70), singleplayer.Color);// Look mommy, I did something by myself!
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            DrawScenery();
            DrawPlayers();
            DrawText();
            DrawRocket();
            DrawSmoke();
            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawScenery()
        {
            Rectangle screenRectangle = new Rectangle(0, 0, screenWidth, screenHeight);
            spriteBatch.Draw(bgTexture, screenRectangle, Color.White);
            spriteBatch.Draw(fgTexture, screenRectangle, Color.White);
        }
    }
}
