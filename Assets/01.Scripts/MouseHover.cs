using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static MouseHover MHinstance = null;//MouseHover 할당

    //마우스 커서의 UI 항목에 대한 Hover 여부
    public bool isUIHover = false;

    void Awake()
    {
        MHinstance = this;
    }

    //UI위에 마우스가 있을 때
    public void OnPointerEnter(PointerEventData eventData)
    {
        isUIHover = true;
    }

    //UI위에 마우스가 없을 때
    public void OnPointerExit(PointerEventData eventData)
    {
        isUIHover = false;
    }
}
