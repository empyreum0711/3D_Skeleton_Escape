using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    Transform m_Camera = null; //카메라 할당

    // Start is called before the first frame update
    void Start()
    {
        m_Camera = Camera.main.transform; //카메라 호출
    }

    // Update is called once per frame
    void Update()
    {
        //스크립트가 달린 오브젝트가 보는 방향 = 카메라가 보는 방향
        this.transform.forward = m_Camera.forward;
    }
}
