using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeysCtrl : MonoBehaviour
{

    public bool m_isNomalKey = false;       //일반 열쇠를 획득하면 true
    public bool m_isSilverKey = false;      //실버 열쇠를 획득하면 true
    public bool m_isGoldenKey = false;      //골드 열쇠를 획득하면 true

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    //키오브젝트를 삭제
    public void KeysOnOff()
    {       
        Destroy(this.gameObject);
    }
}
