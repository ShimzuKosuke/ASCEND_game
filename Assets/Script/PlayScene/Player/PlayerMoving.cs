using UnityEngine;
using UnityEngine.UI;

// Playerの移動・ジャンプ・ダッシュを管理するクラス
// 25/10/08 リファクタリング
public class PlayerMoving : MonoBehaviour
{
    [Header("ステータス")]
    [SerializeField] public float moveSpeed = 5f;
    [SerializeField] public float NotDashSpeed = 5f;
    [SerializeField] public float dashSpeed = 5f;
    [SerializeField] private float dashSpeedDefault = 5f;
    [SerializeField] private float SpeedDefault = 5f;
    [SerializeField] public int Stamina = 250;
    [SerializeField] public int StaminaMax = 250;
    [SerializeField] public int StaminaDefault = 250;
    [SerializeField] private float StaminaRegenStart = 1f;
    [SerializeField] private float StaminaRegenTimer = 0f;
    [SerializeField] public float jumpForce = 5f;
    [SerializeField] public float jumpForceDefault = 5f;
    [SerializeField] private int jumpStaminaCost = 20;
    [SerializeField] private int canDashingStamina = 40; // ダッシュ可能な最低スタミナ
    [SerializeField] private int dashStaminaCost = 1;
    [SerializeField] private int staminaRegenRate = 1;
    [SerializeField] public float aerialMoveBonus = 0f; // 空中移動時の速度補正

    [Header("演算用変数")]
    [SerializeField] private float gravity = -9.8f;
    [SerializeField] private float idleThreshold = 3f; // Idle状態に移行する閾値（速度）
    private Vector3 velocity;       // 加速度を保持する変数
    public bool isGrounded;        // 地面に着地しているかどうかのフラグ変数
    private float jumplocktime = 0.1f; // ジャンプ無効時間
    private float jumplocktimer = 0f;  // ジャンプ無効タイマー
    private bool isJumpLock = false;   // ジャンプ無効フラグ
    private bool shouldFall = false; //アニメーター分岐用
    public bool isDashing = false; // ダッシュ中かどうかのフラグ
    private bool canDoubleJump = true; // 二段ジャンプ可能かどうかのフラグ

    [Header("コンポーネント参照")]
    public CharacterController controller;
    [SerializeField] private Animator animator; // モデル(子)のアニメーター
    public Slider StaminaBar;

    [Header("スクリプト参照")]
    [SerializeField] private PlayerRespawn respawn;
    [SerializeField] private HeadBobber headBobber;
    [SerializeField] private PlayerPOV POV;

    private void OnEnable()
    {
        InitAllStatus();
    }

    /// <summary>
    ///  ステータス完全初期化
    /// </summary>
    public void InitAllStatus()
    {
        moveSpeed = SpeedDefault;
        NotDashSpeed = SpeedDefault;
        dashSpeed = dashSpeedDefault;
        Stamina = StaminaDefault;
        StaminaMax = StaminaDefault;
        jumpForce = jumpForceDefault;
        StaminaBar.value = (float)Stamina / StaminaMax;
        aerialMoveBonus = 0;
    }

    /// <summary>
    ///  ステータス初期化(Wave更新時など)
    /// </summary>
    public void InitStatus()
    {
        Stamina = StaminaMax;

        Stamina = StaminaDefault;
        NotDashSpeed = moveSpeed;
        dashSpeed = dashSpeedDefault;
        StaminaMax = StaminaDefault;
        StaminaBar.value = (float)Stamina / StaminaMax;
        isDashing = false;

        // ジャンプロックをセット(コントローラーでのバフ選択処理とジャンプ入力が競合するため)
        SetJumpLock();

        controller.enabled = false; // CharacterControllerを無効化して位置を変更できるようにする
        transform.position = respawn.Respawn(); // プレイヤーの位置をリスポーンポイントに設定
        ResetNeckRotation(); // 首の回転をリセット
        controller.enabled = true; // CharacterControllerを再度有効化
    }

    // 首の回転をリセット
    private void ResetNeckRotation()
    {
        // transform.rotation : playerの体の回転
        // headBobber.ResetRotation(), POV.ResetNeckRotation() : neckの回転
        // neckの回転はheadBobberとPOVで軸ごとに制御しているため、両方リセットする必要がある
        transform.rotation = Quaternion.identity;
        headBobber.ResetRotation();
        // PlayerPOVのneck回転もリセット
        if (POV != null)
        {
            POV.ResetNeckRotation();
        }
    }

