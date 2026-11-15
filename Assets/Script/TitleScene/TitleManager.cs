using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

/*
 Playerの挙動はPlaySceneと同じスクリプトで制御する(TutorialとPlayで条件分岐させる)
 Item, DummyはTutorial独自のスクリプトで制御する
*/

public class TitleManager : MonoBehaviour
{
    [Header("オブジェクト参照")]
    public Button StartButton;
    public Button TutorialButton;
    public Button QuitButton;
    public Button SettingsButton;
    public Button SettingsButton_Setmode; // 設定モードでの戻るボタン
    public TextMeshProUGUI Txt_Loading;

    public Animator panelFeed;

    [Header("スクリプト参照")]
    public SettingManager settingManager; 
    public TitleUISelector uiSelector; 
    public enum TitleState
    {
        Title, // タイトル画面
        Settings, // 設定画面
    }
    public TitleState CurrentState = TitleState.Title;

    // クラスの先頭にフィールドを追加
    public static string PreviousSceneName = "";

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        CurrentState = TitleState.Title;
        settingManager.SwitchSettingUI();
        Txt_Loading.text = "";

        StartButton.onClick.AddListener(() => LoadSceneAsync("Play"));
        TutorialButton.onClick.AddListener(() => LoadSceneAsync("Tutorial"));
        QuitButton.onClick.AddListener(QuitGame);
        SettingsButton.onClick.AddListener(SwitchSettingMode); // 設定ボタンのクリックイベントを設定
        SettingsButton_Setmode.onClick.AddListener(SwitchSettingMode); // 設定モードでの戻るボタンのクリックイベントを設定
        ShowPanelFeedAfterSceneChange();
    }

    // 他のシーンからの遷移時のみパネルフィードを表示
    private void ShowPanelFeedAfterSceneChange()
    {
        string PreviousSceneName = SceneTransitionTracker.PreviousSceneName; // 遷移元のシーン名を取得
        if (PreviousSceneName == "Play" || PreviousSceneName == "Tutorial")
        {
            panelFeed.SetTrigger("StartScene");
        }
    }

    // ゲームを終了する
    public void QuitGame()
    {
        Application.Quit();
    }

    // Asyncを使用してシーンを非同期でロードする場合の例
    public void LoadSceneAsync(string sceneName)
    {
        panelFeed.SetTrigger("SceneChange");
        StartCoroutine(WaitUIAnimation(sceneName));
    }

    // UIアニメーションが終わるのを待ってからシーンをロード
    private IEnumerator WaitUIAnimation(string sceneName)
    {
        yield return new WaitForSeconds(0.5f);
        Txt_Loading.text = "シーンをロード中...";
        StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    private System.Collections.IEnumerator LoadSceneCoroutine(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            // ロード進行状況を表示する場合はここで更新
            yield return null;
        }
        Txt_Loading.text = ""; // ロード完了後にテキストをクリア
        Debug.Log($"{sceneName}を非同期でロードしました");
    }

    /// <summary>
    ///  Title, Settings
    /// </summary>
    /// <param name="mode"></param>
    public void SwitchSettingMode()
    {
        switch(CurrentState)
        {
            case TitleState.Title:
                CurrentState = TitleState.Settings;
                settingManager.SetSlider(); // 設定を読み込む
                settingManager.SwitchSettingUI(); // 設定UIの表示/非表示を切り替える
                uiSelector.OpenSettings(); // UIの選択状態を設定画面用に切り替える
                break;

            case TitleState.Settings:
                CurrentState = TitleState.Title;
                settingManager.SaveVolumeSettings();
                settingManager.SwitchSettingUI();
                uiSelector.CloseSettings();
                break;
        }
    }

}
