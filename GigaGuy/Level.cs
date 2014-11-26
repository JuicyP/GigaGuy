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

        /// <summary>
        /// NOTE: Outdated info
        /// TODO: Rewrite
        /// Collision in GigaGuy(WorkingTitle) works by having 8 points around the player character:
        /// Four points on the sides and around the center of the character and one around each corner.
        /// If any of the points enter a cell in the tilemap occupied by a tile a corresponding action depending on which point entered the cell will be performed.
        /// The points also have a hierarchy, with the center points being checked before the corner points.
        /// NOTE: The characters maximum terminal velocity can't exceed the height of the center points, otherwise it will break.
        /// </summary>
        private void HandleCollisions()
        {
            float endOffSet = 6;
            float sideOffSet = 6;
            float tileX;
            float tileY;
            int tilesWide = (int)Math.Ceiling(Player.Hitbox.Width / gridCellWidth);
            int tilesHigh = (int)Math.Ceiling(Player.Hitbox.Height / gridCellHeight);
            Tile[] collidingSideTiles = new Tile[(1 + tilesWide) * 2]; // The maximum amount of tiles a character of given height and width can collide with.
            Tile[] collidingEndTiles = new Tile[(1 + tilesHigh) * 2];
            // Sides
            for (float i = 0; i <= tilesHigh; i += 2)
            {
                tileY = FindGridCoordinate(Player.Position.Y + ((1 + i) / (tilesHigh * 2)) * Player.Hitbox.Height, false);
                for (float j = 0; j <= Player.Hitbox.Width; j += Player.Hitbox.Width)
                {
                    tileX = FindGridCoordinate(Player.Position.X + j, true);
                    for (int k = 0; k < collidingSideTiles.Length; k++) // Maybe make a method for this
                    {
                        if (collidingSideTiles[k] == null)
                        {
                            collidingSideTiles[k] = CheckForCollision(tileX, tileY);
                        }
                    }
                }
            }
            CheckArrayForCollidingTiles(collidingSideTiles, true);

            // Ends
            for (float i = 0; i <= tilesWide; i++)
            {
                tileX = FindGridCoordinate(Player.Position.X + (i / tilesWide) * Player.Hitbox.Width, true);
                for (float j = 0; j <= Player.Hitbox.Height; j += Player.Hitbox.Height)
                {
                    tileY = FindGridCoordinate(Player.Position.Y + j, false);
                    for (int k = 0; k < collidingEndTiles.Length; k++)
                    {
                        if (collidingEndTiles[k] == null)
                        {
                            collidingEndTiles[k] = CheckForCollision(tileX, tileY);
                        }
                    }
                }
            }
            CheckArrayForCollidingTiles(collidingEndTiles, false);
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

        private float FindGridCoordinate(float coordinate, bool isX)
        {
            if (isX)
                return (float)Math.Floor(coordinate / gridCellWidth) * gridCellWidth;
            else
                return (float)Math.Floor(coordinate / gridCellHeight) * gridCellHeight;
        }

        private void OnCollision(Tile tile, bool isSide)
        {
            Vector2 playerNewPosition;
            Vector2 playerNewVelocity;

            if (isSide)
            {
                Player.IsOnWall = true;
                if (Player.Hitbox.X < tile.Hitbox.X)
                {
                    playerNewPosition = new Vector2(tile.Hitbox.Left - Player.Hitbox.Width, Player.Position.Y);
                    playerNewVelocity = new Vector2(0, Player.Velocity.Y);
                    Player.IsOnRightWall = true;
                }
                else
                {
                    playerNewPosition = new Vector2(tile.Hitbox.Right, Player.Position.Y);
                    playerNewVelocity = new Vector2(0, Player.Velocity.Y);
                    Player.IsOnRightWall = false;
                }
            }
            else
            {
                if (Player.Hitbox.Y < tile.Hitbox.Y)
                {
                    playerNewPosition = new Vector2(Player.Position.X, tile.Hitbox.Top - Player.Hitbox.Height);
                    playerNewVelocity = new Vector2(Player.Velocity.X, 0);
                    Player.IsOnGround = true;
                }
                else
                {
                    playerNewPosition = new Vector2(Player.Position.X, tile.Hitbox.Bottom);
                    playerNewVelocity = new Vector2(Player.Velocity.X, 0);
                    Player.TerminateJump();
                }
            }
            Player.Position = playerNewPosition;
            Player.Velocity = playerNewVelocity;
        }

        private void CheckArrayForCollidingTiles(Array collidingTiles, bool isSide)
        {
            foreach (Tile tile in collidingTiles)
            {
                if (tile != null)
                {
                    OnCollision(tile, isSide);
                }
            }
        }
    }
}