    /// <summary>
    /// 移動アニメーションを制御
    /// </summary>
    public void PlayAnimaion()
    {
        animator.SetFloat("moveSpeed", controller.velocity.magnitude); // キャラクターの移動速度をアニメーターに設定
        shouldFall = (controller.velocity.magnitude < idleThreshold) || !isGrounded; //moveSpeedが5未満または地面に着地していない場合、shouldFallをtrueに設定
        animator.SetBool("shouldFall", shouldFall);

    }

    /// <summary>
    /// 移動・ジャンプ・ダッシュ処理
    /// </summary>
    public void Move()
    {
        isGrounded = controller.isGrounded;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // ローカル座標をワールド座標に変換して移動方向を計算
        Vector3 moveDirection = transform.TransformDirection(new Vector3(h, 0, v)) * moveSpeed;

        // 重力を加算
        velocity.y += gravity * Time.deltaTime;

        // ジャンプロックのタイマー更新
        JumpLock();

        // ジャンプ処理
        // 接地 + ジャンプロックなし + スタミナが足りている + ジャンプボタンが押されたらジャンプ
        // 非接地 + 二段ジャンプ可能 + スタミナが足りている + ジャンプボタンが押されたら二段ジャンプ
        if (isGrounded && !isJumpLock && Stamina >= jumpStaminaCost &&
            (Input.GetButton("Jump") || Input.GetKey(KeyCode.Space)))
        {
            velocity.y = jumpForce;
            Stamina -= jumpStaminaCost;
            StaminaRegenTimer = 0;
            Stamina = Mathf.Clamp(Stamina, 0, StaminaMax);
            StaminaBar.value = (float)Stamina / StaminaMax;
        }
        else if (!isGrounded && canDoubleJump && Stamina >= jumpStaminaCost
            && (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space)))
        {
            velocity.y = jumpForce; 
            canDoubleJump = false; // 二段ジャンプを使用済みに設定
            Stamina -= jumpStaminaCost * 2; // ダブルジャンプはスタミナ消費2倍
            StaminaRegenTimer = 0;
            Stamina = Mathf.Clamp(Stamina, 0, StaminaMax);
            StaminaBar.value = (float)Stamina / StaminaMax;
        }
        else if (isGrounded && !canDoubleJump)
        {
            canDoubleJump = true; // 地面に着地したら二段ジャンプをリセット
        }
        else
        {
        }

        StaminaRegenTimer += Time.deltaTime; // スタミナの回復開始時間を更新
        // R1 or LShift and スタミナ40以上 and 接地でダッシュ処理
        if ((Input.GetButton("Dash") || Input.GetKey(KeyCode.LeftShift)) 
            && Stamina > canDashingStamina && isGrounded)
        {
            if(!isDashing) // ダッシュを開始した瞬間に一度だけ実行(軽量化のため)
            {
                isDashing = true;
                moveSpeed = NotDashSpeed + dashSpeed;

            }
            Stamina -= dashStaminaCost;
            StaminaRegenTimer = 0;
            Stamina = Mathf.Clamp(Stamina, 0, StaminaMax);
            StaminaBar.value = (float)Stamina / StaminaMax;
        }
        else
        {
            // シフトキーを離したら元の速度に戻す
            if (isDashing) // ダッシュを解除した瞬間に一度だけ実行(軽量化のため)
            {
                isDashing = false;
                moveSpeed = NotDashSpeed;
            }
            // スタミナが最大値未満で、回復開始時間を過ぎていて、接地中ならスタミナ回復
            if (Stamina < StaminaMax && StaminaRegenTimer > StaminaRegenStart 
                && isGrounded)
            {
                Stamina += staminaRegenRate;
                Stamina = Mathf.Clamp(Stamina, 0, StaminaMax);
                StaminaBar.value = (float)Stamina / StaminaMax;
            }

        }

        if(!isGrounded) // 空中にいる場合、空中移動補正を加算
        {
            moveDirection += moveDirection.normalized * aerialMoveBonus;
        }
        // 移動と重力を一度のcontroller.Moveで処理
        controller.Move((moveDirection + velocity) * Time.deltaTime);

        // 着地している場合は落下速度をリセット
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // 地面に着いた場合、速度をリセット
        }
    }

    /// <summary>
    /// ジャンプロックをセット
    /// </summary>
    public void SetJumpLock()
    {
        isJumpLock = true;
        jumplocktimer = jumplocktime;
    }

    // ジャンプロックを管理
    private void JumpLock()
    {
        if (isJumpLock)
        {
            jumplocktimer -= Time.deltaTime;
            if (jumplocktimer <= 0f)
            {
                isJumpLock = false;
            }
        }
    }

}
