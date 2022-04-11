using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCtrl : MonoBehaviour
{
    float m_rotX; //마우스 움직임을 저장할 변수

    public float m_rotSpeed;    //카메라 회전 속도

    private GameObject m_Player = null;
    private Vector3 m_TargetPos = Vector3.zero;

    //---- 카메라 위치 계산용 변수
    private float m_PosX = 0.0f;    //마우스 좌우 조작값 계산용 변수 
    private float m_PosY = 0.0f;    //마우스 상하 조작값 계산용 변수 
    private float xSpeed = 5.0f;    //마우스 좌우 회전에 대한 카메라 회전 스피드 설정값
    private float ySpeed = 2.4f;    //마우스 상하 회전에 대한 카메라 회전 스피드 설정값
    private float yMinLimit = -7.0f; //-7.0f;  //위 아래 각도 제한
    private float yMaxLimit = 80.0f; //80.0f;   //위 아래 각도 제한
    //---- 카메라 위치 계산용 변수 

    //---- 주인공을 기준으로 한 상대적인 구좌표계 기준의 초기값
    private float m_DefaltPosX = 90.0f;   //평면 기준의 회전 각도
    private float m_DefaltPosY = 20.0f;   //높이 기준의 회전 각도
    private float m_DefaltDist = 4.4f;    //타겟에서 카메라까지의 거리
    //---- 주인공을 기준으로 한 상대적인 구좌표계 기준의 초기값

    //---- 계산에 필요한 변수들...
    private Quaternion a_BuffRot;
    private Vector3 a_BasicPos = Vector3.zero;
    public float distance = 2.2f;
    private Vector3 a_BuffPos;
    //---- 계산에 필요한 변수들...

    // Start is called before the first frame update
    void Start()
    {
        if (m_Player == null)
            return;

        m_TargetPos = m_Player.transform.position;
        m_TargetPos.y += 6.4f;




        //-------카메라 위치 계산 공식 (구좌표계를 직각좌표계로 환산하는 부분)
        m_PosX = m_DefaltPosX;  //평면 기준의 회전 각도
        m_PosY = m_DefaltPosY;  //높이 기준의 회전 각도

        distance = m_DefaltDist;

        a_BuffRot = Quaternion.Euler(m_PosY, 0, 0);
        a_BasicPos.x = 0.0f;
        a_BasicPos.y = 0.0f;
        a_BasicPos.z = -distance;

        a_BuffPos = a_BuffRot * a_BasicPos + m_TargetPos;

        transform.position = a_BuffPos;  //<--- 카메라의 직각좌표계 기준의 위치

        transform.LookAt(m_TargetPos);
        //-------카메라 위치 계산 공식
    }

    private void LateUpdate()
    {
        if (m_Player == null)
            return;

        if (m_Player != null)
        {
            m_TargetPos = m_Player.transform.position;
            m_TargetPos.y += 6.4f;
        }



        if (Input.GetMouseButton(1))  //마우스 우측버튼을 누르고 있는 동안
        {
            m_PosY -= Input.GetAxis("Mouse Y") * ySpeed; //마우스를 위아래로 움직였을 때 값

            m_PosY = ClampAngle(m_PosY, yMinLimit, yMaxLimit);
        }

        m_PosX = m_Player.transform.localEulerAngles.y;

        a_BuffRot = Quaternion.Euler(m_PosY, m_PosX, 0);
        a_BasicPos.x = 0.0f;
        a_BasicPos.y = 0.0f;
        a_BasicPos.z = -distance;

        a_BuffPos = a_BuffRot * a_BasicPos + m_TargetPos;

        transform.position = a_BuffPos;  //<--- 카메라의 직각좌표계 기준의 위치

        transform.LookAt(m_TargetPos);
    }

    public void InitCamera(GameObject a_Player)
    {
        m_Player = a_Player;

        m_TargetPos = m_Player.transform.position;
        m_TargetPos.y += 6.4f;

        //-------카메라 위치 계산 공식 (구좌표계를 직각좌표계로 환산하는 부분)
        m_PosX = m_DefaltPosX;  //평면 기준의 회전 각도
        m_PosY = m_DefaltPosY;  //높이 기준의 회전 각도
        distance = m_DefaltDist;

        a_BuffRot = Quaternion.Euler(m_PosY, m_PosX, 0);
        a_BasicPos.x = 0.0f;
        a_BasicPos.y = 0.0f;
        a_BasicPos.z = -distance;

        a_BuffPos = a_BuffRot * a_BasicPos + m_TargetPos;

        transform.position = a_BuffPos;  //<--- 카메라의 직각좌표계 기준의 위치

        transform.LookAt(m_TargetPos);
        //-------카메라 위치 계산 공식
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }
}
