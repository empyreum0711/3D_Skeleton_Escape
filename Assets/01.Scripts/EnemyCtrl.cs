using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class EnemyCtrl : MonoBehaviour
{
    //인스펙터뷰에 표시할 애니메이션 클래스 변수
    public Anim anim;       //AnimSupporter.cs 쪽에 정의

    //Enemy의 현재 상태 정보를 저장할 enum 변수
    public AnimState m_EnemyState = AnimState.idle;

    Animation m_RefAnimation = null;
    
    //Enemy AI
    public GameObject m_AggroTarget = null;
    Vector3 m_MoveDir = Vector3.zero;       //수평 진행 노멀 방향 벡터
    public Vector3 m_CacVLen = Vector3.zero;       //플레이어를 향하는 벡터
    public float a_CacDist = 0.0f;                 //거리 계산용 변수
    public float traceDist = 7.0f;                 //추적 거리
    float attackDist = 1.8f;                //공격 거리
    Quaternion a_TargetRot;                 //회전 계산용 변수
    float m_RotSpeed = 7.0f;                //초당 회전 속도
    GameObject m_Attacker = null;           //이 Enemy를 공격한 캐릭터
    float m_durDieTime;                     //죽고나서 이 시간만큼 지나면 오브젝트 파괴

    [SerializeField] Transform m_rayPos;    //BoxCast의 시작점
    LayerMask m_playerlayer;                //BoxCast에 맞은게 플레이어인지 확인하기 위한 레이어
    RaycastHit hit;                         
    Vector3 m_scales;                       //BoxCast의 크기
    float m_ScaleX = 7.0f;                  //BoxCast의 x축의 길이
    float m_ScaleY = 3.0f;                  //BoxCast의 y축의 길이
    float m_ScaleZ = 0.0f;                  //BoxCast의 z축의 길이
    float m_maxDistance = 7.0f;             //BoxCast의 최대 길이
    public bool m_isHit;                    //BoxCast에 닿았는지 여부
    float m_MoveVelocity = 2.0f;            //평면 초당 이동속도
    bool m_isDamaged = false;        //플레이어에게 공격 당했는지
    //Enemy AI

    protected NavMeshAgent nvAgent;
    protected NavMeshPath movePath;

    protected Vector3 m_PathEndPos = Vector3.zero;
    [HideInInspector] public int m_CurPathIndex = 1;
    protected double m_MoveDurTime = 0.0f;      //목표점까지 도착하는데 걸리는 시간
    protected double m_AddTimeCount = 0.0f;     //누적시간 카운트
    float m_MoveTick = 0.0f;

    //-------------공격 애니 관련 변수
    float a_CacRate = 0.0f;
    float a_NormalTime = 0.0f;
    //-------------공격 애니 관련 변수

    //MyNavCalcPath 함수관련 변수
    Vector3 a_VecLen = Vector3.zero;
    //MyNavCalcPath 함수관련 변수

    //---ㅡMoveToPath 관련 변수들...
    private bool a_isSucessed = true;
    private Vector3 a_CurCPos = Vector3.zero;
    private Vector3 a_CacDestV = Vector3.zero;
    private Vector3 a_TargetDir;
    private float a_CacSpeed = 0.0f;
    private float a_NowStep = 0.0f;
    private Vector3 a_Velocity = Vector3.zero;
    private Vector3 a_vTowardNom = Vector3.zero;
    private int a_OldPathCount = 0;
    ////---ㅡMoveToPath 관련 변수들...

    //Event_Attack 함수관련
    Vector3 a_DistVec = Vector3.zero;
    float a_CacLen = 0.0f;
    //Event_Attack 함수관련

    public GameObject m_EnemyUI = null;
    public float m_MaxEHp = 50.0f;
    public float m_CurEHp = 50.0f;
    public Image m_imgHpbar = null;

    float m_endbattleTime = 0.0f;
    public bool m_isbattle;
    GameManager m_gmMgr = null;

    // Start is called before the first frame update
    void Start()
    {
        m_RefAnimation = GetComponentInChildren<Animation>();

        m_playerlayer = LayerMask.GetMask("Player");
        movePath = new NavMeshPath();
        nvAgent = this.gameObject.GetComponent<NavMeshAgent>();
        nvAgent.updateRotation = false;

        GameObject gm = GameObject.Find("GameManager");
        if (m_gmMgr != null)
            m_gmMgr.GetComponent<GameManager>();


    }

    // Update is called once per frame
    void Update()
    {
        m_scales = new Vector3(m_ScaleX, m_ScaleY, m_ScaleZ);

        m_isHit = Physics.BoxCast(m_rayPos.position, m_scales, transform.forward, out hit, transform.rotation, m_maxDistance, m_playerlayer);

        //죽었을 경우 7초뒤에 게임 오브젝트 삭제
        if (m_CurEHp <= 0)
        {
            m_durDieTime += Time.deltaTime;
            if (m_durDieTime >= 7.0f)
            {
                m_isbattle = false;
                Destroy(gameObject);
            }
        }
        //죽었을 경우 7초뒤에 게임 오브젝트 삭제

        //적의 상태가 die면 리턴
        if (m_EnemyState == AnimState.die)
            return;

        m_MoveTick -= Time.deltaTime;

        if (m_MoveTick < 0.0f)
            m_MoveTick = 0.0f;

        

        EnemyStateUpdate();         
        EnemyActionUpdate();        
    }

    //공격 대상이 있는지 확인하고 대상이 있다면 대미지를 입히는 함수
    public void Event_AttDamage(string Type)
    {
        if (m_AggroTarget == null)
            return;

        SoundManager.Instance.PlayGUISound("SWORD_01");
        a_DistVec = m_AggroTarget.transform.position - transform.position;
        a_CacLen = a_DistVec.magnitude;
        a_DistVec.y = 0.0f;

        //공격각도 안에 있는 경우
        if (Vector3.Dot(transform.forward, a_DistVec.normalized) < 0.0f)//90도를 넘는 범위 안에 있을경우
            return;

        //공격각도 안에 있는 경우
        if ((attackDist + 1.7f) < a_CacLen)
            return;

        if (m_RefAnimation != null)
            m_AggroTarget.GetComponent<PlayerCtrl>().TakeDamage(10.0f);

    }

    //Damage를 받는 함수
    public void TakeDamage(GameObject a_Attacker, float a_Damage = 10.0f)
    {    
        m_Attacker = a_Attacker;

        m_CurEHp -= a_Damage;
        m_isDamaged = true;

        m_imgHpbar.fillAmount = (float)m_CurEHp / (float)m_MaxEHp;

        if (m_CurEHp <= 0.0f)
            m_CurEHp = 0.0f;

        if (m_CurEHp <= 0)
        {
            
            m_EnemyState = AnimState.die;
            MySetAnim(anim.Die.name, 0.13f);
        }
    }

    //State상태를 갱신해주는 함수
    void EnemyStateUpdate()
    {
        PlayerCtrl a_playerHp = GameObject.Find("SKELETON").GetComponent<PlayerCtrl>();//플레이어의 체력 체크를 위한 변수

        if (m_AggroTarget != null)   //타겟이 있을때
        {   
            m_EnemyUI.SetActive(true); //Enemy UI On
            
            m_isbattle = true;
            m_endbattleTime = 0.0f;
            
            if (a_playerHp.m_playerHp <= 0.0f)//플레이어의 체력이 0보다 작을때 행동 중지
            {
                m_EnemyState = AnimState.idle;
            }//if (a_playerHp.m_playerHp <= 0.0f)
            else
            {
                m_CacVLen = m_AggroTarget.transform.position - this.transform.position;
                
                m_CacVLen.y = 0.0f;
                m_MoveDir = m_CacVLen.normalized;   //플레이어를 바라보게함
                a_CacDist = m_CacVLen.magnitude;

                //탐색
                if (2 < movePath.corners.Length)
                    traceDist = 14.0f;  //추적범위
                else
                    traceDist = 7.0f;    //추적범위
                                         //탐색

                if (a_CacDist <= attackDist && m_isHit == true) //플레이어가 공격사거리 안으로 들어왔는지 체크
                {
                    m_EnemyState = AnimState.attack;    //Enemy의 상태를 attack으로 설정        
                }//if (a_CacDist <= attackDist) //플레이어가 공격사거리 안으로 들어왔는지 체크
                else if (a_CacDist <= traceDist)    //추적범위 이내로 들어왔는지 체크
                {
                    m_EnemyState = AnimState.trace;     //Enemy의 상태를 trace으로 설정                                                    
                }//else if (a_CacDist <= traceDist)    //추적범위 이내로 들어왔는지 체크
                else
                {
                    m_EnemyState = AnimState.idle;      //Enemy의 상태를 idle로 설정
                    m_AggroTarget = null;
                }
            }//if (a_playerHp.m_playerHp >= 0.0f)
        }//if(m_AggroTarget != null)   //타겟이 있을경우
        else if (m_AggroTarget == null) //타겟이 없을 경우
        {
            m_EnemyUI.SetActive(false); //Enemy UI Off
            if(m_isbattle && !m_EnemyUI.activeSelf)
            {
                m_endbattleTime += Time.deltaTime;
                if(m_endbattleTime > 5.0f)
                {                
                    m_isbattle = false;
                    m_endbattleTime = 0.0f;
                }               
            }

            GameObject a_player = GameObject.FindGameObjectWithTag("Player");

            m_CacVLen = a_player.transform.position - this.transform.position;
            m_CacVLen.y = 0.0f;
            m_MoveDir = m_CacVLen.normalized;   //플레이어를 바라 보도록
            a_CacDist = m_CacVLen.magnitude;

            //탐색
            if (2 < movePath.corners.Length)
                traceDist = 14.0f;  //추적범위
            else
                traceDist = 7.0f;    //추적범위
                                     //탐색

            if (0.0f < a_playerHp.m_playerHp)
            {
                if (a_CacDist <= attackDist && m_isHit == true)                    //공격 사거리내에 들어왔는지 체크
                {
                    m_EnemyState = AnimState.attack;            //Enemy의 상태를 attack으로 설정
                    m_AggroTarget = a_player.gameObject;        //타겟 설정
                }
                else if (a_CacDist <= traceDist && m_isHit == true || m_isDamaged == true)//추적거리 내에 들어왔고 시야에 들어왔는지 또는 플레이어에게 공격당했는지 체크
                {
                    m_EnemyState = AnimState.trace;             //Enemy의 상태를 trace로 설정
                    m_AggroTarget = a_player.gameObject;        //타겟설정
                    m_isDamaged = false;
                }
            }//if (0.0f < a_playerHp.m_playerHp)
        }//else  //타겟이 없을 경우
    }

    //애니메이션 및 이동을 시켜주는 함수
    void EnemyActionUpdate()
    {
        if (m_EnemyState == AnimState.attack && m_isHit == true)    //공격중이라면
        {
            //아직 공격 애니메이션이 실행중이라면
            if (m_AggroTarget != null)
            {
                if (0.0001f < m_MoveDir.magnitude)
                {
                    a_TargetRot = Quaternion.LookRotation(m_MoveDir);
                    transform.rotation = Quaternion.Slerp(transform.rotation,
                        a_TargetRot, Time.deltaTime * m_RotSpeed);
                }
                MySetAnim(anim.Attack1.name, 0.12f);    //attack1애니메이션 적용
                ClearPath();    //이동 즉시 취소
            }
            else
            {
                MySetAnim(anim.Idle.name, 0.13f);   //idle애니메이션 적용
            }
        }//if(m_EnemyState == AnimState.attack)    //공격중이라면
        else if (m_EnemyState == AnimState.trace)
        {
            if (m_AggroTarget != null)
            {
                if (IsAttackAnim() == false) //공격애니메이션이 끝난 경우에만 추적 or 이동하도록
                {
                    if (m_MoveTick <= 0.0f)
                    {
                        float a_PathLen = 0.0f;
                        if (MyNavCalcPath(this.transform.position, m_AggroTarget.transform.position,
                            ref a_PathLen) == true)
                        {
                            m_MoveDurTime = a_PathLen / m_MoveVelocity;//도착까지 걸리는 시간
                            m_AddTimeCount = 0.0f;
                        }   //if(a_IsPathOK == true)

                        m_MoveTick = 0.2f;
                    }
                    MoveToPath();   //이동
                }//if (IsAttackAnim() == false)
            }//if (m_AggroTarget != null)
            else
            {
                MySetAnim(anim.Idle.name, 0.13f);   //idle애니메이션 적용
            }
        }//else if (m_EnemyState == AnimState.trace)
        else if (m_EnemyState == AnimState.idle)
        {
            MySetAnim(anim.Idle.name, 0.13f);   //idle애니메이션 적용
        }
        else if (m_EnemyState == AnimState.die)
        {
            MySetAnim(anim.Die.name, 0.13f);
        }
    }

    //공격애니메이션의 상태 체크 함수
    public bool IsAttackAnim()  
    {
        if (m_RefAnimation != null)
        {
            if (m_RefAnimation.IsPlaying(anim.Attack1.name) == true)
            {
                a_NormalTime = m_RefAnimation[anim.Attack1.name].time
                    / m_RefAnimation[anim.Attack1.name].length;

                //m_RefAnimation["Attack1h1"].time   //애니메이션이 얼마나 진행되었는지의 현재 시간값
                //m_RefAnimation["Attack1h1"].length //한동작이 끝날 때까지의 시간값

                //소수점 한동작이 몇프로 진행되었는지 계산 변수
                a_CacRate = a_NormalTime - (float)((int)a_NormalTime);

                if (a_CacRate < 0.95f)  //공격 애니메이션의 시전이 끝이 아닐때(시전중일때)
                    return true;
            }
        }//if(m_RefAnimation != null)     
        return false;
    }

    public bool MyNavCalcPath(Vector3 a_StartPos, Vector3 a_TargetPos, ref float a_PathLen) //길찾기
    {
        // 이동이 시작된 상황이므로 초기화 하고 계산 시작
        movePath.ClearCorners();    //경로 모두 제거
        m_CurPathIndex = 1;         //진행 인덱스 초기화
        m_PathEndPos = transform.position;
        // 이동이 시작된 상황이므로 초기화 하고 계산 시작

        if (nvAgent == null || nvAgent.enabled == false)
        {
            return false;
        }

        if (NavMesh.CalculatePath(a_StartPos, a_TargetPos, -1, movePath) == false)
        {
            return false;
            //CalculatePath() 함수 계산이 끝나고 정상적으로 instace.final
            //즉 목적지까지 계산에 도착했다는 뜻이다
            //p.status == UnityEngine.AI.NavMeshPathStatus.PathComplete 일때
            //정상적으로 타겟설정을 해준다는 뜻
            //길 찾기를 실패 했을 경우 점프할 때 도 있다.
        }

        if (movePath.corners.Length < 2)
        {
            return false;
        }

        for (int i = 1; i < movePath.corners.Length; i++)
        {
            a_VecLen = movePath.corners[i] - movePath.corners[i - 1];
            a_PathLen += a_VecLen.magnitude;
        }

        if (a_PathLen <= 0.0f)
        {
            return false;
        }

        //주인공이 마지막 위치에 도착했을 때 정확한 방향을 바라보게 함
        m_PathEndPos = movePath.corners[(movePath.corners.Length - 1)];

        return true;
    }

    //애니메이션을 관리하는 함수
    public void MySetAnim(string newAnim, float CrossTime = 0.0f,
        PlayMode mode = PlayMode.StopSameLayer)
    {
        if (m_RefAnimation != null)
        {
            if (0.0f < CrossTime)
            {
                if (mode != PlayMode.StopSameLayer)
                {
                    m_RefAnimation.CrossFade(newAnim, CrossTime, mode);
                }
                else
                {
                    m_RefAnimation.CrossFade(newAnim, CrossTime);
                }
            }
            else
            {
                m_RefAnimation.Play(newAnim);
            }
        }//if(m_RefAnimation != null)
    }//public void MySetAnim

    public bool MoveToPath(float overSpeed = 1.0f)
    {
        a_isSucessed = true;

        if (movePath == null)
            movePath = new NavMeshPath();

        a_OldPathCount = m_CurPathIndex;

        if (m_CurPathIndex < movePath.corners.Length)    //최소 m_CurPathIndex = 1 보다 큰 경우에 이동
        {
            a_CurCPos = this.transform.position;
            a_CacDestV = movePath.corners[m_CurPathIndex];
            a_CurCPos.y = a_CacDestV.y; //높이 오차때문에 도착판정을 못하는 경우를 위함
            a_TargetDir = a_CacDestV - a_CurCPos;
            a_TargetDir.y = 0.0f;
            a_TargetDir.Normalize();

            a_CacSpeed = m_MoveVelocity;
            a_CacSpeed *= overSpeed;

            a_NowStep = a_CacSpeed * Time.deltaTime;//이번에 이동했을 때 이 안으로만 들어오면 무조건 도착으로 판정

            a_Velocity = a_CacSpeed * a_TargetDir;
            a_Velocity.y = 0.0f;
            nvAgent.velocity = a_Velocity;          //이동

            if ((a_CacDestV - a_CurCPos).magnitude <= a_NowStep)     //중간점에 도착한 것으로 본다.  여기서 a_CurCPos == Old Position의미
            {
                movePath.corners[m_CurPathIndex] = this.transform.position;
                m_CurPathIndex += 1;
            }//if((a_CacDestV - a_CurCPos).magnitude <= a_NowStep) 

            m_AddTimeCount += Time.deltaTime;
            if (m_MoveDurTime <= m_AddTimeCount) //목표점에 도착
            {
                m_CurPathIndex = movePath.corners.Length;
            }
        }//if (m_CurPathIndex < movePath.corners.Length)

        if (m_CurPathIndex < movePath.corners.Length)    //목적지에 도착하지 않았을때 매 프레임
        {
            //캐릭터 회전 및 애니메이션 방향 조정
            a_vTowardNom = movePath.corners[m_CurPathIndex] - this.transform.position;
            a_vTowardNom.y = 0.0f;
            a_vTowardNom.Normalize();   //단위 벡터 생성

            if (0.0001f < a_vTowardNom.magnitude)    //로테이션에서는 모두 들어가야 한다.
            {
                Quaternion a_TargetRot = Quaternion.LookRotation(a_vTowardNom);
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    a_TargetRot, Time.deltaTime * m_RotSpeed);
            }

            MySetAnim(anim.Move.name, 0.12f);
        }
        //최종 목적지에 도착한 경우
        else
        {
            if (a_OldPathCount < movePath.corners.Length)
            {
                MySetAnim(anim.Idle.name, 0.13f);
            }
            a_isSucessed = false;   //아직 목적지에 도착하지 않았다면 재실행 할 것
        }

        return a_isSucessed;
    }//public bool MoveToPath(float overSpeed = 1.0f)

    void ClearPath()
    {
        m_PathEndPos = transform.position;
        if (0 < movePath.corners.Length)
        {
            movePath.ClearCorners();    //경로 모두 제거
        }
        m_CurPathIndex = 1; //진행 인덱스 초기화
    }

    

    //Gizmos를 Scene에서 확인할 수 있게 하는 함수
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;

        if (m_isHit)
        {
            Gizmos.DrawRay(m_rayPos.position, transform.forward * hit.distance);
            Gizmos.DrawWireCube(transform.position + transform.forward * hit.distance, m_scales);
        }
        else
        {
            Gizmos.DrawRay(transform.position, transform.forward * m_maxDistance);
        }
    }
}
