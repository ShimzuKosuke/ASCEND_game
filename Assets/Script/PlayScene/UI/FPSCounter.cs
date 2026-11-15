using UnityEngine;
using System.Collections.Generic;

// FPSを計算し、平均と最大FPSを管理するクラス
// 25/10/09 リファクタリング
public class FPSCounter : MonoBehaviour
{
    [Header("設定")]
    public float interval = 5f; // 直近何秒のFPSを求めるか
    private Queue<float> fpsBuffer = new Queue<float>(); // FPSを保存するキュー
    private float timeBuffer = 0f; // 経過時間を保存するバッファ
    private float fpsSum = 0f; // FPSの合計値

    [HideInInspector] public float AvgFPS { get; private set; }
    [HideInInspector] public float MAXFPS { get; private set; }
    [HideInInspector] public float CurrentFPS { get; private set; }

    void Update()
    {
        CalcFPS();
    }

    // FPSを計算し、平均と最大FPSを更新する関数
    private void CalcFPS()
    {
        // 現在のFPSを計算
        CurrentFPS = 1f / Time.deltaTime;

        // FPSをキューに追加
        fpsBuffer.Enqueue(CurrentFPS);
        fpsSum += CurrentFPS;
        timeBuffer += Time.deltaTime;

        // 指定したinterval秒を超えた場合、古いFPSをキューから削除
        while (timeBuffer > interval && fpsBuffer.Count > 0)
        {
            float removed = fpsBuffer.Dequeue();
            fpsSum -= removed;
            timeBuffer -= 1f / removed; // 古いFPSの時間を減算
        }

        // 平均と最大FPSを更新
        AvgFPS = fpsSum / fpsBuffer.Count;
        MAXFPS = Mathf.Max(MAXFPS, CurrentFPS);
    }
}
