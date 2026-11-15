using UnityEngine;
using UnityEngine.EventSystems;

public class TitleUISelector : UISelector
{
    public GameObject SettingModeFirstSelected; // 設定モードで最初に選択状態になるボタン

    public GameObject titleCanvas; // タイトルキャンバス
    public GameObject settingCanvas; // 設定キャンバス

    private void Start()
    {
        titleCanvas.SetActive(true);
        settingCanvas.SetActive(false);
        // 最初に選択されるボタンをセット
        FirstSelectUI(firstSelected);
    }

    public void OpenSettings()
    {
        titleCanvas.SetActive(false);
        settingCanvas.SetActive(true);
        FirstSelectUI(SettingModeFirstSelected);
    }

    public void CloseSettings()
    {
        settingCanvas.SetActive(false);
        titleCanvas.SetActive(true);
        FirstSelectUI(firstSelected);
    }
}

