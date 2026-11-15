using UnityEngine;
using System.Collections;

// UIのアニメーション制御の関数をまとめたクラス
// 25/10/05 リファクタリング
public class UIAnimation : MonoBehaviour
{
    [Header("アニメーション")]
    [SerializeField] private Animator Anm_PausePanel;
    [SerializeField] private Animator Anm_ResumeButton;
    [SerializeField] private Animator Anm_ReturnTitleButton;
    [SerializeField] private Animator Anm_PauseText;
    [SerializeField] private Animator Anm_StartPanel;
    [SerializeField] private Animator Anm_WaveClearPanel;

    // ポーズパネル表示・非表示
    public void PausePanelAnimation(bool isShow)
    {
        if(isShow)
        {
            Anm_PausePanel.SetTrigger("Show");
            Anm_ResumeButton.SetTrigger("Show");
            Anm_ReturnTitleButton.SetTrigger("Show");
            Anm_PauseText.SetTrigger("Show");
        }
        else
        {
            Anm_PausePanel.SetTrigger("Hide");
            Anm_ResumeButton.SetTrigger("Hide");
            Anm_ReturnTitleButton.SetTrigger("Hide");
            Anm_PauseText.SetTrigger("Hide");
        }
    }

    // スタートパネルのフェードアウト
    public void StartPanelAnimation()
    {
        Anm_StartPanel.SetTrigger("StartScene");
    }

    // スタートパネルのフェードインコルーチン
    public IEnumerator StartPanelReverseAnimation()
    {
        Anm_StartPanel.SetTrigger("SceneChange");
        yield return new WaitForSeconds(0.5f); // アニメーション中  
        UnityEngine.SceneManagement.SceneManager.LoadScene("Title");
    }

    // ウェーブクリアパネルのアニメーション
    public void WaveClearPanelAnimation(bool isActive)
    {
        if (isActive)
        {
            Anm_WaveClearPanel.SetTrigger("WaveClear");
        }
        else
        {
            Anm_WaveClearPanel.SetTrigger("HideWaveClearPanel");
        }
    }
}

