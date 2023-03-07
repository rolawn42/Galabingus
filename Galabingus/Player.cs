﻿using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

// Matthew Rodriguez
// 2023, 3, 7
// Player
// PlayerStates - Player translations states
// Position - position of the player
// Sprite - player sprite
// Scale - scale of the player
// Animation - animation for the player
// Collider - collider for the player
// Update - updates player FSM
// Shoot - triggered when the player presses the shoot button
// Draw - draws the player to the screen

namespace Galabingus
{
    /// <summary>
    ///  Player translation states
    /// </summary>
    public enum PlayerStates
    {
        Idle, // The palyer is not moving
        Move, // The player is moving
        None  // The player has a undetermined state
    }

    /// <summary>
    ///  Player
    ///  Contains basic movment system for a top down player
    ///  Note: there is no gravity
    /// </summary>
    internal class Player : GameObject
    {
        private KeyboardState previousKeyboardState; // Previous KeyboardState (only updates every interval of input buffer time)
        private KeyboardState currentKeyboardState;  // Current KeyboardState (always current keyboard state)
        private Vector2 previousVelocity;            // Holds the previous direction and magnitude of velocity
        private Vector2 velocity;                    // Holds the current direction and magnitude of velocity
        private Vector2 speed;                       // Scalar quantity for velocity 
        private Vector2 acceleration;                // Accleration applied to velocity
        private Vector2 translationRatio;            // Ratio between height and width of the screen
        private PlayerStates playerState;            // Player translation state
        private Keys deAccFrom;                      // What key was last released
        private double totalTime;                    // total ellapsed time
        private float delayBufferTime;               // half the actual input buffer time
        private float inputBufferTime;               // input response time
        private ushort contentName;                  // the content index

        /// <summary>
        ///  Position of the player
        /// </summary>
        public Vector2 Position
        {
            get
            {
                GameObject.Instance.Content = contentName;
                return GameObject.Instance[Vector2.Zero][0];
            }
            set
            {
                GameObject.Instance.Content = contentName;
                GameObject.Instance[Vector2.Zero][0] = value;
            }
        }

        /// <summary>
        ///  Player sprite
        /// </summary>
        public new Texture2D Sprite
        {
            get
            {
                GameObject.Instance.Content = contentName;
                return GameObject.Instance.Sprite;
            }
        }

        /// <summary>
        ///  Single Player sprite bounds
        /// </summary>
        public Rectangle Transform
        {
            get
            {
                GameObject.Instance.Content = contentName;
                return GameObject.Instance[Rectangle.Empty][0];
            }
            set
            {
                GameObject.Instance.Content = contentName;
                GameObject.Instance[Rectangle.Empty][0] = value;
            }
        }

        /// <summary>
        ///  Scale of the palyer sprite
        /// </summary>
        public new float Scale
        {
            get
            {
                GameObject.Instance.Content = contentName;
                return GameObject.Instance.Scale;
            }
            set
            {
                GameObject.Instance.Content = contentName;
                GameObject.Instance.Scale = value;
            }
        }

        /// <summary>
        ///  Player animation
        /// </summary>
        public Animation Animation
        {
            get
            {
                GameObject.Instance.Content = contentName;
                return GameObject.Instance[Animation.Empty][0];
            }
            set
            {
                GameObject.Instance.Content = contentName;
                GameObject.Instance[Animation.Empty][0] = value;
            }
        }

        /// <summary>
        ///  Player collider
        /// </summary>
        public Collider Collider
        {
            get
            {
                GameObject.Instance.Content = contentName;
                return GameObject.Instance[Collider.Empty][0];
            }
            set
            {
                GameObject.Instance.Content = contentName;
                GameObject.Instance[Collider.Empty][0] = value;
            }
        }

