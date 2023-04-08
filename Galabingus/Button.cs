﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Galabingus
{
    internal class Button : UIElement
    {
        #region Events

        public event EventDelegate OnClick;

        #endregion

        #region Fields

        //the current mouseState
        private MouseState mouseState;

        //objects which represent what the button will do
        //(show a menu, change a state, etc)
        private Menu menu;
        private GameState returnState;

        #endregion

        #region Constructor

        /// <summary>
        /// instantiates a basic button
        /// </summary>
        /// <param name="texture">its texure</param>
        /// <param name="position">its position rectangle</param>
        public Button
            (Texture2D texture, Vector2 position, GameState gs)
            : base(texture, position, gs, 5) { }

        #endregion

        #region Methods

        public override void Update()
        {
            mouseState = Mouse.GetState();

            if(uiPosition.Contains(mouseState.Position))
            {
                if(mouseState.LeftButton == ButtonState.Pressed)
                {
                    OnClick(this);
                }
                else
                {
                    clearColor = Color.LightGray;
                }
            }
            else
            {
                clearColor = Color.White;
            }
        }

        public override void Draw(SpriteBatch sb)
        {
            sb.Draw(
                uiTexture,
                uiPosition,
                clearColor
            );
        }

        #endregion
    }
}
