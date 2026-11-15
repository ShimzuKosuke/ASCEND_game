using UnityEngine;

/* 設定の読み込み箇所
BGM, UI, SE                      : AudioManager.Start() (play, tutorial)
Resolution, CamShake             : SettingManager.Start() (title)
MauseSensitivity, PadSensitivity : PlayerPOV.Initialize() (play, tutorial)
rankings                         : GameUI.UpdateRanking() (play)
 */

// ゲーム全体の進行管理を行うクラス
// 25/10/06 リファクタリング
public class GameManager : MonoBehaviour
{
    [Header("スクリプト参照")]
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private BuffManager buffManager;
    [SerializeField] private BuffChoiceUI buffChoiceUI;
    public AudioManager audioManager;
    [SerializeField] private GameUI UI;
    [SerializeField] private UIAnimation uiAnimation;

    [Header("ゲームパラメータ")]
    [SerializeField] public float score = 0f;
    [SerializeField] public float timeLimit;
    [SerializeField] public float timeLimitDefault = 30f;
    [SerializeField] public int waveCount = 1;

    private int ResultPage = 0; // ゲームオーバー時の結果ページ

    [Header("設定項目")]
    [SerializeField] public bool isCamShake = true; // カメラシェイクの有効/無効

    // 内部変数
    private float WaveScore = 0; // 1ウェーブあたりの総獲得経験値
    private static GameManager Instance;

    [HideInInspector] public bool isCountDown = true; // 制限時間のカウントダウンを行うか(デバッグ用)
    [HideInInspector] public bool isUseDebugmode = false; // デバッグモードを使用したか(スコアの無効化に使用)

    // ゲームモード
    public enum Mode
    {
        STANDBY,   // 0.スタンバイ
        PLAY,      // 1.プレイ中
        PAUSE,     // 2.ポーズ中
        WAVECLEAR, // 3.ウェーブクリア(入力待機中)
        STOP,      // 4.ウェーブ終了(バフ選択中)
        GAMEOVER,  // 5.ゲームオーバー
    }
    public Mode mode;

    void OnEnable() //Startでないのは、シーン遷移時に再度初期化するため
    {
        InitGame(); // ゲームの初期化
        ItemDestroy();
    }

    void Update()
    {
        switch (mode)
        {
            case Mode.STANDBY:
                // 〇 or 右クリックでゲーム開始
                if (Input.GetButtonDown("Start") || Input.GetMouseButtonDown(1)) 
                {
                    mode = Mode.PLAY;
                    audioManager.PlayUIClick();
                }
                break;
            case Mode.PLAY:
                
                // Option or Tabでポーズ状態に変更
                if (Input.GetButtonDown("Pause") || Input.GetKeyDown(KeyCode.Tab))
                {
                    UI.PauseUI(true);
                    mode = Mode.PAUSE;
                }
                // 制限時間のカウントダウンが有効な場合、時間を減少
                if (isCountDown)
                {
                    timeLimit -= Time.deltaTime;
                }
                //時間切れでウェーブ終了
                if (timeLimit <= 0.0f)
                {
                    uiAnimation.WaveClearPanelAnimation(true); // ウェーブクリアパネルのアニメーション
                    mode = Mode.WAVECLEAR;
                    UI.NextPageSetUISelect(); // 次のページのUIを選択状態にする
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
                break;

            case Mode.PAUSE:
                // Option or 〇 or Tabでポーズ解除
                if (Input.GetButtonDown("Pause") || Input.GetButtonDown("Pause1") || 
                    Input.GetKeyDown(KeyCode.Tab))
                {
                    UI.PauseUI(false);
                    // ボタン操作との兼ね合いにより、PauseUIからmodeを変更する
                }
                break;
            case Mode.WAVECLEAR:
                break;
            case Mode.STOP:
                break;
            case Mode.GAMEOVER:
                switch (ResultPage)
                {
                    // Jump : ×
                    // Pause1 : 〇
                    case 0: // ゲームオーバー画面
                        if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.F))
                        {
                            audioManager.PlayUIClick();
                            ResultPage = 1; // 次のページへ
                            UI.GameOverUI(ResultPage);
                            if(isUseDebugmode) score = 0; // デバッグモードを使用していた場合、スコアを無効化
                        }
                        break;

                    case 1: // リザルト画面
                        if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.F))
                        {
                            audioManager.PlayUIClick();
                            ResultPage = 2;
                            UI.GameOverUI(ResultPage);
                            UI.UpdateRanking(score);
                            UI.UpDateRankingColor(score);
                        }
                        break;

