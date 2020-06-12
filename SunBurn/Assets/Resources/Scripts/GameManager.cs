using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;


namespace SubBurn
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public string gameState;
        public float playareaOffsetFromTop;
        public int score;
        public bool isOnShade;
        public bool playerIsDead;
        public int numberSunBlocks;
        public bool isInvencible;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }

            
        }


        private void Start()
        {
            Call_MainMenu();
            //Call_StartGame();
        }

        public void Call_StartGame()
        {
            gameState = "PLAYING";

            SceneManager.LoadScene("Resources/Scenes/MainGame");
        }

        public void Call_MainMenu()
        {
            gameState = "MAIN_MENU";

            SceneManager.LoadScene("Resources/Scenes/MainMenu");
        }

        public void Call_StartCoroutine(IEnumerator func)
        {
            StartCoroutine(func);
        }

        public void Call_StopCoroutine(IEnumerator func)
        {
            if (func != null)
            {
                StopCoroutine(func);
            }            
        }


   

    }


}

