using UnityEngine;

// Playerの頭の揺れを管理するクラス
// 25/10/08 リファクタリング
public class HeadBobber : MonoBehaviour
{
    [Header("設定")]
    [SerializeField] [Tooltip("歩行時の揺れ速度")] private float walkBobbingSpeed = 6f;
    [SerializeField] [Tooltip("歩行時の揺れ幅")] private float walkBobbingAmount = 0.05f;
    [SerializeField] [Tooltip("走行時の揺れ速度")] private float dashBobbingSpeed = 10f;
    [SerializeField] [Tooltip("走行時の揺れ幅")] private float dashBobbingAmount = 0.1f;
    [SerializeField] [Tooltip("揺れを発生させる閾値")] private float bobbingThreshold = 0.1f;
    [SerializeField] [Tooltip("カメラ振動OFF時の揺れ軽減率")] private float camShakeOffReduction = 0.5f;

    [Header("スクリプト参照(共通)")]
    [SerializeField] private PlayerMoving playerMoving;
    [SerializeField] private PlayerPOV POV;

    [Header("スクリプト参照(Play)")]
    [SerializeField] private GameManager manager;

    [Header("スクリプト参照(Tutorial)")]
    [SerializeField] private TutorialManager tutorialManager;

    private float defaultPosY;
    private float defaultPosX;
    private float timer; // 揺れのタイマー
    private bool isResetting = false; // 回転リセット中かどうかのフラグ

    void Start()
    {
        Vector3 pos = transform.localPosition;
        defaultPosY = pos.y;
        defaultPosX = pos.x;

        if (!POV.isCamShake) // 揺れ幅を半減
        {
            walkBobbingAmount *= camShakeOffReduction;
            dashBobbingAmount *= camShakeOffReduction;
        }
    }

    void Update()
    {
        HeadBobbing();
    }

    // 頭の揺れを制御するメソッド
    private void HeadBobbing()
    {
        float speed = playerMoving.controller.velocity.magnitude;

        if (isResetting)
        {
            transform.localRotation = Quaternion.identity;
            isResetting = false;
            return;
        }

        // 一定以上の速度で、地面に接地していて、ゲーム中のみ揺れを発生
        if (speed > bobbingThreshold && playerMoving.isGrounded)
        {
            if (manager != null && manager.mode != GameManager.Mode.PLAY) return; // ゲーム中のみ
            if (tutorialManager != null && tutorialManager.mode != TutorialManager.TutorialoMode.MOVE) return; // チュートリアル中のみ

            // 歩きかダッシュかでパラメータ切り替え
            bool isDashing = playerMoving.isDashing;
            float bobSpeed = isDashing ? dashBobbingSpeed : walkBobbingSpeed;
            float bobAmount = isDashing ? dashBobbingAmount : walkBobbingAmount;

            timer += Time.deltaTime * bobSpeed;

            // 上下 (非対称にするため Mathf.Sin(timer) のみ使用)
            float newY = defaultPosY + Mathf.Sin(timer) * bobAmount;

            // 左右揺れ (Cosで位相をずらす)
            float newX = defaultPosX + Mathf.Cos(timer * 0.5f) * bobAmount * 0.5f;

            transform.localPosition = new Vector3(newX, newY, transform.localPosition.z);

            // ロール揺れも軽く追加
            float roll = Mathf.Cos(timer) * bobAmount * 10f;

            // HeadBobberがlocalRotationをZ軸だけで上書きしており、カメラの上下左右の回転が失われ、
            // Raycast方向がズレていた(水平方向にしかRayが飛ばない場合がある)ため、X,Yの回転は維持するように修正
            // 原因に関するキーワード : Unity　カメラ回転, localRotaion上書き, Quaternion.Euler, ピッチ, ヨー, ロール
            // 解決策に関するキーワード : Quaternionの合成, カメラの正しいforward取得, ピッチ, ヨー, ロール個別制御

            // 現在の回転を取得
            Vector3 currentEuler = transform.localEulerAngles;

            // X,Yの回転は維持し、Zの回転のみ変更
            transform.localRotation = Quaternion.Euler(currentEuler.x, currentEuler.y, roll);
        }
        else
        {
            if(manager != null && manager.mode != GameManager.Mode.PLAY) return; // ゲーム中のみ
            if(tutorialManager != null && tutorialManager.mode != TutorialManager.TutorialoMode.MOVE) return; // チュートリアル中のみ
            timer = 0;
            // 元の位置に戻す
            Vector3 targetPos = new Vector3(defaultPosX, defaultPosY, transform.localPosition.z);
            transform.localPosition = Vector3.Lerp(transform.localPosition,
                targetPos, Time.deltaTime * walkBobbingSpeed);
            transform.localRotation = Quaternion.Lerp(transform.localRotation,
                Quaternion.identity, Time.deltaTime * walkBobbingSpeed);

            // X,Yの回転は維持し、Zの回転のみリセット
            Vector3 currentEuler = transform.localEulerAngles;
            transform.localRotation = Quaternion.Lerp(
                transform.localRotation,
                Quaternion.Euler(currentEuler.x, currentEuler.y, 0),
                Time.deltaTime * walkBobbingSpeed);
        }

    }

    // 回転をリセットするメソッド (シーン切り替え時などに使用
    public void ResetRotation()
    {
        isResetting = true;
        transform.localRotation = Quaternion.identity;
    }
}
