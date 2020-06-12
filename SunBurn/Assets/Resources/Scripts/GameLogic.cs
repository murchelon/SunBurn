using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using SubBurn;
using UnityEditor;
using TMPro;

namespace SunBurn
{

    public class clsItem
    {
        public int ID;
        public string name;
        public int value;
        public string tipo;
        public Sprite spriteObj;

        public clsItem(int _ID,
                             string _Name,
                             int _value,
                             string _tipo,
                             Sprite _spriteObj)
        {
            this.ID = _ID;
            this.name = _Name;
            this.value = _value;
            this.tipo = _tipo;
            this.spriteObj = _spriteObj;
        }
    }


    public class clsItemInGame : clsItem
    {
        //public Vector3 positionInGame;
        public int quadrantInGame;
        public GameObject go;
        //public float instanceID;



        public clsItemInGame(clsItem clsitem,
                             int _quadrantInGame,
                             GameObject _go) : base(clsitem.ID, clsitem.name, clsitem.value, clsitem.tipo, clsitem.spriteObj)
        {
            this.quadrantInGame = _quadrantInGame;
            this.go = _go;
        }
    }

    public class Quadrant
    {
        public float X_TL;
        public float Y_TL;

        public float X_TR;
        public float Y_TR;

        public float X_BL;
        public float Y_BL;

        public float X_BR;
        public float Y_BR;
        
        public float offset_from_Zero_X;
        public float offset_from_Zero_Y_to_top;
        public float offset_from_Zero_Y_to_bottom;

        public float gameArea_H;

        public float Y_Middle_HOR;
        public float X_Middle_VERT;

        public Quadrant(float _total_W, float _total_H)
        {
            this.offset_from_Zero_X = _total_W / 2 - 0.13f;
            this.offset_from_Zero_Y_to_top = _total_H * 0.21f;
            this.offset_from_Zero_Y_to_bottom = (_total_H / 2) * 0.97f;
      
            this.gameArea_H = offset_from_Zero_Y_to_top + offset_from_Zero_Y_to_bottom; 

            this.Y_Middle_HOR = (offset_from_Zero_Y_to_bottom * -1) + (this.gameArea_H / 2);
            this.X_Middle_VERT = 0f;

            this.X_TL = offset_from_Zero_X * -1f;   // top left
            this.Y_TL = offset_from_Zero_Y_to_top; //top left

            this.X_TR = offset_from_Zero_X; // top right
            this.Y_TR = offset_from_Zero_Y_to_top; // top right

            this.X_BL = offset_from_Zero_X * -1f; // bottom left
            this.Y_BL = offset_from_Zero_Y_to_bottom * -1f;// bottom left

            this.X_BR = offset_from_Zero_X; // bottom right
            this.Y_BR = offset_from_Zero_Y_to_bottom * -1f; // botom right


        }
    }

    public class GameLogic : MonoBehaviour
    {

        public float timeToDrawNewItem;

        public float timeToFadeAndDestroyItem;

        public float timeBeingInvencible;

        // Sprites de itens
        private Sprite sprite_Item_Bucket;
        private Sprite sprite_Item_Coke;
        private Sprite sprite_Item_Drink1;
        private Sprite sprite_Item_Umbrella;
        private Sprite sprite_Item_Glasses;
        private Sprite sprite_Item_Duck;
        private Sprite sprite_Item_Stereo;
        private Sprite sprite_Item_IceCream;
        private Sprite sprite_Item_SunBlocker;

        // sprites do Sun
        private Sprite spriteSun_Left1;
        private Sprite spriteSun_Left2;
        private Sprite spriteSun_Right1;
        private Sprite spriteSun_Right2;
        private Sprite spriteSun_Front;
        private Sprite spriteSun_Back;



        // Sun
        public float timeToDrawNewSunState;

        private bool isSunCyclingStates;

        private bool isHeatTime;

        private GameObject glowSunGameObject;
        private GameObject tintBackGroundGameObject;

        private int percNoSun;
        private int percAlmostSun;
        //private int percYesSun;

        SpriteRenderer sunSpriteRender;

        private IEnumerator coroutineSunState;
        private IEnumerator coroutineDrawItens;
        private IEnumerator coroutineVanishItem;
        private IEnumerator coroutineTimer;
        private IEnumerator coroutineTimeInvensible; 


        private Animator glowAnimator;

        private SunState sunStates;

        private List<clsItem> aItens_Original = new List<clsItem>();
        private List<clsItem> aItens_Avaliable = new List<clsItem>();
        private List<clsItemInGame> aItens_Used = new List<clsItemInGame>();
        



        private bool debug_Allow_Coroutine_Sun;
        private bool debug_Allow_Coroutine_Itens;
        private bool debug_Allow_Coroutine_VanishItem;

        private float percScaleItemSize;

        private float backWidth;
        private float backHeight;
        private GameObject player;
        private SpriteRenderer playerRender;

        private TextMeshPro txtScore;
        //private TextMeshPro txtScoreInPanel;
        private GameObject txtScoreInPanel;
        

        private SpriteRenderer sprRendSunBlockCounter;
        private TextMeshPro txtLabelSunBlockCounter;

        private TextMeshPro txtLabelTimerValue;

        private Quadrant gameQuadrantsCoord;

        private GameObject panelEndGame;
        
        private float timeSinceBegin;

