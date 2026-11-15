using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// バフ選択UIを管理するクラス
// 25/10/05 リファクタリング
public class BuffChoiceUI : MonoBehaviour
{
    [Header("スクリプト参照")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private BuffManager buffManager;
    [SerializeField] private PlayerManager player;
    [SerializeField] private UISelector uiSelector;

    [Header("コンポーネント参照")]
    [SerializeField] private Button[] buffButtons;           // バフ選択ボタンの配列      
    [SerializeField] private TextMeshProUGUI[] Txt_BuffDesc; // バフの説明テキストの配列  
    [SerializeField] private GameObject Pnl_BuffChoice;      // バフ選択パネル
    private List<BuffData> currentBuffs;   // 現在のバフ選択リスト      

    public float num; // 一時保存用
    public int num_i; // 一時保存用(int)

    public float addedValue { get; private set; } = 0; // バフ適用時の該当ステータス値を保存するための変数

    private void Start()
    {
        // UI要素を初期化(非表示)
        for (int i = 0; i < buffButtons.Length; i++)
        {
            buffButtons[i].gameObject.SetActive(false);
            Pnl_BuffChoice.SetActive(false);
        }

        if (buffManager.currentChoices == null)
        {
            Debug.LogWarning("currentChoicesがnullです。");
        }
    }

    /// <summary>
    /// バフ選択UIを表示し、バフの情報を設定するメソッド
    /// </summary>
    public void ShowBuffChoices(List<BuffData> buffs, float waveScore)
    {
        // マウスカーソルを表示
        Cursor.lockState = CursorLockMode.None; 
        Cursor.visible = true;

        currentBuffs = buffs;

        uiSelector.FirstSelectUI(buffButtons[0].gameObject); // buff1を選択状態にする

        for (int i = 0; i < buffButtons.Length; i++)
        {
            // UI要素を有効化
            buffButtons[i].gameObject.SetActive(true);
            Pnl_BuffChoice.SetActive(true);

            // インスタンスのバフデータを取得
            BuffData data = buffs[i];

            // バフの名前を取得しボタンに設定
            buffButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = data.buffName;

            // 抽選したタイプの現在のプレイヤーステータスを取得
            if (data.type == BuffType.AttackUp) data.current_i = player.shoot.ATK;
            else if (data.type == BuffType.MaxHPUp) data.current_i = player.MaxHP;
            else if (data.type == BuffType.SpeedUp) data.current_f = player.move.moveSpeed;
            else if (data.type == BuffType.HealthRegen) data.current_i = player.HPRegenRate;
            else if (data.type == BuffType.CriticalHitChance) data.current_i = player.shoot.CriticalRate;
            else if (data.type == BuffType.DamageReduction) data.current_i = player.DamageReduction;
            else if (data.type == BuffType.GetEXPUp) data.current_f = player.expMultiplier;
            else if (data.type == BuffType.MagaineSizeUp) data.current_i = player.shoot.magazineSize;
            else if (data.type == BuffType.FireRateUp) data.current_f = player.shoot.fireRate;
            else if (data.type == BuffType.StaminaUP) data.current_i = player.move.StaminaMax;
            else if (data.type == BuffType.DashSpeedUp) data.current_f = player.move.dashSpeed;
            else if (data.type == BuffType.JumpPowerUp) data.current_f = player.move.jumpForce;
            else if (data.type == BuffType.FastReload) data.current_f = player.shoot.reloadTime;
            else if (data.type == BuffType.RangeUp) data.current_f = player.shoot.range;
            else if (data.type == BuffType.AntiRecoil) data.current_f = player.shoot.recoilAngle;
            else if (data.type == BuffType.ChargeSpeedUp) data.current_f = player.shoot.chargeSpeed;
            else if (data.type == BuffType.HitLimitUp) data.current_i = player.shoot.hitLimit;
            else if (data.type == BuffType.BurstIntervalDown) data.current_f = player.shoot.burstInterval;
            else if (data.type == BuffType.AerialAttack) data.current_i = player.shoot.AerialAttackBonus;
            else if (data.type == BuffType.AerialMove) data.current_f = player.move.aerialMoveBonus;

            // バフの説明を設定(テキストにバフの値を追加)
            switch (data.numType)
                {

                    case NumType.Int: // int
                                      // 最大まで強化されている場合の処理
                        if (data.min_i >= data.current_i || data.limit_i <= data.current_i)
                        {
                            Txt_BuffDesc[i].text = "最大まで強化しました。";
                        }
                        else // 通常の処理
                        {
                            addedValue = data.value_i; // 一時保存用の値を設定

                            Txt_BuffDesc[i].text = data.description + addedValue;
                            switch (data.unit) // 単位に応じてテキストを変更
                            {
                                case Unit.Seconds:
                                    Txt_BuffDesc[i].text += "秒";
                                    break;
                                case Unit.Percentage:
                                    Txt_BuffDesc[i].text += "%";
                                    break;
                            }
                        }
                        break;
                    case NumType.Float: // float
                                        // 最大まで強化されている場合の処理
                        if (data.min_f >= data.current_f || data.limit_f <= data.current_f)
                        {
                            Txt_BuffDesc[i].text = "最大まで強化しました。";
                        }
                        else // 通常の処理
                        {
                            if (data.type == BuffType.GetEXPUp)
                            {
                                addedValue = (data.value_f) * 100f; // 経験値獲得量アップはパーセント表示
                                Txt_BuffDesc[i].text = data.description + addedValue.ToString("F2") + "%";
                            }
                            else
                            {
                                addedValue = data.value_f; // 一時保存用の値を設定

                                Txt_BuffDesc[i].text = data.description + addedValue.ToString("F2");
                                switch (data.unit) // 単位に応じてテキストを変更
                                {
                                    case Unit.Seconds:
                                        Txt_BuffDesc[i].text += "秒";
                                        break;
                                    case Unit.Percentage:
                                        Txt_BuffDesc[i].text += "%";
                                        break;
                                }

                            }
                        }
                        break;
                }

            // 既存のリスナーをクリアしてから新しいリスナーを追加
            buffButtons[i].onClick.RemoveAllListeners();
            AddButtonListener(buffButtons[i], i);
        }
    }

    // バフ選択ボタンにリスナーを追加するメソッド
    private void AddButtonListener(Button button, int index)
    {
        button.onClick.AddListener(() => ApplyBuff(index));
    }

    /// <summary>
    /// バフ選択UIを非表示にするメソッド
    /// </summary>
    public void HideBuffChoices()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;  

        for (int i = 0; i < buffButtons.Length; i++)
        {
            buffButtons[i].gameObject.SetActive(false);
        }
        Pnl_BuffChoice.SetActive(false); // バフ選択パネルを非表示にする
    }

