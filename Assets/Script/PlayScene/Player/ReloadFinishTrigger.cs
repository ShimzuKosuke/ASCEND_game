using UnityEngine;

/// <summary>
/// 武器にアタッチして使用。
/// リロードアニメーションの完了をPlayerShootingに伝えるためのトリガー
/// </summary>
public class ReloadFinishTrigger : MonoBehaviour
{
    // 処理の流れが複雑なため、将来的にDoTweenへの移行を検討

    // 1. PlayerShooting.Reload()でisReloadingをtrueにしてreloadアニメーションを再生
    // 2. reloadアニメーション開始時にOnReloadStart()が呼ばれ、isReloadAnimをtrueにする
    // 3. リロード時間が経過した後、PlayerShooting.TimerControl()でisReloadAnimをfalseにしてfinishアニメーションを再生
    // 4. finishアニメーションの最後にOnReloadFinish()が呼ばれ、PlayerShooting.ReloadFinish()を呼んでisReloadAnimをfalseにする

    // isReloading  : アニメーションを含めた、リロードに関する処理を行っているかどうかのフラグ
    // isReloadAnim : リロードアニメーションの再生中かどうかのフラグ(finishは含まない)

    [Header("PlayerShooting")]
    [SerializeField] private PlayerShooting playerShooting;

    public void OnReloadStart()
    {
        if (playerShooting != null)
        {
            playerShooting.isReloadAnim = true;
        }
        else
        {
            Debug.LogWarning("PlayerShootingがアタッチされていません。");
        }
    }

    // アニメーションイベントから呼ばれる
    public void OnReloadFinish()
    {
        if (playerShooting != null)
        {
            playerShooting.ReloadFinish();
        }
    }
}
