using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyEvent : MonoBehaviour
{
    EnemyCtrl m_EnemyCtrl;
    // Start is called before the first frame update
    void Start()
    {
        m_EnemyCtrl = transform.parent.GetComponent<EnemyCtrl>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void Event_AttDamage(string Type)
    {
        m_EnemyCtrl.Event_AttDamage(Type);
    }
    
}
