using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionCtrl : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
 
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //포션을 챙김
    public void TakePotion()
    {
        PlayerCtrl.m_playerMaxHp = 100 + PlayerCtrl.m_playerMaxHp; 
        Destroy(this.gameObject);
    }
}
