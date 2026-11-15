using UnityEngine;

// バフの情報を定義するScriptableObject
// 25/10/05 リファクタリング

// BuffType : バフの種類を列挙型で定義
public enum BuffType
{ 
    AttackUp,          // 攻撃力アップ
    MaxHPUp,           // 最大HPアップ
    SpeedUp,           // 速度アップ
    HealthRegen,       // ヘルス再生
    CriticalHitChance, // クリティカルヒット率アップ
    DamageReduction,   // ダメージ軽減
    GetEXPUp,          // 経験値獲得量アップ
    MagaineSizeUp,     // マガジンサイズアップ
    FireRateUp,        // 弾の発射速度アップ
    StaminaUP,         // スタミナアップ
    DashSpeedUp,       // ダッシュ速度アップ
    JumpPowerUp,       // ジャンプパワーアップ
    FastReload,        // リロード速度アップ
    RangeUp,           // 射程距離アップ
    AntiRecoil,        // 反動軽減
    ChargeSpeedUp,     // チャージ速度アップ
    HitLimitUp,        // チャージショットのヒット数アップ
    BurstIntervalDown, // バースト射撃のインターバルダウン
    AerialAttack,      // 空中攻撃
    AerialMove,        // 空中移動
}

// 効果値の型を指定
public enum NumType
{
    Float,
    Int,
}

// 効果値の単位を指定
public enum Unit
{
    None,       // 単位なし
    Seconds,    // 秒
    Percentage, // パーセンテージ
}

// CreateAssetMenu : このアトリビュートを使用すると、UnityエディタでこのScriptableObjectのインスタンスを作成できるようになる。
// fileName : 作成されるアセットのデフォルトのファイル名を指定する。
// menuName : Unityエディタの「Create」メニューに表示される名前を指定する。
[CreateAssetMenu(fileName = "BuffData", menuName = "Scriptable Objects/BuffData")]

// BuffDataクラス : バフのデータを格納するためのクラス
public class BuffData : ScriptableObject
{
    public string buffName;
    public string description;
    public BuffType type; // 例：攻撃力UP、防御力UPなど
    public NumType numType; // 効果値の型（floatまたはint）
    public Unit unit; // 効果値の単位（秒、パーセンテージなど）

    // バフの効果値（int/float型）
    public float value_f;
    public int value_i;

    public bool isMinusValue; // 値を減少させるバフか(連射速度など)

    // バフの効果値の範囲（上限、下限、現在値）
    public float limit_f; 
    public int limit_i;
    public float min_f; 
    public int min_i;
    public float current_f;
    public int current_i;
}
