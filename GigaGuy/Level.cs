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
                        Tile tile = new Tile(gridCellWidth, gridCellHeight);
                        tile.Texture = tileTexture;
                        tile.Position = new Vector2(j * gridCellWidth, i * gridCellHeight);
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

            CheckForCollisions();

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (Tile tile in tileMap)
            {
                tile.Draw(spriteBatch);
            }

            Player.Draw(spriteBatch);
        }

        private void CheckForCollisions() // TODO: Refactoring
        {
            
            // Top-left centerline
            int gridColumn = (Player.Hitbox.Left / gridCellWidth) * gridCellWidth;
            int gridRow = ((Player.Hitbox.Top + Player.Hitbox.Height / 4) / gridCellHeight) * gridCellHeight;

            foreach (Tile tile in tileMap)
            {
                if (tile.Position.X == gridColumn && tile.Position.Y == gridRow)
                {
                    if (tile.Hitbox.Intersects(Player.Hitbox))
                    {
                        Player.Position = new Vector2(tile.Hitbox.Right, Player.Position.Y);
                    }
                }
            }

            // Top-right centerline
            gridColumn = (Player.Hitbox.Right / gridCellWidth) * gridCellWidth;

            foreach (Tile tile in tileMap)
            {
                if (tile.Position.X == gridColumn && tile.Position.Y == gridRow)
                {
                    if (tile.Hitbox.Intersects(Player.Hitbox))
                    {
                        Player.Position = new Vector2(tile.Hitbox.Left - Player.Hitbox.Width, Player.Position.Y);
                    }
                }
            }

            // Botton-left centerline
            gridColumn = (Player.Hitbox.Left / gridCellWidth) * gridCellWidth;
            gridRow = ((Player.Hitbox.Top + 3 * Player.Hitbox.Height / 4) / gridCellHeight) * gridCellHeight;

            foreach (Tile tile in tileMap)
            {
                if (tile.Position.X == gridColumn && tile.Position.Y == gridRow)
                {
                    if (tile.Hitbox.Intersects(Player.Hitbox))
                    {
                        Player.Position = new Vector2(tile.Hitbox.Right, Player.Position.Y);
                    }
                }
            }

            // Botton-right centerline
            gridColumn = (Player.Hitbox.Right / gridCellWidth) * gridCellWidth;

            foreach (Tile tile in tileMap)
            {
                if (tile.Position.X == gridColumn && tile.Position.Y == gridRow)
                {
                    if (tile.Hitbox.Intersects(Player.Hitbox))
                    {
                        Player.Position = new Vector2(tile.Hitbox.Left - Player.Hitbox.Width, Player.Position.Y);
                    }
                }
            }           

            // Top-left corner
            gridColumn = (Player.Hitbox.Left / gridCellWidth) * gridCellWidth;
            gridRow = (Player.Hitbox.Top / gridCellHeight) * gridCellHeight;

            foreach (Tile tile in tileMap)
            {
                if (tile.Position.X == gridColumn && tile.Position.Y == gridRow)
                {
                    if (tile.Hitbox.Intersects(Player.Hitbox))
                    {
                        Player.Position = new Vector2(Player.Position.X, tile.Hitbox.Bottom);
                    }
                }
            }

            // Top-right corner
            gridColumn = (Player.Hitbox.Right / gridCellWidth) * gridCellWidth;

            foreach (Tile tile in tileMap)
            {
                if (tile.Position.X == gridColumn && tile.Position.Y == gridRow)
                {
                    if (tile.Hitbox.Intersects(Player.Hitbox))
                    {
                        Player.Position = new Vector2(Player.Position.X, tile.Hitbox.Bottom);
                    }
                }
            }

            // Bottom-left corner
            gridColumn = (Player.Hitbox.Left / gridCellWidth) * gridCellWidth;
            gridRow = (Player.Hitbox.Bottom / gridCellHeight) * gridCellHeight;

            foreach (Tile tile in tileMap)
            {
                if (tile.Position.X == gridColumn && tile.Position.Y == gridRow)
                {
                    if (tile.Hitbox.Intersects(Player.Hitbox))
                    {
                        Player.Position = new Vector2(Player.Position.X, tile.Hitbox.Top - Player.Hitbox.Height);
                    }
                }
            }

            // Bottom-right corner
            gridColumn = (Player.Hitbox.Right / gridCellWidth) * gridCellWidth;

            foreach (Tile tile in tileMap)
            {
                if (tile.Position.X == gridColumn && tile.Position.Y == gridRow)
                {
                    if (tile.Hitbox.Intersects(Player.Hitbox))
                    {
                        Player.Position = new Vector2(Player.Position.X, tile.Hitbox.Top - Player.Hitbox.Height);
                    }
                }
            }
        }
    }
}
