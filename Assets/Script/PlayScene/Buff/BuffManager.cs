using System.Collections.Generic;
using UnityEngine;

// バフの登録と抽選を管理するクラス
// 25/10/05 リファクタリング
public class BuffManager : MonoBehaviour
{
    [Header("全バフリスト")]
    public List<BuffData> allBuffs; // インスペクターでScriptableObjectを複数登録

    [Header("1回の抽選で選ばれるバフ数")]
    [SerializeField] private int choiceBuffCount = 3; // 1回の抽選で選ばれるバフの数

    public List<BuffData> currentChoices; // 抽選で選ばれたバフのリスト

    /// <summary>
    /// バフをランダムに抽選して返す
    /// </summary>
    public List<BuffData> GetRandomBuffChoices()
    {
        List<BuffData> shuffled = new List<BuffData>(allBuffs);
        Shuffle(shuffled);

        currentChoices = shuffled.GetRange(0, Mathf.Min(choiceBuffCount, shuffled.Count));
        return currentChoices;

    }

    // フィッシャー・イェーツのシャッフルアルゴリズムを使用してリストをランダムに並べ替えるメソッド
    private void Shuffle(List<BuffData> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]); // C# 7.0以降のタプルを使ったスワップ
        }
    }
    

}
