using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class TitleManager : MonoBehaviour
{
    public Text m_TitleTxt = null;
    public GameObject BtnGroup = null;
    public GameObject m_ExPlainPanel = null;
    public GameObject m_TitlePanel = null;

    public Button m_StartBtn = null;
    public Button m_ExplainBtn = null;
    public Button m_ExplainCloseBtn = null;

    // Start is called before the first frame update
    void Start()
    {
        SoundManager.Instance.PlayBGM("Title_BGM", 0.1f);

        if (m_StartBtn != null)
            m_StartBtn.onClick.AddListener(() =>
            {
                StartBtn();
            });

        if (m_ExplainBtn != null)
            m_ExplainBtn.onClick.AddListener(() =>
            {
                Explain();
            });

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