    /// <summary>
    /// 選択されたバフを適用するメソッド
    /// </summary>
    public void ApplyBuff(int index)
    {
        var selectedBuff = currentBuffs[index];
        CheckEmpty(selectedBuff.value_i, selectedBuff.value_f); // 値が設定されているか確認

        // プレイヤーにバフを適用      
        switch (selectedBuff.type) // 修正: 'buffType' を 'type' に変更      
        {
            case BuffType.AttackUp: // 攻撃力アップ処理      
                player.shoot.ATK += 50;// (int)addedValue;
                player.shoot.ATK = Mathf.Clamp(player.shoot.ATK,
                    selectedBuff.min_i, selectedBuff.limit_i);
                selectedBuff.current_i = player.shoot.ATK;
                break;

            case BuffType.MaxHPUp: // 最大HPアップ処理      
                player.MaxHP += 20; // (int)addedValue;
                player.MaxHP = Mathf.Clamp(player.MaxHP,
                    selectedBuff.min_i, selectedBuff.limit_i);
                selectedBuff.current_i = player.MaxHP;
                break;

            case BuffType.SpeedUp: // スピードアップ処理      
                player.move.moveSpeed += 0.2f; // addedValue;
                player.move.moveSpeed = Mathf.Clamp(player.move.moveSpeed,
                    selectedBuff.min_f, selectedBuff.limit_f);
                selectedBuff.current_f = player.move.moveSpeed;
                break;

            case BuffType.HealthRegen: //再生処理  
                player.HPRegenRate += 5;// (int)addedValue;
                player.HPRegenRate = Mathf.Clamp(player.HPRegenRate, selectedBuff.min_i, selectedBuff.limit_i);
                selectedBuff.current_i = player.HPRegenRate;
                break;

            case BuffType.CriticalHitChance: // クリティカルヒット率アップ処理
                player.shoot.CriticalRate += 5;// (int)addedValue; 
                player.shoot.CriticalRate = Mathf.Clamp(player.shoot.CriticalRate, 
                    selectedBuff.min_i, selectedBuff.limit_i);
                selectedBuff.current_i = player.shoot.CriticalRate;
                break;

            case BuffType.DamageReduction: // ダメージ軽減処理 
                player.DamageReduction += 10;// (int)addedValue;
                player.DamageReduction = Mathf.Clamp(player.DamageReduction,
                    selectedBuff.min_i, selectedBuff.limit_i);
                selectedBuff.current_i = player.DamageReduction;
                break;

             case BuffType.GetEXPUp: // 経験値獲得量アップ処理
                player.expMultiplier += 0.1f;// addedValue;
                player.expMultiplier = Mathf.Clamp(player.expMultiplier,
                    selectedBuff.min_f, selectedBuff.limit_f);
                selectedBuff.current_f = player.expMultiplier;
                break;

             case BuffType.MagaineSizeUp: // マガジンサイズアップ処理
                player.shoot.magazineSize += 5; // (int)addedValue;
                player.shoot.magazineSize = Mathf.Clamp(player.shoot.magazineSize,
                    selectedBuff.min_i, selectedBuff.limit_i);
                selectedBuff.current_i = player.shoot.magazineSize;
                break;

            case BuffType.FireRateUp:  // 弾の発射速度アップ処理9
                player.shoot.fireRate -= 0.01f; //addedValue;
                player.shoot.fireRate = Mathf.Clamp(player.shoot.fireRate,
                    selectedBuff.min_f, selectedBuff.limit_f);
                selectedBuff.current_f = player.shoot.fireRate;
                Debug.Log("FireRate +" + addedValue); // デバッグ用ログ出力
                break;

            case BuffType.StaminaUP: // スタミナアップ処理
                player.move.StaminaMax += 20;// (int)addedValue;
                player.move.StaminaMax = Mathf.Clamp(
                    player.move.StaminaMax, selectedBuff.min_i, selectedBuff.limit_i);
                selectedBuff.current_i = player.move.StaminaMax;
                break;

            case BuffType.DashSpeedUp: // ダッシュ速度アップ処理
                player.move.dashSpeed += 2f;// addedValue;
                player.move.dashSpeed = Mathf.Clamp(
                    player.move.dashSpeed,
                    selectedBuff.min_f, selectedBuff.limit_f);
                selectedBuff.current_f = player.move.dashSpeed;
                break;

            case BuffType.JumpPowerUp: // ジャンプパワーアップ処理
                player.move.jumpForce += 2;//addedValue;
                player.move.jumpForce = Mathf.Clamp(player.move.jumpForce,
                    selectedBuff.min_f, selectedBuff.limit_f);
                selectedBuff.current_f = player.move.jumpForce;
                break;

            case BuffType.FastReload: // リロード速度アップ処理
                player.shoot.reloadTime -= 0.01f;// addedValue;
                //player.reloadTime = Mathf.Clamp(player.reloadTime,
                //    selectedBuff.min_f, selectedBuff.limit_f);
                selectedBuff.current_f = player.shoot.reloadTime;
                break;

            case BuffType.RangeUp: // 射程距離アップ処理
                player.shoot.range += 10; // addedValue;
                player.shoot.range = Mathf.Clamp(player.shoot.range,
                    selectedBuff.min_f, selectedBuff.limit_f);
                break;

            case BuffType.AntiRecoil: // 反動軽減処理
                player.shoot.recoilAngle += 0.1f;
                player.shoot.recoilAngle = Mathf.Clamp(player.shoot.recoilAngle,
                    selectedBuff.min_f, selectedBuff.limit_f);
                break;

            case BuffType.ChargeSpeedUp: // チャージ速度アップ処理
                player.shoot.chargeSpeed += 0.4f;
                player.shoot.chargeSpeed = Mathf.Clamp(player.shoot.chargeSpeed,
                    selectedBuff.min_f, selectedBuff.limit_f);
                break;

            case BuffType.HitLimitUp: // チャージショットのヒット数アップ処理
                player.shoot.hitLimit += 1;
                player.shoot.hitLimit = Mathf.Clamp(player.shoot.hitLimit,
                    selectedBuff.min_i, selectedBuff.limit_i);
                break;

            case BuffType.BurstIntervalDown: // バースト射撃のインターバルダウン処理
                player.shoot.burstInterval -= 0.1f;
                player.shoot.burstInterval = Mathf.Clamp(player.shoot.burstInterval,
                    selectedBuff.min_f, selectedBuff.limit_f);
                break;

            case BuffType.AerialAttack: // 空中攻撃処理
                player.shoot.AerialAttackBonus += 75; // (int)addedValue;
                player.shoot.AerialAttackBonus = Mathf.Clamp(player.shoot.AerialAttackBonus,
                    selectedBuff.min_i, selectedBuff.limit_i);
                break;

            case BuffType.AerialMove: // 空中移動補正アップ処理
                player.move.aerialMoveBonus += 0.3f; // addedValue;
                player.move.aerialMoveBonus = Mathf.Clamp(player.move.aerialMoveBonus,
                    selectedBuff.min_f, selectedBuff.limit_f);
                break;

                // 他のバフタイプもここに追加      
        }
        HideBuffChoices();
        addedValue = 0; // 一時保存用の値をリセット
        gameManager.InitWave(); // 次のウェーブに移行  
    }

    // 値が設定されているか確認するメソッド
    private void CheckEmpty(int value_i,float value_f)
    {
        if(value_i == 0 || value_f == 0f)
        {
            Debug.LogWarning("バフの効果値が0のものがあります。適切な値を設定してください。");
        }
    }
}
