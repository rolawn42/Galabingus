﻿using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Galabingus
{

    #region Enums

    /// <summary>
    /// Represents the overall states of the game
    /// </summary>
    public enum GameState
    {
        Menu,
        Game,
        Pause
    }

    public enum DebugState
    {
        DebugOn,
        DebugOff
    }

    /// <summary>
    /// represents the type of level data which should be retrieved
    /// </summary>
    public enum Type
    {
        Enemy,
        Tile
    }

    #endregion

    sealed class UIManager
    {
        #region Fields

        //the instance of the UIManager
        private static UIManager instance = null;

        //the list of UIObjects it manages
        private Dictionary<UIObject, GameState> uiObjects;

        //variable containing the current gamestate
        private GameState gs;

        //variable containing the debugstate
        private DebugState ds;

        //create a current and previous keyboardstate variable
        private KeyboardState currentKBS;
        private KeyboardState previousKBS;

        //the current state of the mouse
        private MouseState mouseState;

        //the Game1 / Galabingus class' managers
        //for updating the game
        private GraphicsDeviceManager gr;
        private ContentManager cm;
        private SpriteBatch sb;

        //File reading
        StreamReader reader;

        //The list of tile values to be
        //returned by the LevelReader
        List<int[]> objectData;

        #endregion

        #region Properties

        /// <summary>
        /// returns the instance of the UI manager
        /// </summary>
        public static UIManager UserInterface
        {
            get
            {
                return instance;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private UIManager()
        {
            //initialize the gamestate variable
            gs = new GameState();
            ds = new DebugState();

            //set the debug to off to start
            ds = DebugState.DebugOn;

        }

        #endregion

        #region Methods

        public void Initialize(
            GraphicsDeviceManager gr,
            ContentManager cm,
            SpriteBatch sb)
        {
            //initialize the monogame managers in the UI manager
            this.gr = gr;
            this.cm = cm;
            this.sb = sb;
        }

        public void LoadContent()
        {
            //creates the list of UIObjects
            uiObjects.Add(
                new Button("txt", cm, 100, 100, 1), GameState.Menu
            );
        }

        public void Update(GameState gameState)
        {
            //set this classes gamestate to the current gamestate
            gs = gameState;

            //set the keyboardstate
            currentKBS = Keyboard.GetState();

            switch (gs)
            {
                case GameState.Menu:

                    if (ds == DebugState.DebugOff)
                    {

                    }
                    else
                    {
                        //if the shift button is pressed, change the state
                        if (SingleKeyPress(Keys.LeftShift)
                        || SingleKeyPress(Keys.RightShift))
                        {
                            gs = GameState.Game;
                        }
                    }
                    

                    break;

                case GameState.Game:

                    //DEBUG: if the shift button is pressed, change the state
                    if (SingleKeyPress(Keys.LeftShift)
                        || SingleKeyPress(Keys.RightShift))
                    {
                        gs = GameState.Menu;
                    }

                    break;

            }

            previousKBS = currentKBS;
        }

        public void Draw()
        {
            sb.Begin();

            switch (gs)
            {

            }

            sb.End();
        }

        #endregion

        #region Helpers

        /// <summary>
        /// determines if a the key was click on the prior frame
        /// </summary>
        /// <param name="key">the key that was pressed</param>
        /// <returns>true if it was pressed last frame and false if it wasn't</returns>
        public bool SingleKeyPress(Keys key)
        {
            if (currentKBS.IsKeyDown(key) && previousKBS.IsKeyUp(key))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// reads in a level file and creates a list of a certain element within it
        /// </summary>
        /// <param name="type">the type of data you want returned</param>
        /// <returns>a list of a certain type of data</returns>
        public List<int[]> LevelReader(Type type)
        {
            //a new stream reader from a level file
            reader = new StreamReader("Content/level1.level");

            if (!reader.EndOfStream)
            {
                //read the line, split it to a list of string
                string line = reader.ReadLine();
                string[] s_values = line.Split('|');
                int[] i_values = new int[s_values.Length];

                //parse the list of strings to a list of ints
                for (int i = 0; i < s_values.Length; i++)
                {
                    i_values[i] = int.Parse(s_values[i]);
                }

                //add integer list to the overall list
                objectData.Add(i_values);
            }

            //a list to return the desired values
            List<int[]> returnList = new List<int[]>();

            //determines what values of object data should be returned
            foreach (int[] value in objectData)
            {
                if (value[0] == (int)type)
                {
                    returnList.Add(value);
                }
            }

            //clears object data for the next time
            objectData.Clear();

            //returns the list of desired values
            return returnList;
        }

        #endregion
    }
}
