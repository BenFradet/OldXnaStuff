using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Platformer
{
    class Animation
    {
        public Texture2D Texture { get; private set; }
        public float FrameTime { get; private set; }
        public bool IsLooping { get; private set; }

        public int FrameWidth
        {
            get { return Texture.Height; }
        }

        public int FrameCount
        {
            get { return Texture.Width / FrameWidth; }
        }

        public int FrameHeight
        {
            get { return Texture.Height; }
        }

        public Animation(Texture2D texture, float frameTime, bool isLooping)
        {
            Texture = texture;
            FrameTime = frameTime;
            IsLooping = isLooping;
        }
    }
}
