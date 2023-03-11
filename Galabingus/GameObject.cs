﻿using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

// Matthew Rodriguez
// 2023, 3, 6
// GameObject
// Provides essentails for all game objects
//
// Setup:
// Create a dynamic for the initalization process
// Set the dynamic to the result of the Initlizer method Initialize()
// Accessing GameObjects:
// Set the instance like so: GameObject.Instance.Content = GameObject.Instance.Content.YOURCONTENTNAME
// Note: YOURCONTENTNAME must follow file name specifications ie _strip(i) where (i) is the number of sprites
//
// Inharatence:
// There is no default constructor so use one of the parameter constructors,
// you MUST re-implement every used property using the content setter like so:
// GameObject.Instance.Content = GameObject.Instance.Content.YOURCONTENTNAME
// then use the property as normal
//
// GameObject.Instance - All of the instances of GameObject
// GameObject.Instance.Index - The current GameObject
// GameObject.Instance.Content - Dynamic required for creating content on the fly
// GameObject.Instance.GraphicsDevice - a GraphicsDevice
// GameObject.Instance.SpriteBatch - a SpriteBatch
// GameObject.Instance[Animation.Empty] - animation array of the current Instance
// GameObject.Instance[Collider.Empty] - collider array of the current Instance
// GameObject.Instance[Vector2.Zero] - position array of the current Instance
// GameObject.Instance[Rectangle.Empty] - transform array of the current Instance
// GameObject.Instance[i, Rectangle.Empty] - transform at i of the current Instance
// GameObject.Instance[i, Vector2.Zero] - position at i of the current Instance
// GameObject.Instance[i, Animation.Empty] - animation at i of the current Instance
// GameObject.Instance[i, Collider.Empty] - collider at i of the current Instance
// GameObject.Sprite - sprite of the current Instance
// GameObject.Scale - scale of the sprite of the current Instance
// GameObject.Colliders - All instances collider arrays <!> Warning <!>

namespace Galabingus
{
    /// <summary>
    ///  GameObject contains
    ///  the common funtionalites 
    ///  all game objects have
    /// </summary>
    internal class GameObject : DynamicObject
    {
        private const byte animationConst = 0;
        private const byte colliderConst = 1;
        private const byte transformConst = 2;
        private const byte positionConst = 3;
        private const byte spritesConst = 4;
        private const byte scalesConst = 5;
        private const byte objectEnumsConst = 6;
        private static GameObject allGameObjects = null; // GameObject singleton: contains all instances for all GameObjects
        private static List<Animation> animations = null;              // Animation content
        private static List<Collider> colliders = null;                // Collider content
        private static List<Rectangle> transforms = null;              // Transform content
        private static List<Vector2> positions = null;                 // Position content
        private static List<Texture2D> sprites = null;                 // Sprite content
        private static List<float> scales = null;                      // Scale content
        private static List<string> objectEnums = null;                // Actual content names
        unsafe private static GameObjectTrie<Animation> animationsI;              // Animation content
        unsafe private static GameObjectTrie<Collider> collidersI;                // Collider content
        unsafe private static GameObjectTrie<Rectangle> transformsI;              // Transform content
        unsafe private static GameObjectTrie<Vector2> positionsI;                 // Position content
        unsafe private static GameObjectTrie<Texture2D> spritesI;                 // Sprite content
        unsafe private static GameObjectTrie<float> scalesI;                      // Scale content
        unsafe private static GameObjectTrie<string> objectEnumsI;                // Actual content names
        private ushort index;                            // The current content index in all of the content arrays
        private ushort instance;
        private ContentManager contentManager;           // Used to load in the sprite
        private GraphicsDevice graphicsDevice;           // Graphics Device
        private SpriteBatch spriteBatch;                 // Sprite Batch
        private static List<List<List<ushort>>> trie;

        public struct GameObjectTrie<T>
        {
            