        /// <summary>
        ///  Creates a player from a playerSprite
        /// </summary>
        /// <param name="playerSprite">player sprite image</param>
        /// <param name="speed">player translation speed</param>
        public Player(Vector2 speed, ushort contentName) :
            base(contentName)
        {
            this.contentName = contentName;
            this.Position = Vector2.Zero;
            this.velocity = Vector2.Zero;
            this.acceleration = Vector2.Zero;
            this.speed = speed;
            this.playerState = PlayerStates.Idle;
            this.totalTime = 0;
            this.inputBufferTime = 0.004f;
            this.delayBufferTime = inputBufferTime / 2.0f;
            this.Scale = 2.5f;
            // Ratio is calclated via the shape of the player sprite
            // against the width and height of the screen
            // With the third factor a vector of 1 ie the directional vector ie normalized velocity
            this.translationRatio = new Vector2(
                ((((float)this.Transform.Width) / ((float)this.Transform.Height))) / 2.0f,
                ((((float)this.Transform.Height) / ((float)this.Transform.Width))) / 2.0f
            );
        }

        /// <summary>
        ///  Updates the player Finite State Machine
        /// </summary>
        public void Update(GameTime gameTime)
        {
            Vector2 translationAjdustedRatio = translationRatio;
            float bufferTime = inputBufferTime;

            // Adjust velocity magnitude for dirational change
            if (
                (previousVelocity.Y > 0 && velocity.Y < 0) ||
                (previousVelocity.X > 0 && velocity.X < 0) ||
                (Math.Abs(previousVelocity.X - velocity.X) > 1f) ||
                (Math.Abs(previousVelocity.Y - velocity.Y) > 1f)
            )
            {
                // Micro adjust velocity for de-acceleration
                if ((Math.Abs(velocity.X) > 0.5 && Math.Abs(velocity.Y) > 0.5))
                {
                    velocity -= velocity * inputBufferTime * 2.0f;
                }
                else
                {
                    // If not vertical or horizontal de-acceleration
                    if (Math.Abs(velocity.X) > 0.5)
                    {
                        velocity.Y -= velocity.Y * inputBufferTime * 2.0f;
                    }
                    if (Math.Abs(velocity.Y) > 0.5)
                    {
                        velocity.X -= velocity.X * inputBufferTime * 2.0f;
                    }
                }
            }

            // Ease the transition of velocity when in diagnal
            if (previousVelocity != velocity)
            {
                if ((Math.Abs(previousVelocity.X) > 0 && Math.Abs(previousVelocity.Y) > 0) ||
                    (Math.Abs(velocity.X) > 0 && Math.Abs(velocity.Y) > 0)
                )
                {
                    velocity += previousVelocity / (3.0f);
                }
            }

            // Add the acceleration to the velocity
            // Clamp the velocity to zero when too small
            velocity += acceleration;
            if (Math.Abs(velocity.X) < 0.0000001)
            {
                velocity.X = 0;
            }
            if (Math.Abs(velocity.Y) < 0.0000001)
            {
                velocity.Y = 0;
            }

            // When not in a change the speed to be 45 to the large side
            if (!(Math.Abs(velocity.X) > 0 && Math.Abs(velocity.Y) > 0))
            {
                velocity.X = velocity.X / speed.Y * translationRatio.Y;
            }
            else if (Math.Abs(velocity.X) > 0 && Math.Abs(velocity.Y) > 0)
            {
                // When in a diagnal set the proper translation ratio
                // reset the buffer time
                bufferTime = delayBufferTime;
                if (translationRatio.X < translationRatio.Y)
                {
                    translationAjdustedRatio.X = translationRatio.Y;
                }
                else
                {
                    translationAjdustedRatio.Y = translationRatio.X;
                }
            }

            // Determine if idle
            if (playerState == PlayerStates.Idle || playerState == PlayerStates.None)
            {
                // Get the directional velocity to de-accelerate
                switch (deAccFrom)
                {
                    case Keys.D:
                        velocity.X = (velocity == Vector2.Zero ? velocity.X : (velocity.X) / 1.8f);
                        velocity.Y = 0;
                        break;
                    case Keys.A:
                        velocity.X = (velocity == Vector2.Zero ? velocity.X : (velocity.X) / 1.8f);
                        velocity.Y = 0;
                        break;
                    case Keys.W:
                        velocity.Y = (velocity == Vector2.Zero ? velocity.Y : (velocity.Y) / 1.8f);
                        velocity.X = 0;
                        break;
                    case Keys.S:
                        velocity.Y = (velocity == Vector2.Zero ? velocity.Y : (velocity.Y) / 1.8f);
                        velocity.X = 0;
                        break;
                }

                // Slow de-accelerate
                Position -= velocity;
            }
            else
            {
                // When the player is not idle normalize their velocity to extract direction and translate by the speed of the player
                Position += (velocity == Vector2.Zero ? velocity : Vector2.Normalize(velocity) * speed * translationAjdustedRatio);
            }

            // Adjust the animation speed based upon velocity speed
            if (velocity.Length() > 0.005f && velocity.Length() < 0.05f)
            {
                // Animation is faster
                this.Animation.AnimationDuration = velocity.Length();
            }
            else
            {
                // Minimum speed
                this.Animation.AnimationDuration = 0.05f;
            }

            // Update the animation and collider
            this.Transform = this.Animation.Play(gameTime);
            Collision intercept = this.Collider.UpdateTransform(
                this.Sprite,                         // Player Sprite
                this.Position,                       // Player position
                this.Transform,                      // Player transform for sprite selection
                GameObject.Instance.GraphicsDevice,
                GameObject.Instance.SpriteBatch,
                this.Scale,                          // Player scale
                SpriteEffects.None,
                contentName                          // Content
            );

            bool isBullet = false;
            // TODO: Collision check for bullet

            if (intercept.other != null && intercept.position != new Vector2(-1, -1) && !isBullet)
            {
                Position = intercept.position;
                this.Collider.Resolved = true;
            }
            else if (isBullet)
            {
                // TODO: take damage
            }

            previousVelocity = velocity;
            totalTime += gameTime.ElapsedGameTime.TotalSeconds;

            currentKeyboardState = Keyboard.GetState();
            // Player Finite State Machine
            switch (playerState)
            {
                // Input Transition state
                case PlayerStates.None:
                    if (
                        previousKeyboardState.IsKeyDown(Keys.D) ||
                        previousKeyboardState.IsKeyDown(Keys.A) ||
                        previousKeyboardState.IsKeyDown(Keys.W) ||
                        previousKeyboardState.IsKeyDown(Keys.S)
                    )
                    {
                        playerState = PlayerStates.Move;
                    }
                    else
                    {
                        playerState = PlayerStates.Idle;
                    }
                    break;

                // Player is idle so slow the velocity and get rid of acceleration
                // Exit to the Player None state
                case PlayerStates.Idle:
                    acceleration = Vector2.Zero;
                    playerState = PlayerStates.None;
                    break;

                // Updates the player velocity and acceleration
                // Uses standard WASD controls
                // Exits to the player None state when not presing WASD
                case PlayerStates.Move:
                    // When a directional key is lifted pick out
                    // and update the directional de acceleration
                    if (previousKeyboardState.IsKeyUp(Keys.D))
                    {
                        deAccFrom = Keys.D;
                    }
                    else if (previousKeyboardState.IsKeyUp(Keys.A))
                    {
                        deAccFrom = Keys.A;
                    }
                    else if (previousKeyboardState.IsKeyUp(Keys.W))
                    {
                        deAccFrom = Keys.W;
                    }
                    else if (previousKeyboardState.IsKeyUp(Keys.S))
                    {
                        deAccFrom = Keys.S;
                    }

                    // If we are not translating
                    if (!(
                        previousKeyboardState.IsKeyDown(Keys.D) ||
                        previousKeyboardState.IsKeyDown(Keys.A) ||
                        previousKeyboardState.IsKeyDown(Keys.W) ||
                        previousKeyboardState.IsKeyDown(Keys.S)
                    ))
                    {
                        // Exit this player state
                        // force next key update
                        // Instantly update the key
                        totalTime = inputBufferTime;
                        playerState = PlayerStates.None;
                        previousKeyboardState = Keyboard.GetState();
                    }

                    // Directional X +
                    if (
                        previousKeyboardState.IsKeyDown(Keys.D)
                    )
                    {
                        if (velocity.X >= 1)
                        {
                            velocity.X = 1;
                        }
                        if (velocity.X < -inputBufferTime && !currentKeyboardState.IsKeyDown(Keys.A))
                        {
                            velocity.X = inputBufferTime;
                            acceleration.X = acceleration.X / 10f;
                        }
                        velocity.X += 0.05f;
                        acceleration.X += 0.0005f;
                        if (!currentKeyboardState.IsKeyDown(Keys.W) && !currentKeyboardState.IsKeyDown(Keys.S))
                        {
                            velocity.Y = velocity.Y / 10.0f;
                            acceleration.Y = acceleration.Y / 10f;
                            acceleration.X = acceleration.X / 10f;
                        }
                    }

                    // Directional X -
                    if (
                        previousKeyboardState.IsKeyDown(Keys.A)
                    )
                    {
                        if (velocity.X <= -1)
                        {
                            velocity.X = -1;
                        }
                        if (velocity.X > inputBufferTime && !currentKeyboardState.IsKeyDown(Keys.D))
                        {
                            velocity.X = -inputBufferTime;
                            acceleration.X = acceleration.X / 10f;

                        }
                        velocity.X += -0.05f;
                        acceleration.X += -0.0005f;
                        if (!currentKeyboardState.IsKeyDown(Keys.W) && !currentKeyboardState.IsKeyDown(Keys.S))
                        {
                            velocity.Y = velocity.Y / 10.0f;
                            acceleration.Y = acceleration.Y / 10f;
                            acceleration.X = acceleration.X / 10f;
                        }
                    }

                    // Directional Y -
                    if (
                        previousKeyboardState.IsKeyDown(Keys.W)
                    )
                    {
                        if (velocity.Y <= -1)
                        {
                            velocity.Y = -1;
                        }
                        if (velocity.Y > inputBufferTime && !currentKeyboardState.IsKeyDown(Keys.S))
                        {
                            velocity.Y = -inputBufferTime;
                            acceleration.Y = acceleration.Y / 10f;
                        }
                        velocity.Y += -0.05f;
                        acceleration.Y += -0.0005f;
                        if (!currentKeyboardState.IsKeyDown(Keys.A) && !currentKeyboardState.IsKeyDown(Keys.D))
                        {
                            velocity.X = velocity.X / 10.0f;
                            acceleration.X = acceleration.X / 10f;
                            acceleration.Y = acceleration.Y / 10f;
                        }
                    }

                    // Directional Y +
                    if (
                        previousKeyboardState.IsKeyDown(Keys.S)
                    )
                    {
                        if (velocity.Y >= 1)
                        {
                            velocity.Y = 1;
                        }
                        if (velocity.Y < inputBufferTime && !currentKeyboardState.IsKeyDown(Keys.W))
                        {
                            velocity.Y = inputBufferTime;
                            acceleration.Y = acceleration.Y / 10f;
                        }
                        velocity.Y += 0.05f;
                        acceleration.Y += 0.0005f;
                        if (!currentKeyboardState.IsKeyDown(Keys.A) && !currentKeyboardState.IsKeyDown(Keys.D))
                        {
                            velocity.X = velocity.X / 10.0f;
                            acceleration.X = acceleration.X / 10f;
                            acceleration.Y = acceleration.Y / 10f;
                        }
                    }
                    break;
            }

            // When space is pressed trigger shoot
            if (previousKeyboardState.IsKeyDown(Keys.Space) && currentKeyboardState.IsKeyUp(Keys.Space))
            {
                Shoot();
            }

            // When the ellapsed time is the buffer time update the keyboard state
            if (totalTime >= bufferTime)
            {
                previousKeyboardState = currentKeyboardState;
                totalTime -= bufferTime;
            }
        }

        /// <summary>
        ///  Triggered when the 
        ///  Player presses the shoot button
        /// </summary>
        public void Shoot()
        {
            // TODO: Player Shooting
        }


        /// <summary>
        ///  Draws the player
        /// </summary>
        public void Draw()
        {
            GameObject.Instance.SpriteBatch.Draw(
                Sprite,                          // The sprite-sheet for the player
                Position,                        // The position for the player
                Transform,                       // The scale and bounding box for the animation
                Color.White,                     // The color for the palyer
                0.0f,                            // There cannot be any rotation of the player
                Vector2.Zero,                    // Starting render position
                this.Scale,                      // The scale of the sprite
                SpriteEffects.None,              // Which direction the sprite faces
                0.0f                             // Layer depth of the player is 0.0
            );
        }
    }
}