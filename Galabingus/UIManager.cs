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
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Xml.Linq;

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
        Pause,
        PlayerDead,
        PlayerWins,
        GameOver,
        Victory,
        NoState
    }

    /// <summary>
    /// deterimines whether the program has debug features enabled
    /// </summary>
    public enum DebugState
    {
        DebugOn,
        DebugOff
    }

    /// <summary>
    /// (todo) represents the ways the player can interact with the UI
    /// </summary>
    public enum UIControlState
    {
        Keys,
        Mouse
    }

    /// <summary>
    /// represents the type of level data which should be retrieved
    /// </summary>
    public enum TileType
    {
        Enemy,
        Tile
    }

    /// <summary>
    /// represents the different levels of the games UI
    /// </summary>
    public enum UIState
    {
        BaseMenu,
        BaseGame,
        BasePause,
        BaseGameOver
    }


    #endregion

    sealed class UIManager
    {
        #region Fields

        private bool reset;

        private double fadeValue;

        //the instance of the UIManager
        private static UIManager instance = null;

        //variable containing the current gamestate
        private GameState gs;
        private GameState pgs;

        //variable containing the debugstate
        private DebugState ds;

        //controls whether the user naviagtes with mouse or keyboard
        private UIControlState cs;

        //keyboard control state
        private bool keyboardIsActive;
        private bool keyboardTakeOver;

        //selected button identifier
        private float selectedButton;

        //create a current and previous keyboardstate variable
        private KeyboardState currentKBS;
        private KeyboardState previousKBS;

        //the current mouseState
        private MouseState currentMS;
        private MouseState previousMS;

        //the Game1 / Galabingus class' managers
        //for updating the game
        private GraphicsDeviceManager gr;
        private ContentManager cm;
        private SpriteBatch sb;

        //File reading
        private StreamReader reader;
        private StreamWriter writer;

        //Screen Dimensions
        int width;
        int height;

        //The list of tile values to be
        //returned by the LevelReader
        List<int[]> objectData;

        // Temporary Backgrounds
        private Texture2D menuBackground;

        //the sets of menus in the game
        List<UIElement> menu;
        List<UIElement> game;
        List<UIElement> pause;
        List<UIElement> gameOver;
        List<UIElement> victory;

        Stack<List<UIElement>> currentMenu;
        Dictionary<GameState, List<UIElement>> gameStates;
        int menuState;

        //for unique menus
        bool displayMenu;
        List<UIElement> menuToDisplay;

        private bool prevBossOnScreen;

        bool currentActive;
        bool previousActive;

        #endregion

        #region Properties

        public bool IsReset
        {
            get
            {
                return reset;
            }
            set
            {
                reset = value;
            }
        }

        /// <summary>
        /// returns the instance of the UI manager
        /// </summary>
        public static UIManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new UIManager();
                }
                return instance;
            }
        }

        /// <summary>
        /// can return current game state
        /// </summary>
        public GameState GS
        {
            get { return gs; }
            set { gs = value; }
        }

        /// <summary>
        /// returns and sets the current debugState
        /// </summary>
        public DebugState DS
        {
            get { return ds; }
            set { ds = value; }
        }

        /// <summary>
        /// return and set the UIControlState
        /// </summary>
        public UIControlState CS
        {
            get { return cs; }
            set { cs = value; }
        }

        /// <summary>
        ///  The keyboard code running?
        /// </summary>
        public bool KeyboardTakeOver
        {
            get
            {
                return keyboardIsActive;
            }
        }

        /// <summary>
        ///  The keyboard is in control?
        /// </summary>
        public bool IsKeyboardActive
        {
            get
            {
                return keyboardTakeOver;
            }
        }

        /// <summary>
        ///  Y coordinate of the button
        /// </summary>
        public float ButtonSelection
        {
            get
            {
                return selectedButton;
            }
            set
            {
                selectedButton = value;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// creates the initial instance of the UIManager 
        /// by instantiating its basic components
        /// </summary>
        private UIManager()
        {
            //initialize the gamestate variable
            gs = new GameState();
            ds = new DebugState();
            cs = new UIControlState();

            pgs = gs;

            //set the base game and debug states
            gs = GameState.Menu;
            ds = DebugState.DebugOn;
            cs = UIControlState.Mouse;

            //list of menu levels
            currentMenu = new Stack<List<UIElement>>();
            gameStates = new Dictionary<GameState, List<UIElement>>();
        }

        #endregion

        #region Initialize

        /// <summary>
        /// initializes the UIManager with data from the Game1(Galabingus) class
        /// </summary>
        /// <param name="gr">Game1's Graphics Device Manager (for editing the game window)</param>
        /// <param name="cm">Game1's Content Manager (for loading)</param>
        /// <param name="sb">Game1's Sprite Batch (for drawing)</param>
        public void Initialize(
            GraphicsDeviceManager gr,
            ContentManager cm,
            SpriteBatch sb)
        {
            //initialize the monogame managers in the UI manager
            this.gr = gr;
            this.cm = cm;
            this.sb = sb;

            //set the viewport size to the current screen width and height
            width = gr.GraphicsDevice.Viewport.Width;
            height = gr.GraphicsDevice.Viewport.Height;

            fadeValue = 0.00009;
            prevBossOnScreen = false;
            menuState = 0;
        }

        #endregion

        #region Load Content

        /// <summary>
        /// loads all of the necesary content for the game and creates the
        /// data structures needed for the UI to function correctly
        /// </summary>
        public void LoadContent()
        {
            //the sets of menus in the game
            menu = new List<UIElement>();
            game = new List<UIElement>();
            pause = new List<UIElement>();
            gameOver = new List<UIElement>();
            victory = new List<UIElement>();

            //dummy variables
            Button button;
            Background background;
            Texture2D texture;
            Slider slider;

            //different menus
            List<UIElement> howToPlayMenu = new List<UIElement>();
            List<UIElement> optionsMenu = new List<UIElement>();
            List<UIElement> creditsMenu = new List<UIElement>();


            //more dummy variables
            EventDelegate event1;
            EventDelegate event2;
            TextEvent textEvent1;

            #region Add stuffs

            //Create the play button
            event1 = StartGame;
            event2 = HoverTexture;
            textEvent1 = null;

            button = AddButton("buttonPlay_base_strip1", 0.6f,
            new Vector2(width / 2 + 30, height / 2),
            event1, event2, menu);

            button.HoverTexture = cm.Load<Texture2D>("buttonPlay_hover_strip1");
            button.UITexture = button.HoverTexture;

            // Change button selection:
            selectedButton = button.UIPosition.Y;

            //Create the other buttons
            event1 = DisplayMenu;

            //how to play button
            button = AddButton("buttonHowToPlay_base_strip1", 0.6f,
            new Vector2(width / 2 + 30, height / 2 + 100),
            event1, event2, menu);

            button.HoverTexture = cm.Load<Texture2D>("buttonHowToPlay_hover_strip1");

            AddBackground(
                "HowToPlayMenu_strip1", 0.4f,
                new Vector2(width / 2, height / 2 - 50),
                howToPlayMenu);

            button.DisplayMenu = howToPlayMenu;

            //options button
            button = AddButton("buttonOptions_base_strip1", 0.6f,
            new Vector2(width / 2 + 30, height / 2 + 200),
            event1, event2, menu);

            button.HoverTexture = cm.Load<Texture2D>("buttonOptions_hover_strip1");

            AddText(
                "arial_18",
                "sorry, you just gotta get good",
                new Vector2(width / 2, height / 2),
                Color.White, textEvent1, optionsMenu);

            button.DisplayMenu = optionsMenu;

            //credits button
            button = AddButton("buttonCredits_base_strip1", 0.6f,
            new Vector2(width / 2 + 30, height / 2 + 300),
            event1, event2, menu);

            AddText(
                "arial_18",
                "gamer",
                new Vector2(width / 2, height / 2),
                Color.White, textEvent1, creditsMenu);

            button.DisplayMenu = creditsMenu;
            button.HoverTexture = cm.Load<Texture2D>("buttonCredits_hover_strip1");

            //add the logo to the screen
            AddBackground("galabinguslogo_strip1", 5,
                new Vector2(width / 2, height / 4),
                menu);

            //Pause Text
            AddText("arial_36", "Pause",
                new Vector2(width / 2 - 200,
                height / 2 - 200), Color.White, textEvent1, pause);

            AddBackground("pauseMenu_strip1", 2f,
                new Vector2(width / 2, height / 2), pause);

            button = AddButton("buttonOptions_base_strip1", 0.6f,
            new Vector2(width / 2 + 40, height / 2 + 50),
            event1, event2, pause);

            button.HoverTexture = cm.Load<Texture2D>("buttonOptions_hover_strip1");
            button.DisplayMenu = optionsMenu;

            event1 = ReturnMenu;

            button = AddButton("quitButton_base_strip1", 2.3f,
            new Vector2(width / 2 + 40, height / 2 + 150),
            event1, event2, pause);

            button.HoverTexture = cm.Load<Texture2D>("quitButton_hover_strip1");

            event1 = StartGame;
            
            button = AddButton("resumeButton_base_strip1", 2.3f,
            new Vector2(width / 2 + 35, height / 2 - 50),
            event1, event2, pause);

            button.HoverTexture = cm.Load<Texture2D>("resumeButton_hover_strip1");

            //GameOver
            AddBackground("deathTitle_strip1", 1f,
                new Vector2(width / 2, height / 4),
                gameOver);

            event1 = ReturnMenu;
            event2 = HoverTexture;

            //add the return to the menu in the game over
            button = AddButton("buttonMenu_base_strip1", 0.6f,
            new Vector2(width / 2 + 30, height / 2 + 150),
            event1, event2, gameOver);

            button.HoverTexture = cm.Load<Texture2D>("buttonMenu_hover_strip1");

            //Victory
            AddBackground("victoryTitle_strip1", 1f,
                new Vector2(width / 2, height / 4),
                victory);


            //add the return to the menu in victory
            button = AddButton("buttonMenu_base_strip1", 0.6f,
            new Vector2(width / 2 + 30, height / 2 + 150),
            event1, event2, victory);

            button.HoverTexture = cm.Load<Texture2D>("buttonMenu_hover_strip1");

            //add menu back buttons
            event1 = HideMenu;

            button = AddButton("buttonBack_base_strip1", 0.6f,
                new Vector2(width / 2 + 30, height / 2 + 350),
                event1, event2, howToPlayMenu);

            button.HoverTexture = cm.Load<Texture2D>("buttonBack_hover_strip1");

            button = AddButton("buttonBack_base_strip1", 0.6f,
                new Vector2(width / 2 + 30, height / 2 + 350),
                event1, event2, optionsMenu);

            button.HoverTexture = cm.Load<Texture2D>("buttonBack_hover_strip1");

            button = AddButton("buttonBack_base_strip1", 0.6f,
                new Vector2(width / 2 + 30, height / 2 + 350),
                event1, event2, creditsMenu);

            button.HoverTexture = cm.Load<Texture2D>("buttonBack_hover_strip1");

            #endregion

            //add the background
            menuBackground = cm.Load<Texture2D>("menubackground_strip1");

            gameStates.Add(GameState.Menu, menu);
            gameStates.Add(GameState.Game, game);
            gameStates.Add(GameState.Pause, pause);
            gameStates.Add(GameState.GameOver, gameOver);
            gameStates.Add(GameState.Victory, victory);

            currentMenu.Push(menu);

        }

        #endregion

        #region Update

        /// <summary>
        /// updates the UI every frame
        /// </summary>
        public void Update()
        {
            //set the keyboardstate
            currentKBS = Keyboard.GetState();
            currentMS = Mouse.GetState();
            currentActive = keyboardIsActive;

            // Arrow keys trigger keyboard take over
            if (SingleKeyPress(Keys.Down) || SingleKeyPress(Keys.Up) && !keyboardIsActive)
            {
                keyboardTakeOver = true;
                keyboardIsActive = true;

                ResetButtons();
            }
            else if (currentMS != previousMS)
            {
                keyboardTakeOver = false;
                keyboardIsActive = false;
                ResetButtons();
            }
            else
            {
                keyboardIsActive = false;
            }


            //if the back key is pressed and the current level isn't the base one
            if (!(currentMenu.Count <= 1))
            {
                if (SingleKeyPress(Keys.Back))
                {
                    menuState = 1;
                }
            }

            switch (menuState)
            {
                case 1:
                    currentMenu.Pop();
                    break;
                case 2:
                    currentMenu.Push(menuToDisplay);
                        break;
                default:
                    break;
            }

            // Use the keyboard to take control of selections
            if (keyboardIsActive)
            {
                KeyboardSelection(currentMenu.Peek());
            }

            menuState = 0;

            UpdateObjects(currentMenu.Peek());

            //finite state machine for the UI to update the UI based on user input
            switch (gs)
            {
                case GameState.Menu:

                    if (ds == DebugState.DebugOn)
                    {
                        //if the shift button is pressed, change the state
                        if (SingleKeyPress(Keys.L))
                        {
                            gs = GameState.Game;
                        }
                    }

                    break;

                case GameState.Game:

                    if (ds == DebugState.DebugOn)
                    {
                        //if the shift button is pressed, change the state
                        if (SingleKeyPress(Keys.L))
                        {
                            gs = GameState.Menu;
                        }
                    }

                    //pause the game when tab is pressed
                    if (SingleKeyPress(Keys.Tab))
                    {
                        gs = GameState.Pause;
                    }

                    //if boss health = 0
                    //go to player wins
                    if (!EnemyManager.Instance.BossOnScreen && prevBossOnScreen)
                    {
                        gs = GameState.PlayerWins;
                    }

                    prevBossOnScreen = EnemyManager.Instance.BossOnScreen;

                    //if player health = 0
                    //go to player dead
                    if (Player.PlayerInstance.Health == 0)
                    {
                        gs = GameState.PlayerDead;
                    }


                    break;

                case GameState.PlayerDead:

                    if (GameObject.Fade < fadeValue)
                    {
                        gs = GameState.GameOver;

                    }

                    break;

                case GameState.Pause:

                    //if the game is paused, unpause when tab is hit
                    if (SingleKeyPress(Keys.Tab))
                    {
                        gs = GameState.Game;
                    }

                    break;

                case GameState.PlayerWins:


                    if (GameObject.Fade < fadeValue)
                    {
                        gs = GameState.Victory;

                    }

                    break;

                case GameState.GameOver:

                    //System.Diagnostics.Debug.WriteLine("hello");

                    if (SingleKeyPress(Keys.Enter))
                    {
                        gs = GameState.Menu;
                        reset = true;
                    }

                    break;

                case GameState.Victory:

                    if (SingleKeyPress(Keys.Enter))
                    {
                        gs = GameState.Menu;
                        reset = true;
                    }

                    break;
            }

            if (pgs != gs)
            {
                if (gs != GameState.PlayerDead && gs != GameState.PlayerWins)
                {
                    currentMenu.Clear();
                    currentMenu.Push(gameStates[gs]);
                }
            }

            //set the previous KeyboardState to the current one for next frame
            previousKBS = currentKBS;
            previousMS = currentMS;
            previousActive = currentActive;
            pgs = gs;
        }

        #endregion

        #region Draw

        /// <summary>
        /// draws UI elements to the screen
        /// </summary>
        public void Draw()
        {
            //finite state machine for determining what to draw this frame
            switch (gs)
            {
                case GameState.Menu:

                    //changes the screen background
                    sb.Draw(
                        menuBackground,
                        new Rectangle(
                            0,
                            0,
                            (int)(menuBackground.Width / 1.1),
                            (int)(menuBackground.Height / 1.1)),
                        Color.White);

                    break;
            }

            DrawObjects(currentMenu.Peek());
        }

        public void Reset()
        {
            instance = null;
        }

        #endregion

        #region Event Methods

        private void StartGame(object sender)
        {
            gs = GameState.Game;
        }

        private void ReturnMenu(object sender)
        {
            gs = GameState.Menu;
            reset = true;

        }

        private void DisplayMenu(object sender)
        {
            Button button = (Button)sender;

            menuState = 2;
            menuToDisplay = button.DisplayMenu;

        }

        private void HideMenu(object sender)
        {
            Button button = (Button)sender;

            menuState = 1;
        }

        private void HoverLightGray(object sender)
        {
            Button button = (Button)sender;

            button.ClearColor = Color.LightGray;
        }

        private void HoverTexture(object sender)
        {
            Button button = (Button)sender;

            button.UITexture = button.HoverTexture;
        }

        private void BaseTexture(object sender)
        {
            Button button = (Button)sender;

            button.UITexture = button.BaseTexture;
        }

        #endregion

        #region Element Creation

        /// <summary>
        /// creates a UIElement and adds it to the list elements
        /// </summary>
        /// <param name="uiObject">the ui object / element</param>
        /// <param name="gs">the gamestate the element exists in</param>
        /// <param name="uiEvent">the data which it needs for its events</param>
        /// <param name="types">the event types it can call</param>
        private Button AddButton
            (string filename, float scale, Vector2 position, EventDelegate clickEvent, List<UIElement> listToAdd)
        {
            //create the button texture
            Texture2D texture = cm.Load<Texture2D>(filename);

            //create the button
            Button button = new Button(texture, position, scale);

            button.OnClick += clickEvent;

            listToAdd.Add(button);

            return button;
        }

        private Button AddButton
            (string filename, float scale, Vector2 position, EventDelegate clickEvent, EventDelegate hoverEvent, List<UIElement> listToAdd)
        {
            //create the button texture
            Texture2D texture = cm.Load<Texture2D>(filename);

            //create the button
            Button button = new Button(texture, position, scale);

            button.OnClick += clickEvent;
            button.OnHover += hoverEvent;

            listToAdd.Add(button);

            return button;
        }

        /// <summary>
        /// creates a UIElement and adds it to the list elements
        /// </summary>
        /// <param name="uiObject">the ui object / element</param>
        /// <param name="gs">the gamestate the element exists in</param>
        /// <param name="uiEvent">the data which it needs for its events</param>
        /// <param name="types">the event types it can call</param>
        private Background AddBackground
            (string filename, float scale, Vector2 position, List<UIElement> listToAdd)
        {
            //create the menus texture
            Texture2D texture = cm.Load<Texture2D>(filename);

            //create the button
            Background background = new Background(texture, position, scale);

            listToAdd.Add(background);

            return background;
        }

        public Text AddText(string content, Vector2 position, float scale, Color tint, UIState uIState)
        {

            if (scale < 14 && scale > 0)
            {
                scale = 12;
            }
            else if (scale < 24 && scale > 15)
            {
                scale = 18;
            }
            else if (scale > 25)
            {
                scale = 36;
            }
            else
            {
                scale = 12;
            }

            switch (uIState)
            {
                case UIState.BaseMenu:
                    return AddText($"arial_{scale}", content, position, tint, null, menu);
                    break;
                case UIState.BaseGame:
                    return AddText($"arial_{scale}", content, position, tint, null, game);
                    break;
                case UIState.BasePause:
                    return AddText($"arial_{scale}", content, position, tint, null, pause);
                    break;
                case UIState.BaseGameOver:
                    return AddText($"arial_{scale}", content, position, tint, null, gameOver);
                    break;
                default:
                    return null;
                    break;
            }
        }

        private Text AddText(string filename, string content, Vector2 position, TextEvent changeText, List<UIElement> listToAdd)
        {
            SpriteFont font = cm.Load<SpriteFont>(filename);

            Text text = new Text(font, content, position);

            text.UpdateText += changeText;

            listToAdd.Add(text);

            return text;
        }

        private Text AddText(string filename, string content, Vector2 position, Color tint, TextEvent changeText, List<UIElement> listToAdd)
        {
            SpriteFont font = cm.Load<SpriteFont>(filename);

            Text text = new Text(font, content, position, tint);

            text.UpdateText += changeText;

            listToAdd.Add(text);

            return text;
        }

        private List<Text> AddText(string filename, string content, Vector2 position, int lineCapacity, int spacing, List<UIElement> listToAdd)
        {
            SpriteFont font = cm.Load<SpriteFont>(filename);

            List<string> contentDivided = new List<string>();

            while (content.Length != 0)
            {
                int finalPoint = 0;

                for (int i = 0; i < lineCapacity; i++)
                {
                    if (i + 1 == content.Length)
                    {
                        finalPoint = i;
                        break;
                    }
                    if (content[i] == ' ')
                    {
                        finalPoint = i;
                    }

                }

                string dividedPortion = content.Substring(0, finalPoint + 1);
                contentDivided.Add(dividedPortion);

                if (content.Length != 0)
                {
                    content = content.Substring(finalPoint + 1, content.Length - finalPoint - 1);
                }
            }

            List<Text> textList = new List<Text>();

            for (int i = 0; i < contentDivided.Count; i++)
            {
                Text text = new Text(font, contentDivided[i],
                    new Vector2(position.X, position.Y + spacing));

                listToAdd.Add(text);

                textList.Add(text);
            }

            return textList;
        }

        private List<Text> AddText(string filename, string content, Vector2 position, int lineCapacity, int spacing, Color tint, List<UIElement> listToAdd)
        {
            SpriteFont font = cm.Load<SpriteFont>(filename);

            List<string> contentDivided = new List<string>();

            while (content.Length != 0)
            {
                int finalPoint = 0;

                for (int i = 0; i < lineCapacity; i++)
                {
                    if (i + 1 == content.Length)
                    {
                        finalPoint = i;
                        break;
                    }
                    if (content[i] == ' ')
                    {
                        finalPoint = i;
                    }
                }

                string dividedPortion = content.Substring(0, finalPoint + 1);
                contentDivided.Add(dividedPortion);

                if (content.Length != 0)
                {
                    content = content.Substring(finalPoint + 1, content.Length - finalPoint - 1);
                }
            }

            List<Text> textList = new List<Text>();

            for (int i = 0; i < contentDivided.Count; i++)
            {
                Text text = new Text(font, contentDivided[i],
                    new Vector2(position.X, position.Y + (spacing * i)), tint);

                listToAdd.Add(text);

                textList.Add(text);
            }

            return textList;
        }

        public Slider AddSlider(string back, string knotch, float scale, Vector2 position, List<UIElement> listToAdd)
        {
            //create the menus texture
            Texture2D backTexture = cm.Load<Texture2D>(back);
            Texture2D knotchTexture = cm.Load<Texture2D>(knotch);

            //create the button
            Slider slider = new Slider(backTexture, knotchTexture, position, scale);

            listToAdd.Add(slider);

            return slider;
        }

        #endregion

        #region Element Updates

        /// <summary>
        /// updates all of the objects within the list of UIElements
        /// </summary>
        /// <param name="gs">the current gameState</param>
        private void UpdateObjects(List<UIElement> elementList)
        {
            foreach (UIElement element in elementList)
            {
                //casting the object down to its original form
                if (element is Button)
                {
                    Button button = (Button)element;

                    //run the update of the button and store what event it returns
                    button.Update();
                }
                else if (element is Background)
                {
                    Background background = (Background)element;

                    //run the update of the menu and store what event it returns
                    background.Update();
                }
                else if (element is Text)
                {
                    Text text = (Text)element;

                    text.Update();
                }
                else if (element is Slider)
                {
                    Slider slider = (Slider)element;

                    slider.Update();
                }
            }
        }

        /// <summary>
        /// draw every object in the current game state to the screen
        /// </summary>
        /// <param name="gs">the current gameState</param>
        private void DrawObjects(List<UIElement> elementList)
        {
            foreach (UIElement element in elementList)
            {
                //cast it down to its original form
                if (element is Button)
                {
                    Button button = (Button)element;

                    //and draw it
                    button.Draw(sb);
                }
                else if (element is Background)
                {
                    Background background = (Background)element;

                    //and draw it
                    background.Draw(sb);
                }
                else if (element is Text)
                {
                    Text text = (Text)element;

                    text.Draw(sb);
                }
                else if (element is Slider)
                {
                    Slider slider = (Slider)element;

                    slider.Draw(sb);
                }
            }

        }

        #endregion

        #region Input Helpers

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

        public void KeyboardSelection(List<UIElement> current)
        {
            if (keyboardIsActive)
            {
                bool switchedButton = false;
                float closeButton = 10000000;

                foreach (UIElement element in current)
                {
                    if (element is Button)
                    {
                        if (Keyboard.GetState().IsKeyDown(Keys.Down) && element.UIPosition.Y > selectedButton)
                        {
                            if (Math.Abs(selectedButton - element.UIPosition.Y) < Math.Abs(selectedButton - closeButton))
                            {
                                closeButton = element.UIPosition.Y;
                            }
                            switchedButton = true;
                        }
                        if (Keyboard.GetState().IsKeyDown(Keys.Up) && element.UIPosition.Y < selectedButton)
                        {
                            if (Math.Abs(selectedButton - element.UIPosition.Y) < Math.Abs(selectedButton - closeButton))
                            {
                                closeButton = element.UIPosition.Y;
                            }
                            switchedButton = true;
                        }

                    }
                }
                if (switchedButton)
                {
                    selectedButton = closeButton;
                }
                
            }
        }

        public void ResetButtons()
        {
            foreach(UIElement element in currentMenu.Peek())
            {
                if (element is Button)
                {
                    Button button = (Button)element;

                    button.UITexture = button.BaseTexture;
                }
            }
        }
           

        #endregion

        #region FileIO Helpers

        /// <summary>
        /// reads in a level file and creates a list of a certain element within it
        /// </summary>
        /// <param name="type">the type of data you want returned</param>
        /// <returns>a list of a certain type of data</returns>
        public List<int[]> LevelReader(TileType type)
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
