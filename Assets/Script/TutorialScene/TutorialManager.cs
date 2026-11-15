using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    [Header("スクリプト参照")]
    public TutorialUI UI;
    public new AudioManager audio;

    [Header("UI要素")]
    public TextMeshProUGUI Txt_Tutorial;
    public TextMeshProUGUI Txt_Page;
    public Animator Anm_panelFeed;

    [Header("変数")]
    [SerializeField] private string[] tutorialTexts;
    [SerializeField] private int currentPage = 0;
    [SerializeField] private int totalPages;
    //public bool isCamShake = true;
    private float dpadX; // 十字キーの左右入力
    private float prevDpadX; // 前フレームの十字キーの左右入力

    // PlayerPrefsのキー定数
    public const string BGMVolume = "BGMVolume";
    public const string UIVolume = "UIVolume";
    public const string FireVolume = "FireVolume";
    public const string ResolutionKey_w = "ScreenWidth";
    public const string ResolutionKey_h = "ScreenHeight";
    public const string CamShakeKey = "CamShake";
    public enum TutorialoMode
    {
        START,
        MOVE,
        PAUSE,
    }
    public TutorialoMode mode;

    private void Start()
    {
        ApplySettings();
        tutorialTexts = new string[] {
            "チュートリアルへようこそ\nこのモードでは、ゲームのルールや\n操作方法について説明します。",
            "ゲームの目的は、敵を倒しながら\n自分のキャラクターを強化し、\nより多くの敵を倒してスコアを稼ぐことです",
            "一定時間生存することでWevwクリアとなり、\nWaveをクリアするごとにバフを1つ選択し\nプレイヤーを強化できます",
            "基本的な操作方法は以下の通りです:\n\n移動 : W,A,S,D / 左スティック\nダッシュ:左SHIFT / R1\nジャンプ : Space / ×\n射撃:左マウスクリック / R2\n射撃モード切り替え:B / L1\n" +
            "リロード:R / □\n一時停止:Tab / Option\n※ゲームパッドはDualShock4 推奨",
            "早速操作を試してみましょう！\n\n",
            ""
        };
        totalPages = tutorialTexts.Length;
        currentPage = 0;
        UpdateText();
        mode = TutorialoMode.START;
        Anm_panelFeed.SetTrigger("StartScene");
        //isCamShake = PlayerPrefs.GetInt(CamShakeKey, 1) == 1; // PlayerPrefsからカメラシェイクの設定を取得
    }

    private void Update()
    {

        switch (mode)
        {
            case TutorialoMode.START:
                // ページの入力を受け付ける
                InputNextText();
                if(Input.GetKeyDown(KeyCode.X) || Input.GetButtonDown("Pause"))
                {
                    TutorialSkip();
                }
                break;

            case TutorialoMode.MOVE:
                // Tab or Optionで一時停止
                if (Input.GetKeyDown(KeyCode.Tab) || Input.GetButtonDown("Pause"))
                {
                    mode = TutorialoMode.PAUSE;
                    audio.PlayUIClick();
                    UI.PauseUI(true);
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
                // enter or R3でゲーム開始
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetButtonDown("FinishTutotial")) 
                {
                    Anm_panelFeed.SetTrigger("SceneChange");
                    audio.PlayUIClick();
                    UI.Txt_Page.text = "シーンをロード中...";
                    StartCoroutine(WaitUIAnimation("Play"));
                }
                break;
            case TutorialoMode.PAUSE:
                // Tab or Optionで再開
                if (Input.GetKeyDown(KeyCode.Tab) || Input.GetButtonDown("Pause"))
                {
                    mode = TutorialoMode.MOVE;
                    audio.PlayUIClick();
                    UI.PauseUI(false);
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
                break;
        }
    }

    public void InputNextText()
    {
        dpadX = Input.GetAxis("Page"); // 十字左右
        if (Input.GetKeyDown(KeyCode.D) || GetInputAxis(dpadX,false))
        {
            audio.PlayUIClick();
            NextPage();
            UpdateText();
            // 最後のページに到達したら、状態をMOVEに変更
            if (currentPage >= totalPages - 1)
            {
                mode = TutorialoMode.MOVE;
                Txt_Page.text = "Enter / R3:ゲーム開始";
                UI.HUD(true);
                UI.Pnl_TutorialText.SetActive(false);
            }
        }
        // A or 十字左 and currentPage > 0で前のページへ
        else if (Input.GetKeyDown(KeyCode.A) || GetInputAxis(dpadX, true)
            && currentPage > 0)
        {
            audio.PlayUIClick();
            PreviousPage();
            UpdateText();
        }
        prevDpadX = dpadX; // 今フレームの値を保存
    }

    public void NextPage()
    {
        if (currentPage < totalPages - 1)
        {
            currentPage++;
        }
    }

    public void PreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
        }
        else
        {
            currentPage = totalPages - 1; // 最初のページから最後のページに戻る
        }
    }

    public void TutorialSkip()
    {
        currentPage = totalPages - 1;
        UpdateText();
        mode = TutorialoMode.MOVE;
        Txt_Page.text = "Enter / R3:ゲーム開始";
        UI.HUD(true);
        UI.Pnl_TutorialText.SetActive(false);
        audio.PlayUIClick();
    }


    public void UpdateText()
    {
        Txt_Tutorial.text = tutorialTexts[currentPage];
        Txt_Page.text = "A / ← :戻る D / → :進む" + (currentPage + 1) + "/" + totalPages;
    }

    /// <summary>
    /// 入力された値が前フレームから変化したかどうかをチェックするメソッド。
    /// Downの入力検出に使用
    /// </summary>
    /// <param name="value">今フレームの入力値</param>
    /// <returns>前フレームで入力されていなければtrue</returns>
    public bool GetInputAxis(float value, bool isMinus)
    {
        if (!isMinus) // 正の値のとき(右方向)
        {
            if (value > 0.5f && prevDpadX <= 0.5f) {
                return true;
            } 
            else {
                return false;
            }
        }
        else // 負の値のとき(左方向)
        {
            if (value < -0.5f && prevDpadX >= -0.5f) {
                return true;
            }
            else {
                return false;
            }
        }
    }

    // UIアニメーションが終わるのを待ってからシーンをロード
    public IEnumerator WaitUIAnimation(string sceneName)
    {
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(sceneName);
    }


    // 設定内容を反映(音量設定はAudioManagerで行う)
    public void ApplySettings()
    {
        // カメラシェイクの有効/無効を設定
        //isCamShake = PlayerPrefs.GetInt(CamShakeKey, 1) == 1;
    }

}
