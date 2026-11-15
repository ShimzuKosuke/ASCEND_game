using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class TutorialUI : MonoBehaviour
{
    [Header("スクリプト参照")]
    public TutorialManager manager;
    public PlayerManager player;
    [SerializeField] private PlayerShooting shoot;
    public UISelector uiSelector;
    public FPSCounter fps;
    public UIAnimation uiAnimation;

    [Header("コンポーネント参照")]
    public TextMeshProUGUI Txt_Tutorial;
    public TextMeshProUGUI Txt_Page;
    public TextMeshProUGUI Txt_FPS;
    public TextMeshProUGUI Txt_Ammo;
    public TextMeshProUGUI Txt_HP;
    public TextMeshProUGUI Txt_Stamina;
    public Button Btn_Resume;
    public Button Btn_ReturnTitle;
    public TextMeshProUGUI Txt_FireModeChange;
    public TextMeshProUGUI Txt_CharageLv;
    public TextMeshProUGUI Txt_PlayGuide;
    public TextMeshProUGUI Txt_GunGuide;
    public TextMeshProUGUI Txt_Pause;
    public GameObject Pnl_Pause;
    [SerializeField] private TextMeshProUGUI Txt_FireInfo;
    // ↓表示/非表示が異なるクラスで処理されているため、今後モードごとにCanvasを分割する予定
    public GameObject Pnl_TutorialText;

    public Canvas PlayCanvas;
    public Canvas PauseCanvas;

    void Start()
    {
        HUD(false); // 初期状態ではHUDを非表示にする

        // コルーチン開始
        StartCoroutine(UpdateUISecond()); // UIの更新を開始

        Btn_ReturnTitle.onClick.AddListener(ReturnTitle);
        Btn_Resume.onClick.AddListener(() => { PauseUI(false); });
        PauseCanvas.enabled = false;
        hideSetFireModeCange(); // 射撃モード変更の非表示
    }

    void Update()
    {
        switch (manager.mode)
        {
            case TutorialManager.TutorialoMode.START:
                Pnl_TutorialText.SetActive(true);
                break;

            case TutorialManager.TutorialoMode.MOVE:
                if(!player.shoot.isReloading)
                {
                    Txt_Ammo.text = player.shoot.currentAmmo + "/" + player.shoot.magazineSize;
                    SetTextColor(Txt_Ammo, player.shoot.currentAmmo, player.shoot.magazineSize);
                }
                else // リロード中
                {
                    Txt_Ammo.text = "Reloading...";
                    Txt_Ammo.color = Color.white;
                }

                Txt_HP.text = player.HP.ToString("f0") + " / " + player.MaxHP;
                Txt_Stamina.text = player.move.Stamina.ToString("f0") + " / " + player.move.StaminaMax;
                showSetChargeLv();

                break;
            case TutorialManager.TutorialoMode.PAUSE:
                break;
        }
        
    }

    // 割合に応じてテキストの色を変更
    public void SetTextColor(TextMeshProUGUI text, int current, int max)
    {
        float percent = (float)current / max * 100f; // 割合を計算

        if (percent <= 25) // 25%以下
        {
            text.color = Color.red;
        }
        else if (percent <= 50) // 26~50%
        {
            text.color = Color.yellow;
        }
        else
        {
            text.color = Color.white;
        }
    }

    // HUDの表示変更
    public void HUD(bool isShow)
    {
        player.HPBar.gameObject.SetActive(isShow);
        player.move.StaminaBar.gameObject.SetActive(isShow);
        player.ReloadBar.gameObject.SetActive(isShow);

        Txt_Ammo.gameObject.SetActive(isShow);
        Txt_HP.gameObject.SetActive(isShow);
        Txt_Stamina.gameObject.SetActive(isShow);
        Txt_FPS.gameObject.SetActive(isShow);
        Txt_PlayGuide.gameObject.SetActive(isShow);
        Txt_GunGuide.gameObject.SetActive(isShow);
    }

    // ポーズUIの表示変更
    public void PauseUI(bool isShow)
    {
        // チュートリアル開始前はポーズUIを表示しない
        if (manager.mode == TutorialManager.TutorialoMode.START) return;

        Cursor.visible = isShow;
        uiAnimation.PausePanelAnimation(isShow);
        if (isShow)
        {
            Cursor.lockState = CursorLockMode.None;
            uiSelector.FirstSelectUI(Btn_Resume.gameObject);
            PauseCanvas.enabled = true;
            PlayCanvas.enabled = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;

            //！PlaySceneと構造が異なるので注意
            StartCoroutine(WaitAndResume(0.3f));
        }
    }

    // 1秒ごとにUI情報を更新
    private void UpdateUI()
    {
        Txt_FPS.text = "FPS: " + fps.CurrentFPS.ToString("f0")
            + " MAX:" + fps.MAXFPS.ToString("f0")
            + " AVG:" + fps.AvgFPS.ToString("f0");
    }

    private IEnumerator UpdateUISecond()
    {
        while (true)
        {
            UpdateUI();
            yield return new WaitForSeconds(1f);
        }
    }

    // タイトルに戻る
    public void ReturnTitle()
    {
        manager.Anm_panelFeed.SetTrigger("SceneChange");
        manager.StartCoroutine(manager.WaitUIAnimation("Title"));
    }

    // 射撃モード変更の表示
    public void showSetFireModeCange()
    {
        float TextLifeTime = 2f;
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
        Invoke("hideSetFireModeCange", TextLifeTime);
    }

    // 射撃モード変更の非表示
    public void hideSetFireModeCange()
    {
        Txt_FireModeChange.gameObject.SetActive(false);
    }

    // チャージショットレベルの表示
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

    // 一定時間待機し、ポーズ解除
    private IEnumerator WaitAndResume(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        manager.mode = TutorialManager.TutorialoMode.MOVE;
        PauseCanvas.enabled = false;
        PlayCanvas.enabled = true;
    }

    /// <summary>
    /// 射撃回数・命中率の情報を更新
    /// </summary>
    public void setFireInfoText()
    {
        Txt_FireInfo.text = "射撃 : " + shoot.fireCnt +
            "\n命中 : " + shoot.accuracy.ToString("F2") + "%";
    }
}