            public object GetPass(ushort layer1Find, ushort layer3Pass)
            {
                switch (layer1Find)
                {
                    case animationConst:
                        return GameObjectTrie<Animation>.Get(layer1Find, layer3Pass, GameObject.AnimationsI);
                    case colliderConst:
                        return GameObjectTrie<Collider>.Get(layer1Find, layer3Pass, GameObject.CollidersI);
                    case transformConst:
                        return GameObjectTrie<Rectangle>.Get(layer1Find, layer3Pass, GameObject.TransformsI);
                    case positionConst:
                        return GameObjectTrie<Vector2>.Get(layer1Find, layer3Pass, GameObject.PositionsI);
                    case spritesConst:
                        return GameObjectTrie<Texture2D>.Get(layer1Find, layer3Pass, GameObject.SpritesI);
                    case objectEnumsConst:
                        return GameObjectTrie<string>.Get(layer1Find, layer3Pass, GameObject.ObjectEnumsI);
                    case scalesConst:
                        return GameObjectTrie<float>.Get(layer1Find, layer3Pass, GameObject.ScalesI);
                    default:
                        return GameObjectTrie<Texture2D>.Get(layer1Find, layer3Pass, GameObject.SpritesI);
                }
            }

            public void SetPass(ushort layer1Pass, ushort layer3Pass, object value)
            {
                switch (layer1Pass)
                {
                    case animationConst:
                        GameObjectTrie<Animation>.Set(layer1Pass, layer3Pass, GameObject.AnimationsI, (Animation)value);
                        break;
                    case colliderConst:
                        GameObjectTrie<Collider>.Set(layer1Pass, layer3Pass, GameObject.CollidersI, (Collider)value);
                        break;
                    case transformConst:
                        GameObjectTrie<Rectangle>.Set(layer1Pass, layer3Pass, GameObject.TransformsI, (Rectangle)value);
                        break;
                    case positionConst:
                        GameObjectTrie<Vector2>.Set(layer1Pass, layer3Pass, GameObject.PositionsI, (Vector2)value);
                        break;
                    case spritesConst:
                        GameObjectTrie<Texture2D>.Set(layer1Pass, layer3Pass, GameObject.SpritesI, (Texture2D)value);
                        break;
                    case objectEnumsConst:
                        GameObjectTrie<string>.Set(layer1Pass, layer3Pass, GameObject.ObjectEnumsI, (string)value);
                        break;
                    case scalesConst:
                        GameObjectTrie<float>.Set(layer1Pass, layer3Pass, GameObject.ScalesI, (float)value);
                        break;
                    default:
                        GameObjectTrie<float>.Set(layer1Pass, layer3Pass, GameObject.ScalesI, (float)value);
                        break;
                }
            }

            #nullable disable
            public static T Get(ushort layer1Find, ushort layer3Find, List<T> data)
            {
                if (layer1Find >= Trie.Count)
                {
                    for (ushort i = (ushort)Trie.Count; i <= layer1Find; i++)
                    {
                        Trie.Add(new List<List<ushort>>());
                    }
                }
                if (GameObject.Instance.Index >= Trie[layer1Find].Count)
                {
                    for (ushort i = (ushort)Trie[layer1Find].Count; i <= GameObject.Instance.Index; i++)
                    {
                        Trie[layer1Find].Add(new List<ushort>());
                    }
                }
                if (layer3Find >= Trie[layer1Find][GameObject.Instance.Index].Count)
                {
                    for (ushort i = (ushort)Trie[layer1Find][GameObject.Instance.Index].Count; i <= layer3Find; i++)
                    {
                        Trie[layer1Find][GameObject.Instance.Index].Add((ushort)(data.Count));
                        data.Add(default(T));
                    }
                }

                return data[Trie[layer1Find][GameObject.Instance.Index][layer3Find]];
            }

