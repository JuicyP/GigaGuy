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
        public Player Player { get; set; }

        private Texture2D tileTexture;
        private List<Tile> tileMap;

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
        }

        public void Update(GameTime gameTime)
        {
            Player.Update(gameTime);
            HandleCollisions();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (Tile tile in tileMap)
            {
                tile.Draw(spriteBatch);
            }
            Player.Draw(spriteBatch);
        }

        private void HandleCollisions()
        {

            // Top-left centerline
            float tileX = (float)Math.Floor(Player.Hitbox.Left / gridCellWidth) * gridCellWidth; // Consider changing to int
            float tileY = (float)Math.Floor((Player.Hitbox.Top + Player.Hitbox.Height / 4) / gridCellHeight) * gridCellHeight;
            Tile tile = CheckForCollision(tileX, tileY);

            if (tile != null)
            {
                Player.Position = new Vector2(tile.Hitbox.Right, Player.Position.Y);
                Player.Velocity = new Vector2(0, Player.Velocity.Y);
            }

            // Top-right centerline
            tileX = (float)Math.Floor(Player.Hitbox.Right / gridCellWidth) * gridCellWidth;
            tile = CheckForCollision(tileX, tileY);

            if (tile != null)
            {
                Player.Position = new Vector2(tile.Hitbox.Left - Player.Hitbox.Width, Player.Position.Y);
                Player.Velocity = new Vector2(0, Player.Velocity.Y);
            }

            // Bottom-left centerline
            tileX = (float)Math.Floor(Player.Hitbox.Left / gridCellWidth) * gridCellWidth;
            tileY = (float)Math.Floor((Player.Hitbox.Top + - 5 + 3 * Player.Hitbox.Height / 4) / gridCellHeight) * gridCellHeight;
            tile = CheckForCollision(tileX, tileY);

            if (tile != null)
            {
                Player.Position = new Vector2(tile.Hitbox.Right, Player.Position.Y);
                Player.Velocity = new Vector2(0, Player.Velocity.Y);
            }

            // Bottom-right centerline
            tileX = (float)Math.Floor(Player.Hitbox.Right / gridCellWidth) * gridCellWidth;
            tile = CheckForCollision(tileX, tileY);

            if (tile != null)
            {
                Player.Position = new Vector2(tile.Hitbox.Left - Player.Hitbox.Width, Player.Position.Y);
                Player.Velocity = new Vector2(0, Player.Velocity.Y);
            }

            // Top-left corner
            int cornerOffSet = 0;
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
            bool collision = false;

            if (tile != null)
            {
                collision = true;
                Player.Position = new Vector2(Player.Position.X, tile.Hitbox.Top - Player.Hitbox.Height);
                Player.Velocity = new Vector2(Player.Velocity.X, 0);
            }

            // Bottom-right corner
            tileX = (float)Math.Floor((Player.Hitbox.Right - cornerOffSet) / gridCellWidth) * gridCellWidth;
            tile = CheckForCollision(tileX, tileY);

            if (tile != null)
            {
                collision = true;
                Player.Position = new Vector2(Player.Position.X, tile.Hitbox.Top - Player.Hitbox.Height);
                Player.Velocity = new Vector2(Player.Velocity.X, 0);
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