        enum SunState
        {
            YES_Sun,
            NO_Sun,
            ALMOST_Sun
        }




        void OnGUI()
        {
            GUIStyle style = new GUIStyle();

            style.normal.textColor = Color.gray;

            GUI.Label(new Rect(0, 0, 100, 100), "FPS: " + (1.0f / Time.smoothDeltaTime).ToString(), style);
        }


        private void Init_Params()
        {


            // sun
            timeToDrawNewSunState = 2f;

            // itens
            timeToDrawNewItem = 2f; // 
            timeToFadeAndDestroyItem = 0.1f;  // the less the number is, the fastest the fade will be

            timeBeingInvencible = 3f;

            isSunCyclingStates = true;

            sunStates = SunState.NO_Sun;
            isHeatTime = false;

            percNoSun = 30;
            percAlmostSun = 45;
            //percYesSun = 25;

            debug_Allow_Coroutine_Sun = true;
            debug_Allow_Coroutine_Itens = true;
            debug_Allow_Coroutine_VanishItem = true;

            GameManager.Instance.playareaOffsetFromTop = 2.6f;
            GameManager.Instance.score = 0;
            GameManager.Instance.isOnShade = false;
            GameManager.Instance.numberSunBlocks = 0;
            GameManager.Instance.playerIsDead = false;
            GameManager.Instance.isInvencible = false;

            

            percScaleItemSize = 0.25f;


        }


        void Awake()
        {

            Init_Params();

            Init_Sprites();

            Init_Itens();

            CleanAndCopyItensListToGameList();


            // get the sun sprite render component
            sunSpriteRender = GameObject.Find("Sun").GetComponent<SpriteRenderer>();
            sunSpriteRender.sprite = spriteSun_Back;

            // get the glow game object and animator
            glowSunGameObject = GameObject.Find("GlowAnimation").gameObject;
            glowAnimator = glowSunGameObject.GetComponent<Animator>();
            glowSunGameObject.SetActive(false);

            // get the red backgrund game object
            tintBackGroundGameObject = GameObject.Find("back_tint_red").gameObject;
            tintBackGroundGameObject.SetActive(false);

            // get the player
            player = GameObject.Find("Player").gameObject;
            playerRender = player.GetComponent<SpriteRenderer>();

            // position player
            player.transform.position = new Vector3(-2.6f, -3.8f, 1f);
            //player.transform.position = new Vector3(0, 0, 1f);

            player.GetComponent<PlayerMove>().moveSpeed = 3.3f;


            txtScore = GameObject.Find("label_ScoreValue").gameObject.GetComponent<TextMeshPro>();
            txtScore.SetText(GameManager.Instance.score.ToString());


            // define the size of the area on where the itens appear
            backWidth = GameObject.Find("Background").GetComponent<SpriteRenderer>().bounds.size.x;
            backHeight = GameObject.Find("Background").GetComponent<SpriteRenderer>().bounds.size.y;


            // define the quadrants and its borders
            gameQuadrantsCoord = new Quadrant(backWidth, backHeight);

            // draw the umbrella:
            int umbrellaQuadrant;
            Vector3 umbrellaPos;
            bool flipUmbrella;

            if (Random.Range(0, 2) == 0)
            {
                umbrellaQuadrant = 1;
            }
            else
            {
                umbrellaQuadrant = 3;
            }


            umbrellaPos = GetXYforItem(umbrellaQuadrant, new Vector2(sprite_Item_Umbrella.bounds.size.x, sprite_Item_Umbrella.bounds.size.y), percScaleItemSize, "INCLUDE");

            // Debug.Log("umbrellaQuadrant = " + umbrellaQuadrant + " | pos = " + umbrellaPos);


            if (Random.Range(0, 2) == 0)
            {
                flipUmbrella = false;
            }
            else
            {
                flipUmbrella = true;
            }

            GameObject go = RenderItemInScreen(sprite_Item_Umbrella,
                                               umbrellaPos,
                                               transform,
                                               "UMBRELLA",
                                               "fore",
                                               0,
                                               flipUmbrella
                                               );


            sprRendSunBlockCounter = GameObject.Find("SunBlockCounter").GetComponent<SpriteRenderer>();
            sprRendSunBlockCounter.color = new Color(sprRendSunBlockCounter.color.r, sprRendSunBlockCounter.color.g, sprRendSunBlockCounter.color.b, 0);

            txtLabelSunBlockCounter = GameObject.Find("label_SunBlockCounter").gameObject.GetComponent<TextMeshPro>();
            txtLabelSunBlockCounter.SetText(GameManager.Instance.numberSunBlocks.ToString());
            txtLabelSunBlockCounter.color = new Color(txtLabelSunBlockCounter.color.r, txtLabelSunBlockCounter.color.g, txtLabelSunBlockCounter.color.b, 0);

            txtLabelTimerValue = GameObject.Find("label_TimerValue").gameObject.GetComponent<TextMeshPro>();
            txtLabelTimerValue.SetText("00:00:00");

            txtScoreInPanel = GameObject.Find("label_TimerValue").gameObject;
            //txtScoreInPanel.SetText(GameManager.Instance.score.ToString());


            //Canvas_Dead

            panelEndGame = GameObject.Find("Canvas_Dead").gameObject;
            panelEndGame.SetActive(false);



        }


