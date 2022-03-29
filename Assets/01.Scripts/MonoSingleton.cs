using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//참고 링크 : http://lonpeach.com/2017/02/04/unity3d-singleton-pattern-example/

public class G_Singleton<T> : MonoBehaviour where T : G_Singleton<T>    //Scene이 넘어가도 제거 되지 않음
{
    private static T m_Instance = null;
    private static object _syncobj = new object();
    private static bool appIsClosing = false;

    public static T Instance
    {
        get
        {
            if (appIsClosing)
                return null;

            lock (_syncobj)
            {
                if (m_Instance == null)
                {
                    T[] objs = FindObjectsOfType<T>();

                    if (objs.Length > 0)
                        m_Instance = objs[0];

                    if (objs.Length > 1)
                        Debug.Log("There is more than one " + typeof(T).Name + " in the scene.");

                    if (m_Instance == null)
                    {
                        //첫번째로 들어옴
                        string goName = typeof(T).ToString();
                        GameObject a_go = GameObject.Find(goName);
                        if (a_go == null)
                            a_go = new GameObject(goName);

                        m_Instance = a_go.AddComponent<T>();
                    }//if(m_Instance == null)
                }//if(m_Instance == null)
                return m_Instance;
            }//lock(_syncobj)
        }//get
    }//public static T Instance

    public virtual void Awake()
    {
        //두번째로 들어옴
        Init();
    }

    protected virtual void Init()
    {
        if (m_Instance == null)
        {
            m_Instance = this as T;
            DontDestroyOnLoad(base.gameObject);
        }
        else
        {
            if (m_Instance != this)
            {
                DestroyImmediate(base.gameObject);
            }
        }
    } //초기화를 상속을 통해 구현

    private void OnApplicationQuit()
    {
        m_Instance = null;
        appIsClosing = true;
    }
}

public class A_Singleton<T> : MonoBehaviour where T : A_Singleton<T> //Scene이 넘어갈때 사라지는 싱글톤
{
    private static T m_Instance = null;
    private static object _syncobj = new object();
    private static bool appIsClosing = false;

    public static T Instance
    {
        get
        {
            if (appIsClosing)
                return null;

            lock (_syncobj)
            {
                if (m_Instance == null)
                {
                    T[] objs = FindObjectsOfType<T>();

                    if (objs.Length > 0)
                        m_Instance = objs[0];

                    if (objs.Length > 1)
                        Debug.Log("There is more than one " + typeof(T).Name + " in the scene.");

                    if (m_Instance == null)
                    {
                        //이쪽이 첫번째로 들어오고...
                        string goName = typeof(T).ToString();
                        GameObject a_go = GameObject.Find(goName);
                        if (a_go == null)
                            a_go = new GameObject(goName);
                        m_Instance = a_go.AddComponent<T>();  //Awake()가 이쪽에서 발생된다.
                    }
                    else
                    {
                        m_Instance.Init();
                    }
                }

                return m_Instance;
            }//lock (_syncobj)
        } // get
    }//public static T Instance

    public virtual void Awake()
    {
        //이쪽이 두번째로 들어옴
        Init();
    }

    protected virtual void Init()
    {
        if (m_Instance == null)
        {
            m_Instance = this as T;
        }
        else
        {
            if (m_Instance != this)
            {
                DestroyImmediate(base.gameObject);
            }
        }
    } // 초기화를 상속을 통해 구현   

    private void OnApplicationQuit()
    {
        m_Instance = null;
        appIsClosing = true;
    }
}
