using SubBurn;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;



namespace SunBurn
{
    public class PlayerMove : MonoBehaviour
    {

        public float moveSpeed = 0;

        


        private Vector2 movement;

        private Sprite spriteLeft;
        private Sprite spriteRight;
        private Sprite spriteBack;
        private Sprite spriteFront;

        private Sprite spriteLeft_shade;
        private Sprite spriteRight_shade;
        private Sprite spriteBack_shade;
        private Sprite spriteFront_shade;



        private SpriteRenderer playerSpriteRender;
        private Rigidbody2D playerRb;

        private float playerWidthOffset;
        private float playerHeightOffset;

        private float backWidthOffset;
        private float backHeightOffset;

        private float minBound_x;
        private float maxBound_x;
        private float minBound_y;
        private float maxBound_y;

        public GameLogic gLogic;

        private void OnTriggerEnter2D(Collider2D collider)
        {
            gLogic.OnTriggerEnter2D_fromPlayerMove(collider);
        }

        public void OnTriggerExit2D(Collider2D collider)
        {
            gLogic.OnTriggerExit2D_fromPlayerMove(collider);
        }


        private void Start()
        {

            gLogic = GameObject.Find("GameLogic").gameObject.GetComponent<GameLogic>();

            if (moveSpeed == 0)
            {
                moveSpeed = 100f;   // huge velocity ... just to be obvious that this var was not defined elsewhere in the code
            }
            

            //playArea = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));

            playerSpriteRender = this.GetComponent<SpriteRenderer>();

            playerRb = this.GetComponent<Rigidbody2D>();
            //playerRb.transform.position = new Vector3(-2.6f, -3.8f, 1f);

            playerWidthOffset = transform.GetComponent<SpriteRenderer>().bounds.size.x / 2 + 0.1f;
            playerHeightOffset = transform.GetComponent<SpriteRenderer>().bounds.size.y / 2 + 0.16f;

            backWidthOffset = GameObject.Find("Background").GetComponent<SpriteRenderer>().bounds.size.x / 2;
            backHeightOffset = GameObject.Find("Background").GetComponent<SpriteRenderer>().bounds.size.y / 2;

            minBound_x = 0f - backWidthOffset + playerWidthOffset;
            maxBound_x = 0f + backWidthOffset - playerWidthOffset;

            minBound_y = 0f - backHeightOffset + playerHeightOffset;
            maxBound_y = 0f + backHeightOffset - playerHeightOffset - GameManager.Instance.playareaOffsetFromTop;


            spriteLeft = Resources.Load("images/Player_ESQ", typeof(Sprite)) as Sprite;
            spriteRight = Resources.Load("images/Player_DIR", typeof(Sprite)) as Sprite;
            spriteBack = Resources.Load("images/Player_TRAS", typeof(Sprite)) as Sprite;
            spriteFront = Resources.Load("images/Player_FRENTE", typeof(Sprite)) as Sprite;

            spriteLeft_shade = Resources.Load("images/Player_ESQ_Sombra", typeof(Sprite)) as Sprite;
            spriteRight_shade = Resources.Load("images/Player_DIR_Sombra", typeof(Sprite)) as Sprite;
            spriteBack_shade = Resources.Load("images/Player_TRAS_Sombra", typeof(Sprite)) as Sprite;
            spriteFront_shade = Resources.Load("images/Player_FRENTE_Sombra", typeof(Sprite)) as Sprite;






        }

        private void Update()
        {
            // input
            movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));


        }

        private void FixedUpdate()
        {
            // physics


            Sprite spriteFront_ToUse;
            Sprite spriteBack_ToUse;
            Sprite spriteLeft_ToUse;
            Sprite spriteRight_ToUse;

            if (GameManager.Instance.playerIsDead == true)
            {
                return;
            }


            if (GameManager.Instance.isOnShade == true)
            {
                spriteFront_ToUse = spriteFront_shade;
                spriteBack_ToUse = spriteBack_shade;
                spriteLeft_ToUse = spriteLeft_shade;
                spriteRight_ToUse = spriteRight_shade;
            }
            else
            {
                spriteFront_ToUse = spriteFront;
                spriteBack_ToUse = spriteBack;
                spriteLeft_ToUse = spriteLeft;
                spriteRight_ToUse = spriteRight;
            }

            //Debug.Log(movement.x + ", " + movement.y);

            if (movement.x == -1)
            {
                if (movement.y == 1)
                {
                    playerSpriteRender.sprite = spriteBack_ToUse;
                }
                else
                {
                    playerSpriteRender.sprite = spriteLeft_ToUse;
                }
            }
            else if (movement.x == 1)
            {
                if (movement.y == 1)
                {
                    playerSpriteRender.sprite = spriteBack_ToUse;
                }
                else
                {
                    playerSpriteRender.sprite = spriteRight_ToUse;
                }
            }
            else if (movement.y == 1)
            {
                playerSpriteRender.sprite = spriteBack_ToUse;
            }
            else
            {
                playerSpriteRender.sprite = spriteFront_ToUse;
            }



            Vector2 newPos = playerRb.position + movement * moveSpeed * Time.fixedDeltaTime;

            newPos.x = Mathf.Clamp(newPos.x, minBound_x, maxBound_x);
            newPos.y = Mathf.Clamp(newPos.y, minBound_y, maxBound_y);

            playerRb.MovePosition(newPos);

            //Vector3 pos = rb.transform.position;

            //pos.x = Mathf.Clamp(pos.x, playArea.x, playArea.x * -1);
            //pos.y = Mathf.Clamp(pos.y, playArea.y, playArea.y * -1);

            //rb.transform.position = pos;

            //Debug.Log(newPos);

        }



    }
}

