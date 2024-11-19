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
        public Rectangle Bounds;
        public Rectangle WindowBounds;
       public Invader(Vector2 position, Vector2 velocity, Rectangle windowbounds)
        {
            WindowBounds = windowbounds;
            Position = position;
            Velocity = velocity;
            Bounds = new Rectangle();
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
    public class InvadersGame : Game
    {
        enum State
        {
            Menu,
            Game,
            GameOver
        }

        private State gameState;

        private Random random = new Random();
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        Player player;
        List<Invader> invaders = new List<Invader>();
        List<Bullet> bullets = new List<Bullet>();

        Texture2D invaderTexture;
        Texture2D playerTexture;
        Texture2D bulletTexture;
        Texture2D explosionTexture;

        SoundEffect invaderDeath;
        SoundEffect explosion;
        SoundEffect playerShoot;

        int score = 0;

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
            for (int i = 0; i < 2000; i++)
            {
                invaders.Add(new Invader(new Vector2((i % (Window.ClientBounds.Width / 16))*16, (i % Window.ClientBounds.Width/16) * 4),new Vector2(2f, 0.4f), Window.ClientBounds));
            }

            Window.AllowUserResizing = true;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            invaderTexture = Content.Load<Texture2D>("invader");
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

            for (int i = 0;i < bullets.Count; i++)
            {
                bullets[i].WindowBounds = Window.ClientBounds;
                bullets[i].Update();
                for (int j = 0; j < invaders.Count; j++)
                {
                    if (invaders[j].Bounds.Intersects(bullets[i].Bounds))
                    {
                        invaders.RemoveAt(j);
                        invaderDeath.Play();
                        if (bullets[i].Explodes)
                        {
                            bullets[i].shouldDie = true;
                            explosion.Play();
                        }
                    }
                }
                if (bullets[i].shouldDie)
                {
                    bullets.RemoveAt(i);
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
                            gameState = State.Game;
                        }
                        break;
                    }
                case State.Game:
                {
                        UpdateGame();
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
                            _spriteBatch.Draw(invaderTexture, invaders[i].Position, Color.White);
                        }
                        for (int i = 0; i < bullets.Count; i++)
                        {
                            _spriteBatch.Draw(bulletTexture, bullets[i].Position, Color.White);
                        }
                        _spriteBatch.Draw(playerTexture, player.Position, Color.White);
                        break;
                    }
            }
            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}