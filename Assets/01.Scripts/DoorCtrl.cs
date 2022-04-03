using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DoorCtrl : MonoBehaviour
{
    [HideInInspector] public bool IsOpen;                   //문이 열렸는지
    [HideInInspector] public bool m_needGoldenKey = false;  //골드열쇠로 열 수 있다
    [HideInInspector] public bool m_needSilverKey = false;  //실버열쇠로 열 수 있다
    [HideInInspector] public bool m_needNormalKey = false;  //일반열쇠로 열 수 있다

    [HideInInspector] public PlayerCtrl m_refHero = null;   //플레이어스크립트를 사용하기 위한 변수

    // Start is called before the first frame update
    void Start()
    {
        GameObject a_Player = GameObject.Find("SKELETON");

        if (a_Player != null)
        {
            m_refHero = a_Player.GetComponent<PlayerCtrl>();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    //문을 열 때 열쇠가 필요한지? 필요하다면 어떤 열쇠가 필요한지 체크하는 함수
    public void CheckKeys()
    {
        if (m_needNormalKey && m_needSilverKey && m_needGoldenKey)    //모든 열쇠가 필요할때
        {
            if (m_refHero.m_Keys[2] == null && m_refHero.m_Keys[1] == null && m_refHero.m_Keys[0] == null)
            {
                DoorOpenClose();
            }         
        }
        else if (m_needSilverKey)   //실버 열쇠가 필요할때
        {
            if (m_refHero.m_Keys[1] == null)
            {
                DoorOpenClose();
            }
        }
        else if (m_needGoldenKey)   //골드 열쇠가 필요할때
        {
            if (m_refHero.m_Keys[2] == null)
            {
                DoorOpenClose();
            }
        }
        else if (m_needNormalKey) //일반 열쇠가 필요할때
        {
            if (m_refHero.m_Keys[0] == null)
            {
                DoorOpenClose();
            }
        }
        else    //열쇠가 필요 없을때
        {
            DoorOpenClose();
        }
    }

    //문을 열고 닫는 함수
    void DoorOpenClose()
    {
        if (IsOpen == true)//문을 닫는다
        {
            SoundManager.Instance.PlayGUISound("MP_Door_Close");
            transform.eulerAngles += new Vector3(0.0f, 0.0f, 90.0f);
            IsOpen = false;
        }
        else//문을 연다
        {
            SoundManager.Instance.PlayGUISound("MP_Door_Open");
            transform.eulerAngles += new Vector3(0.0f, 0.0f, -90.0f);
            IsOpen = true;
        }
    }
}