        private void Start()
        {
            //DrawSelectedItensOnScreen();

            // start the timer for the game
            coroutineTimer = CountDownTimer(30);
            StartCoroutine(coroutineTimer);

            //TesteGetXY(100, 3, "INCLUDE", sprite_Item_Coke, transform);
            //TesteGetXY(100, 4, "INCLUDE", sprite_Item_Coke, transform);

            timeSinceBegin = Time.time;

        }

        private void SetGameFininshed(string opt)
        {
            StopAllCoroutines();

            debug_Allow_Coroutine_Sun = false;
            debug_Allow_Coroutine_Itens = false;
            debug_Allow_Coroutine_VanishItem = false;

            coroutineSunState = null;
            coroutineDrawItens = null;
            coroutineVanishItem = null;


            //txtScoreInPanel.SetText(GameManager.Instance.score.ToString());

            panelEndGame.SetActive(true);

            if (opt == "DEAD")
            {

            }
            else
            {

            }
        }


        private void Update()
        {

            //DrawDebugLinesQuadrants();

            if (isHeatTime == true)
            {
                if (GameManager.Instance.isInvencible == false)
                {

                    if (GameManager.Instance.isOnShade == false)
                    {

                        if (GameManager.Instance.numberSunBlocks > 0)
                        {
                            GameManager.Instance.numberSunBlocks--;
                            txtLabelSunBlockCounter.SetText("x" + GameManager.Instance.numberSunBlocks.ToString());

                            if (GameManager.Instance.numberSunBlocks <= 0)
                            {
                                txtLabelSunBlockCounter.color = new Color(txtLabelSunBlockCounter.color.r, txtLabelSunBlockCounter.color.g, txtLabelSunBlockCounter.color.b, 0);
                                sprRendSunBlockCounter.color = new Color(sprRendSunBlockCounter.color.r, sprRendSunBlockCounter.color.g, sprRendSunBlockCounter.color.b, 0);
                            }

                            GameManager.Instance.isInvencible = true;

                            coroutineTimeInvensible = KeepPlayerInvensible(timeBeingInvencible);
                            StartCoroutine(coroutineTimeInvensible);
              
                        }
                        else
                        {
                            GameManager.Instance.playerIsDead = true;
                            SetGameFininshed("DEAD");
                        }

                    }
                }

            }
            else
            {
                //Debug.Log("IsHeatTime = FALSE");
            }


            //if (GameManager.Instance.isOnShade != true)
            //{
            //    if (GameManager.Instance.numberSunBlocks > 0)
            //    {
            //        GameManager.Instance.numberSunBlocks--;
            //        txtLabelSunBlockCounter.SetText(GameManager.Instance.numberSunBlocks.ToString());

            //        if (GameManager.Instance.numberSunBlocks == 0)
            //        {
            //            txtLabelSunBlockCounter.color = new Color(txtLabelSunBlockCounter.color.r, txtLabelSunBlockCounter.color.g, txtLabelSunBlockCounter.color.b, 0);
            //        }
            //    }
            //    else
            //    {

            //        GameManager.Instance.playerIsDead = true;


            //    }

            //}


            if (GameManager.Instance.playerIsDead == true)
            {
                //StopAllCoroutines();

                //debug_Allow_Coroutine_Sun = false;
                //debug_Allow_Coroutine_Itens = false;
                //debug_Allow_Coroutine_VanishItem = false;

                //coroutineSunState = null;
                //coroutineDrawItens = null;
                //coroutineVanishItem = null;

                panelEndGame.SetActive(true);

            }
           
            // Coroutine: Sun cycles and heatTime defining
            if (isSunCyclingStates && coroutineSunState == null && debug_Allow_Coroutine_Sun)
            {
                coroutineSunState = DefineAndRenderSunState();
                StartCoroutine(coroutineSunState);
            }

            // Coroutine: item draw on screen, add to the List with spawned itens
            if (coroutineDrawItens == null && debug_Allow_Coroutine_Itens)
            {
                coroutineDrawItens = wrapper_DrawSelectedItensOnScreen();
                StartCoroutine(coroutineDrawItens);
            }

        }


        private void Init_Sprites()
        {
            // Sun
            spriteSun_Left1 = Resources.Load("images/Sol_ESQ1_final", typeof(Sprite)) as Sprite;
            spriteSun_Left2 = Resources.Load("images/Sol_ESQ2_final", typeof(Sprite)) as Sprite;
            spriteSun_Right1 = Resources.Load("images/Sol_DIR1_final", typeof(Sprite)) as Sprite;
            spriteSun_Right2 = Resources.Load("images/Sol_DIR2_final", typeof(Sprite)) as Sprite;
            spriteSun_Front = Resources.Load("images/Sol_FRENTE_final", typeof(Sprite)) as Sprite;
            spriteSun_Back = Resources.Load("images/Sol_TRAS_final", typeof(Sprite)) as Sprite;

            // itens
            sprite_Item_Bucket = Resources.Load("images/Balde", typeof(Sprite)) as Sprite;
            sprite_Item_Coke = Resources.Load("images/CocaCola", typeof(Sprite)) as Sprite;
            sprite_Item_Drink1 = Resources.Load("images/Drink1", typeof(Sprite)) as Sprite;
            sprite_Item_Umbrella = Resources.Load("images/GuardaSol", typeof(Sprite)) as Sprite;
            sprite_Item_Glasses = Resources.Load("images/Oculos", typeof(Sprite)) as Sprite;
            sprite_Item_Duck = Resources.Load("images/Pato", typeof(Sprite)) as Sprite;
            sprite_Item_Stereo = Resources.Load("images/Som", typeof(Sprite)) as Sprite;
            sprite_Item_IceCream = Resources.Load("images/Sorvete", typeof(Sprite)) as Sprite;
            sprite_Item_SunBlocker = Resources.Load("images/SunBlocker", typeof(Sprite)) as Sprite;
        }