                    case 2:
                        // × or F
                        if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.F))
                        {
                            //タイトルに戻る
                            audioManager.PlayUIClick();
                            StartCoroutine(uiAnimation.StartPanelReverseAnimation()); // UIアニメーション→タイトル遷移
                            Cursor.lockState = CursorLockMode.None;
                        }
                        // 〇 or Pause1でリスタート
                        if (Input.GetButtonDown("Pause1") || Input.GetKeyDown(KeyCode.LeftShift))
                        {
                            audioManager.PlayUIClick();
                            mode = Mode.STANDBY;
                            ReturnPoolAllEnemies();
                            InitGame();
                            UI.HideGameOverUI();
                        }
                        break;
                }
                break;
        } // switch (mode)
    }

    // 関数**********************************************************************************

    //ゲームを完全初期化(シーン開始時など)
    private void InitGame()
    {
        isCountDown = true;
        score = 0f;
        timeLimit = timeLimitDefault;
        mode = Mode.STANDBY;
        waveCount = 1;
        WaveScore = 0;
        playerManager.InitAllStatus();
        enemySpawner.InitSpawner();
    }

    /// <summary>
    /// ゲームを初期化(ウェーブ開始)
    /// </summary>
    public void InitWave()
    {
        mode = Mode.PLAY;
        waveCount++;
        WaveScore = 0;
        timeLimit = timeLimitDefault;
        playerManager.InitStatus();
        enemySpawner.ResetSpawner();
        ItemDestroy();
    }

    /// <summary>
    /// スコアを加算
    /// </summary>
    public void AddScore(bool isKill)
    {
        if (isKill)
        {
            score += (waveCount * 10) * playerManager.expMultiplier;
        }
        else
        {
            score += (waveCount * 5) * playerManager.expMultiplier;
        }
    }

    /// <summary>
    /// Enemyを全てプールに戻す
    /// </summary>
    public void ReturnPoolAllEnemies()
    {
        var enemies = FindObjectsByType<EnemyManager>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            enemy.gameObject.SetActive(false);
            enemy.ResetStatus();
        }
    }

    // シングルトンパターン
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // 動作未確認
        QualitySettings.vSyncCount = 0; // VSyncを無効化
        Application.targetFrameRate = 60; // ToDo: 設定から変更可能にする
    }

    // 自身の子オブジェクト(Item)を全て削除
    private void ItemDestroy()
    {
        // foreach : ある配列やコレクションの全ての要素に対して順番に処理を行うための構文
        foreach (Transform child in transform)
        {
            if (child.CompareTag("Item"))
            {
                Destroy(child.gameObject);
            }
        }
    }

    // waveclearボタンに割り当てるイベント(参照の都合によりGameManagerに配置)
    public void OnWaveClearButton()
    {
        if (mode == Mode.WAVECLEAR)
        {
            audioManager.PlayUIClick();
            uiAnimation.WaveClearPanelAnimation(false);
            ReturnPoolAllEnemies(); // 全ての敵をプールに戻す
            var buffs = buffManager.GetRandomBuffChoices();
            buffChoiceUI.ShowBuffChoices(buffs, WaveScore); // バフ選択UIを表示
            playerManager.Standby();
            mode = Mode.STOP;

        }
    }
}