            public static ushort Add(ushort layer1Find, List<T> data, T value)
            {
                if (layer1Find >= Trie.Count)
                {
                    for (ushort i = (ushort)Trie.Count; i <= layer1Find; i++)
                    {
                        Trie.Add(new List<List<ushort>>());
                    }
                }
                if (GameObject.Instance.Index >= Trie[layer1Find].Count)
                {
                    for (ushort i = (ushort)Trie[layer1Find].Count; i <= GameObject.Instance.Index; i++)
                    {
                        Trie[layer1Find].Add(new List<ushort>());
                    }
                }
                Trie[layer1Find][GameObject.Instance.Index].Add((ushort)data.Count);
                ushort layer3Find = (ushort)(data.Count);
                data.Add(value);
                return layer3Find;
            }

            public static void Set(ushort layer1Find, ushort layer3Find, List<T> data, T value)
            {
                if (layer1Find >= Trie.Count)
                {
                    for (ushort i = (ushort)Trie.Count; i <= layer1Find; i++)
                    {
                        Trie.Add(new List<List<ushort>>());
                    }
                }
                if (GameObject.Instance.Index >= Trie[layer1Find].Count)
                {
                    for (ushort i = (ushort)Trie[layer1Find].Count; i <= GameObject.Instance.Index; i++)
                    {
                        Trie[layer1Find].Add(new List<ushort>());
                    }
                }
                if (layer3Find >= Trie[layer1Find][GameObject.Instance.Index].Count)
                {
                    for (int i = Trie[layer1Find][GameObject.Instance.Index].Count; i <= layer3Find; i++)
                    {
                        Trie[layer1Find][GameObject.Instance.Index].Add((ushort)(data.Count));
                        data.Add(value);
                    }
                }

                data[Trie[layer1Find][GameObject.Instance.Index][layer3Find]] = value;
            }

            public static List<T> GetArray(ushort layer1Find, List<T> data)
            {
                if (layer1Find >= Trie.Count)
                {
                    for (int i = Trie.Count; i <= layer1Find; i++)
                    {
                        Trie.Add(new List<List<ushort>>());
                    }
                }
                if (GameObject.Instance.Index >= Trie[layer1Find].Count)
                {
                    for (int i = Trie[layer1Find].Count; i <= GameObject.Instance.Index; i++)
                    {
                        Trie[layer1Find].Add(new List<ushort>());
                    }
                }

                List<T> result = new List<T>();
                foreach (int index in Trie[layer1Find][GameObject.Instance.Index])
                {
                    result.Add(data[index]);
                }

                return result;
            }
            #nullable enable
        }

        public ushort InstanceID
        {
            get
            {
                return instance;
            }
            set
            {
                instance = value;
            }
        }

        /// <summary>
        ///  GameObject master storage place
        /// </summary>
        public static GameObject Instance
        {
            // Singleton for GameObject
            get
            {
                if (allGameObjects == null)
                {
                    allGameObjects = new GameObject();
                }
                return allGameObjects;
            }
        }

        private static List<List<List<ushort>>> Trie
        {
            get
            {
                if (trie == null)
                {
                    trie = new List<List<List<ushort>>>();
                }
                return trie;
            }
            set
            {
                trie = value;
            }
        }

        private static List<Texture2D> SpritesI
        {
            get
            {
                if (sprites == null)
                {
                    sprites = new List<Texture2D>();
                }
                return sprites;
            }
            set
            {
                sprites = value;
            }
        }

        private static List<float> ScalesI
        {
            get
            {
                if (scales == null)
                {
                    scales = new List<float>();
                }
                return scales;
            }
            set
            {
                scales = value;
            }
        }

        private static List<string> ObjectEnumsI
        {
            get
            {
                if (objectEnums == null)
                {
                    objectEnums = new List<string>();
                }
                return objectEnums;
            }
            set
            {
                objectEnums = value;
            }
        }

        private static List<Animation> AnimationsI
        {
            get
            {
                if (animations == null)
                {
                    animations = new List<Animation>();
                }
                return animations;
            }
            set
            {
                animations = value;
            }
        }

