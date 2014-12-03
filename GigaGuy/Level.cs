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
            Texture2D tileTexture = Content.Load<Texture2D>("tile");
            Texture2D _45RightSlopeTexture = Content.Load<Texture2D>("_45RightSlope");
            Texture2D _45LeftSlopeTexture = Content.Load<Texture2D>("_45LeftSlope");
            string row;

            for (int i = 0; i < levelInText.Length; i++)
            {
                row = levelInText[i];

                for (int j = 0; j < row.Length; j++)
                {
                    if (row[j].Equals('1'))
                    {
                        RectangleF hitbox = new RectangleF(j * gridCellWidth, i * gridCellHeight, gridCellWidth, gridCellHeight);
                        Tile tile = new Tile(tileTexture, hitbox);
                        tileMap.Add(tile);
                    }
                    else if (row[j].Equals('R'))
                    {
                        RectangleF hitbox = new RectangleF(j * gridCellWidth, i * gridCellHeight, gridCellWidth, gridCellHeight);
                        Slope slope = new Slope(_45RightSlopeTexture, hitbox, SlopeType._45Right);
                        tileMap.Add(slope);
                    }
                    else if (row[j].Equals('L'))
                    {
                        RectangleF hitbox = new RectangleF(j * gridCellWidth, i * gridCellHeight, gridCellWidth, gridCellHeight);
                        Slope slope = new Slope(_45LeftSlopeTexture, hitbox, SlopeType._45Left);
                        tileMap.Add(slope);
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
            float sideOffSet;
            float sideOffSetDefault = 6;
            float endOffSet;
            float endOffSetDefault = 6;
            float tileX;
            float tileY;
            int tilesWide = (int)Math.Ceiling(Player.Hitbox.Width / gridCellWidth);
            int tilesHigh = (int)Math.Ceiling(Player.Hitbox.Height / gridCellHeight);
            Tile[] collidingSlopeTiles = new Tile[2];
            Tile[] collidingSideTiles = new Tile[tilesHigh * 2]; // The maximum amount of tiles a character of given height and width can collide with.
            Tile[] collidingEndTiles = new Tile[(1 + tilesWide) * 2];

            // Slopes
            tileX = FindGridCoordinate(Player.Hitbox.Center.X, true);
            tileY = FindGridCoordinate(Player.Hitbox.Center.Y, false);
            collidingSlopeTiles[0] = CheckForCollision(tileX, tileY);
            tileY = FindGridCoordinate(Player.Hitbox.Bottom - 2, false);
            collidingSlopeTiles[1] = CheckForCollision(tileX, tileY);

            foreach (Tile tile in collidingSlopeTiles)
            {
                if (tile is Slope)
                {
                    OnSlopeCollision(tile);
                }
            }
            // Sides
            for (float i = 0; i <= tilesHigh; i += 2)
            {
                if ((1 + i) / (tilesHigh * 2) < 0.5f)
                    sideOffSet = 0;
                else if ((1 + i) / (tilesHigh * 2) > 0.5f)
                    sideOffSet = -sideOffSetDefault;
                else
                    sideOffSet = -sideOffSetDefault; // Compromise for 1x1 characters
                tileY = FindGridCoordinate(Player.Position.Y + ((1 + i) / (tilesHigh * 2)) * Player.Hitbox.Height + sideOffSet, false);
                for (float j = 0; j <= 1; j++)
                {
                    tileX = FindGridCoordinate(Player.Position.X + j * Player.Hitbox.Width, true);
                    for (int k = 0; k < collidingSideTiles.Length; k++) // Maybe make a method for this
                    {
                        if (collidingSideTiles[k] == null)
                        {
                            collidingSideTiles[k] = CheckForCollision(tileX, tileY);
                            break;
                        }
                    }
                }
            }
            CheckArrayForCollidingTiles(collidingSideTiles, true);

            // Ends
            for (float i = 0; i <= tilesWide; i++)
            {
                if ((i / tilesWide) < 0.5f)
                    endOffSet = endOffSetDefault;
                else if ((i / tilesWide) > 0.5f)
                    endOffSet = -endOffSetDefault;
                else
                    endOffSet = 0;

                tileX = FindGridCoordinate(Player.Position.X + endOffSet + (i / tilesWide) * Player.Hitbox.Width, true);
                for (float j = 0; j <= 1; j++)
                {
                    tileY = FindGridCoordinate(Player.Position.Y + j * Player.Hitbox.Height, false);
                    for (int k = 0; k < collidingEndTiles.Length; k++)
                    {
                        if (collidingEndTiles[k] == null)
                        {
                            collidingEndTiles[k] = CheckForCollision(tileX, tileY);
                            break;
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
                    Player.IsJumping = false;
                }
            }
            Player.Position = playerNewPosition;
            Player.Velocity = playerNewVelocity;
        }

        private void CheckArrayForCollidingTiles(Array collidingTiles, bool isSide)
        {
            foreach (Tile tile in collidingTiles)
            {
                if (tile != null && !(tile is Slope))
                {
                    OnCollision(tile, isSide);
                }
            }
        }

        private void OnSlopeCollision(Tile tile)
        {
            float slopeHeight;
            Slope slope = (Slope)tile;
            if (slope.slopeType == SlopeType._45Right)
            {
                slopeHeight = Player.Hitbox.Center.X - slope.Hitbox.X;
            }
            else
            {
                slopeHeight = slope.Hitbox.Height - (Player.Hitbox.Center.X - slope.Hitbox.X);
            }
            if (Player.Hitbox.Bottom > slopeHeight)
            {
                Player.Position = new Vector2(Player.Position.X, slope.Hitbox.Bottom - slopeHeight - Player.Hitbox.Height);
                Player.Velocity = new Vector2(Player.Velocity.X, 0);
                Player.IsOnGround = true;
            }
        }
    }
}