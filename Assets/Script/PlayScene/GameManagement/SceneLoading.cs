using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// シーン遷移時のロード画面を制御するクラス。ロードに時間がかかる場合に使用。(25/10/15時点で未使用)
public class SceneLoading : MonoBehaviour
{
    private AsyncOperation async; // 非同期動作で使用するAsyncOperation型の変数
    [SerializeField] private GameObject loadUI; // ロード中に表示するUI
    [SerializeField] private Slider slider; // 読み込み率
    public void NextScene()
    {
        //　ロード画面UIをアクティブにする
        loadUI.SetActive(true);

        //　コルーチンを開始
        StartCoroutine("LoadData");
    }

    IEnumerator LoadData()
    {
        // シーンの読み込みをする
        async = SceneManager.LoadSceneAsync("Play");
        Debug.Log("シーン読み込み開始");

        //　読み込みが終わるまで進捗状況をスライダーの値に反映させる
        while (!async.isDone)
        {
            var progressVal = Mathf.Clamp01(async.progress / 0.9f);
            slider.value = progressVal;
            yield return null;
        }
    }
}
