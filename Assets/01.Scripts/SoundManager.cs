using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : G_Singleton<SoundManager>
{
    //C++의 STL map에 해당되는 C#의 콜렉션 Dictionary
    [HideInInspector]
    public Dictionary<string, AudioClip> m_ADClipList =
                        new Dictionary<string, AudioClip>(); // AudioClip List

    [HideInInspector] public GameObject m_bgmObj = null;   // 백그라운드 오브젝트
    [HideInInspector] public AudioSource m_bgmSrc = null;  // 백그라운드 AudioSource 컴포넌트
    private float m_bgmVolume = 0.2f;

    [HideInInspector] public GameObject m_GUI_SdObj = null;  // GUI 오브젝트
    [HideInInspector] public AudioSource m_GUI_SdSrc = null; // GUI AudioSource 컴포넌트
    private float m_GUIVolume = 0.2f;

    private int m_EffSdCount = 4;                   // 지금은 4개 레이어로 플레이...
    [HideInInspector] public int m_iSndCount = 0;   // 최대 4개까지 재생되게 제어 렉방지(Androud: 4개, PC: 무제한)
    [HideInInspector] public List<GameObject> m_sndObjList = new List<GameObject>(); //ArrayList m_sndObjList = new ArrayList();          // 효과음 오브젝트
    [HideInInspector] public AudioSource[] m_sndSrcList = new AudioSource[10];  // 넉넉히 만들어 놓는다.
    private float[] m_EffVolume = new float[10];

    [HideInInspector] public bool m_SoundOnOff = true;
    [HideInInspector] public float m_SoundVolume = 1.0f;

    AudioClip a_GAudioClip = null;

    // Start is called before the first frame update
    void Start()
    {
        for (int a_ii = 0; a_ii < 10; a_ii++)
        {
            m_EffVolume[a_ii] = 0.2f;
        }//for (int a_ii = 0; a_ii < 10; a_ii++)

        LoadChildGameObj();

        //---각종 초기값 로딩
        int a_SoundOnOff = PlayerPrefs.GetInt("SoundOnOff", 1);
        if (a_SoundOnOff == 0)
            SoundOnOff(false);
        else
            SoundOnOff(true);

        float a_SoundV = PlayerPrefs.GetFloat("SoundVolume", 1.0f);
        SoundVolume(a_SoundV);
        //---각종 초기값 로딩
    }

    //오디오클립을 가져오는 함수
    public void LoadAudioClip(string a_FileName, AudioClip a_AudioClip)
    {
        if (m_ADClipList.ContainsKey(a_FileName) == false)
        {
            m_ADClipList.Add(a_FileName, a_AudioClip);
        }
    }

    //사운드 매니저에 미리 사운드를 재생시켜줄 오브젝트를 생성하는 함수
    public void LoadChildGameObj()
    {
        //m_bgmObj == null 이면 PlayBGM()하게 되면 다시 로딩하게 된다. 
        if (m_bgmObj == null)
        {
            m_bgmObj = new GameObject();
            m_bgmObj.transform.SetParent(this.transform);
            m_bgmObj.transform.position = Vector3.zero;
            m_bgmSrc = m_bgmObj.AddComponent<AudioSource>();
            m_bgmSrc.playOnAwake = false;
            m_bgmObj.name = "BgmSoundOBJ";
        }

        // m_GUI_SdObj == null 이면 PlayGUISound()하게 되면 다시 로딩하게 된다. 
        if (m_GUI_SdObj == null)
        {
            m_GUI_SdObj = new GameObject();
            m_GUI_SdObj.transform.SetParent(this.transform);
            m_GUI_SdObj.transform.position = Vector3.zero;
            m_GUI_SdSrc = m_GUI_SdObj.AddComponent<AudioSource>();
            m_GUI_SdSrc.playOnAwake = false;
            m_GUI_SdSrc.loop = false;
            m_GUI_SdObj.name = "GUISoundOBJ";
        }

        for (int a_ii = 0; a_ii < m_EffSdCount; a_ii++)
        {
            // 최대 4개까지 재생되게 제어 렉방지(Androud: 4개, PC: 무제한)  
            if (m_sndObjList.Count < m_EffSdCount)
            {
                GameObject newSoundOBJ = new GameObject();
                newSoundOBJ.transform.SetParent(this.transform);
                newSoundOBJ.transform.localPosition = Vector3.zero;
                AudioSource a_AudioSrc =
                                    newSoundOBJ.AddComponent<AudioSource>();
                a_AudioSrc.playOnAwake = false;
                a_AudioSrc.loop = false;
                newSoundOBJ.name = "SoundEffObj";

                m_sndSrcList[m_sndObjList.Count] = a_AudioSrc;
                m_sndObjList.Add(newSoundOBJ);
            }
        }//for (int a_ii = 0; a_ii < m_EffSdCount; a_ii++)

        //---------------- 사운드 미리 로드
        a_GAudioClip = null;
        object[] temp = Resources.LoadAll("Sounds");
        for (int a_ii = 0; a_ii < temp.Length; a_ii++)
        {
            a_GAudioClip = temp[a_ii] as AudioClip;
            SoundManager.Instance.LoadAudioClip(a_GAudioClip.name, a_GAudioClip);
            //Debug.Log(a_GAudioClip.name);
        }
        //---------------- 사운드 미리 로드
    }

    //BGM을 재생하는 함수
    public void PlayBGM(string a_FileName, float fVolume = 0.8f)
    {
        a_GAudioClip = null;
        if (m_ADClipList.ContainsKey(a_FileName) == true)
        {
            a_GAudioClip = m_ADClipList[a_FileName] as AudioClip;
        }
        else
        {
            a_GAudioClip = Resources.Load("BGM/" + a_FileName) as AudioClip;
            m_ADClipList.Add(a_FileName, a_GAudioClip);
        }

        //Scene이 넘어가면 GameObject는 지워지고, m_bgmObj == null 이면 
        //PlayBGM()하게 되면 다시 로딩하게 된다. 
        if (m_bgmObj == null)
        {
            m_bgmObj = new GameObject();
            m_bgmObj.transform.SetParent(this.transform);
            m_bgmObj.transform.position = Vector3.zero;
            m_bgmSrc = m_bgmObj.AddComponent<AudioSource>();
            m_bgmSrc.playOnAwake = false;
            m_bgmObj.name = "BgmSoundOBJ";
        }

        if (a_GAudioClip != null && m_bgmSrc != null)
        {
            m_bgmSrc.clip = a_GAudioClip;
            m_bgmSrc.volume = fVolume * m_SoundVolume;
            m_bgmVolume = fVolume;
            m_bgmSrc.loop = true;
            m_bgmSrc.Play(0);
        }
    }

    //GUI사운드를 재생하는 함수
    public void PlayGUISound(string a_FileName, float fVolume = 0.2f)
    {
        if (m_SoundOnOff == false)
            return;

        a_GAudioClip = null;
        if (m_ADClipList.ContainsKey(a_FileName) == true)
        {
            a_GAudioClip = m_ADClipList[a_FileName] as AudioClip;
        }
        else
        {
            a_GAudioClip = Resources.Load("Sounds/" + a_FileName) as AudioClip;
            m_ADClipList.Add(a_FileName, a_GAudioClip);
        }

        //Scene이 넘어가면 GameObject는 지워지고, m_GUI_SdObj == null 이면 
        //PlayGUISound()하게 되면 다시 로딩하게 된다. 
        if (m_GUI_SdObj == null)
        {
            m_GUI_SdObj = new GameObject();
            m_GUI_SdObj.transform.SetParent(this.transform);
            m_GUI_SdObj.transform.position = Vector3.zero;
            m_GUI_SdSrc = m_GUI_SdObj.AddComponent<AudioSource>();
            m_GUI_SdSrc.playOnAwake = false;
            m_GUI_SdSrc.loop = false;
            m_GUI_SdObj.name = "GUISoundOBJ";
        }

        if (a_GAudioClip != null && m_GUI_SdSrc != null)
        {
            //m_GUI_SdSrc.clip = a_GAudioClip;
            //m_GUI_SdSrc.volume = fVolume;
            //m_GUI_SdSrc.loop = false;            
            m_GUI_SdSrc.PlayOneShot(a_GAudioClip, fVolume * m_SoundVolume);
            m_GUIVolume = fVolume;
        }
    }
    //이펙트 사운드를 재생하는 함수
    public void PlayEffSound(string a_FileName, float fVolume = 0.2f)
    {
        if (m_SoundOnOff == false)
            return;

        //fVolume = fVolume * m_SoundVolume;

        a_GAudioClip = null;
        if (m_ADClipList.ContainsKey(a_FileName) == true)
        {
            a_GAudioClip = m_ADClipList[a_FileName] as AudioClip;
        }
        else
        {
            a_GAudioClip = Resources.Load("Sounds/" + a_FileName) as AudioClip;
            m_ADClipList.Add(a_FileName, a_GAudioClip);
        }

        // 최대 4개까지 재생되게 제어 렉방지(Androud: 4개, PC: 무제한)  
        if (m_sndObjList.Count < m_EffSdCount)
        {
            GameObject newSoundOBJ = new GameObject();
            newSoundOBJ.transform.SetParent(this.transform);
            newSoundOBJ.transform.localPosition = Vector3.zero;
            AudioSource a_AudioSrc = newSoundOBJ.AddComponent<AudioSource>();
            a_AudioSrc.playOnAwake = false;
            a_AudioSrc.loop = false;
            newSoundOBJ.name = "SoundEffObj";

            m_sndSrcList[m_sndObjList.Count] = a_AudioSrc;
            m_sndObjList.Add(newSoundOBJ);
        }

        if (a_GAudioClip != null && m_sndSrcList[m_iSndCount] != null)
        {
            m_sndSrcList[m_iSndCount].clip = a_GAudioClip;
            m_sndSrcList[m_iSndCount].volume = fVolume * m_SoundVolume;
            m_sndSrcList[m_iSndCount].loop = false;
            m_sndSrcList[m_iSndCount].Play(0);
            //m_sndSrcList[m_iSndCount].PlayOneShot(a_GAudioClip, fVolume);
            m_EffVolume[m_iSndCount] = fVolume;

            m_iSndCount++;
            if (m_EffSdCount <= m_iSndCount)
                m_iSndCount = 0;
        }

    }//public void PlayEffSound(string a_FileName, float fVolume)

    //사운드의 온오프를 설정하는 함수
    public void SoundOnOff(bool a_OnOff = true)
    {
        bool a_MuteOnOff = !a_OnOff;

        if (m_bgmSrc != null)
        {
            m_bgmSrc.mute = a_MuteOnOff; //mute == true 끄기 mute == false 커지기
            if (a_MuteOnOff == false)
            {
                m_bgmSrc.time = 0;  //처음부터 다시 플레이
            }
        }

        if (m_GUI_SdSrc != null)
        {
            m_GUI_SdSrc.mute = a_MuteOnOff;
            if (a_MuteOnOff == false)
            {
                m_GUI_SdSrc.time = 0;  //처음부터 다시 플레이
            }
        }

        for (int a_ii = 0; a_ii < m_sndSrcList.Length; a_ii++)
        {
            if (m_sndSrcList[a_ii] != null)
            {
                m_sndSrcList[a_ii].mute = a_MuteOnOff;

                if (a_MuteOnOff == false)
                {
                    m_sndSrcList[a_ii].time = 0;  //처음부터 다시 플레이
                }
            }
        }

        m_SoundOnOff = a_OnOff;

    }//public void SoundOnOff(bool a_OnOff = true)

    //사운드의 볼륨을 조절해주는 함수
    //배경음은 지금 볼륨을 가져온 다음에 플레이 해 준다. 
    public void SoundVolume(float fVolume)
    {
        if (m_bgmSrc != null)
        {
            m_bgmSrc.volume = m_bgmVolume * fVolume;
        }

        if (m_GUI_SdSrc != null)
        {
            m_GUI_SdSrc.volume = m_GUIVolume * fVolume;
        }

        for (int a_ii = 0; a_ii < m_sndSrcList.Length; a_ii++)
        {
            if (m_sndSrcList[a_ii] != null)
            {
                m_sndSrcList[a_ii].volume = m_EffVolume[a_ii] * fVolume;
            }
        }

        m_SoundVolume = fVolume;
    }

}