        private void Init_Itens()
        {
            aItens_Original.Add(new clsItem(0, "BUCKET", 10, "NORMAL_ITEM", sprite_Item_Bucket));
            aItens_Original.Add(new clsItem(1, "COKE", 10, "NORMAL_ITEM", sprite_Item_Coke));
            aItens_Original.Add(new clsItem(2, "DRINK1", 10, "NORMAL_ITEM", sprite_Item_Drink1));
            //aItens_Original.Add(new clsItem(3, "UMBRELLA", 0, "SAFE_ITEM", sprite_Item_Umbrella));
            aItens_Original.Add(new clsItem(3, "GLASSES", 10, "NORMAL_ITEM", sprite_Item_Glasses));
            aItens_Original.Add(new clsItem(4, "DUCK", 20, "NORMAL_ITEM", sprite_Item_Duck));
            aItens_Original.Add(new clsItem(5, "STEREO", 15, "NORMAL_ITEM", sprite_Item_Stereo));
            aItens_Original.Add(new clsItem(6, "ICEREAM", 10, "NORMAL_ITEM", sprite_Item_IceCream));
            aItens_Original.Add(new clsItem(7, "SUNBLOCKER", 15, "POWER_ITEM", sprite_Item_SunBlocker));
        }


        private void CleanAndCopyItensListToGameList()
        {
            aItens_Avaliable = new List<clsItem>(aItens_Original);
            //aItens_Used= new List<clsItemInGame>();
        }


        // get the desired itens that must appear on screen and render it
        private void DrawSelectedItensOnScreen()
        {

            int rand_number = -1;

            int player_quadrant = -1;
            int item_quadrant = -1;

            Vector3 newItemPosition;


            // define player and item quadrants
            player_quadrant = GetQuadrant(player.transform.position);


            int x = 0;

            // remove any pre-existing item
            foreach (clsItemInGame item in aItens_Used)
            {
                //Destroy(item.go);

                // Coroutine: item removal from screen and from list with spawned itens. Draws effect of disapiaring item (fading item)
                if (debug_Allow_Coroutine_VanishItem)
                {
                    coroutineVanishItem = VanishItemFromScreen(x, item.go);
                    StartCoroutine(coroutineVanishItem);

                    x++;
                }
            }

            // if all itens have been shown/the list is empty, reset the list
            if (aItens_Avaliable.Count == 0)
            {
                CleanAndCopyItensListToGameList();
            }




            //aItens_Used = new List<clsItemInGame>();

            bool numberOK = false;
            bool exitWhile = false;

            while (exitWhile == false)
            {
                // get a random item

                rand_number = Random.Range(0, aItens_Avaliable.Count);

                //Debug.Log("aItens_Avaliable.Count = " + aItens_Avaliable.Count + " | number = " + rand_number + " | name = " + aItens_Avaliable[rand_number].name);

                if (aItens_Avaliable[rand_number].name == "SUNBLOCKER")
                {
                    if ((GameManager.Instance.numberSunBlocks >= 2))
                    {

                        if (aItens_Avaliable.Count == 1)
                        {
                            aItens_Avaliable.RemoveAt(rand_number);
                            exitWhile = true;
                            numberOK = false;
                        }
                        else
                        {
                            exitWhile = false;
                            numberOK = false;
                        }

                    }
                    else
                    {
                        exitWhile = true;
                        numberOK = true;
                    }
                }
                else
                {
                    exitWhile = true;
                    numberOK = true;
                }



            }



            if (numberOK == true)
            {


                // define the new item quadrand, different then the player's quadrant
                newItemPosition = GetXYforItem(player_quadrant, new Vector2(aItens_Avaliable[rand_number].spriteObj.bounds.size.x, aItens_Avaliable[rand_number].spriteObj.bounds.size.y), percScaleItemSize);

                // get the item quadrant
                item_quadrant = GetQuadrant(newItemPosition);

                bool flipItem = false;

                if (aItens_Avaliable[rand_number].name != "SUNBLOCKER" && aItens_Avaliable[rand_number].name != "COKE" && aItens_Avaliable[rand_number].name != "DUCK")
                {
                    if (Random.Range(0, 2) == 0)
                    {
                        flipItem = true;
                    }
                }


                GameObject go = RenderItemInScreen(aItens_Avaliable[rand_number].spriteObj,
                                    newItemPosition,
                                    transform,
                                    aItens_Avaliable[rand_number].name,
                                    "fore",
                                    1,
                                    flipItem
                                    );




                aItens_Used.Add(new clsItemInGame(aItens_Avaliable[rand_number],
                                                  item_quadrant,
                                                  go));



                aItens_Avaliable.RemoveAt(rand_number);
            }

        }


