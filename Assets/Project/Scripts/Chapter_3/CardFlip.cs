using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardFlip : MonoBehaviour
{
    public GameObject frontSide;
    public GameObject backSide;
    private bool isFront = true;   // 当前是否正面

    void Start()
    {
        frontSide.SetActive(true);
        backSide.SetActive(false);
        isFront = true;
    }


    public void Flip()
    {
        isFront = !isFront;
        frontSide.SetActive(isFront);
        backSide.SetActive(!isFront);
    }
}