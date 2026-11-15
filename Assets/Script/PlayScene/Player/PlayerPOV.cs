using UnityEngine;

// カメラの視点移動を管理するクラス
// 25/10/08 リファクタリング
public class PlayerPOV : MonoBehaviour
{
    [Header("スクリプト参照(共通)")]
    [SerializeField] private PlayerManager player;

    [Header("スクリプト参照(Play)")]
    [SerializeField] private GameManager gameManager;

    [Header("スクリプト参照(Tutorial)")]
    [SerializeField] private TutorialManager tutorialManager;

    [Header("コンポーネント参照")]
    public Transform neck; // プレイヤーの首のTransformを指定

    [Header("設定")]
    [SerializeField] private float MouseSensitivity = 2.0f;
    [SerializeField] private float PadSensitivity = 2.0f;
    [SerializeField] private float minVertical = -80.0f;    // 視点の最小角度（縦の回転制限）
    [SerializeField] private float maxVertical = 80.0f;     // 視点の最大角度（縦の回転制限）
    private float rotationX = 0f;         // 縦方向の回転角度（首の回転）
    private float recoilOffset = 0f;      // リコイルオフセット（銃の反動による視点のずれ）
    public bool isCamShake = true;        // カメラシェイクの有効/無効

    // PlayerPrefsのキー定数
    public const string MauseSensitivityKey = "MouseSensitivity";
    public const string PadSensitivityKey = "PadSensitivity";
    public const string CamShakeKey = "CamShake";

    /// <summary>
    /// リコイルを加算するメソッド
    /// </summary>
    public void AddRecoil(float amount)
    {
        recoilOffset += amount;
        rotationX += recoilOffset; // リコイルオフセットを加算（銃の反動による視点のずれを適用）
        recoilOffset = 0f; // リコイルオフセットをリセット（次のフレームで再度適用するため）
    }

    /// <summary>
    /// 視点移動の処理
    /// </summary>
    public void CameraMove()
    {
        // マウス入力の取得
        float mouseX = Input.GetAxis("Mouse X") * MouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * MouseSensitivity;

        // コントローラー入力の取得
        mouseX += Input.GetAxis("RightStick X") * PadSensitivity;
        mouseY += Input.GetAxis("RightStick Y") * PadSensitivity;

        // Player（体）の回転（左右）
        transform.Rotate(0, mouseX, 0);   // プレイヤー（体）の左右の回転をマウスX方向の入力に合わせて行う

        // Neck（首）の回転（上下）
        rotationX -= mouseY; // マウスY方向の入力によって縦方向の回転を更新

        rotationX = Mathf.Clamp(rotationX, minVertical, maxVertical);   // 回転角度を指定された範囲に制限
        neck.localRotation = Quaternion.Euler(rotationX, 0, 0);         // 首の回転を設定。縦方向のみ回転させる

    }

    /// <summary>
    /// カメラ設定を初期化
    /// </summary>
    public void Initialize()
    {
        // カーソルを非表示＆ロック
        Cursor.lockState = CursorLockMode.Locked;   // カーソルを画面中央に固定
        Cursor.visible = false;                     // カーソルを非表示にする

        // 設定を反映
        MouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 2.0f);
        PadSensitivity = PlayerPrefs.GetFloat("PadSensitivity", 2.0f);
        isCamShake = PlayerPrefs.GetInt(CamShakeKey, 1) == 1;
    }

    /// <summary>
    /// Neckの回転をリセットするメソッド
    /// </summary>
    public void ResetNeckRotation()
    {
        if (neck != null)
        {
            rotationX = 0f;
            neck.localRotation = Quaternion.identity;
        }
    }
}