        // draw 1 specific object on screen
        private GameObject RenderItemInScreen(Sprite theSprite, Vector3 position, Transform parent = null, string name = "NewGameObjRunTime", string layer="fore", int orderInLayer=1, bool flipSprite=false)
        {
            GameObject go;

            go = new GameObject();


            if (name != "NewGameObjRunTime")
            {
                go.name = name;
            }

            if (parent != null)
            {
                go.transform.SetParent(parent);
            }

            //item.AddComponent<Transform>();
            go.transform.position = position;
            go.transform.localScale = new Vector2(percScaleItemSize, percScaleItemSize);

            go.AddComponent<SpriteRenderer>();
            SpriteRenderer renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = theSprite;
            renderer.sortingLayerName = layer;
            renderer.sortingOrder = orderInLayer;

            if (flipSprite == true)
            {
                renderer.flipX = true;
            }
            

            go.AddComponent<BoxCollider2D>();
            go.GetComponent<BoxCollider2D>().isTrigger = true;
            
            return go;
        }



        private void TesteGetXY(int qtde, int quadrant, string opt, Sprite sprite, Transform parent)
        {

            float scale = 0.15f;

            for (int i = 0; i < qtde; i++)
            {
                Vector3 pos = GetXYforItem(quadrant, new Vector2(sprite.bounds.size.x, sprite.bounds.size.y), scale, opt);

                GameObject go;

                go = new GameObject();
                go.name = "Test_" + sprite.name + "_" + i.ToString();
                go.transform.SetParent(parent);
                go.transform.position = pos;
                go.transform.localScale = new Vector2(scale, scale);

                go.AddComponent<SpriteRenderer>();
                SpriteRenderer renderer = go.GetComponent<SpriteRenderer>();
                renderer.sprite = sprite;

                go.AddComponent<BoxCollider2D>();
                go.GetComponent<BoxCollider2D>().isTrigger = true;

                Debug.Log(i.ToString() + ". -- quadrant = " + quadrant + " | pos = " + pos + " | SizeX: " + gameQuadrantsCoord.offset_from_Zero_X + "| Y_Middle_HOR = " + gameQuadrantsCoord.Y_Middle_HOR + " | y_UP: " + gameQuadrantsCoord.offset_from_Zero_Y_to_top + " | y_DOWN: -" + gameQuadrantsCoord.offset_from_Zero_Y_to_bottom);



            }
        }


        private void DrawDebugLinesQuadrants()
        {

            Color color_red = new Color(1f, 0, 0, 1f);
            Color color_blue = new Color(0, 0, 1f, 1f);


            // line HORIZONTAL TOP
            Debug.DrawLine(new Vector3(gameQuadrantsCoord.X_TL, gameQuadrantsCoord.Y_TL, 0), new Vector3(gameQuadrantsCoord.X_TR, gameQuadrantsCoord.Y_TR, 0), color_red);

            // line HORIZONTAL BOTTOM
            Debug.DrawLine(new Vector3(gameQuadrantsCoord.X_BL, gameQuadrantsCoord.Y_BL, 0), new Vector3(gameQuadrantsCoord.X_BR, gameQuadrantsCoord.Y_BR, 0), color_red);

            // line VERTICAL LEFT
            Debug.DrawLine(new Vector3(gameQuadrantsCoord.X_TL, gameQuadrantsCoord.Y_TL, 0), new Vector3(gameQuadrantsCoord.X_BL, gameQuadrantsCoord.Y_BL, 0), color_red);

            // line VERTICAL RIGHT
            Debug.DrawLine(new Vector3(gameQuadrantsCoord.X_TR, gameQuadrantsCoord.Y_TR, 0), new Vector3(gameQuadrantsCoord.X_BR, gameQuadrantsCoord.Y_BR, 0), color_red);

            // line HORIZONTAL MIDDLE
            Debug.DrawLine(new Vector3(-gameQuadrantsCoord.offset_from_Zero_X, gameQuadrantsCoord.Y_Middle_HOR, 0), new Vector3(gameQuadrantsCoord.offset_from_Zero_X, gameQuadrantsCoord.Y_Middle_HOR, 0), color_blue);

            // line VERTICAL MIDDLE
            Debug.DrawLine(new Vector3(0, gameQuadrantsCoord.offset_from_Zero_Y_to_top, 0), new Vector3(0, -gameQuadrantsCoord.offset_from_Zero_Y_to_bottom, 0), color_blue);

        }


        private int GetQuadrant(Vector3 objPos)
        {

            int ret = -1;

            //Debug.Log("pos = " + objPos + " | backWidth = " + backWidth + " | backHeight = " + backHeight + " | norm = " + GameObject.Find("Background").GetComponent<SpriteRenderer>().bounds.size.normalized);
            //Debug.Log("pos = " + objPos + " | backWidth = " + backWidth + " | backHeight = " + backHeight + " | obj8 = " + aItens_Avaliable[8].spriteObj.bounds.size.y * percScaleItemSize);

            if (objPos.x <= gameQuadrantsCoord.X_Middle_VERT)
            {
                if (objPos.y <= gameQuadrantsCoord.Y_Middle_HOR)
                {
                    ret = 4;
                }
                else
                {
                    ret = 1;
                }
            }
            else
            {
                if (objPos.y <= gameQuadrantsCoord.Y_Middle_HOR)
                {
                    ret = 3;
                }
                else
                {
                    ret = 2;
                }
            }


            return ret;
        }


