using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using SubBurn;
using UnityEditor;

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



    public class GameLogic : MonoBehaviour
    {

        public float timeToDrawNewItem;

        public float timeToFadeAndDestroyItem;

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

        enum SunState
        {
            YES_Sun,
            NO_Sun,
            ALMOST_Sun
        }



        void OnGUI()
        {
            GUIStyle style = new GUIStyle();

            style.normal.textColor = Color.black;

            GUI.Label(new Rect(0, 0, 100, 100), (1.0f / Time.smoothDeltaTime).ToString(), style);
        }


        private void Init_Params()
        {


            // sun
            timeToDrawNewSunState = 2f;

            // itens
            timeToDrawNewItem = 2f; // 
            timeToFadeAndDestroyItem = 0.1f;  // the less the number is, the fastest the fade will be

            isSunCyclingStates = true;

            sunStates = SunState.NO_Sun;
            isHeatTime = false;

            percNoSun = 30;
            percAlmostSun = 45;
            //percYesSun = 25;

            debug_Allow_Coroutine_Sun = true;
            debug_Allow_Coroutine_Itens = true;
            debug_Allow_Coroutine_VanishItem = true;

            GameManager.Instance.playareaOffsetFromTop = 2.8f;


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

            // position player
            player.transform.position = new Vector3(-2.6f, -3.8f, 1f);
            //player.transform.position = new Vector3(0, 0, 1f);

            player.GetComponent<PlayerMove>().moveSpeed = 2.8f;




            // define the size of the area on where the itens appear
            backWidth = GameObject.Find("Background").GetComponent<SpriteRenderer>().bounds.size.x;
            backHeight = GameObject.Find("Background").GetComponent<SpriteRenderer>().bounds.size.y;

        }


        private void Start()
        {
            //DrawSelectedItensOnScreen();
        }


        private void Update()
        {


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

            int rand_number;

            int player_quadrant = -1;
            int item_quadrant = -1;

            Vector3 newItemPosition;


            // define player and item quadrants
            player_quadrant = GetQuadrant(player.transform.position);

            //if (player_quadrant == 1) { item_quadrant = 3; }
            //if (player_quadrant == 2) { item_quadrant = 4; }
            //if (player_quadrant == 3) { item_quadrant = 1; }
            //if (player_quadrant == 4) { item_quadrant = 2; }

            //Debug.Log("Player Quadrante = " + player_quadrant + " | item_quadrant = " + item_quadrant);

            // define the new item quadrand, oposed to the player
            newItemPosition = GetXYforItem(player_quadrant);

            // get the item quadrant
            item_quadrant = GetQuadrant(newItemPosition);
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


            // get a random item
            rand_number = Random.Range(0, aItens_Avaliable.Count);

            //Debug.Log("aItens_Avaliable.Count = " + aItens_Avaliable.Count + " | number = " + rand_number + " | name = " + aItens_Avaliable[rand_number].name);

            GameObject go = RenderItemInScreen(aItens_Avaliable[rand_number].spriteObj,
                                               newItemPosition,
                                               transform,
                                               aItens_Avaliable[rand_number].name);



            aItens_Used.Add(new clsItemInGame(aItens_Avaliable[rand_number],
                                              item_quadrant,
                                              go));



            aItens_Avaliable.RemoveAt(rand_number);

        }


        // draw 1 specific object on screen
        private GameObject RenderItemInScreen(Sprite theSprite, Vector3 position, Transform parent = null, string name = "NewGameObjRunTime")
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

            go.AddComponent<BoxCollider2D>();
            go.GetComponent<BoxCollider2D>().isTrigger = true;

            return go;
        }


        private int GetQuadrant(Vector3 objPos)
        {

            int ret = -1;

            //Debug.Log("pos = " + objPos + " | backWidth = " + backWidth + " | backHeight = " + backHeight + " | norm = " + GameObject.Find("Background").GetComponent<SpriteRenderer>().bounds.size.normalized);
            //Debug.Log("pos = " + objPos + " | backWidth = " + backWidth + " | backHeight = " + backHeight + " | obj8 = " + aItens_Avaliable[8].spriteObj.bounds.size.y * percScaleItemSize);
     
            if (objPos.x <= 0)
            {
                if (objPos.y <= 0)
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
                if (objPos.y <= 0)
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
        private Vector3 GetXYforItem(int quadrant, string opt="EXCLUDE")
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

            if (chosenQuadrant == 1) 
            {
                _rand_X = Random.Range(-3f, 0.001f);
                _rand_Y = Random.Range(0f, 1.55f);
            }

            if (chosenQuadrant == 2) 
            {
                _rand_X = Random.Range(0f, 3.001f);
                _rand_Y = Random.Range(0f, 1.55f);
            }

            if (chosenQuadrant == 3) 
            {
                _rand_X = Random.Range(0f, 3.001f);
                _rand_Y = Random.Range(-4.2f, 0.001f);
            }

            if (chosenQuadrant == 4) 
            {
                _rand_X = Random.Range(-3f, 0.001f);
                _rand_Y = Random.Range(-4.2f, 0.001f);
            }

            retVec = new Vector3(_rand_X, _rand_Y, 1);

            //Debug.Log("chosenQuadrant = " + chosenQuadrant + " | GetQuadrant = " + GetQuadrant(retVec) + " | retVec = " + retVec);

            return retVec;
        }



        public void OnTriggerEnter2D_fromPlayerMove(Collider2D collider)
        {
            Debug.Log("Colisao: " + collider.name);

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

            //Debug.Break();


            // account points

            // destroy item

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

            RemoveItemFromGame(indexUsedItem, collider.gameObject);
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
                number = Random.Range(1, 101);

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


    }

}
