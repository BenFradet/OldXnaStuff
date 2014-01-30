using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Platformer
{
    enum TileCollision
    {
        Passable = 0,
        Impassable = 1,
        Platform = 2,
        Question = 3
    }

    struct Tile
    {
        public Texture2D Texture;
        public TileCollision Collision;
    
        public const int Width = 32;
        public const int Height = 32;

        public static readonly Vector2 Size = new Vector2(Width, Height);

        public Tile(Texture2D texture, TileCollision collision)
        {
            this.Texture = texture;
            this.Collision = collision;
        }
    }
}