        // get a xy position, random, for the item. It can use a specific quadrant (include) or a random other then the specified (exclude)
        private Vector3 GetXYforItem(int quadrant, Vector2 sprite_size, float scale, string opt="EXCLUDE")
        {
            Vector3 retVec;
            int chosenQuadrant = -1;

            if (opt == "EXCLUDE")
            {

                // choose a random quadrant that is not the informed one
                int _randNumber = Random.Range(0, 3);

                if (quadrant == 1)
                {
                    if (_randNumber == 0) { chosenQuadrant = 2; }
                    if (_randNumber == 1) { chosenQuadrant = 3; }
                    if (_randNumber == 2) { chosenQuadrant = 4; }
                }

                if (quadrant == 2)
                {
                    if (_randNumber == 0) { chosenQuadrant = 1; }
                    if (_randNumber == 1) { chosenQuadrant = 3; }
                    if (_randNumber == 2) { chosenQuadrant = 4; }
                }

                if (quadrant == 3)
                {
                    if (_randNumber == 0) { chosenQuadrant = 1; }
                    if (_randNumber == 1) { chosenQuadrant = 2; }
                    if (_randNumber == 2) { chosenQuadrant = 4; }
                }

                if (quadrant == 4)
                {
                    if (_randNumber == 0) { chosenQuadrant = 1; }
                    if (_randNumber == 1) { chosenQuadrant = 2; }
                    if (_randNumber == 2) { chosenQuadrant = 3; }
                }

            }
            else
            {
                chosenQuadrant = quadrant;
            }

            // with the choosen quadrant, get a random position there
            float _rand_X = 0;
            float _rand_Y = 0;

            // gets half of the sprite size to subtract from the avaliable size, so the sprite borders dont pass the limit
            sprite_size = (sprite_size / 2) * scale;

            if (chosenQuadrant == 1) 
            {
                _rand_X = Random.Range(gameQuadrantsCoord.X_Middle_VERT, (gameQuadrantsCoord.X_TL - 0.001f) + sprite_size.x);
                _rand_Y = Random.Range(gameQuadrantsCoord.Y_Middle_HOR, (gameQuadrantsCoord.Y_TL + 0.001f) - sprite_size.y);

                //// test if width explode to right
                //if ((_rand_X + sprite_size.x) > gameQuadrantsCoord.X_Middle_VERT)
                //{
                //    _rand_X = gameQuadrantsCoord.X_Middle_VERT - sprite_size.x;
                //}

                // test if width explode to left
                if ((_rand_X - sprite_size.x) < -gameQuadrantsCoord.offset_from_Zero_X)
                {
                    _rand_X = -gameQuadrantsCoord.offset_from_Zero_X + sprite_size.x;
                }

                //// test if width explode to bottom
                //if ((_rand_Y - sprite_size.y) < gameQuadrantsCoord.Y_Middle_HOR)
                //{
                //    _rand_Y = gameQuadrantsCoord.Y_Middle_HOR + sprite_size.y;
                //}

                // test if width explode to top
                if ((_rand_Y + sprite_size.y) > gameQuadrantsCoord.offset_from_Zero_Y_to_top)
                {
                    _rand_Y = gameQuadrantsCoord.offset_from_Zero_Y_to_top - sprite_size.y;
                }
            }

            if (chosenQuadrant == 2) 
            {
                _rand_X = Random.Range(gameQuadrantsCoord.X_Middle_VERT, (gameQuadrantsCoord.X_TR + 0.001f) - sprite_size.x);
                _rand_Y = Random.Range(gameQuadrantsCoord.Y_Middle_HOR, (gameQuadrantsCoord.Y_TR + 0.001f) - sprite_size.y);

                // test if width explode to right
                if ((_rand_X + sprite_size.x) > gameQuadrantsCoord.offset_from_Zero_X)
                {
                    _rand_X = gameQuadrantsCoord.offset_from_Zero_X - sprite_size.x;
                }

                //// test if width explode to left
                //if ((_rand_X - sprite_size.x) < gameQuadrantsCoord.X_Middle_VERT)
                //{
                //    _rand_X = -gameQuadrantsCoord.X_Middle_VERT + sprite_size.x;
                //}

                //// test if width explode to bottom
                //if ((_rand_Y - sprite_size.y) < gameQuadrantsCoord.Y_Middle_HOR)
                //{
                //    _rand_Y = gameQuadrantsCoord.Y_Middle_HOR + sprite_size.y;
                //}

                // test if width explode to top
                if ((_rand_Y + sprite_size.y) > gameQuadrantsCoord.offset_from_Zero_Y_to_top)
                {
                    _rand_Y = gameQuadrantsCoord.offset_from_Zero_Y_to_top - sprite_size.y;
                }
            }

            if (chosenQuadrant == 3) 
            {
                //_rand_X = Random.Range(gameQuadrantsCoord.X_Middle_VERT, (gameQuadrantsCoord.X_BR + 0.001f) - sprite_size.x);
                //_rand_Y = Random.Range(gameQuadrantsCoord.Y_Middle_HOR, (gameQuadrantsCoord.Y_BR - 0.001f) + sprite_size.y);
                
                _rand_X = Random.Range(gameQuadrantsCoord.X_Middle_VERT, (gameQuadrantsCoord.X_BR + 0.001f));
                _rand_Y = Random.Range(gameQuadrantsCoord.Y_Middle_HOR, (gameQuadrantsCoord.Y_BR - 0.001f));


                // test if width explode to right
                if ((_rand_X + sprite_size.x) > gameQuadrantsCoord.offset_from_Zero_X)
                {
                    _rand_X = gameQuadrantsCoord.offset_from_Zero_X - sprite_size.x;
                }

                //// test if width explode to left
                //if ((_rand_X - sprite_size.x) < gameQuadrantsCoord.X_Middle_VERT)
                //{
                //    _rand_X = gameQuadrantsCoord.X_Middle_VERT + sprite_size.x;
                //}

                // test if width explode to bottom
                if ((_rand_Y - sprite_size.y) < -gameQuadrantsCoord.offset_from_Zero_Y_to_bottom)
                {
                   _rand_Y = -gameQuadrantsCoord.offset_from_Zero_Y_to_bottom + sprite_size.y;
                }

                //// test if width explode to top
                //if ((_rand_Y + sprite_size.y) > gameQuadrantsCoord.Y_Middle_HOR)
                //{
                //    _rand_Y = gameQuadrantsCoord.Y_Middle_HOR - sprite_size.y;
                //}


            }

            if (chosenQuadrant == 4) 
            {
                _rand_X = Random.Range(gameQuadrantsCoord.X_Middle_VERT, (gameQuadrantsCoord.X_BL - 0.001f) + sprite_size.x);
                _rand_Y = Random.Range(gameQuadrantsCoord.Y_Middle_HOR, (gameQuadrantsCoord.Y_BL - 0.001f) + sprite_size.y);


                // test if width explode to right
                if ((_rand_X + sprite_size.x) > gameQuadrantsCoord.X_Middle_VERT)
                {
                    _rand_X = gameQuadrantsCoord.X_Middle_VERT - sprite_size.x;
                }

                //// test if width explode to left
                //if ((_rand_X - sprite_size.x) < -gameQuadrantsCoord.offset_from_Zero_X)
                //{
                //    _rand_X = -gameQuadrantsCoord.offset_from_Zero_X + sprite_size.x;
                //}

                //// test if width explode to bottom
                //if ((_rand_Y - sprite_size.y) < -gameQuadrantsCoord.offset_from_Zero_Y_to_bottom)
                //{
                //    _rand_Y = -gameQuadrantsCoord.offset_from_Zero_Y_to_bottom + sprite_size.y;
                //}

                // test if width explode to top
                if ((_rand_Y + sprite_size.y) > gameQuadrantsCoord.Y_Middle_HOR)
                {
                    _rand_Y = gameQuadrantsCoord.Y_Middle_HOR - sprite_size.y;
                }

            }

            retVec = new Vector3(_rand_X, _rand_Y, 1);

            //Debug.Log("chosenQuadrant = " + chosenQuadrant + " | retVec = " + retVec + " | GetQuadrant(retVec) = " + GetQuadrant(retVec) + " | SizeX: " + gameQuadrantsCoord.offset_from_Zero_X + "| y_UP: " + gameQuadrantsCoord.offset_from_Zero_Y_to_top + " | y_DOWN: -" + gameQuadrantsCoord.offset_from_Zero_Y_to_bottom);

            return retVec;
        }




