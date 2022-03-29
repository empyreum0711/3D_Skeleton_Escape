using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeysCtrl : MonoBehaviour
{

    public bool m_isNomalKey = false;
    public bool m_isSilverKey = false;
    public bool m_isGoldenKey = false;

    //[HideInInspector] public PlayerCtrl m_refHero = null;

    // Start is called before the first frame update
    void Start()
    {
        //GameObject a_Player = GameObject.Find("SKELETON");

        //if (a_Player != null)
        //{
        //    m_refHero = a_Player.GetComponent<PlayerCtrl>();
        //}
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void KeysOnOff()
    {
        
        Destroy(this.gameObject);
    }
}
