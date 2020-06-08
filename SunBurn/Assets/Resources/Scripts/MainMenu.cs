using SubBurn;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{

    public void onClick_Play()
    {
        GameManager.Instance.Call_StartGame();
    }

}
