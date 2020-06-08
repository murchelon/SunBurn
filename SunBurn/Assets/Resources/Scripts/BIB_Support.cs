using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;



public static class BIB_Support 
{

    public static void Sleep(float waitForSeconds)
    {
        DateTime startTime = System.DateTime.Now;
        bool timerStart = true;

        while (timerStart == true)
        {
            TimeSpan waitTime = System.DateTime.Now - startTime;

            if (waitTime.TotalSeconds >= waitForSeconds)
            {
                timerStart = false;
            }
        }
    }

    // overflow:
    public static void Sleep(int waitForSeconds)
    {
        Sleep((float)waitForSeconds);
    }

}
