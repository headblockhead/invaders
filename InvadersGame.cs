using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Security.Cryptography;

namespace Invaders
{ 
    public class Player
    {
        public enum Direction
        {
            Left = -1,
            None= 0,
            Right = 1,
        }
        public Direction MovementDirection;
        public int Speed;
        public Vector2 Position;
        public bool FiredWeapon;
        public Rectangle WindowBounds;
        public Player(int speed, Rectangle windowbounds)
        {
            Speed = speed;
            WindowBounds = windowbounds;
        }
        public void Update()
        {
            Position.X += ((int)MovementDirection * Speed);
            Position.Y = WindowBounds.Height - 64;
            if (Position.X < 0)
            {
                Position.X = 0;
            }
            else if (Position.X > WindowBounds.Width - 128)
            {
                Position.X = WindowBounds.Width - 128;
            }
        }
    }
    public class Invader
    {
        private Random random = new Random();
        public Vector2 Position = Vector2.Zero;
        public Vector2 Velocity = Vector2.Zero;
        public bool shouldDie = false;
        public bool Mutated = false;
        public Rectangle Bounds;
        public Rectangle WindowBounds;
       public Invader(Vector2 position, Vector2 velocity, Rectangle windowbounds, bool mutated)
        {
            WindowBounds = windowbounds;
            Position = position;
            Velocity = velocity;
            Bounds = new Rectangle();
            Mutated = mutated;
            Bounds.Width = 16;
            Bounds.Height = 16;
            Bounds.Location = new Point((int)Position.X, (int)Position.Y);
        }
        public void Update()
        {
            Bounds.Location = new Point((int)Position.X, (int)Position.Y);
            Position += Velocity * new Vector2((float)random.NextDouble(), (float)random.NextDouble());
            if (Position.X < 0)
            {
                Velocity.X *= -1;
                Position.X = 0;
            }
            else if (Position.X >= WindowBounds.Width - 16)
            {
               Velocity.X *= -1;
                Position.X = WindowBounds.Width - 16;
            }
            if (Position.Y < 0)
            {
                Position.Y = 0;
                Velocity.Y *= -1;
            }
            if (Position.Y > WindowBounds.Height - 16)
            {
                Position.Y = WindowBounds.Height - 16;
                Velocity.Y *= -1;
            }
        }
    }

