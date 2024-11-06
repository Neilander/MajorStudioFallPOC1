using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndAble : MonoBehaviour
{
    public bool ifEndGame = false;
    public virtual void doEndGame()
    {
        ifEndGame = true;
    }
}
