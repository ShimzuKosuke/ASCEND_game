using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SettingManager : MonoBehaviour
{
    [Header("スクリプト参照")]
    public TitleManager titleManager;

    [Header("設定画面の要素")]
    public GameObject SettingsPanel; // 設定画面のパネル
    public TextMeshProUGUI Txt_Settings; // 設定画面のテキスト
    public Slider V_BGM; // BGM
    public Slider V_UI; // UI音量
    public Slider V_SE; // 効果音量
    public TMP_Dropdown Resolution;
    public TextMeshProUGUI Txt_Resolution;
    public TextMeshProUGUI Txt_Volume;
    public Toggle Tgl_CamShake;
    public Slider Mause;
    public Slider Pad;
    public TextMeshProUGUI Txt_MauseSensitivity; // マウス感度のテキスト
    public TextMeshProUGUI Txt_BGM; // 音量数値(右側)
    public TextMeshProUGUI Txt_UI;  // 音量数値
    public TextMeshProUGUI Txt_SE;  // 音量数値
    public TextMeshProUGUI Txt_PadSensitivity; // パッド感度のテキスト
    public TextMeshProUGUI Txt_RankingReset; // ランキングリセットのテキスト

    // 変数
    private Resolution[] resolutions; // 利用可能な解像度の配列
    private bool isCamShake = true; // カメラシェイクの有効/無効

    // PlayerPrefsのキー定数
    public const string BGMVolume = "BGMVolume";
    public const string UIVolume = "UIVolume";
    public const string FireVolume = "FireVolume";
    public const string ResolutionKey_w = "ScreenWidth";
    public const string ResolutionKey_h = "ScreenHeight";
    public const string CamShakeKey = "CamShake";
    public const string MauseSensitivityKey = "MouseSensitivity";
    public const string PadSensitivityKey = "PadSensitivity";

    void Start()
    {
        SetSlider();
        InitResolutionDropDown();
        // カメラシェイクのトグルの初期値をPlayerPrefsから取得
        isCamShake = PlayerPrefs.GetInt(CamShakeKey, 1) == 1; // デフォルトは有効(1)
        Tgl_CamShake.isOn = isCamShake;
        Tgl_CamShake.onValueChanged.AddListener(SwitchCamShake); // トグルの状態が変わったときに呼び出すメソッドを登録
    }

    private void Update()
    {
        Txt_BGM.text = (V_BGM.value * 100).ToString("f0") + "%";
        Txt_UI.text =   (V_UI.value * 100).ToString("f0") + "%";
        Txt_SE.text =   (V_SE.value * 100).ToString("f0") + "%";
    }

    // 解像度のドロップダウンを初期化するメソッド
    void InitResolutionDropDown()
    {
        //　重複排除と表示
        resolutions = Screen.resolutions; // 利用可能な解像度を取得
        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        // PlayerPrefsに保存された解像度がある場合はそれを適用
        if (PlayerPrefs.HasKey("ScreenWidth") && PlayerPrefs.HasKey("ScreenHeight"))
        {
            int savedWidth = PlayerPrefs.GetInt(ResolutionKey_w);
            int savedHeight = PlayerPrefs.GetInt(ResolutionKey_h);
            Screen.SetResolution(savedWidth, savedHeight, false);
        }
        else Screen.SetResolution(1280, 720, false);

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = $"{resolutions[i].width} x {resolutions[i].height}";
            // 16:9 のみ表示
            if (resolutions[i].width * 9 != resolutions[i].height * 16) continue;

            if (!options.Contains(option)) // 重複排除
            {
                options.Add(option);
            }

            if (resolutions[i].width == Screen.currentResolution.width &&
               resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i; // 現在の解像度のインデックスを保存
            }
        }

        Resolution.ClearOptions(); // 既存のオプションをクリア
        Resolution.AddOptions(options); // 解像度のオプションを追加
        Resolution.value = currentResolutionIndex; // 現在の解像度を選択
        Resolution.RefreshShownValue(); // ドロップダウンの表示を更新

        // 関数を登録
        Resolution.onValueChanged.AddListener(ChangeResolution);

    }

    // カメラシェイクのトグル状態を切り替えるメソッド
    public void SwitchCamShake(bool isShake)
    {
        // カメラシェイクのトグル状態をPlayerPrefsに保存
        PlayerPrefs.SetInt(CamShakeKey, isShake ? 1 : 0);
        isCamShake = isShake; // トグルの状態を更新
        PlayerPrefs.Save(); // 動的に保存
    }

    // 解像度を変更するメソッド
    void ChangeResolution(int index)
    {
        // DropDownの選択肢の文字列から解像度を取得
        string[] res = Resolution.options[index].text.Split('x');
        int width = int.Parse(res[0].Trim());
        int height = int.Parse(res[1].Trim());

        // 解像度を変更
        Screen.SetResolution(width, height, false);

        // PlayerPrefsに解像度を保存
        PlayerPrefs.SetInt(ResolutionKey_w, width);
        PlayerPrefs.SetInt(ResolutionKey_h, height);
    }

    // PlayerPrefsの値をSliderの値に設定するメソッド(設定画面を開いたときに呼び出す)
    public void SetSlider()
    {
        V_BGM.value = PlayerPrefs.GetFloat("BGMVolume", 1.0f);
        V_UI.value = PlayerPrefs.GetFloat("UIVolume", 1.0f);
        V_SE.value = PlayerPrefs.GetFloat("FireVolume", 1.0f);
        Mause.value = PlayerPrefs.GetFloat(MauseSensitivityKey, 2.0f); // マウス感度の初期値を設定
        Pad.value = PlayerPrefs.GetFloat(PadSensitivityKey, 2.0f); // パッド感度の初期値を設定
    }

    // Sliderの値をPlayerPrefsに保存するメソッド(設定画面を閉じたときに呼び出す)
    public void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat("BGMVolume", V_BGM.value);
        PlayerPrefs.SetFloat("UIVolume", V_UI.value);
        PlayerPrefs.SetFloat("FireVolume", V_SE.value);
        PlayerPrefs.SetFloat(MauseSensitivityKey, Mause.value); // マウス感度の保存
        PlayerPrefs.SetFloat(PadSensitivityKey, Pad.value); // パッド感度の保存

        // 動的に保存(なくても可)
        PlayerPrefs.Save();
    }

    // 設定UIの表示/非表示を切り替えるメソッド
    public void SwitchSettingUI()
    {
        switch (titleManager.CurrentState)
        {
            case TitleManager.TitleState.Title:
                SettingsPanel.SetActive(false);
                Txt_Settings.text = "";
                Txt_Resolution.text = "";
                Txt_Volume.text = "";
                V_BGM.gameObject.SetActive(false);
                V_UI.gameObject.SetActive(false);
                V_SE.gameObject.SetActive(false);
                Resolution.gameObject.SetActive(false);
                Tgl_CamShake.gameObject.SetActive(false);
                Mause.gameObject.SetActive(false);
                Txt_MauseSensitivity.text = "";
                Debug.Log("設定画面を閉じました");
                break;

            case TitleManager.TitleState.Settings:
                SettingsPanel.SetActive(true);
                Txt_Settings.text = "設定";
                Txt_Resolution.text = "解像度";
                Txt_Volume.text = "音量";
                V_BGM.gameObject.SetActive(true);
                V_UI.gameObject.SetActive(true);
                V_SE.gameObject.SetActive(true);  
                Resolution.gameObject.SetActive(true);
                Tgl_CamShake.gameObject.SetActive(true);
                Mause.gameObject.SetActive(true);
                Txt_MauseSensitivity.text = Mause.value.ToString("f1");
                Debug.Log("設定画面を開きました");
                break;
        }
    }

    // マウス感度の値を更新(テキストに反映)
    public void UpdateMouseSensitivity()
    {
        Txt_MauseSensitivity.text = Mause.value.ToString("f1");
    }

    // パッド感度の値を更新(テキストに反映)
    public void UpdatePadSensitivity()
    {
        Txt_PadSensitivity.text = Pad.value.ToString("f1");
    }

    // ランキングリセット
    public void ResetRanking()
    {
        PlayerPrefs.SetFloat("Ranking1", 0);
        PlayerPrefs.SetFloat("Ranking2", 0);
        PlayerPrefs.SetFloat("Ranking3", 0);
        PlayerPrefs.SetFloat("Ranking4", 0);
        PlayerPrefs.SetFloat("Ranking5", 0);
        PlayerPrefs.Save();
        Txt_RankingReset.text = "ランキングをリセットしました";
        Txt_RankingReset.color = Color.red;
        Invoke("ClearResetText", 2f); // 2秒後にテキストをクリア
    }
    // リセット完了のテキストをクリア
    private void ClearResetText()
    {
        Txt_RankingReset.text = "ランキングリセット";
        Txt_RankingReset.color = Color.black;
    }
}
