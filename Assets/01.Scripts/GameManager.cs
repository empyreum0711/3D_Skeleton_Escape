using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
enum JoyStickType
{
    Fixed = 0,
    Flexible = 1,
    FlexibleOnOff = 2
}

public class GameManager : MonoBehaviour
{
    [Header("플레이어관련변수")]
    public Text m_PHpTxt = null;         //플레이어의 체력값의 텍스트
    [HideInInspector] public PlayerCtrl m_refHero = null;   //플레이어 스크립트의 변수나 함수를 받아오기위한 변수
    [HideInInspector] public bool m_isRecovery = false;     //체력이 회복되고 있는지에 대한 변수
    public EnemyCtrl[] m_refEnemy;        //적 스크립트의 변수나 함수를 받아오기위한 변수

    //Fixed JoyStick 처리부분
    JoyStickType m_JoyStickType = JoyStickType.Fixed;
    [Header("조이스틱 관련변수")]
    public GameObject m_JoyStickBackObj = null;
    public Image m_JoyStickImg = null;
    float m_Radius = 0.0f;
    Vector3 m_OrignPos = Vector3.zero;
    Vector3 m_Axis = Vector3.zero;
    Vector3 m_JsCacVec = Vector3.zero;
    float m_JsCacDist = 0.0f;
    //Fixed JoyStick 처리부분

    //Flexible JoyStick 처리 부분
    public GameObject m_JoyStickPickPanel = null;
    private Vector2 posJoyBack;
    private Vector2 dirStick;
    //Flexible JoyStick 처리 부분
    [Header("모바일 UI")]
    public GameObject mobileUIObj = null;
    //모바일UI버튼
    public Button m_Attack_Btn = null;                      //공격버튼
    public Button m_Interaction_Btn = null;                 //상호작용 버튼
    //모바일UI버튼

    [Header("Game UI")]
    public GameObject m_SystemPanel = null;                 //시스템창 판넬
    public Image m_UIPanel = null;                          //UI판넬의 이미지
    public Text m_UIText = null;                            //UI판넬의 텍스트
    [HideInInspector] public bool m_isGameOver = false;     //게임오버의 여부
    [HideInInspector] public bool m_isGameClear = false;    //게임을 클리어 했는지의 여부
    public GameObject m_GOBtnCol = null;    //게임 오버시 나오는 버튼들의 부모 오브젝트
    public GameObject m_GCBtnCol = null;    //게임 클리어시 나오는 버튼들의 부모 오브젝트
    public Button m_RetryBtn = null;        //게임 오버시 재시작하는 버튼
    public Button m_OExitBtn = null;        //게임 오버시 나가는 버튼
    public Button m_CExitBtn = null;        //게임 클리어시 나가는 버튼
    public GameObject[] m_KeysImg;          //열쇠의 이미지들



    [Header("붉게 점멸하는 UI")]
    //체력이 30퍼센트 이하일때 붉은색 화면이 점멸하는 부분
    public Image m_BloodScreen = null;  //붉은 화면의 이미지
    private float m_AniDuring = 0.8f;   //점멸 시간
    [HideInInspector] public bool m_StartFade = false;   //점멸 조건
    private float m_CacTime = 0.0f;     //시간 계산용 변수
    private float m_AddTimer = 0.0f;    //현재 시간에 더해주는 시간
    private Color m_Color;              //컬러값 변수
    private readonly float m_StartValue = 1.0f;  //시작 알파값
    private readonly float m_EndValue = 0.0f;    //끝나는 알파값
    //체력이 30퍼센트 이하일때 붉은색 화면이 점멸하는 부분

    public bool m_isClearbgm = false;
    public bool m_isBackbgm = false;
    public AudioSource m_SoundMgr = null;

