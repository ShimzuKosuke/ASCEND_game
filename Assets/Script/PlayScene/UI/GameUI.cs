using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class GameUI : MonoBehaviour
{
    [Header("スクリプト参照")]
    [SerializeField] private GameManager manager;
    [SerializeField] private PlayerManager player;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private UISelector uiSelector;
    [SerializeField] private FPSCounter fps;
    [SerializeField] private UIAnimation uiAnimation;

    [Header("コンポーネント参照")]
    [SerializeField] private TextMeshProUGUI Txt_ammo;
    [SerializeField] private TextMeshProUGUI Txt_Score;
    [SerializeField] private TextMeshProUGUI Txt_Time;
    [SerializeField] private TextMeshProUGUI Txt_HP;
    [SerializeField] private TextMeshProUGUI Txt_Stamina;
    [SerializeField] private TextMeshProUGUI Txt_Wave;
    public TextMeshProUGUI Txt_Level;
    [SerializeField] private TextMeshProUGUI Txt_Debug;
    [SerializeField] private TextMeshProUGUI Txt_GameOver;
    [SerializeField] private TextMeshProUGUI Txt_NextPage;
    [SerializeField] private TextMeshProUGUI Txt_FPS;
    [SerializeField] private Button Btn_Resume;            // イベント割り当てに使用
    [SerializeField] private Button Btn_ReturnTitle;       // イベント割り当てに使用
    [SerializeField] private TextMeshProUGUI Txt_Status1;  // 画面左部に表示
    [SerializeField] private TextMeshProUGUI Txt_Status2;  // 画面右部に表示
    [SerializeField] private TextMeshProUGUI Txt_Result;   // 画面左部に表示
    [SerializeField] private TextMeshProUGUI Txt_Result2;  // 画面右部に表示
    [SerializeField] private TextMeshProUGUI Txt_PageName; // 画面上部に表示
    [SerializeField] private TextMeshProUGUI Txt_Ranking;
    [SerializeField] private TextMeshProUGUI Txt_FireModeChange; // オート/セミオート/バースト切り替え表示
    [SerializeField] private TextMeshProUGUI Txt_CharageLv;      // チャージショットのレベル表示
    [SerializeField] private TextMeshProUGUI Txt_Pause;
    [SerializeField] private Button Btn_Next; // waveclear > buff選択へ

    // Canvas参照(モードごとにCanvasを分けて管理)
    [Header("Canvas参照")]
    [SerializeField] private Canvas PlayCanvas;
    [SerializeField] private Canvas PauseCanvas;
    [SerializeField] private Canvas ResultCanvas;
    [SerializeField] private Canvas StandbyCanvas;
    [SerializeField] private Canvas StopCanvas; // buff選択中
    [SerializeField] private Canvas PanelFeedCanvas; // シーン遷移時のフェード用

    [Header("Canvasの初期状態")]
    [SerializeField] private bool playCanvasAwake = false;

    // 内部変数
    private bool isDebugMode = false;
    private int ResultPage = 0;
    private const float UIUpdateInterval = 0.5f;  // UI更新の間隔(秒)
    private const float DebugUpdateInterval = 1f; // デバッグログ更新の間隔(秒)
    private float No1 = 0f; // ランキング1位
    private float No2 = 0f;
    private float No3 = 0f;
    private float No4 = 0f;
    private float No5 = 0f;

    // PlayerPrefsのキー定数
    private const string Ranking1 = "Ranking1";
    private const string Ranking2 = "Ranking2";
    private const string Ranking3 = "Ranking3";
    private const string Ranking4 = "Ranking4";
    private const string Ranking5 = "Ranking5";

    void Start()
    {
        StartCoroutine(UpdateDebugLog());
        StartCoroutine(UpdateUISecond());

        Btn_ReturnTitle.onClick.AddListener(ReturnTitle);
        Btn_Resume.onClick.AddListener(() => { PauseUI(false);});
        hideSetFireModeCange();

        // 非表示で開始してもいいように全てのCanvasを有効化しておく
        PlayCanvas.gameObject.SetActive(true);
        PauseCanvas.gameObject.SetActive(true);
        ResultCanvas.gameObject.SetActive(true);
        StandbyCanvas.gameObject.SetActive(true);
        StopCanvas.gameObject.SetActive(true);
        PanelFeedCanvas.gameObject.SetActive(true);
    }

    private void OnEnable()
    {
        if(PanelFeedCanvas != null && !PanelFeedCanvas.isActiveAndEnabled)
        {
            PanelFeedCanvas.gameObject.SetActive(true);
        }
        uiAnimation.StartPanelAnimation();
    }

    /*
     変更内容
    ・Canvasをゲームモードごとに分けて管理し、モードに対応するCanvasのみを表示するように変更
    ・一部の変動するUIを除き、UI自体の表示/非表示やテキストクリアは行わない
    ・UI管理を行う関数の見直し

    UISelectorとの兼ね合いで、Pause以外のモードでもBtn_Resameが選択されてしまう問題がある。
    mode == PAUSEのときのみ実行するようにしているが、根本的な解決にはなっていない。
    */

    /// <summary>
    /// ゲームモードに応じてCanvasを切り替える
    /// </summary>
    public void shiftCanvas()
    {
        switch (manager.mode)
        {
            case GameManager.Mode.STANDBY: // StandbyCanvas
                StandbyCanvas.enabled = true;

                PlayCanvas.enabled   = false;
                PauseCanvas.enabled  = false;
                ResultCanvas.enabled = false;
                StopCanvas.enabled   = false;
                break;

            case GameManager.Mode.PLAY: // PlayCanvas
            case GameManager.Mode.WAVECLEAR: // PlayCanvas
                PlayCanvas.enabled = true;
                UpdatePlayUI();

                PauseCanvas.enabled   = false;
                ResultCanvas.enabled  = false;
                StandbyCanvas.enabled = false;
                StopCanvas.enabled    = false;
                break;

            case GameManager.Mode.PAUSE: // PauseCanvas
                PauseCanvas.enabled = true;

                PlayCanvas.enabled    = false;
                ResultCanvas.enabled  = false;
                StandbyCanvas.enabled = false;
                StopCanvas.enabled    = false;

                break;

            case GameManager.Mode.STOP: // StopCanvas
                StopCanvas.enabled = true;

                StandbyCanvas.enabled = false;
                PlayCanvas.enabled    = false;
                PauseCanvas.enabled   = false;
                ResultCanvas.enabled  = false;
                break;

            case GameManager.Mode.GAMEOVER: // ResultCanvas
                ResultCanvas.enabled = true;

                PlayCanvas.enabled    = false;
                PauseCanvas.enabled   = false;
                StandbyCanvas.enabled = false;
                StopCanvas.enabled    = false;
                break;
        }
    }

    void Update()
    {
        shiftCanvas();
    }

    /// <summary>
    /// テキストの色を割合に応じて変更する
    /// </summary>
    /// <param name="text">色を変更するTextMeshProUGUIオブジェクト</param>
    /// <param name="current">現在の値</param>
    /// <param name="max">最大値(割合計算用)</param>
    public void SetTextColor(TextMeshProUGUI text, int current, int max)
    {
        float percent = (float)current / max * 100f;

        if (percent <= 25) // <=25%
        {
            text.color = Color.red;
        }
        else if (percent <= 50)
        {
            text.color = Color.yellow;
        }
        else
        {
            text.color = Color.white;
        }
    }

    /// <summary>
    /// UIの更新を行う(UIUpdateInterval秒ごと)。
    /// UpdateUISecondコルーチンから呼び出される。
    /// </summary>
    private void UpdateUI()
    {
        if (manager.mode == GameManager.Mode.PLAY)
        {
            Txt_Time.text = manager.timeLimit.ToString("f0") + "s";
            SetTextColor(Txt_Time, (int)manager.timeLimit, (int)manager.timeLimitDefault);
            Txt_FPS.text = "FPS: " + fps.CurrentFPS.ToString("f0")
                + " MAX:" + fps.MAXFPS.ToString("f0")
                + " AVG:" + fps.AvgFPS.ToString("f0");
        }
    }

    /// <summary>
    /// UIの更新を行うコルーチン
    /// </summary>
    private IEnumerator UpdateUISecond()
    {
        while (true)
        {
            UpdateUI();
            yield return new WaitForSeconds(UIUpdateInterval);
        }
    }

    /// <summary>
    /// ゲームオーバーUIを非表示にする
    /// </summary>
    public void HideGameOverUI()
    {
        ResultCanvas.enabled = false;
        ResultPage = 0;
    }

    /// <summary>
    /// ゲームオーバーUIの表示
    /// </summary>
    /// <param name="pageNum">表示するページ番号</param>
    public void GameOverUI(int pageNum)
    {
        ResultCanvas.enabled = true;
        switch (pageNum)
        {
            case 0:
                Txt_GameOver.text = "GAME OVER！"; 
                Txt_NextPage.text = "× / F : 次のページ";
                break;

            case 1:
                Txt_Result.text = "";
                Txt_GameOver.text = "";
                ToggleStatusPanel(true);
                Txt_PageName.text = "ステータス一覧";
                Txt_NextPage.text = "× / F : 次のページ";
                break;
                
            case 2:
                ToggleStatusPanel(false);
                Txt_Result.text = "Score: " + manager.score.ToString("f0") +
                "\nWave: " + manager.waveCount.ToString("f0") +
                "\nLevel: " + player.level;
                Txt_Ranking.text = "ランキング";
                Txt_Result2.text =
                    "1位:  " + No1.ToString("f0") + "\n" +
                    "2位:  " + No2.ToString("f0") + "\n" +
                    "3位:  " + No3.ToString("f0") + "\n" +
                    "4位:  " + No4.ToString("f0") + "\n" +
                    "5位:  " + No5.ToString("f0");
                Txt_PageName.text = "リザルト";
                Txt_NextPage.text = "〇 / SHIFT : もう一度プレイ  × / F : タイトルに戻る";
                Txt_GameOver.text = "リザルト画面";
                Txt_GameOver.text = "";
                break;

        }
    }

    /// <summary>
    /// ステータス一覧の表示/非表示
    /// </summary>
    /// <param name="show">表示するかどうか</param>
    public void ToggleStatusPanel(bool show)
    {
        if (show)
        {
            Txt_Status1.text = "レベル : " + player.level +
                "\nHP : " + player.MaxHP +
                "\n攻撃力 : " + player.shoot.ATK +
                "\nクリティカル率 : " + player.shoot.CriticalRate + "%" +
                "\n速度 : " + player.move.moveSpeed +
                "\nダッシュ速度 : " + player.move.dashSpeed +
                "\nスタミナ : " + player.move.StaminaMax +
                "\nジャンプ力 : " + player.move.jumpForce +
                "\nHP自動回復 : " + player.HPRegenRate;

            Txt_Status2.text = "ダメージ軽減 : " + player.DamageReduction +
                "\n経験値倍率 : " + player.expMultiplier + "倍" +
                "\n連射速度 : " + player.shoot.fireRate + "s" +
                "\nマガジンサイズ : " + player.shoot.magazineSize +
                "\nリロード時間 : " + player.shoot.reloadTime + "s" +
                "\n射程距離 : " + player.shoot.range +
                "\n反動の大きさ : " + player.shoot.recoilAngle + "°" + 
                "\nチャージ速度倍率 : " + player.shoot.chargeSpeed + "倍" +
                "\n最大チャージ貫通数 : " + player.shoot.hitLimit + "体" +
                "\nバースト射撃間隔 : " + player.shoot.burstInterval + "s";
        }
        else
        {
            Txt_Status1.text = ""; 
            Txt_Status2.text = ""; 
        }
    }

    /// <summary>
    /// ポーズUIの表示/非表示を切り替える
    /// </summary>
    /// <param name="show">表示状態</param>
    // shiftCanvvas()に移行も検討
    public void PauseUI(bool show)
    {
        Cursor.visible = show; 
        uiAnimation.PausePanelAnimation(show);
        switch (show)
        {
            case true:
                Cursor.lockState = CursorLockMode.None;

                // 演出のため個別にUIを表示(Canvas全体の表示/非表示ではUIが表示されないため)
                Btn_Resume.gameObject.SetActive(true);
                Btn_ReturnTitle.gameObject.SetActive(true);
                Txt_Pause.gameObject.SetActive(true);

                uiSelector.FirstSelectUI(Btn_Resume.gameObject);
                audioManager.PlayUIClick();
                break;

            case false:
                // EventSystemによりPlay中にもBtn_Resameが選択されてしまう問題の対策
                if (manager.mode != GameManager.Mode.PAUSE) return;

                // 演出のため先にUIを非表示にする
                //Txt_Pause.gameObject.SetActive(false);

                StartCoroutine(WaitAndResume(0.3f)); //アニメーションの時間待機
                audioManager.PlayUIClick();
                Cursor.lockState = CursorLockMode.Locked;
                player.move.SetJumpLock(); // SubmitとJumpの競合を防ぐ
                break;
        }
    }

    /// <summary>
    /// タイトルに戻るボタンが押されたときの処理
    /// </summary>
    public void ReturnTitle()
    {
        if (manager.mode == GameManager.Mode.PAUSE)
        {
            audioManager.PlayUIClick();
            StartCoroutine(uiAnimation.StartPanelReverseAnimation()); // UIアニメーション→タイトル遷移
        }
    }

    /// <summary>
    /// ランキングの更新
    /// </summary>
    /// <param name="score">スコア</param>
    public void UpdateRanking(float score)
    {
        No1 = PlayerPrefs.GetFloat(Ranking1, 0f);
        No2 = PlayerPrefs.GetFloat(Ranking2, 0f);
        No3 = PlayerPrefs.GetFloat(Ranking3, 0f);
        No4 = PlayerPrefs.GetFloat(Ranking4, 0f);
        No5 = PlayerPrefs.GetFloat(Ranking5, 0f);
        // ランキングの更新
        if (score > No1)
        {
            No5 = No4;
            No4 = No3;
            No3 = No2;
            No2 = No1;
            No1 = score;
        }
        else if (score > No2)
        {
            No5 = No4;
            No4 = No3;
            No3 = No2;
            No2 = score;
        }
        else if (score > No3)
        {
            No5 = No4;
            No4 = No3;
            No3 = score;
        }
        else if (score > No4)
        {
            No5 = No4;
            No4 = score;
        }
        else if (score > No5)
        {
            No5 = score;
        }
        // PlayerPrefsに保存
        PlayerPrefs.SetFloat(Ranking1, No1);
        PlayerPrefs.SetFloat(Ranking2, No2);
        PlayerPrefs.SetFloat(Ranking3, No3);
        PlayerPrefs.SetFloat(Ranking4, No4);
        PlayerPrefs.SetFloat(Ranking5, No5);
    }

    /// <summary>
    /// ランキングの色を更新
    /// </summary>
    /// <param name="score">スコア</param>
    public void UpDateRankingColor(float score)
    {
        // 赤 = #FF0000
        if (score >= No1)
        {
            Txt_Result2.text = "1位:  <color=#FF0000>" + No1.ToString("f0") + "</color>" + " 新記録!\n" + "2位:  " + No2.ToString("f0") + "\n" + "3位:  " + No3.ToString("f0") + "\n" + "4位:  " + No4.ToString("f0") + "\n" + "5位:  " + No5.ToString("f0");
        }
        else if(score >= No2)
        {
            Txt_Result2.text = "1位:  " + No1.ToString("f0") + "\n" + "2位:  <color=#FF0000>" + No2.ToString("f0") + "</color>" + " 新記録!\n" + "3位:  " + No3.ToString("f0") + "\n" + "4位:  " + No4.ToString("f0") + "\n" + "5位:  " + No5.ToString("f0");
        }
        else if (score >= No3)
        {
            Txt_Result2.text = "1位:  " + No1.ToString("f0") + "\n" + "2位:  " + No2.ToString("f0") + "\n" + "3位:  <color=#FF0000>" + No3.ToString("f0") + "</color>" +  "新記録!\n" + "4位:  " + No4.ToString("f0") + "\n" + "5位:  " + No5.ToString("f0");
        }
        else if (score >= No4)
        {
            Txt_Result2.text = "1位:  " + No1.ToString("f0") + "\n" + "2位:  " + No2.ToString("f0") + "\n" + "3位:  " + No3.ToString("f0") + "\n" + "4位:  <color=#FF0000>" + No4.ToString("f0") + "</color>" + "新記録!\n" +"5位:  " + No5.ToString("f0");
        }
        else if (score >= No5)
        {
            Txt_Result2.text = "1位:  " + No1.ToString("f0") + "\n" + "2位:  " + No2.ToString("f0") + "\n" + "3位:  " + No3.ToString("f0") + "\n" + "4位:  " + No4.ToString("f0") + "\n" + "5位: <color=#FF0000>" + No5.ToString("f0") + "</color>" + "新記録!";
        }
    }

    /// <summary>
    /// Debug用のテキストを更新
    /// </summary>
    public void UpdateDebugText()
    {
        Txt_Debug.text = "Debug View\n" +
        "MODE: " + manager.mode.ToString() + "\n" +
        "Pos : " + player.transform.position + "\n" +
        "PlayerLv:" + player.level + "\n" +
        "Next: " + (player.levelUpThreshold - manager.score) + "\n" +
        "ResultPage: " + ResultPage + "\n" +
        "EnemySpawnInterval: " + enemySpawner.spawnInterval + "\n";
    }

    /// <summary>
    /// デバッグログの更新を行うコルーチン
    /// </summary>
    private IEnumerator UpdateDebugLog()
    {
        while (true && Txt_Debug != null)
        {
            if(isDebugMode)
            {
                UpdateDebugText();
            }
            else
            {
                Txt_Debug.text = ""; // デバッグビューがオフの場合はテキストをクリア
            }
            yield return new WaitForSeconds(DebugUpdateInterval);
        }
    }

    /// <summary>
    /// プレイ中のUIを更新
    /// </summary>
    public void UpdatePlayUI()
    {
        // デバッグビューの表示状態を切り替え
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isDebugMode = !isDebugMode;
        }

        if (!player.shoot.isReloading) // リロード中でないとき
        {
            Txt_ammo.text = player.shoot.currentAmmo + "/" + player.shoot.magazineSize;
            SetTextColor(Txt_ammo, player.shoot.currentAmmo, player.shoot.currentAmmo);
        }
        else // リロード中
        {
            Txt_ammo.text = "Reloading...";
            Txt_ammo.color = Color.white;
        }
        Txt_Score.text = "Score: " + manager.score.ToString("f0");
        Txt_HP.text = player.HP.ToString("f0") + " / " + player.MaxHP;
        Txt_Wave.text = "Wave: " + manager.waveCount.ToString("f0");
        Txt_Level.text = "Lv. " + player.level.ToString("f0");
        Txt_Stamina.text = player.move.Stamina.ToString("f0") + " / " + player.move.StaminaMax;
        showSetChargeLv(); // チャージショットレベルの表示(セミオート時のみ有効)
    }

    /// <summary>
    /// 射撃モード変更の表示
    /// </summary>
    public void showSetFireModeCange()
    {
        Txt_FireModeChange.gameObject.SetActive(true);
        switch (player.shoot.fireMode)
        {
            case PlayerShooting.FireMode.FULLAUTO:
                Txt_FireModeChange.text = "フルオート";
                break;

            case PlayerShooting.FireMode.BURST:
                Txt_FireModeChange.text = "バースト";
                break;

            case PlayerShooting.FireMode.CHARGE:
                Txt_FireModeChange.text = "チャージ";
                break;
        }
        Invoke("hideSetFireModeCange", 2f);
    }

    /// <summary>
    /// 射撃モード変更の非表示
    /// </summary>
    public void hideSetFireModeCange()
    {
        Txt_FireModeChange.gameObject.SetActive(false);
    }

    /// <summary>
    /// チャージショットレベルの表示
    /// </summary>
    public void showSetChargeLv()
    {
        if (player.shoot.chargeLevel == 0) Txt_CharageLv.text = "";
        else if (player.shoot.chargeLevel == 1)
        {
            Txt_CharageLv.text = "チャージ Lv.1";
            Txt_CharageLv.color = Color.white;
        }
        else if (player.shoot.chargeLevel == 2)
        {
            Txt_CharageLv.text = "チャージ Lv.2";
            Txt_CharageLv.color = Color.yellow;
        }
        else if (player.shoot.chargeLevel == 3)
        {
            Txt_CharageLv.text = "チャージ Lv.MAX!!!";
            Txt_CharageLv.color = Color.red;
        }
    }

    /// <summary>
    /// 一定時間待機し、ポーズ解除
    /// </summary>
    /// <param name="waitTime">待機時間</param>
    private IEnumerator WaitAndResume(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        manager.mode = GameManager.Mode.PLAY;
        Debug.Log("Game Resumed");
    }

    /// <summary>
    /// 次のページへ進むボタンを選択状態にする
    /// </summary>
    public void NextPageSetUISelect()
    {
        uiSelector.FirstSelectUI(Btn_Next.gameObject);
    }
}
