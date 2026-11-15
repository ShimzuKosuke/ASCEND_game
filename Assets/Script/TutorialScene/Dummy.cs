using UnityEngine;

public class Dummy : MonoBehaviour
{
    public EnemyDamageUI damageUI; // ダメージUIの参照

    // 被弾したとき
    public void OnDamage(int damage, bool isCritical)
    {
        damageUI.ShowDamage(transform.position); // ダメージUIを表示
    }
}