    // Start is called before the first frame update
    void Start()
    {
#if UNITY_ANDROID
        
#else

#endif
        if (mobileUIObj != null)
            mobileUIObj.SetActive(true);
        SoundManager.Instance.PlayBGM("InGame_BGM",0.1f);

        GameObject a_Player = GameObject.Find("SKELETON");
        if (a_Player != null)
        {
            m_refHero = a_Player.GetComponent<PlayerCtrl>();
        }

        PlayerCtrl.m_playerMaxHp = 50;

        GameObject a_SdMgr = GameObject.Find("SoundManager");
        if(a_SdMgr != null)
        {
            m_SoundMgr = a_SdMgr.GetComponentInChildren<AudioSource>();
        }
           
        GameObject[] a_Enemy = GameObject.FindGameObjectsWithTag("Enemy");
        for (int i = 0; i < m_refEnemy.Length; i++)
        {
            if (a_Enemy[i] != null)
            {
                m_refEnemy[i] = a_Enemy[i].GetComponent<EnemyCtrl>();
            }
        }

        //Fixed JoyStick 처리부분
        #region
        if (m_JoyStickBackObj != null && m_JoyStickImg != null
            && m_JoyStickBackObj.activeSelf == true
            && m_JoyStickPickPanel.activeSelf == false)
        {
            m_JoyStickType = JoyStickType.Fixed;

            Vector3[] v = new Vector3[4];
            m_JoyStickBackObj.GetComponent<RectTransform>().GetWorldCorners(v);
            //[0]:좌측하단 [1]:좌측상단 [2]:우측상단 [3]:우측하단
            //v[0] 촤측하단이 0, 0 좌표인 스크린 좌표(Screen.width, Screen.height)를 기준으로
            m_Radius = v[2].y - v[0].y;
            m_Radius = m_Radius / 3.0f;

            m_OrignPos = m_JoyStickImg.transform.position;

            //스크립트로만 대기하고자 할때
            EventTrigger trigger = m_JoyStickBackObj.GetComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.Drag;
            entry.callback.AddListener((data) => { OnDragJoyStick((PointerEventData)data); });
            trigger.triggers.Add(entry);

            entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.EndDrag;
            entry.callback.AddListener((data) => { OnEngDragJoyStick((PointerEventData)data); });
            trigger.triggers.Add(entry);
            //스크립트로만 대기하고자 할때

        }
        #endregion
        //Fixed JoyStick 처리부분

        //Flexible JoyStick 처리부분
        #region
        if (m_JoyStickPickPanel != null && m_JoyStickBackObj != null
            && m_JoyStickImg != null
            && m_JoyStickPickPanel.activeSelf == true)
        {
            if (m_JoyStickBackObj.activeSelf == true)
            {
                m_JoyStickType = JoyStickType.Flexible;
            }
            else
            {
                m_JoyStickType = JoyStickType.FlexibleOnOff;
            }

            EventTrigger a_JoyBackTrigger
                = m_JoyStickBackObj.GetComponent<EventTrigger>();

            if (a_JoyBackTrigger != null)
            {
                Destroy(a_JoyBackTrigger);  //조이스틱 백에 설치되어 있는 이벤트 트리거 제거
            }

            Vector3[] v = new Vector3[4];
            m_JoyStickBackObj.GetComponent<RectTransform>().GetWorldCorners(v);
            m_Radius = v[2].y - v[0].y;
            m_Radius = m_Radius / 3.0f;

            m_OrignPos = m_JoyStickImg.transform.position;
            m_JoyStickBackObj.GetComponent<Image>().raycastTarget = false;
            m_JoyStickImg.raycastTarget = false;

            EventTrigger trigger = m_JoyStickPickPanel.GetComponent<EventTrigger>();    //인스펙터에서 이벤트트리거가 있어야한다

            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerDown;
            entry.callback.AddListener((data) =>
            {
                OnPointerDown_Flx((PointerEventData)data);
            });
            trigger.triggers.Add(entry);

            entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerUp;
            entry.callback.AddListener((data) =>
            {
                OnPointerUp_Flx((PointerEventData)data);
            });
            trigger.triggers.Add(entry);

            entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.Drag;
            entry.callback.AddListener((data) =>
            {
                OnDragJoyStick_Flx((PointerEventData)data);
            });
            trigger.triggers.Add(entry);

        }
        #endregion
        //Flexible JoyStick 처리부분

        //공격버튼
        if (m_Attack_Btn != null)
            m_Attack_Btn.onClick.AddListener(() =>
            {
                if (m_refHero != null)
                    m_refHero.AttackOrder();
            });
        //공격버튼

        //점프 버튼
        if (m_Interaction_Btn != null)
            m_Interaction_Btn.onClick.AddListener(() =>
            {
                m_refHero.DoorOnOff();
                m_refHero.TakeKeys();
                m_refHero.Potion();
            });
        //점프 버튼    

        //게임 오버시 나오는 버튼들
        if (m_RetryBtn != null)
            m_RetryBtn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("02.InGame");
            });

        if (m_OExitBtn != null)
            m_OExitBtn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("01.TitleScene");
            });

        if (m_CExitBtn != null)
            m_CExitBtn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("01.TitleScene");
            });
        //게임 오버시 나오는 버튼들
    }

    // Update is called once per frame
    void Update()
    {
        m_PHpTxt.text = PlayerCtrl.m_playerMaxHp + " / " + (int)m_refHero.m_playerHp;

        //좌클릭을 했을때
        if (Input.GetMouseButtonDown(0))
        {
            if (MouseHover.MHinstance.isUIHover == false)
            {
                m_refHero.AttackOrder();
            }
        }
        //좌클릭을 했을때    

        //게임 UI 출력하는 함수
        GameUI();
        BloodScreen();
        ShowKeyImg();
        BGMChange();  
    }

    //Fixed JoyStick 처리 부분
    void OnDragJoyStick(PointerEventData _data)  //Delegate
    {
        if (m_JoyStickImg == null)
            return;

        m_JsCacVec = Input.mousePosition - m_OrignPos;
        m_JsCacVec.z = 0.0f;
        m_JsCacDist = m_JsCacVec.magnitude;
        m_Axis = m_JsCacVec.normalized;

        //조이스틱이 백그라운드를 벗어나지 못하게 막는 부분
        if (m_Radius < m_JsCacDist)
        {
            m_JoyStickImg.transform.position =
                m_OrignPos + m_Axis * m_Radius;
        }
        else
        {
            m_JoyStickImg.transform.position =
                m_OrignPos + m_Axis * m_JsCacDist;
        }

        if (1.0f < m_JsCacDist)
            m_JsCacDist = 1.0f;

        //캐릭터 이동
        if (m_refHero != null)
            m_refHero.SetJoyStickMove(m_JsCacDist, m_Axis);

    }

    void OnEngDragJoyStick(PointerEventData _data)  //Delegate
    {
        if (m_JoyStickImg == null)
            return;

        m_Axis = Vector3.zero;
        m_JoyStickImg.transform.position = m_OrignPos;

        m_JsCacDist = 0.0f;

        //캐릭터 정지 처리
        if (m_refHero != null)
            m_refHero.SetJoyStickMove(0.0f, m_Axis);
    }
    //Fixed JoyStick 처리 부분

    //Flexible JoyStick 처리 부분
    void OnPointerDown_Flx(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)   //마우스 왼쪽버튼만
            return;

        if (m_JoyStickBackObj == null)
            return;

        if (m_JoyStickImg == null)
            return;

        m_JoyStickBackObj.transform.position = eventData.position;
        m_JoyStickImg.transform.position = eventData.position;

        m_JoyStickBackObj.SetActive(true);
    }

    public void OnPointerUp_Flx(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)   //마우스 왼쪽버튼만
            return;

        if (m_JoyStickBackObj == null)
            return;

        if (m_JoyStickImg == null)
            return;

        m_JoyStickBackObj.transform.position = m_OrignPos;
        m_JoyStickImg.transform.position = m_OrignPos;

        if (m_JoyStickType == JoyStickType.FlexibleOnOff)
        {
            m_JoyStickBackObj.SetActive(false);//꺼진 상태로 시작하는 방식일 때는 활성화
        }

        m_Axis = Vector3.zero;
        m_JsCacDist = 0.0f;

        //캐릭터 정지
        if (m_refHero != null)
        {
            m_refHero.SetJoyStickMove(0.0f, Vector3.zero);
        }
    }

    void OnDragJoyStick_Flx(PointerEventData eventData) //Delegate
    {
        //eventData.position 현재 마우스의 월드 좌표

        if (eventData.button != PointerEventData.InputButton.Left) //마우스 왼쪽 버튼만
            return;

        if (m_JoyStickImg == null)
            return;

        posJoyBack = (Vector2)m_JoyStickBackObj.transform.position;
        //조이스틱 백그라운드의 현재위치 기준

        m_JsCacDist = Vector2.Distance(posJoyBack, eventData.position); //거리
        dirStick = eventData.position - posJoyBack; //방향

        if (m_Radius < m_JsCacDist)
        {
            m_JsCacDist = m_Radius;
            m_JoyStickImg.transform.position =
                (Vector3)(posJoyBack + (dirStick.normalized * m_Radius));
        }
        else
        {
            m_JoyStickImg.transform.position = (Vector3)eventData.position;
        }

        if (m_JsCacDist > 1.0f)
            m_JsCacDist = 1.0f;

        m_Axis = (Vector3)dirStick.normalized;

        //캐릭터 이동처리
        if (m_refHero != null)
            m_refHero.SetJoyStickMove(m_JsCacDist, m_Axis);
        //캐릭터 이동처리
    }
    //Flexible JoyStick 처리 부분

    void GameUI()   //승리 또는 사망시 출력되는 UI
    {
        Color a_PanelColor = m_UIPanel.color; 

        if (m_isGameOver)
        {
            m_SystemPanel.SetActive(true);

            //UI의 색을 점점 어둡게 하는 코드
            a_PanelColor.a += 0.4f * Time.deltaTime;
            a_PanelColor.r -= 0.3f * Time.deltaTime;
            a_PanelColor.g -= 0.5f * Time.deltaTime;
            a_PanelColor.b -= 0.5f * Time.deltaTime;

            m_UIPanel.color = a_PanelColor;
        }
        else if (m_isGameClear)
        {           
            if (m_isClearbgm)//Clear시의 BGM 재생
            {       
                SoundManager.Instance.PlayBGM("Clear_BGM", 0.1f);
            }
                
            m_SystemPanel.SetActive(true);

            m_UIText.text = "Game Clear!!!";

            a_PanelColor.a += 0.4f * Time.deltaTime;

            m_UIPanel.color = a_PanelColor;
            m_isClearbgm = false;
        }

        if (a_PanelColor.a >= 1.0f)
        {
            if (m_isGameOver)//게임오버(플레이어가 사망)했다면
            {
                m_GOBtnCol.SetActive(true);
            }
            else if (m_isGameClear)//게임클리어(목표달성)했다면
            {
                m_GCBtnCol.SetActive(true);
            }
        }
    }

    public void BloodScreen()   //체력이 30퍼센트 이하로 떨어지면 출력되는 UI
    {
        //플레이어의 체력이 30퍼센트 미만일때
        if (m_refHero.m_playerHp / PlayerCtrl.m_playerMaxHp <= 0.3f && !m_isGameOver && !m_isGameClear)
        {
            //붉은색으로 점멸하는 부분
            if (m_StartFade)
            {
                if (m_CacTime < 1.0f)
                {
                    m_AddTimer += Time.deltaTime;
                    m_CacTime = m_AddTimer / m_AniDuring;
                    m_Color = m_BloodScreen.color;
                    m_Color.a = Mathf.Lerp(m_StartValue, m_EndValue, m_CacTime);
                    m_BloodScreen.color = m_Color;

                    if (1.0f <= m_CacTime)
                    {
                        if (m_StartValue == 1.0f && m_EndValue == 0.0f)
                        {
                            m_Color.a = 0.0f;
                            m_BloodScreen.color = m_Color;
                            m_AddTimer = 0.0f;
                            m_CacTime = 0.0f;
                            m_StartFade = false;
                        }//if(m_StartValue == 1.0f && m_EndValue == 0.0f)
                    }// if(1.0f <= m_CacTime)
                }//if(m_CacTime < 1.0f)
            }//if(m_StartFade)
            m_StartFade = true;
        }
        else
        {
            //플레이어의 체력이 30퍼센트 이상이 되었을때 초기화
            m_Color = m_BloodScreen.color;
            m_Color.a = 0.0f;
            m_BloodScreen.color = m_Color;
            m_AddTimer = 0.0f;
            m_CacTime = 0.0f;
            m_StartFade = false;
        }
    }

    //플레이어의 체력이 서서히 회복되는 함수
    public void PlayerRecovery()
    {
        m_isRecovery = true;

        for (int i = 0; i < m_refEnemy.Length; i++)
        {
            for (int j = 0; j < m_refEnemy.Length; j++)
            {
                if (m_refEnemy[j].m_isbattle == true)//적이 한명이라도 전투중이라면
                {
                    m_isRecovery = false;
                    return;
                }
            }

            if (m_refEnemy[i].m_isbattle == false)//단 하나의 적도 전투 중이 아니라면
            {              
                if (m_refHero.m_playerHp < PlayerCtrl.m_playerMaxHp)
                {         
                    m_refHero.m_playerHp += 1.0f * Time.deltaTime;//체력을 회복함

                    if (m_refHero.m_playerHp >= PlayerCtrl.m_playerMaxHp)
                    {
                        m_refHero.m_playerHp = PlayerCtrl.m_playerMaxHp;
                    }

                    m_refHero.m_playerHpbar.value = m_refHero.m_playerHp / PlayerCtrl.m_playerMaxHp;
                }//if (m_refHero.m_playerHp < m_refHero.m_playerMaxHp)
            }//if (m_refEnemy[i].m_isbattle == false)                
        }//for(int i= 0; i < m_refEnemy.Length; i++)

    }//public void PlayerRecovery()

    //열쇠를 획득했을 때 획득한 열쇠를 보여주는 함수
    void ShowKeyImg()
    {
        for (int i = 0; i < m_refHero.m_Keys.Length; i++)
        {
            for (int j = 0; j < m_refHero.m_Keys.Length; j++)
            {
                if (m_refHero.m_Keys[j] == null)
                {
                    m_KeysImg[j].SetActive(true);
                }
            }
        }
    }

    //전투/비전투시 BGM을 바꿔주는 함수
    void BGMChange()
    {
        m_isBackbgm = false;

        for (int i = 0; i < m_refEnemy.Length; i++)
        {
            for (int j = 0; j < m_refEnemy.Length; j++)
            {
                if (m_refEnemy[j].m_isbattle)//적중에 하나라도 전투중이라면
                {
                    m_isBackbgm = true;
                }                     
            }//for (int j = 0; j < m_refEnemy.Length; j++)
            if (!m_refEnemy[i].m_isbattle && m_SoundMgr.clip.name == "Battle_BGM" && !m_isBackbgm)
            {
                //전투 종료 후 인게임 BGM
                SoundManager.Instance.PlayBGM("InGame_BGM", 0.1f);
            }//if (!m_refEnemy[i].m_isbattle && m_isBackbgm && m_SoundMgr.clip.name == "Battle_BGM" && m_isBackbgm)
            else if (m_refEnemy[i].m_isbattle && m_SoundMgr.clip.name == "InGame_BGM" && m_isBackbgm)
            {
                //전투 중 배틀 BGM
                SoundManager.Instance.PlayBGM("Battle_BGM", 0.1f);
            }//if (m_refEnemy[i].m_isbattle && m_SoundMgr.clip.name == "InGame_BGM" && !m_isBackbgm)   
        }//for (int i = 0; i < m_refEnemy.Length; i++)      
    }

}
