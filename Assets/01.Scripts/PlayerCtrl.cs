using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class PlayerCtrl : MonoBehaviour
{
    public static int m_playerMaxHp = 50;  //플레이어의 최대 hp
    public float m_playerHp = 50;         //플레이어의 현재 hp
    public Slider m_playerHpbar = null;     //플레이어의 체력바

    #region//애니메이션용 변수
    public Anim anim;   //AnimSupporter.cs 쪽에 정의 / 인스펙터뷰에 표시할 애니메이션 클래스 변수
    Animator m_RefAnimator = null;          //Animator 컴포넌트를 받을 변수
    string m_prevState = "";                //현재 state의 이름
    AnimState m_CurState = AnimState.idle;  //현재 state의 상태
    //애니메이션용 변수
    #endregion

    #region//공격시 방향 전환용 변수
    [SerializeField]
    GameObject[] m_EnemyList = null;        //적의 위치를 알기위한 변수
    float m_AttackDist = 7.6f;              //공격 사거리
    Vector3 a_CacTgVec = Vector3.zero;      //타겟과의 거리 계산용 변수
                                            //공격시 방향 전환용 변수                                  
    #endregion

    #region//데미지 계산용 변수
    float a_fCacLen = 0.0f;         //적과의 거리 계산용 변수
    int iCount = 0;                 //적이 얼마나 있는지
    //데미지 계산용 변수
    #endregion

    #region//키보드 입력값 변수
    float h = 0, v = 0;             //각각Horizontal입력과 Vertical입력을 받는 변수
    Vector3 m_MoveNextStep;         //보폭계산을 위한 변수
    Vector3 a_MoveVStep;            //Vertical움직임 계산 변수
    float m_MoveVelocity = 20.0f;       //평면 초당 이동속도
    float a_CalcRotY = 0.0f;           //y축 회전 계산용 변수
    float rotSpeed = 150.0f; //초당 150도 회전하라는 속도
    protected float m_RotSpeed = 2.0f;          //초당 회전 속도
    //키보드 입력값 변수
    #endregion

    #region//joyStick 이동 변수
    private float m_JoyMvLen = 0.0f;                //조이스틱 이동용 변수
    private Vector3 m_JoyMvDir = Vector3.zero;      //조이스틱 방향체크변수
    //joyStick 이동 변수
    #endregion

    #region//플레이어의 등장 연출 변수
    bool m_isBirth = false;             //리스폰시 한번만 셰이더가 동작하기 위한 변수
    float m_dissolve;                   //dissolve값을 조절하기 위한 변수
    Renderer[] renderers;               //Renderer 컴포넌트를 가져오기위한 변수
    Material[] mats;                    //플레이어의 material을 가져오기 위한 변수
    #endregion

    #region//오브젝트와의 상호 작용 관련 변수
    Vector3 m_rayEndPos;                    //ray의 끝지점
    GameManager m_gameMgr;                  //GameManager의 변수를 가져오기 위한 변수
    DoorCtrl m_doorCtrl;                    //DoorCtrl의 변수를 가져오기 위한 변수
    [SerializeField] Transform m_rayPos;    //ray의 시작점
    LayerMask m_doorlayer;                  //door의 레이어
    LayerMask m_Keyslayer;                  //Key의 레이어
    LayerMask m_Potionlayer;                //Potion의 레이어
    RaycastHit hit;                         //ray에 닿았는지의 정보
    public GameObject m_userText;           //유저에게 보여지는 텍스트 변수
    public GameObject[] m_Keys;             //유저가 획득한 열쇠를 보기위한 변수
    #endregion

    protected NavMeshAgent nvAgent;         //네브메쉬에이전트 할당용 변수
    private Vector3 m_MoveDir = Vector3.zero;   //평면 진행 방향

    private void Awake()
    {
        Camera.main.GetComponent<CameraCtrl>().InitCamera(this.gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        m_RefAnimator = this.gameObject.GetComponent<Animator>();
        m_doorlayer = LayerMask.GetMask("Door");
        m_Keyslayer = LayerMask.GetMask("Keys");
        m_Potionlayer = LayerMask.GetMask("Potion_Health");
        GameObject[] door = GameObject.FindGameObjectsWithTag("Door");

        nvAgent = this.gameObject.GetComponent<NavMeshAgent>();

        for (int i = 0; i < door.Length; i++)
        {
            if (door[i] != null)
            {
                m_doorCtrl = door[i].GetComponent<DoorCtrl>();
            }
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

        m_rayEndPos = m_rayPos.position + transform.forward * 9.0f;
        Debug.DrawLine(m_rayPos.position, m_rayEndPos, Color.white);

        if (Physics.Linecast(m_rayPos.position, m_rayEndPos, out hit, m_doorlayer))         //Door 레이어에 레이가 맞았다면
        {
            m_userText.SetActive(true);
            ChangeText();
        }
        else if (Physics.Linecast(m_rayPos.position, m_rayEndPos, out hit, m_Keyslayer))    //Key 레이어에 레이가 맞았다면
        {
            ChangeText();
            m_userText.SetActive(true);
        }
        else if (Physics.Linecast(m_rayPos.position, m_rayEndPos, out hit, m_Potionlayer))  //Potion 레이어에 레이가 맞았다면
        {
            ChangeText();
            m_userText.SetActive(true);
        }
        else
        {
            m_userText.SetActive(false);
        }

        //플레이어의 체력이 0보다 작다면
        if (m_playerHp <= 0)
        {
            MySetAnim(AnimState.die);

            m_gameMgr.m_isGameOver = true;

            return;
        }

        m_playerHpbar.value = (float)m_playerHp / (float)m_playerMaxHp;

        KeyBoardMove();         //키보드 조작 함수
        JoyStickMoveUpdate();   //조이스틱 조작 함수
        NaturalRecovery();      //체력 회복 함수
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

    //플레이어의 셰이더값을 조절해 서서히 나타나게 하는 함수
    IEnumerator Birth()
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

    //키보드로 이동
    void KeyBoardMove()
    {
        //상호작용
        if (Input.GetKeyDown(KeyCode.F))
        {
            DoorOnOff();        //문열고닫기
            TakeKeys();         //열쇠챙기기
            Potion();           //포션먹기
        }

        //가감속 없이 이동하는 법
        h = Input.GetAxisRaw("Horizontal"); //화살표 좌우키를 누르면 -1.0f, 0.0f,1.0f 사이값을 리턴해준다
        v = Input.GetAxisRaw("Vertical");   //화살표 위아래키를 누르면 -1.0f, 0.0f,1.0f 사이값을 리턴해준다
        //가감속 없이 이동하는 법

        if (v < 0.0f)
            v = 0.0f;

        if (h != 0.0f || v != 0.0f)
        {
            if (ISAttack() == true)
                return;

            a_CalcRotY = transform.eulerAngles.y;
            a_CalcRotY += (h * rotSpeed * Time.deltaTime);
            transform.eulerAngles = new Vector3(0.0f, a_CalcRotY, 0.0f);

            a_MoveVStep = transform.forward * v;
            m_MoveNextStep = a_MoveVStep;
            //기본이동
            //transform.position = transform.position + m_MoveNextStep;

            //네브메쉬를 활용한 이동
            m_MoveNextStep = m_MoveNextStep.normalized * m_MoveVelocity;
            m_MoveNextStep.y = 0.0f;
            nvAgent.velocity = m_MoveNextStep;
            //네브메쉬를 활용한 이동

            MySetAnim(AnimState.move);
        }
        else
        {
            //키보드 이동중이 아닐 때만 Idle 동작으로 돌아가게 한다.
            if (m_JoyMvLen <= 0.0f && ISAttack() == false)
            {
                MySetAnim(AnimState.idle);
            }
        }
    }

    //조이스틱 관련 변수
    Vector3 a_CacCamVec = Vector3.zero;    //카메라의 위치
    Vector3 a_RightDir = Vector3.zero;  //오른쪽방향
    //조이스틱의 움직임과 동기화
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

            m_JoyMvDir += (a_RightDir * a_JoyMoveDir.x); //좌우 조작(카메라가 바라보고 있는 기준으로 좌, 우로 얼만큼 이동시킬 것인지?)
            m_JoyMvDir.y = 0.0f;
            m_JoyMvDir.Normalize();
            //--------카메라가 바라보고 있는 전면을 기준으로 회전 시켜줘야 한다. 
        }

        if (a_JoyMoveLen == 0.0f)
        {
            //공격 애니메이션 중이면 공격 애니메이션이 끝나고 숨쉬기 애니로 돌아가게 한다.
            if (ISAttack() == false)
                MySetAnim(AnimState.idle);
        }
    }

    //조이스틱 이동
    void JoyStickMoveUpdate()
    {
        if (h != 0.0f || 0.0f != v)
            return;

        //조이스틱 코드
        if (0.0f < m_JoyMvLen)   //조이스틱으로 움직일 때
        {
            if (ISAttack() == true)
                return;

            //캐릭터 스프링 회전
            if (0.0001f < m_JoyMvDir.magnitude)
            {
                Quaternion a_TargetRot = Quaternion.LookRotation(m_JoyMvDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, a_TargetRot,
                                     Time.deltaTime * m_RotSpeed / 2.0f);
            }
            //캐릭터 스프링 회전


            //네비게이션 메시를 이용한 이동방법
            m_MoveNextStep = m_JoyMvDir * m_MoveVelocity;
            m_MoveNextStep.y = 0.0f;
            nvAgent.velocity = m_MoveNextStep;

            MySetAnim(AnimState.move);
        }
    }

    //공격을 하는 중이라면
    public bool ISAttack()
    {
        if (m_prevState != null && !string.IsNullOrEmpty(m_prevState))
        {
            if (m_prevState.ToString() == AnimState.attack.ToString())
                return true;
        }
        return false;
    }

    //애니메이션 관리 함수
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

    //공격하는 함수
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

    //문 열고 닫는 함수
    public void DoorOnOff()
    {
        if (Physics.Linecast(m_rayPos.position, m_rayEndPos, out hit, m_doorlayer))
        {
            if (hit.collider.CompareTag("Door"))
            {
                hit.collider.transform.GetComponent<DoorCtrl>().CheckKeys();
            }
        }
    }

    //열쇠 챙기는 함수
    public void TakeKeys()
    {
        if (Physics.Linecast(m_rayPos.position, m_rayEndPos, out hit, m_Keyslayer))
        {
            if (hit.collider.CompareTag("Keys"))
            {
                hit.collider.transform.GetComponent<KeysCtrl>().KeysOnOff();
            }
        }
    }

    //포션 먹는 함수
    public void Potion()
    {
        if (Physics.Linecast(m_rayPos.position, m_rayEndPos, out hit, m_Potionlayer))
        {
            if (hit.collider.CompareTag("Potion_Health"))
            {
                if (m_Keys[2] == null)
                {
                    hit.collider.transform.GetComponent<PotionCtrl>().TakePotion();
                }//if (m_Keys[2] == null)
            }//if (hit.collider.CompareTag("Potion_Health"))
        }//if (Physics.Linecast(m_rayPos.position, m_rayEndPos, out hit, m_Potionlayer))
    }//public void Potion()

    //공격의 끝을 확인하는 이벤트함수
    public void IsAttackFinish()
    {
        MySetAnim(AnimState.idle);
    }

    //애니메이션 이벤트 호출용 함수
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

                if (m_AttackDist + 0.4f < a_fCacLen)
                    continue;

                m_EnemyList[i].GetComponent<EnemyCtrl>().TakeDamage(this.gameObject, 10.0f);
            }//for(int i=0; i<iCount; ++i)
        }//if(Type == AnimState.attack.ToString())       
    }

    //피격당했다면
    public void TakeDamage(float a_Damage = 10.0f)
    {
        if (m_playerHp <= 0.0f)
            return;

        m_playerHp -= a_Damage;

        m_playerHpbar.value = (float)m_playerHp / (float)m_playerMaxHp;

        if (m_playerHp <= 0.0f)
            m_playerHp = 0;
    }

    //클리어 체크를 위한 트리거 함수
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("GameClear!!!");

        m_gameMgr.m_isGameClear = true;
        m_gameMgr.m_isClearbgm = true;
    }

    //어떤 플랫폼인지에 따라 문에 뜨는 텍스트 변경하는 함수
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

        //열쇠가 체크된다면
        if (hit.collider.CompareTag("Keys"))
        {
            if (hit.collider.transform.GetComponent<KeysCtrl>().m_isNomalKey)
            {
                //일반열쇠라면
                m_userText.GetComponentInChildren<Text>().text = "일반 열쇠를 획득합니다.";
            }
            else if (hit.collider.transform.GetComponent<KeysCtrl>().m_isSilverKey)
            {
                //실버열쇠라면
                m_userText.GetComponentInChildren<Text>().text = "은 열쇠를 획득합니다.";
            }
            else if (hit.collider.transform.GetComponent<KeysCtrl>().m_isGoldenKey)
            {
                //골드열쇠라면
                m_userText.GetComponentInChildren<Text>().text = "금 열쇠를 획득합니다.";
            }
        }

        //포션이 체크된다면
        if (hit.collider.CompareTag("Potion_Health"))
        {
            //포션이라면
            m_userText.GetComponentInChildren<Text>().text = "포션을 섭취합니다";
        }
    }

    //체력을 서서히 회복시키고 회복이 끝난후 체력에 소수점이 있다면 반올림하여 정수로 바꿔주는 함수
    void NaturalRecovery()
    {
        m_gameMgr.PlayerRecovery();
        if (!m_gameMgr.m_isRecovery)
        {
            //체력의 소수점을 반올림하여 정수로 만든다
            m_playerHp = Mathf.Round(m_playerHp);
        }
    }
}