        private static List<Collider> CollidersI
        {
            get
            {
                if (colliders == null)
                {
                    colliders = new List<Collider>();
                }
                return colliders;
            }
            set
            {
                colliders = value;
            }
        }

        private static List<Rectangle> TransformsI
        {
            get
            {
                if (transforms == null)
                {
                    transforms = new List<Rectangle>();
                }
                return transforms;
            }
            set
            {
                transforms = value;
            }
        }

        private static List<Vector2> PositionsI
        {
            get
            {
                if (positions == null)
                {
                    positions = new List<Vector2>();
                }
                return positions;
            }
            set
            {
                positions = value;
            }
        }

        public Texture2D GetSprite(ushort instancePass)
        {
            #nullable disable
            unsafe
            {
                return (spritesI).GetPass(spritesConst,instancePass) as Texture2D;
            }
            #nullable enable
        }

        public float GetScale(ushort instancePass)
        {
            #nullable disable
            unsafe
            {
                return (float)(scalesI).GetPass(scalesConst, instancePass);
            }
            #nullable enable
        }

        public string GetObjectEnum(ushort instancePass)
        {
            #nullable disable
            unsafe
            {
                return (objectEnumsI).GetPass(objectEnumsConst, instancePass) as string;
            }
            #nullable enable
        }

        public Animation GetAnimation(ushort instancePass)
        {
            #nullable disable
            unsafe
            {
                return (animationsI).GetPass(animationConst, instancePass) as Animation;
            }
            #nullable enable
        }

        public Collider GetCollider(ushort instancePass)
        {
            #nullable disable
            unsafe
            {
                return (collidersI).GetPass(colliderConst, instancePass) as Collider;
            }
            #nullable enable
        }

        public Rectangle GetTransform(ushort instancePass)
        {
            #nullable disable
            unsafe
            {
                return (Rectangle)(transformsI).GetPass(transformConst, instancePass);
            }
            #nullable enable
        }

        public Vector2 GetPosition(ushort instancePass)
        {
            #nullable disable
            unsafe
            {
                return (Vector2)(positionsI).GetPass(positionConst, instancePass);
            }
            #nullable enable
        }



        public void SetSprite(ushort instancePass, object value)
        {
            #nullable disable
            unsafe
            {
                (spritesI).SetPass(spritesConst, instancePass, value);
            }
            #nullable enable
        }

        public void SetScale(ushort instancePass, object value)
        {
            #nullable disable
            unsafe
            {
                (scalesI).SetPass(scalesConst, instancePass, value);
            }
            #nullable enable
        }

        public void SetObjectEnum(ushort instancePass, object value)
        {
            #nullable disable
            unsafe
            {
                (objectEnumsI).SetPass(objectEnumsConst, instancePass, value);
            }
            #nullable enable
        }

        public void SetAnimation(ushort instancePass, object value)
        {
            #nullable disable
            unsafe
            {
                (animationsI).SetPass(animationConst, instancePass, value);
            }
            #nullable enable
        }

        public void SetCollider(ushort instancePass, object value)
        {
            #nullable disable
            unsafe
            {
                (collidersI).SetPass(colliderConst, instancePass, value);
            }
            #nullable enable
        }

        public void SetTransform(ushort instancePass, object value)
        {
            #nullable disable
            unsafe
            {
                (transformsI).SetPass(transformConst, instancePass, value);
            }
            #nullable enable
        }

        public void SetPosition(ushort instancePass, object value)
        {
            #nullable disable
            unsafe
            {
                (positionsI).SetPass(positionConst, instancePass, value);
            }
            #nullable enable
        }

        /// <summary>
        ///  Current index relation to all of the content arrays
        /// </summary>
        public ushort Index
        {
            // ONLY allow for receiving the index here
            get
            {
                return GameObject.Instance.index;
            }
        }

        /// <summary>
        ///  Gets the GameObject Instance for the index
        ///  Sets the index for the Instance
        /// </summary>
        public dynamic Content
        {
            get
            {
                return this;
            }
            set
            {
                GameObject.Instance.index = value;
            }
        }

