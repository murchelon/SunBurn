using SubBurn;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Panel_Dead : MonoBehaviour
{

    public void onClick_MainMenu()
    {
        GameManager.Instance.Call_MainMenu();
    }

    public void onClick_PlayAgain()
    {
        GameManager.Instance.Call_StartGame();
    }


}
