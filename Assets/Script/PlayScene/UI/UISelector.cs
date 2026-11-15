using UnityEngine;
using UnityEngine.EventSystems;

public class UISelector : MonoBehaviour
{
    public GameObject firstSelected; // 最初に選択状態になるボタン

    void Start()
    {
        // 最初に選択されるボタンをセット
        FirstSelectUI(firstSelected);
    }

    // UI表示時、最初に選択されるUIを設定する
    public void FirstSelectUI(GameObject ui)
    {
        EventSystem.current.SetSelectedGameObject(ui);
    }
}
