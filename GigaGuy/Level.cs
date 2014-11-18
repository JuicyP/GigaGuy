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

        private Texture2D tileTexture;
        private string[] levelInText;
        private List<Tile> tileMap;

        private int gridCellWidth = 32;
        private int gridCellHeight = 32;

        public Level()
        {
            levelInText = File.ReadAllLines("Level.txt");
            tileMap = new List<Tile>();
        }

        public void LoadContent(ContentManager Content)
        {
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
        }

        public void Update(GameTime gameTime)
        {
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (Tile tile in tileMap)
            {
                tile.Draw(spriteBatch);
            }
        }
    }
}