        public void OnTriggerExit2D_fromPlayerMove(Collider2D collider)
        {
            if (collider.name == "UMBRELLA")
            {
                GameManager.Instance.isOnShade = false;
            }
        }


        public void OnTriggerEnter2D_fromPlayerMove(Collider2D collider)
        {
            Debug.Log("Colisao: " + collider.name);


            if (collider.name == "UMBRELLA")
            {
                GameManager.Instance.isOnShade = true;
            }
            else
            {


                if (collider.name == "SUNBLOCKER")
                {
                    GameManager.Instance.numberSunBlocks++;

                    if (GameManager.Instance.numberSunBlocks >= 1)
                    {
                        sprRendSunBlockCounter.color = new Color(sprRendSunBlockCounter.color.r, sprRendSunBlockCounter.color.g, sprRendSunBlockCounter.color.b, 1);
                        txtLabelSunBlockCounter.color = new Color(txtLabelSunBlockCounter.color.r, txtLabelSunBlockCounter.color.g, txtLabelSunBlockCounter.color.b, 1);
                        txtLabelSunBlockCounter.SetText("x" + GameManager.Instance.numberSunBlocks.ToString());
                    }
                }


                // stop vanish corroutine if collider is vanishing
                SpriteRenderer spr = collider.GetComponent<SpriteRenderer>();

                if (spr.color.a < 1f)
                {
                    if (coroutineVanishItem != null)
                    {
                        StopCoroutine(coroutineVanishItem);
                        coroutineVanishItem = null;
                    }
                }

                // tint item green and alpha 100%
                spr.color = new Color(0, 1f, 0f, 1f);



                // get index of  iten in used itens array
                int i = 0;
                int indexUsedItem = -1;

                foreach (clsItemInGame item in aItens_Used)
                {
                    if (item.name == collider.name)
                    {
                        indexUsedItem = i;
                        break;
                    }

                    i++;
                }

                // account points
                if (indexUsedItem >= 0)
                {
                    GameManager.Instance.score += aItens_Used[indexUsedItem].value;

                    txtScore.SetText(GameManager.Instance.score.ToString());
                }

                // destroy item
                RemoveItemFromGame(indexUsedItem, collider.gameObject);

            }

        }

