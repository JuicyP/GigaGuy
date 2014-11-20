using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GigaGuy
{
    class Level
    {
        public Player Player { get; private set; }

        private Texture2D tileTexture;
        private List<Tile> tileMap;
        private Camera camera;

        private const int gridCellWidth = 32;
        private const int gridCellHeight = 32;

        private string levelPath;

        public Level(string levelPath)
        {
            this.levelPath = levelPath;
            tileMap = new List<Tile>();
        }

        public void LoadContent(ContentManager Content)
        {
            string[] levelInText = File.ReadAllLines(levelPath);
            tileTexture = Content.Load<Texture2D>("tile");
            string row;

            for (int i = 0; i < levelInText.Length; i++)
            {
                row = levelInText[i];

                for (int j = 0; j < row.Length; j++)
                {
                    if (row[j].Equals('1'))
                    {
                        RectangleF hitbox = new RectangleF(j * gridCellWidth, i * gridCellHeight, gridCellWidth, gridCellHeight);
                        Tile tile = new Tile(hitbox);
                        tile.Texture = tileTexture;
                        tileMap.Add(tile);
                    }
                }
            }
            Player = new Player();
            Player.LoadContent(Content);
            camera = new Camera();
        }

        public void Update(GameTime gameTime)
        {
            Player.Update(gameTime);
            HandleCollisions();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Vector2 offSet = camera.CalculateOffSet(Player);
            foreach (Tile tile in tileMap)
            {
                tile.Draw(spriteBatch, offSet);
            }
            Player.Draw(spriteBatch, offSet);
        }

        private void HandleCollisions()
        {
            // Centerlines
            bool collision = false;
            // Top-left centerline
            float tileX = (float)Math.Floor(Player.Hitbox.Left / gridCellWidth) * gridCellWidth; // Consider changing to int
            float tileY = (float)Math.Floor((Player.Hitbox.Top + Player.Hitbox.Height / 4) / gridCellHeight) * gridCellHeight;
            Tile tile = CheckForCollision(tileX, tileY);

            if (tile != null)
            {
                Player.Position = new Vector2(tile.Hitbox.Right, Player.Position.Y);
                Player.Velocity = new Vector2(0, Player.Velocity.Y);
                collision = true;
                Player.IsOnRightWall = false;
            }

            // Top-right centerline
            tileX = (float)Math.Floor(Player.Hitbox.Right / gridCellWidth) * gridCellWidth;
            tile = CheckForCollision(tileX, tileY);

            if (tile != null)
            {
                Player.Position = new Vector2(tile.Hitbox.Left - Player.Hitbox.Width, Player.Position.Y);
                Player.Velocity = new Vector2(0, Player.Velocity.Y);
                collision = true;
                Player.IsOnRightWall = true;
            }

            // Bottom-left centerline
            tileX = (float)Math.Floor(Player.Hitbox.Left / gridCellWidth) * gridCellWidth;
            tileY = (float)Math.Floor((Player.Hitbox.Top + - 5 + 3 * Player.Hitbox.Height / 4) / gridCellHeight) * gridCellHeight;
            tile = CheckForCollision(tileX, tileY);

            if (tile != null)
            {
                Player.Position = new Vector2(tile.Hitbox.Right, Player.Position.Y);
                Player.Velocity = new Vector2(0, Player.Velocity.Y);
                collision = true;
                Player.IsOnRightWall = false;
            }

            // Bottom-right centerline
            tileX = (float)Math.Floor(Player.Hitbox.Right / gridCellWidth) * gridCellWidth;
            tile = CheckForCollision(tileX, tileY);

            if (tile != null)
            {
                Player.Position = new Vector2(tile.Hitbox.Left - Player.Hitbox.Width, Player.Position.Y);
                Player.Velocity = new Vector2(0, Player.Velocity.Y);
                collision = true;
                Player.IsOnRightWall = true;
            }

            Player.IsOnWall = collision;

            //Corners
            collision = false;
            // Top-left corner
            int cornerOffSet = 0; // For tweaking purposes
            tileX = (float)Math.Floor((Player.Hitbox.Left + cornerOffSet) / gridCellWidth) * gridCellWidth;
            tileY = (float)Math.Floor(Player.Hitbox.Top / gridCellHeight) * gridCellHeight;
            tile = CheckForCollision(tileX, tileY);

            if (tile != null)
            {
                Player.Position = new Vector2(Player.Position.X, tile.Hitbox.Bottom);
                Player.Velocity = new Vector2(Player.Velocity.X, 0);
            }

            // Top-right corner
            tileX = (float)Math.Floor((Player.Hitbox.Right - cornerOffSet) / gridCellWidth) * gridCellWidth;
            tile = CheckForCollision(tileX, tileY);

            if (tile != null)
            {
                Player.Position = new Vector2(Player.Position.X, tile.Hitbox.Bottom);
                Player.Velocity = new Vector2(Player.Velocity.X, 0);
            }
            
            // Bottom-left corner
            tileX = (float)Math.Floor((Player.Hitbox.Left + cornerOffSet) / gridCellWidth) * gridCellWidth;
            tileY = (float)Math.Floor(Player.Hitbox.Bottom / gridCellHeight) * gridCellHeight;
            tile = CheckForCollision(tileX, tileY);
            
            if (tile != null)
            {             
                Player.Position = new Vector2(Player.Position.X, tile.Hitbox.Top - Player.Hitbox.Height);
                Player.Velocity = new Vector2(Player.Velocity.X, 0);
                collision = true;
            }

            // Bottom-right corner
            tileX = (float)Math.Floor((Player.Hitbox.Right - cornerOffSet) / gridCellWidth) * gridCellWidth;
            tile = CheckForCollision(tileX, tileY);

            if (tile != null)
            {
                Player.Position = new Vector2(Player.Position.X, tile.Hitbox.Top - Player.Hitbox.Height);
                Player.Velocity = new Vector2(Player.Velocity.X, 0);
                collision = true;
            }
            Player.IsOnGround = collision;
        }

        private Tile CheckForCollision(float tileX, float tileY)
        {
            foreach (Tile tile in tileMap)
            {
                if (tile.Hitbox.X == tileX && tile.Hitbox.Y == tileY)
                {
                    if (tile.Hitbox.Intersects(Player.Hitbox))
                    {
                        return tile;
                    }
                }
            }
            return null;
        }
    }
}