using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class TitleManager : MonoBehaviour
{
    public Text m_TitleTxt = null;                  //타이틀
    public GameObject BtnGroup = null;              //스타트 버튼과 설명 버튼
    public GameObject m_ExPlainPanel = null;        //설명 판넬
    public GameObject m_TitlePanel = null;          //타이틀 판넬

    public Button m_StartBtn = null;                //스타트 버튼
    public Button m_ExplainBtn = null;              //설명 버튼
    public Button m_ExplainCloseBtn = null;         //설명 닫기 버튼

    // Start is called before the first frame update
    void Start()
    {
        SoundManager.Instance.PlayBGM("Title_BGM", 0.1f);

        //스타트 버튼
        if (m_StartBtn != null)
            m_StartBtn.onClick.AddListener(() =>
            {
                StartBtn();
            });

        //설명 버튼
        if (m_ExplainBtn != null)
            m_ExplainBtn.onClick.AddListener(() =>
            {
                Explain();
            });

        //설명 나가기 버튼
        if (m_ExplainCloseBtn != null)
            m_ExplainCloseBtn.onClick.AddListener(() =>
            {
                ExplainClose();
            });
    }

    // Update is called once per frame
    void Update()
    {
        if (m_TitleTxt.GetComponent<Text>().fontSize < 190)//폰트 사이즈가 190보다 작다면
        {
            m_TitleTxt.GetComponent<Text>().fontSize += 1 + (int)Time.deltaTime;//폰트 사이즈를 점점 키운다

            if (m_TitleTxt.GetComponent<Text>().fontSize >= 190)//폰트 사이즈가 190과 같거나 크다면
            {
                m_TitleTxt.GetComponent<Text>().fontSize = 190;
                BtnGroup.SetActive(true);
            }
        }
    }

    //게임씬으로 이동
    void StartBtn()
    {
        SceneManager.LoadScene("02.InGame");//인게임 씬으로 이동
    }

    //설명 패널 열기
    void Explain()
    {
        m_ExPlainPanel.SetActive(true);
        m_TitlePanel.SetActive(false);
    }

    //설명 패널 닫기
    void ExplainClose()
    {
        m_ExPlainPanel.SetActive(false);
        m_TitlePanel.SetActive(true);
    }
}