        /// <summary>
        ///  Graphics Device
        /// </summary>
        public GraphicsDevice GraphicsDevice
        {
            // Only allow to recive the GraphicsDevice
            get
            {
                return GameObject.Instance.graphicsDevice;
            }
        }

        /// <summary>
        ///  Sprite Batch
        /// </summary>
        public SpriteBatch SpriteBatch
        {
            // Only alow to set the SpriteBatch
            get
            {
                return GameObject.Instance.spriteBatch;
            }
        }

        /// <summary>
        ///  Generates a index for the instance Content 
        ///  property that cannot be found
        ///  The binder name will be stored at the index
        ///  in objectEnums that matches the index generated
        /// </summary>
        /// <param name="binder">Property binder</param>
        /// <param name="result">The generated index</param>
        /// <returns>GameObject Index</returns>
        public override bool TryGetMember(
            GetMemberBinder binder,
            out object result
        )
        {
            bool exist = false; // If the property exist
            ushort index = 0;   // The index of the property

            // Search to find the property
            foreach (string sprite in GameObject.ObjectEnumsI)
            {
                if (binder.Name == sprite)
                {
                    exist = true;
                }
                if (exist)
                {
                    this.index = index;
                    result = index;
                    //GameObject.Instance.Content = index;
                    return exist;
                }
                index++;
            }

            // When teh property does not exist create it
            if (!exist)
            {
                exist = true;
                GameObject.ObjectEnumsI.Add(binder.Name);
            }
            this.index = index;
            result = index;
            //GameObject.Instance.Content = index;
            return exist;
        }

        private GameObject()
        {
            // Does nothing, just is used to create a singleton instance
        }

        /// <summary>
        ///  Creates a GameObject from the content name
        ///  the content name must be in the format file_strip(i) where 
        ///  (i) is the number of sprites in the sprite sheet
        ///  Define this GameObject as a specific instance via its instanceNumber
        /// </summary>
        /// <param name="contentName">content name</param>
        /// <param name="instanceNumber">index of the instance</param>
        unsafe public GameObject(
            ushort contentName,
            ushort instanceNumber
        )
        {
            GameObject.Instance.Content = contentName;
            instance = instanceNumber;
            string path = GameObject.ObjectEnumsI[contentName];
            ushort strip = ushort.Parse(path.Split("strip")[1]);
            GameObject.Instance.index = contentName;
            SetSprite(instanceNumber,GameObject.Instance.contentManager.Load<Texture2D>(path));
            SetScale(instanceNumber,1.0f);
            SetAnimation(instanceNumber, new Animation(GetSprite(instanceNumber).Width, GetSprite(instanceNumber).Height, strip));
            Collider newCollider = new Collider();
            newCollider.Layer = contentName;
            SetCollider(instanceNumber, newCollider);
            SetPosition(instanceNumber, Vector2.Zero);
            SetTransform(instanceNumber,
                new Rectangle(
                    (GetSprite(instanceNumber).Width / strip),         // Sprite starts at the frame starting position
                    0,                                                 // Sprite starts at Y = 0
                    GetSprite(instanceNumber).Width / strip,           // Width of the sprite
                    GetSprite(instanceNumber).Height                   // Height of the sprite
                )
            );
        }

        /// <summary>
        ///  Initalizes the instance with 
        ///  a contentManager, graphicsDevice,
        ///  spriteBatch
        /// </summary>
        /// <param name="contentManager">Any: ContentManager</param>
        /// <param name="graphicsDevice">Any: GraphicsDevice</param>
        /// <param name="spriteBatch">Any: SpriteBatch</param>
        /// <returns></returns>
        public dynamic Initialize(ContentManager contentManager, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            this.graphicsDevice = graphicsDevice;
            this.spriteBatch = spriteBatch;
            this.contentManager = contentManager;
            return new GameObject();
        }
    }
}