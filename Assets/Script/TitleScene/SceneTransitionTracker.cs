using UnityEngine;
using UnityEngine.SceneManagement;

// シーン遷移を追跡するクラス。TitleScene遷移時に前のシーン名を取得するために使用。
public class SceneTransitionTracker : MonoBehaviour
{
    // static : 静的変数。クラス自体に属し、インスタンス化せずにアクセス可能。シーンを跨いで値を保持できる。

    // どのシーンが読み込まれるよりも前に呼ばれるメソッド。アタッチしなくても動作する。
    /* RuntimeInitializeOnLoadMethod : Unityの特定のタイミングでメソッドを自動的に呼び出す属性。この場合、
       シーンが読み込まれる前に呼ばれる。*/

    /* RuntimeInitializeLoadType.BeforeSceneLoad : 呼び出すタイミングを指定する列挙型。
       シーンが読み込まれる前にメソッドを実行することを示す。 */

    /* BeforeSceneLoad : シーンが読み込まれる前にメソッドを実行することを示す。 */

    /* SceneManager.activeSceneChanged : シーンが変更されたときに発生するイベント。
       2つの引数（前のシーンと次のシーン）を受け取り、シーンが変更されるたびに呼び出される。
       シーンの記録はInitializeメソッド内で自動で行われる。
    */
    public static string PreviousSceneName { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        // アクティブシーンの変更時・シーンのアンロード時に前のシーン名を記録する

        // アクティブシーンが変わったとき
        SceneManager.activeSceneChanged += (prev, next) =>
        {
            if (prev.IsValid())
            {
                PreviousSceneName = prev.name;
                Debug.Log($"[activeSceneChanged] 前のシーン: {PreviousSceneName}, 次のシーン: {next.name}");
            }
        };

        // シーンがアンロードされたとき（アクティブでなかった場合も拾える）
        SceneManager.sceneUnloaded += (scene) =>
        {
            PreviousSceneName = scene.name;
            Debug.Log($"[sceneUnloaded] 前のシーン: {PreviousSceneName}");
        };
    }
}
