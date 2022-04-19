using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class LoadingManager : MonoBehaviour
{
    static string m_nextScene;      //넘어갈 씬의 이름

    [SerializeField]
    Image m_progressBar;          //로딩바

    public static void LoadScene(string a_sceneName)
    {
        m_nextScene = a_sceneName;
        SceneManager.LoadScene("02.LoadingScene");
    }
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(LoadSceneProcess());
    }

    //로딩창을 구현하는 코루틴
    IEnumerator LoadSceneProcess()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(m_nextScene);

        op.allowSceneActivation = false;//로딩되지 않은 오브젝트들이 깨져보이는걸 방지하기 위함

        float a_time = 0.0f;
        while(!op.isDone)
        {
            yield return null;

            if(op.progress < 0.9f)
            {
                m_progressBar.fillAmount = op.progress;
            }
            else
            {
                a_time += Time.deltaTime;
                m_progressBar.fillAmount = Mathf.Lerp(0.9f, 1f, a_time);

                if(m_progressBar.fillAmount >= 1.0f)
                {
                    op.allowSceneActivation = true;
                    yield break;
                }
            }
        }
    }
}