        private void RemoveItemFromGame(int indexUsedItem, GameObject go)
        {
            if (indexUsedItem >= 0)
            {
                aItens_Used.RemoveAt(indexUsedItem);
            }

            Destroy(go);
        }


        private void SetHeatTime(bool theState)
        {
            isHeatTime = theState;

            tintBackGroundGameObject.SetActive(theState);
            glowSunGameObject.SetActive(theState);
            glowAnimator.SetBool("isHeatTime", theState);


        }
        
        



        private IEnumerator DefineAndRenderSunState()
        {
            int number;


            while (isSunCyclingStates)
            {

                if (Time.time >= timeSinceBegin + 3f)
                {
                    number = Random.Range(1, 101);
                }
                else
                {
                    number = Random.Range(1, percAlmostSun + percNoSun);
                }
                

                if (number <= percNoSun)
                {
                    sunStates = SunState.NO_Sun;
                }
                else
                {
                    if (number <= percAlmostSun + percNoSun)
                    {
                        sunStates = SunState.ALMOST_Sun;
                    }
                    else
                    {
                        sunStates = SunState.YES_Sun;
                    }
                }

                //Debug.Log("sunStates = " + sunStates + " | number = " + number);



                switch (sunStates)
                {
                    case SunState.NO_Sun:
                        sunSpriteRender.sprite = spriteSun_Back;
                        SetHeatTime(false);
                        break;

                    case SunState.ALMOST_Sun:

                        int number2 = Random.Range(1, 3);
                        int number3 = Random.Range(1, 3);

                        if (number2 == 1)
                        {
                            if (number3 == 1)
                            {
                                sunSpriteRender.sprite = spriteSun_Right1;
                            }
                            else
                            {
                                sunSpriteRender.sprite = spriteSun_Right2;
                            }
                        }
                        else
                        {
                            if (number3 == 1)
                            {
                                sunSpriteRender.sprite = spriteSun_Left1;
                            }
                            else
                            {
                                sunSpriteRender.sprite = spriteSun_Left2;
                            }
                        }

                        SetHeatTime(false);

                        break;

                    case SunState.YES_Sun:
                        sunSpriteRender.sprite = spriteSun_Front;

                        SetHeatTime(true);

                        break;

                }

                yield return new WaitForSeconds(timeToDrawNewSunState);

                if (isSunCyclingStates == false)
                {
                    StopCoroutine(coroutineSunState);
                    coroutineSunState = null;
                }
            }
        }

        private IEnumerator wrapper_DrawSelectedItensOnScreen()
        {

            while (GameManager.Instance.gameState == "PLAYING")
            {
                DrawSelectedItensOnScreen();

                yield return new WaitForSeconds(timeToDrawNewItem);
            }

            if (GameManager.Instance.gameState != "PLAYING")
            {
                StopCoroutine(coroutineDrawItens);
                coroutineDrawItens = null;
            }

        }

        private IEnumerator VanishItemFromScreen(int indexUsedItem, GameObject go)
        {
            SpriteRenderer spr = go.GetComponent<SpriteRenderer>();

            while (spr.color.a >= 0.01f)
            {
                spr.color = new Color(spr.color.r, spr.color.g, spr.color.b, (spr.color.a - 0.02f));


                //yield return new WaitForSecondsRealtime(0.0001f);
                yield return new WaitForSecondsRealtime(timeToFadeAndDestroyItem / 100f);
                //yield return new WaitForSecondsRealtime(0.1f);
            }

            //aItens_Avaliable.RemoveAt(0);

            StopCoroutine(coroutineVanishItem);
            coroutineVanishItem = null;

            RemoveItemFromGame(indexUsedItem, go);

        }

        private IEnumerator CountDownTimer(int totalSeconds = 60)
        {
            bool exitWhile = false;
            int countSeconds = totalSeconds;

            string outTime = "";


            if (totalSeconds == 60)
            {
                txtLabelTimerValue.SetText("00:01:00");

            }
            else
            {
                txtLabelTimerValue.SetText("00:00:" + totalSeconds.ToString());
            }



            while (exitWhile == false)
            {
                yield return new WaitForSecondsRealtime(1f);

                countSeconds--;

                if (countSeconds <= 9)
                {
                    outTime = "0" + countSeconds.ToString();
                }
                else
                {
                    outTime = countSeconds.ToString();
                }

                txtLabelTimerValue.SetText("00:00:" + outTime);

                if (countSeconds <= 0)
                {
                    exitWhile = true;
                }

            }

            StopCoroutine(coroutineTimer);
            coroutineTimer = null;

        }




        private IEnumerator KeepPlayerInvensible(float totalTimeInvensible = 3f)
        {
                
            bool exitWhile = false;
            int countBlink = 0;

            GameManager.Instance.isInvencible = true;


            while (exitWhile == false)
            {

                playerRender.color = new Color(playerRender.color.r, playerRender.color.g, playerRender.color.b, 0);

                yield return new WaitForSecondsRealtime(0.07f);

                playerRender.color = new Color(playerRender.color.r, playerRender.color.g, playerRender.color.b, 1);

                yield return new WaitForSecondsRealtime(0.07f);


                if (countBlink >= 10)
                {
                    exitWhile = true;
                }

                countBlink++;

            }

            GameManager.Instance.isInvencible = false;

            StopCoroutine(coroutineTimeInvensible);
            coroutineTimeInvensible = null;

        }


    }

}
