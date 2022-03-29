using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCtrl : MonoBehaviour
{
    public static int m_playerMaxHp = 50;
    [Range(0, 50)]
    public float m_playerHp = 50;         //플레이어의 hp
    public Slider m_playerHpbar = null;



    #region//애니메이션용 변수
    public Anim anim;   //AnimSupporter.cs 쪽에 정의 / 인스펙터뷰에 표시할 애니메이션 클래스 변수
    Animator m_RefAnimator = null;
    string m_prevState = "";
    AnimState m_CurState = AnimState.idle;
    AnimatorStateInfo animatorStateInfo;
    //애니메이션용 변수
    #endregion

    #region//공격시 방향 전환용 변수
    GameObject[] m_EnemyList = null;        //적의 위치를 알기위한 변수

    float m_AttackDist = 1.9f;              //공격 사거리
    private GameObject m_TargetUnit = null;
    bool m_AttRotPermit = false;

    Vector3 a_CacTgVec = Vector3.zero;
    Vector3 a_CacAtDir = Vector3.zero;      //방향 전환용 변수
    //공격시 방향 전환용 변수                                  
    #endregion

    #region//데미지 계산용 변수
    float a_fCacLen = 0.0f;
    int iCount = 0;
    //GameObject a_EffObj = null;
    //Vector3 a_EffPos = Vector3.zero;
    //데미지 계산용 변수
    #endregion

    #region//키보드 입력값 변수
    float h = 0, v = 0;
    Vector3 a_MoveNextStep;         //보폭계산을 위한 변수
    Vector3 a_MoveHStep;
    Vector3 a_MoveVStep;
    float m_MoveVelocity = 5.0f;       //평면 초당 이동속도
    float a_CalcRotY = 0.0f;
    float rotSpeed = 150.0f; //초당 150도 회전하라는 속도
    float m_jumpPower = 4.5f;
    //키보드 입력값 변수
    #endregion

    #region//Picking 관련 변수 
    private bool m_isPickMvOnOff = false;       //피킹 이동 OnOff
    private Vector3 m_TargetPos = Vector3.zero; //최종 목표 위치
    private Vector3 m_MoveDir = Vector3.zero;   //평면 진행 방향
    private double m_MoveDurTime = 0.0;         //목표점까지 도착하는데 걸리는 시간
    private double m_AddTimeCount = 0.0;        //누적시간 카운트 
    protected float m_RotSpeed = 3.0f;          //초당 회전 속도
    Vector3 a_StartPos = Vector3.zero;
    Vector3 a_CacLenVec = Vector3.zero;
    Quaternion a_TargetRot;
    //Picking 관련 변수 
    #endregion

    #region//joyStick 이동 변수
    private float m_JoyMvLen = 0.0f;
    private Vector3 m_JoyMvDir = Vector3.zero;
    //joyStick 이동 변수
    #endregion

    #region//플레이어의 등장 연출 변수
    bool m_isBirth = false;
    float m_dissolve;
    Renderer[] renderers;
    Material[] mats;
    #endregion

    #region//오브젝트와의 상호 작용 관련 변수
    GameManager m_gameMgr;
    DoorCtrl m_doorCtrl;
    [SerializeField] Transform m_rayPos;
    LayerMask m_doorlayer;
    LayerMask m_Keyslayer;
    LayerMask m_Potionlayer;
    RaycastHit hit;
    public GameObject m_userText;
    public GameObject[] m_Keys;
    #endregion



    // Start is called before the first frame update
    void Start()
    {
        m_RefAnimator = this.gameObject.GetComponent<Animator>();
        m_doorlayer = LayerMask.GetMask("Door");
        m_Keyslayer = LayerMask.GetMask("Keys");
        m_Potionlayer = LayerMask.GetMask("Potion_Health");
        GameObject[] door = GameObject.FindGameObjectsWithTag("Door");

        for (int i = 0; i < door.Length; i++)
            if (door[i] != null)
            {
                m_doorCtrl = door[i].GetComponent<DoorCtrl>();
            }

        GameObject gm = GameObject.Find("GameManager");
        if (gm != null)
        {
            m_gameMgr = gm.GetComponent<GameManager>();
        }


        ResponStart();//플레이어를 서서히 리스폰 시키는 함수
    }

    // Update is called once per frame
    void Update()
    {
        if (m_isBirth != true || m_gameMgr.m_isGameClear == true)
            return;

        Vector3 a_rayEndPos = m_rayPos.position + transform.forward * 3.0f;
        Debug.DrawLine(m_rayPos.position, a_rayEndPos, Color.white);

        if (Physics.Linecast(m_rayPos.position, a_rayEndPos, out hit, m_doorlayer))         //Door 레이어에 레이가 맞았다면
        {
            m_userText.SetActive(true);
            ChangeText();
        }
        else if (Physics.Linecast(m_rayPos.position, a_rayEndPos, out hit, m_Keyslayer))    //Key 레이어에 레이가 맞았다면
        {          
            ChangeText();
            m_userText.SetActive(true);
        }
        else if (Physics.Linecast(m_rayPos.position, a_rayEndPos, out hit, m_Potionlayer))
        {          
            ChangeText();
            m_userText.SetActive(true);
        }
        else
        {
            m_userText.SetActive(false);
        }

        if (m_playerHp <= 0)        //플레이어의 체력이 0보다 작다면
        {
            MySetAnim(AnimState.die);

            m_gameMgr.m_isGameOver = true;

            return;
        }

        m_playerHpbar.value = (float)m_playerHp / (float)m_playerMaxHp;

        KeyBoardMove();     //키보드 조작 함수
        JoyStickMoveUpdate();   //조이스틱 조작 함수
        NaturalRecovery();
    }

    void ResponStart()//게임 시작후 플레이어를 리스폰 시키는 함수
    {
        //시작할때 플레이어를 서서히 리스폰시키는 연출
        renderers = GetComponentsInChildren<Renderer>();

        mats = new Material[renderers.Length];

        for (int i = 0; i < mats.Length; i++)
        {
            mats[i] = renderers[i].material;
        }

        m_dissolve = 1f;

        StartCoroutine(Birth());
        //시작할때 플레이어를 리스폰시키는 연출
    }

    IEnumerator Birth()//플레이어의 셰이더값을 조절해 서서히 나타나게 하는 함수
    {
        while (m_dissolve > 0f)
        {
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i].SetFloat("_DissolveAmount", m_dissolve);
            }

            yield return new WaitForSeconds(0.01f);

            m_dissolve -= 0.01f;
        }
        for (int i = 0; i < mats.Length; i++)
        {
            mats[i].SetFloat("_DissolveAmount", 0);
            m_isBirth = true;
        }
    }

    void KeyBoardMove()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            DoorOnOff();
            TakeKeys();
            Potion();
        }

        //가감속 없이 이동하는 법
        h = Input.GetAxis("Horizontal"); //화살표 좌우키를 누르면 -1.0f, 0.0f,1.0f 사이값을 리턴해준다
        v = Input.GetAxis("Vertical");   //화살표 위아래키를 누르면 -1.0f, 0.0f,1.0f 사이값을 리턴해준다
        //가감속 없이 이동하는 법

        if (v < 0.0f)
            v = 0.0f;

        if (h != 0.0f || v != 0.0f)
        {
            if (ISAttack() == true)
                return;


            //기본적인 이동
            a_CalcRotY = transform.eulerAngles.y;
            a_CalcRotY += (h * rotSpeed * Time.deltaTime);
            transform.eulerAngles = new Vector3(0.0f, a_CalcRotY, 0.0f);

            a_MoveVStep = transform.forward * v;
            a_MoveNextStep = a_MoveVStep;
            a_MoveNextStep = a_MoveNextStep.normalized * m_MoveVelocity * Time.deltaTime;

            transform.position = transform.position + a_MoveNextStep;
            //기본적인 이동

            MySetAnim(AnimState.move);
        }
        else
        {
            //키보드 이동중이 아닐 때만 Idle 동작으로 돌아가게 한다.
            if (m_isPickMvOnOff == false && m_JoyMvLen <= 0.0f && ISAttack() == false)
            {
                MySetAnim(AnimState.idle);
            }

        }

    }

    //조이스틱 관련 변수
    Vector3 a_CacCamVec;    //카메라의 위치
    Vector3 a_RightDir = Vector3.zero;
    public void SetJoyStickMove(float a_JoyMoveLen, Vector3 a_JoyMoveDir)
    {
        m_JoyMvLen = a_JoyMoveLen;

        if (0.0f < a_JoyMoveLen)
        {
            //--------카메라가 바라보고 있는 전면을 기준으로 회전 시켜줘야 한다. 
            a_CacCamVec = Camera.main.transform.forward;
            a_CacCamVec.y = 0.0f;
            a_CacCamVec.Normalize();
            m_JoyMvDir = a_CacCamVec * a_JoyMoveDir.y; //위 아래 조작(카메라가 바라보고 있는 기준으로 위, 아래로 얼만큼 이동시킬 것인지?)
            a_RightDir = Vector3.Cross(Vector3.up, a_CacCamVec).normalized;
            m_JoyMvDir = m_JoyMvDir + (a_RightDir * a_JoyMoveDir.x); //좌우 조작(카메라가 바라보고 있는 기준으로 좌, 우로 얼만큼 이동시킬 것인지?)
            m_JoyMvDir.y = 0.0f;
            m_JoyMvDir.Normalize();
            //--------카메라가 바라보고 있는 전면을 기준으로 회전 시켜줘야 한다. 
        }

        if (a_JoyMoveLen == 0.0f)
        {
            //공격 애니메이션 중이면 공격 애니메이션이 끝나고 숨쉬기 애니로 돌아가게 한다.
            if (m_isPickMvOnOff == false && ISAttack() == false)
                MySetAnim(AnimState.idle);
        }
    }

    void JoyStickMoveUpdate()
    {
        if (h != 0.0f || 0.0f != v)
            return;

        //조이스틱 코드
        if (0.0f < m_JoyMvLen)   //조이스틱으로 움직일 때
        {
            if (ISAttack() == true)
                return;

            m_MoveDir = m_JoyMvDir;
            float amtToMove = m_MoveVelocity * Time.deltaTime;

            //캐릭터 스프링 회전
            if (0.0001f < m_JoyMvDir.magnitude)
            {
                Quaternion a_TargetRot = Quaternion.LookRotation(m_JoyMvDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, a_TargetRot,
                    (Time.deltaTime * m_RotSpeed) / 3);
            }
            //캐릭터 스프링 회전

            a_MoveNextStep = m_JoyMvDir * (m_MoveVelocity * Time.deltaTime);
            a_MoveNextStep.y = 0.0f;

            transform.position = transform.position + a_MoveNextStep;

            MySetAnim(AnimState.move);

        }

    }

    public bool ISAttack()
    {
        if (m_prevState != null && !string.IsNullOrEmpty(m_prevState))
        {
            if (m_prevState.ToString() == AnimState.attack.ToString())
                return true;
        }

        return false;
    }

    public void MySetAnim(AnimState newAnim, float CrossTime = 1.0f, string AnimName = "") //PlayMode mode = PlayMode.StopSameLayer)
    {
        if (m_RefAnimator == null)
            return;

        if (m_prevState != null && !string.IsNullOrEmpty(m_prevState))
        {
            if (m_prevState.ToString() == newAnim.ToString())
                return;
        }

        if (!string.IsNullOrEmpty(m_prevState))
        {
            m_RefAnimator.ResetTrigger(m_prevState.ToString());
            m_prevState = null;
        }

        m_AttRotPermit = false; //모든 애니메이션이 시작할 때 먼저 꺼준다

        if (0.0f < CrossTime)
        {
            m_RefAnimator.SetTrigger(newAnim.ToString());
        }
        else
        {
            m_RefAnimator.Play(AnimName, -1, 0.0f);   //가운데는 Layer Index, 0.0f는 처음부터 다시시작
        }

        m_prevState = newAnim.ToString();   //이전 스테이트에 현재 스테이트 저장
        m_CurState = newAnim;
    }//MySetAnim

    public void AttackOrder()
    {
        if (m_prevState == AnimState.idle.ToString()
            || m_prevState == AnimState.move.ToString())
        {
            if ((h != 0.0f || v != 0.0f) || m_JoyMvLen > 0.0f)
                return;

            MySetAnim(AnimState.attack);
        }
    }

    public void DoorOnOff()
    {
        Vector3 a_rayEndPos = m_rayPos.position + transform.forward * 3.0f;
        if (Physics.Linecast(m_rayPos.position, a_rayEndPos, out hit, m_doorlayer))
        {
            if (hit.collider.CompareTag("Door"))
            {
                hit.collider.transform.GetComponent<DoorCtrl>().DoorOpenMin90();
            }
        }
    }

    public void TakeKeys()
    {
        Vector3 a_rayEndPos = m_rayPos.position + transform.forward * 3.0f;
        if (Physics.Linecast(m_rayPos.position, a_rayEndPos, out hit, m_Keyslayer))
        {
            if (hit.collider.CompareTag("Keys"))
            {
                hit.collider.transform.GetComponent<KeysCtrl>().KeysOnOff();
            }
        }
    }

    public void Potion()
    {
        Vector3 a_rayEndPos = m_rayPos.position + transform.forward * 3.0f;
        if (Physics.Linecast(m_rayPos.position, a_rayEndPos, out hit, m_Potionlayer))
        {

            if (hit.collider.CompareTag("Potion_Health"))
            {
                if (m_Keys[2] == null)
                {
                    hit.collider.transform.GetComponent<PotionCtrl>().TakePotion();
                }

            }
        }
    }

    public void IsAttackFinish()
    {
        MySetAnim(AnimState.idle);
    }

    public void Event_AttDamage(string Type)
    {
        m_EnemyList = GameObject.FindGameObjectsWithTag("Enemy");
        iCount = m_EnemyList.Length;
        SoundManager.Instance.PlayGUISound("SWORD_09");
        if (Type == AnimState.attack.ToString())
        {
            //주변 모든 몬스터를 찾아서 데미지를 준다(범위공격)
            for (int i = 0; i < iCount; ++i)
            {
                a_CacTgVec = m_EnemyList[i].transform.position - transform.position;
                a_fCacLen = a_CacTgVec.magnitude;
                a_CacTgVec.y = 0.0f;
                //공격각도 안에 있는 경우
                if (Vector3.Dot(transform.forward, a_CacTgVec.normalized) < 0.15f)
                    continue;

                if (m_AttackDist + 0.1f < a_fCacLen)
                    continue;

                m_EnemyList[i].GetComponent<EnemyCtrl>().TakeDamage(this.gameObject, 10.0f);
            }//for(int i=0; i<iCount; ++i)
        }//if(Type == AnimState.attack.ToString())       
    }

    public void TakeDamage(float a_Damage = 10.0f)
    {
        if (m_playerHp <= 0.0f)
            return;

        m_playerHp -= a_Damage;

        m_playerHpbar.value = (float)m_playerHp / (float)m_playerMaxHp;

        if (m_playerHp <= 0.0f)
            m_playerHp = 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("GameClear!!!");

        m_gameMgr.m_isGameClear = true;
        m_gameMgr.m_isClearbgm = true;
    }

    void ObjectText()
    {
        if (hit.collider.transform.GetComponent<DoorCtrl>().IsOpen == true)
        {
            m_userText.GetComponentInChildren<Text>().text = "F를 누르면 문이 닫힙니다.";
        }
        else if (hit.collider.transform.GetComponent<DoorCtrl>().IsOpen == false)
        {
            m_userText.GetComponentInChildren<Text>().text = "F를 누르면 문이 열립니다.";
        }

#if UNITY_ANDROID     //모바일 일때
        if (hit.collider.transform.GetComponent<DoorCtrl>().IsOpen == true)
        {
            m_userText.GetComponentInChildren<Text>().text = "상호작용 버튼을 누르면 문이 닫힙니다.";
        }
        else if (hit.collider.transform.GetComponent<DoorCtrl>().IsOpen == false)
        {
            m_userText.GetComponentInChildren<Text>().text = "상호작용 버튼을 누르면 문이 열립니다.";
        }      
#endif


    }

    //문이나 열쇠등 오브젝트에 닿았을 때 출력되는 글씨를 바꿔주는 함수
    public void ChangeText()
    {
        if (hit.collider.CompareTag("Door"))
        {
            //모든 열쇠가 필요함
            if (hit.collider.transform.GetComponent<DoorCtrl>().m_needNormalKey && m_Keys[0] == null
                && hit.collider.transform.GetComponent<DoorCtrl>().m_needSilverKey && m_Keys[1] == null
                && hit.collider.transform.GetComponent<DoorCtrl>().m_needGoldenKey && m_Keys[2] == null)
            {
                ObjectText();
            }
            //모든 열쇠가 있을때
            else if (hit.collider.transform.GetComponent<DoorCtrl>().m_needNormalKey
                && hit.collider.transform.GetComponent<DoorCtrl>().m_needSilverKey
                && hit.collider.transform.GetComponent<DoorCtrl>().m_needGoldenKey)
            {
                m_userText.GetComponentInChildren<Text>().text = "모든 열쇠가 필요합니다.";
            }
            //일반 열쇠만 필요할때
            else if (hit.collider.transform.GetComponent<DoorCtrl>().m_needNormalKey && m_Keys[0] != null
                && !hit.collider.transform.GetComponent<DoorCtrl>().m_needSilverKey
                && !hit.collider.transform.GetComponent<DoorCtrl>().m_needGoldenKey)
            {
                m_userText.GetComponentInChildren<Text>().text = "일반 열쇠가 필요합니다.";
            }
            //일반열쇠가 있을 때
            else if (hit.collider.transform.GetComponent<DoorCtrl>().m_needNormalKey && m_Keys[0] == null
                    && !hit.collider.transform.GetComponent<DoorCtrl>().m_needSilverKey && m_Keys[1] != null
                    && !hit.collider.transform.GetComponent<DoorCtrl>().m_needGoldenKey && m_Keys[2] != null)
            {
                ObjectText();
            }
            //실버 열쇠만 필요할 때
            else if (hit.collider.transform.GetComponent<DoorCtrl>().m_needSilverKey && m_Keys[1] != null
                && !hit.collider.transform.GetComponent<DoorCtrl>().m_needNormalKey
                && !hit.collider.transform.GetComponent<DoorCtrl>().m_needGoldenKey)
            {
                m_userText.GetComponentInChildren<Text>().text = "은 열쇠가 필요합니다.";
            }
            //실버 열쇠가 있을 때
            else if (hit.collider.transform.GetComponent<DoorCtrl>().m_needSilverKey && m_Keys[1] == null
                && !hit.collider.transform.GetComponent<DoorCtrl>().m_needNormalKey && m_Keys[2] != null
                && !hit.collider.transform.GetComponent<DoorCtrl>().m_needGoldenKey && m_Keys[0] != null)
            {
                ObjectText();
            }
            //골드 열쇠가 있을 때
            else if (hit.collider.transform.GetComponent<DoorCtrl>().m_needGoldenKey && m_Keys[2] != null
                && !hit.collider.transform.GetComponent<DoorCtrl>().m_needNormalKey
                && !hit.collider.transform.GetComponent<DoorCtrl>().m_needSilverKey)
            {
                m_userText.GetComponentInChildren<Text>().text = "금 열쇠가 필요합니다.";
            }
            //골드 열쇠가 있을 때
            else if (hit.collider.transform.GetComponent<DoorCtrl>().m_needGoldenKey && m_Keys[2] == null
                && !hit.collider.transform.GetComponent<DoorCtrl>().m_needNormalKey && m_Keys[0] != null
                && !hit.collider.transform.GetComponent<DoorCtrl>().m_needSilverKey && m_Keys[1] != null)
            {
                ObjectText();

            }
            //열쇠가 필요하지 않을 때
            else
            {
                ObjectText();
            }

        }

        if (hit.collider.CompareTag("Keys"))
        {
            if (hit.collider.transform.GetComponent<KeysCtrl>().m_isNomalKey)
            {
                m_userText.GetComponentInChildren<Text>().text = "일반 열쇠를 획득합니다.";
            }
            else if (hit.collider.transform.GetComponent<KeysCtrl>().m_isSilverKey)
            {
                m_userText.GetComponentInChildren<Text>().text = "은 열쇠를 획득합니다.";
            }
            else if (hit.collider.transform.GetComponent<KeysCtrl>().m_isGoldenKey)
            {
                m_userText.GetComponentInChildren<Text>().text = "금 열쇠를 획득합니다.";
            }
        }

        if (hit.collider.CompareTag("Potion_Health"))
        {
            m_userText.GetComponentInChildren<Text>().text = "포션을 섭취합니다";
        }
    }

    //체력을 서서히 회복시키고 회복이 끝난후 체력에 소수점이 있다면 반올림하여 정수로 바꿔주는 함수
    void NaturalRecovery()
    {
        m_gameMgr.PlayerRecovery();
        if (!m_gameMgr.m_isRecovery)
        {
            m_playerHp = Mathf.Round(m_playerHp);
        }
    }


}
