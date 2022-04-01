using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRay : MonoBehaviour
{
    float m_rotX; //마우스 움직임을 저장할 변수

    public float m_rotSpeed;    //카메라 회전 속도
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //안드로이드일때
#if UNITY_ANDROID
        if (Input.GetMouseButton(0))
        {
            m_rotX = -Input.GetAxis("Mouse Y") * m_rotSpeed;    //마우스 정보 받기
            this.transform.eulerAngles += new Vector3(m_rotX, 0.0f, 0.0f);
        }
#endif
        if (Input.GetMouseButton(1))
        {
            //우클릭으로 화면회전
            m_rotX = -Input.GetAxis("Mouse Y") * m_rotSpeed;    //마우스 정보 받기
            this.transform.eulerAngles += new Vector3(m_rotX, 0.0f, 0.0f);
        }

    }
}
