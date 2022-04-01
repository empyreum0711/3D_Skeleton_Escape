using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyEvent : MonoBehaviour
{
    EnemyCtrl m_EnemyCtrl;  //적의 스크립트를 할당받기위한 변수
    // Start is called before the first frame update
    void Start()
    {
        //스크립트 할당
        m_EnemyCtrl = transform.parent.GetComponent<EnemyCtrl>();
    }

    //애니메이션 이벤트 호출 함수
    void Event_AttDamage(string Type)
    {
        m_EnemyCtrl.Event_AttDamage(Type);
    }
    
}
