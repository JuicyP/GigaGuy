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
        /// Collision in GigaGuy(WorkingTitle) works by having 8 points around the player character:
        /// Four points on the sides and around the center of the character and one around each corner.
        /// If any of the points enter a cell in the tilemap occupied by a tile a corresponding action depending on which point entered the cell will be performed.
        /// The points also have a hierarchy, with the center points being checked before the corner points.
        /// NOTE: Why use an 8-point system over a 4-point? Because it allows for a much wider variety of shapes and sizes to be used as character hitboxes.
        /// It's also a much better idea to use a +4-point system if you want to use characters whose size exceeds the size of a single cell.
        /// </summary>
        private void HandleCollisions()
        {
            // Centerpoints
            if (!Player.IsDucking)
            {
                // Right-bottom center
                CheckForCollision(true, true, true);
                // Left-bottom center
                CheckForCollision(false, true, true);
            }
            // Right-top center
            CheckForCollision(true, false, true);
            // Left-top center
            CheckForCollision(false, false, true);
            // Corners
            // Right-bottom corner
            CheckForCollision(true, true, false);
            // Left-bottom corner
            CheckForCollision(false, true, false);
            // Right-top corner
            CheckForCollision(true, false, false);
            // Left-top corner
            CheckForCollision(false, false, false);
        }

        private void CheckForCollision(bool isRight, bool isBottom, bool isCenter)
        {
            float cornerOffSet = 6;
            float centerLineOffSet = 6;
            float tileX;
            float tileY;

            if (isCenter)
            {
                if (isRight)
                    tileX = FindGridCoordinate(Player.Hitbox.Right, true);
                else
                    tileX = FindGridCoordinate(Player.Hitbox.Left, true);
                float playerHeightOffSet = Player.Hitbox.Height / 4;
                if (Player.IsDucking)
                {
                    playerHeightOffSet = Player.Hitbox.Height / 2;
                    centerLineOffSet = -6;
                }
                if (isBottom)
                    tileY = FindGridCoordinate((Player.Hitbox.Bottom - playerHeightOffSet) - centerLineOffSet, false); // 3*x/4
                else
                    tileY = FindGridCoordinate((Player.Hitbox.Top + playerHeightOffSet) + centerLineOffSet, false); // x/4
            }
            else
            {
                if (isRight)
                    tileX = FindGridCoordinate(Player.Hitbox.Right - cornerOffSet, true);
                else
                    tileX = FindGridCoordinate(Player.Hitbox.Left + cornerOffSet, true);

                if (isBottom)
                    tileY = FindGridCoordinate(Player.Hitbox.Bottom, false);
                else
                    tileY = FindGridCoordinate(Player.Hitbox.Top, false);
            }
            foreach (Tile tile in tileMap)
            {
                if (tile.Hitbox.X == tileX && tile.Hitbox.Y == tileY)
                {
                    if (tile.Hitbox.Intersects(Player.Hitbox))
                    {
                        OnCollision(tile, isRight, isBottom, isCenter);
                    }
                }
            }
        }

        private float FindGridCoordinate(float coordinate, bool isX)
        {
            if (isX)
                return (float)Math.Floor(coordinate / gridCellWidth) * gridCellWidth;
            else
                return (float)Math.Floor(coordinate / gridCellHeight) * gridCellHeight;
        }

        private void OnCollision(Tile tile, bool isRight, bool isBottom, bool isCenter)
        {
            Vector2 playerNewPosition;
            Vector2 playerNewVelocity;

            if (tile != null)
            {
                if (isCenter)
                {
                    Player.IsOnWall = true;
                    if (isRight)
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
                    if (isBottom)
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
        }
    }
}