using SubBurn;
using System.Collections;
using System.Collections.Generic;
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

        private void OnTriggerEnter2D(Collider2D collider)
        {
            GameObject.Find("GameLogic").gameObject.GetComponent<GameLogic>().OnTriggerEnter2D_fromPlayerMove(collider);
        }

        private void Start()
        {
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






    }

        private void Update()
        {
            // input
            movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));


        }

        private void FixedUpdate()
        {
            // physics

            

            //Debug.Log(movement.x + ", " + movement.y);

            if (movement.x == -1)
            {
                if (movement.y == 1)
                {
                    playerSpriteRender.sprite = spriteBack;
                }
                else
                {
                    playerSpriteRender.sprite = spriteLeft;
                }
            }
            else if (movement.x == 1)
            {
                if (movement.y == 1)
                {
                    playerSpriteRender.sprite = spriteBack;
                }
                else
                {
                    playerSpriteRender.sprite = spriteRight;
                }
            }
            else if (movement.y == 1)
            {
                playerSpriteRender.sprite = spriteBack;
            }
            else
            {
                playerSpriteRender.sprite = spriteFront;
            }



            Vector2 newPos = playerRb.position + movement * moveSpeed * Time.fixedDeltaTime;

            newPos.x = Mathf.Clamp(newPos.x, minBound_x, maxBound_x);
            newPos.y = Mathf.Clamp(newPos.y, minBound_y, maxBound_y);

            playerRb.MovePosition(newPos);

            //Vector3 pos = rb.transform.position;

            //pos.x = Mathf.Clamp(pos.x, playArea.x, playArea.x * -1);
            //pos.y = Mathf.Clamp(pos.y, playArea.y, playArea.y * -1);

            //rb.transform.position = pos;



        }

        private void LateUpdate()
        {

        }

    }
}