    public class Bullet
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public Rectangle Bounds;
        public Rectangle WindowBounds;
        public bool shouldDie;
        public bool Explodes;
        public Bullet(Vector2 position, Vector2 velocity,Rectangle windowbounds, bool explodes = false)
        {
            Position = position;
            Velocity = velocity;
            WindowBounds = windowbounds;
            shouldDie = false;
            Bounds = new Rectangle();
            Bounds.Width = 16;
            Bounds.Height = 32;
            Bounds.Location = new Point((int)Position.X, (int)Position.Y);
            Explodes = explodes;
        }
        public void Update()
        {
            Bounds.Location = new Point((int)Position.X, (int)Position.Y);
            Position += Velocity;
            if (Position.Y < 0)
            {
                shouldDie = true;
            }
        }
        }

    public class Explosion
    {
        public Vector2 Position;
        public Rectangle Bounds;
        private int timer;
        public int ExplosionSpeed;
        public bool Exploding = true;
        public Explosion(Vector2 position, int explosionSpeed)
        {
            Position = position;
            ExplosionSpeed = explosionSpeed;
            Bounds = new Rectangle();
            Bounds.Width = 1;
            Bounds.Height = 1;
            Bounds.Location = new Point((int)Position.X, (int)Position.Y);
        }
        public void Update() {
            timer++;
            Bounds.Width+=ExplosionSpeed;
            Bounds.Height+= ExplosionSpeed;
            Bounds.Location = new Point((int)Position.X- Bounds.Width/2, (int)Position.Y - Bounds.Width / 2);
            if (timer > 64)
            {
                Exploding = false;
            }
        }
    }
    public class InvadersGame : Game
    {
        enum State
        {
            Menu,
            Game,
            LevelSplash,
            GameOver
        }

        private State gameState;

        private Random random = new Random();
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        Player player;
        List<Invader> invaders = new List<Invader>();
        List<Bullet> bullets = new List<Bullet>();
        List<Explosion> explosions = new List<Explosion>();

        Texture2D invaderTexture;
        Texture2D playerTexture;
        Texture2D bulletTexture;
        Texture2D explosionTexture;

        SoundEffect invaderDeath;
        SoundEffect explosion;
        SoundEffect playerShoot;

        int score = 0;
        int level = 0;
        int lives = 5;
        int[] levels = {
            128,
            256,
            512,
            4096
        };

        public InvadersGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 800;
            _graphics.ApplyChanges();

            player = new Player(8, Window.ClientBounds);
            for (int i = 0; i < levels[level]; i++)
            {
                invaders.Add(new Invader(new Vector2((i % (Window.ClientBounds.Width / 16))*16, (i % Window.ClientBounds.Width/16) * 4),new Vector2(2f, 0.4f), Window.ClientBounds, false));
            }

            Window.AllowUserResizing = true;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            invaderTexture = Content.Load<Texture2D>("invader");
            explosionTexture = Content.Load<Texture2D>("explosionImage");
            playerTexture = Content.Load<Texture2D>("player");
            bulletTexture = Content.Load<Texture2D>("bullet");

            invaderDeath = Content.Load<SoundEffect>("invaderDeath");
            playerShoot = Content.Load<SoundEffect>("playerShoot");
            explosion = Content.Load<SoundEffect>("explosion");
        }

        public void UpdateGame()
        {
            player.WindowBounds = Window.ClientBounds;

            for (int i = 0; i < invaders.Count; i++)
            {
                invaders[i].WindowBounds = Window.ClientBounds;
                invaders[i].Update();
            }

            for (int i = 0; i < explosions.Count; i++)
            {
                explosions[i].Update();
                if (!explosions[i].Exploding)
                {
                   explosions.RemoveAt(i);
                   break;
                }
                for (int j = 0; j < invaders.Count; j++)
                {
                    if (invaders[j].Bounds.Intersects(explosions[i].Bounds) && !invaders[j].Mutated)
                    {
                        if (random.Next(0, 128) == 0)
                        {
                            Vector2 newPosition = new Vector2(explosions[i].Position.X + random.Next(-explosions[i].Bounds.Width/2, explosions[i].Bounds.Width/2), explosions[i].Position.Y + random.Next(-explosions[i].Bounds.Height/2, explosions[i].Bounds.Height/2));
                            invaders.Add(new Invader(newPosition, new Vector2(4f, 0.5f), Window.ClientBounds, true));
                        }
                        invaders[j].shouldDie = true;
                        invaderDeath.Play();
                    }
                    }
                }

            for (int i = 0;i < bullets.Count; i++)
            {
                bullets[i].WindowBounds = Window.ClientBounds;
                bullets[i].Update();
                for (int j = 0; j < invaders.Count; j++)
                {
                    if (invaders[j].Bounds.Intersects(bullets[i].Bounds))
                    {
                        if (bullets[i].Explodes)
                        {
                            if (!invaders[j].Mutated)
                            {
                                invaders[j].shouldDie = true;
                                invaderDeath.Play();
                            }
                            explosion.Play();
                            explosions.Add(new Explosion(bullets[i].Position, 3));
                        }
                        else
                        {
                            invaders[j].shouldDie = true;
                            invaderDeath.Play();
                        }
                        bullets[i].shouldDie = true;
                    }
                }
                if (bullets[i].shouldDie)
                {
                    bullets.RemoveAt(i);
                }
            }

            bool removedOne = true;
            while (removedOne)
            {
                removedOne = false;
                for (int i = 0; i < invaders.Count; i++)
                {
                    if (invaders[i].shouldDie)
                    {
                        invaders.RemoveAt(i);
                        removedOne = true;
                        break;
                    }
                }
            }

            player.MovementDirection = Player.Direction.None;
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                player.MovementDirection = Player.Direction.Left;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                player.MovementDirection = Player.Direction.Right;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                if (!player.FiredWeapon)
                {
                    playerShoot.Play();
                    player.FiredWeapon = true;
                    bullets.Add(new Bullet(new Vector2(player.Position.X+56, player.Position.Y), new Vector2(0, -24), Window.ClientBounds));
                }
            } 
            if (Keyboard.GetState().IsKeyDown(Keys.B))
            {
                if (!player.FiredWeapon)
                {
                    playerShoot.Play();
                    player.FiredWeapon = true;
                    bullets.Add(new Bullet(new Vector2(player.Position.X + 56, player.Position.Y), new Vector2(0, -24), Window.ClientBounds, true));
                }
            }
            if (!Keyboard.GetState().IsKeyDown(Keys.B) && !Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                player.FiredWeapon = false;
            }
            player.Update();
        }

        int splashUpdates = 0;
        int lastLevel = -1;
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            switch (gameState)
            {
                case State.Menu:
                    {
                        if (Keyboard.GetState().IsKeyDown(Keys.S))
                        {
                            gameState = State.LevelSplash;
                        }
                        break;
                    }
                case State.LevelSplash:
                    {
                        splashUpdates++;
                        if (splashUpdates > 2000)
                        {
                            lastLevel = level;
                            gameState = State.Game;
                        }
                        break;
                    }
                case State.Game:
                {
                        UpdateGame();
                        if (lives == 0)
                        {
                            gameState = State.GameOver;
                            break;
                        }
                        if (level != lastLevel)
                        {
                            gameState = State.LevelSplash;
                        }
                        break;
                }
                case State.GameOver:
                    {
                        if (Keyboard.GetState().IsKeyDown(Keys.R))
                        {
                            gameState = State.Game;
                        }
                        if (Keyboard.GetState().IsKeyDown(Keys.M))
                        {
                            gameState = State.Menu;
                        }
                        break;
                    }
            }

            _graphics.ApplyChanges();
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin(SpriteSortMode.Deferred,BlendState.AlphaBlend,SamplerState.LinearWrap, DepthStencilState.Default,RasterizerState.CullNone,null,null);
            switch (gameState)
            {
                case State.Game:
                    {
                        for (int i = 0; i < invaders.Count; i++)
                        {
                            _spriteBatch.Draw(invaderTexture, invaders[i].Position, invaders[i].Mutated? new Color(255, 64, 255, 232) : Color.White);
                        }
                        for (int i = 0; i < bullets.Count; i++)
                        {
                            _spriteBatch.Draw(bulletTexture, bullets[i].Position, Color.White);
                        }
                        for (int i = 0; i < explosions.Count; i++)
                        {
                            _spriteBatch.Draw(explosionTexture, explosions[i].Bounds, Color.White);
                        }
                        _spriteBatch.Draw(playerTexture, player.Position, Color.White);
                        break;
                    }
                case State.LevelSplash:
                    {

                        break;
                    }
            }
            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}