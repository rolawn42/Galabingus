﻿using Microsoft.Xna.Framework;
using System;

// Matthew Rodriguez
// 2023, 3, 6
// Animation
// Empty - Empty Animation
// AnimationDuration - Duration of one frame in the animation
// Width - Total width of the anmation sprite
// Height - Total height of the animation sprite
// Play - Plays the animation

namespace Galabingus
{
    /// <summary>
    ///  Provides required 
    ///  information for a animation
    /// </summary>
    internal class Animation
    {
        private static Animation empty = null; // Singelton blank Animation
        private double animationTime;          // Total time ellapsed in the animation
        private float animationDuration;       // The frame duration of the animation
        private int width;                     // Width of the entire animation
        private int height;                    // Height of the entire animation
        private int spritesInAnimation;        // Number of indvidual sprites/frames in animation
        private int currentFrame;              // Current frame in animation

        /// <summary>
        ///  Empty is just a Empty Animation
        ///  Primarily the Empty is a singleton for blank animation
        /// </summary>
        public static Animation Empty
        {
            get
            {
                // When the singleton has not been defined
                // define the singleton
                if (empty != null)
                {
                    return empty;
                }
                else
                {
                    // Defines the singleton as a blank animation
                    empty = new Animation();
                    return empty;
                }
            }
        }

        /// <summary>
        ///  The Animation duration per frame
        /// </summary>
        public float AnimationDuration
        {
            // Allow the animaton frame
            // duration to be adjusted
            get
            {
                return animationDuration;
            }
            set
            {
                animationDuration = value;
            }
        }

        /// <summary>
        ///  The Animation sprite width
        /// </summary>
        public int Width
        {
            // Allow the animation sprite
            // width to be adjusted
            get
            {
                return width;
            }
            set
            {
                width = value;
            }
        }

        /// <summary>
        ///  The Animation sprite height
        /// </summary>
        public int Height
        {
            // Allow the animation
            // sprite height to be adjusted
            get
            {
                return height;
            }
            set
            {
                height = value;
            }
        }

        /// <summary>
        ///  ONLY for singleton
        /// </summary>
        private Animation()
        {
            this.animationTime = 0;
            this.animationDuration = 0;
            this.width = 0;
            this.height = 0;
            this.spritesInAnimation = 0;
            this.currentFrame = 1;
        }

        /// <summary>
        ///  Creates a Animation from height, width
        ///  and the spritesInAnimation
        /// </summary>
        /// <param name="width">The width of the entire animation sprite</param>
        /// <param name="height">The height of the entire animation sprite</param>
        /// <param name="spritesInAnimation">The number of sprite sin the animation</param>
        public Animation(int width, int height, int spritesInAnimation)
        {
            this.animationTime = 0;
            this.animationDuration = 0;
            this.width = width;
            this.height = height;
            this.spritesInAnimation = spritesInAnimation;
            this.currentFrame = 1;
        }

        /// <summary>
        ///  Creates a Animation from height, width
        ///  spritesInAnimation, and the currentFrame
        /// </summary>
        /// <param name="width">The width of the entire animation sprite</param>
        /// <param name="height">The height of the entire animation sprite</param>
        /// <param name="spritesInAnimation">The number of sprite sin the animation</param>
        /// <param name="currentFrame">The current frame in the aniomation</param>
        public Animation(int width, int height, int spritesInAnimation, int currentFrame)
        {
            this.animationTime = 0;
            this.animationDuration = 0;
            this.width = width;
            this.height = height;
            this.spritesInAnimation = spritesInAnimation;
            this.currentFrame = currentFrame;
        }

        /// <summary>
        ///  Plays the animation, requires gameTime to play
        /// </summary>
        /// <param name="gameTime">Game Time</param>
        /// <returns>
        ///  Transform of the frame from the animation sprite
        /// </returns>
        public Rectangle Play(GameTime gameTime)
        {
            // Increase the total anmation time
            animationTime += gameTime.ElapsedGameTime.TotalSeconds;

            // Check to see if the animation time has ellapsed past the animation duration
            if (animationTime >= animationDuration)
            {
                // If there are more fames in the animation
                // change the frame to the next frame
                // otherwise siwtch the frame to the first
                if (currentFrame + 1 > spritesInAnimation)
                {
                    currentFrame = 0;
                }
                else
                {
                    currentFrame++;
                }

                // Keep the ellpase offset but make sure
                // that we are a whole duration back
                animationTime -= animationDuration;
            }

            // Return the transform that is formed from the factor of current frame of the width of the sprite
            // The height is always 0, the x will be the frame offset
            return new Rectangle(
                (width / spritesInAnimation * currentFrame),
                0, +
                (int)Math.Round(width / (double)spritesInAnimation),
                height
            );
        }


        /// <summary>
        ///  Use this to select a specific sprite in the sprite sheet
        /// </summary>
        /// <param name="currentFrame">Specific sprite in the sprite sheet</param>
        /// <returns>Transform to view this currentFrame</returns>
        public Rectangle GetFrame(int currentFrame)
        {
            return new Rectangle(
                    (width / spritesInAnimation * currentFrame),
                    0, +
                    (int)Math.Round(width / (double)spritesInAnimation),
                    height
                );
        }
    }
}
