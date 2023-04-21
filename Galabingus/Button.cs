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
        public event EventDelegate OnHover;
        public event EventDelegate OnRelease;

        #endregion

        #region Fields

        //the current mouseState
        private MouseState currentMS;
        private MouseState prevMS;

        //a second texture for hover
        private Texture2D baseTexture;
        private Texture2D hoverTexture;
        
        #endregion

        #region Properties

        public Texture2D BaseTexture
        {
            get { return baseTexture; }
            set { baseTexture = value; }
        }

        public Texture2D HoverTexture
        {
            get { return hoverTexture; }
            set { hoverTexture = value; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// instantiates a basic button
        /// </summary>
        /// <param name="texture">its texure</param>
        /// <param name="position">its position rectangle</param>
        public Button
            (Texture2D texture, Vector2 position, float scale)
            : base(texture, position, scale) 
        {
            baseTexture = texture;
        }

        #endregion

        #region Methods

        public override void Update()
        {
            currentMS = Mouse.GetState();

            if (uiPosition.Contains(currentMS.Position))
            {
                if (currentMS.LeftButton == ButtonState.Pressed)
                {
                    if (OnClick != null)
                        OnClick(this);

                }
                else
                {
                    if (OnHover != null)
                        OnHover(this);
                }
            }
            else
            {
                if(uiTexture != baseTexture)
                    uiTexture = baseTexture;
                else if(clearColor != Color.White)
                    clearColor = Color.White;

                if (OnRelease != null)
                    OnRelease(this);
            }

            prevMS = currentMS;
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
