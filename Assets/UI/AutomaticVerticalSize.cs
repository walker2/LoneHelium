using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomaticVerticalSize : MonoBehaviour
{
    public float ChildHeight = 35f;

    void Start()
    {
        AdjustSize();
    }

    public void AdjustSize()
    {
        Vector2 size = this.GetComponent<RectTransform>().sizeDelta;
        size.y = this.transform.childCount * ChildHeight;
        this.GetComponent<RectTransform>().sizeDelta = size;
    }
}