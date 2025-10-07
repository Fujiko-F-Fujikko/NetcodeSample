using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 各Playerが所有している場合のみ、そのPlayerにアタッチされた
/// XRカメラ（または一人称カメラ）を有効化するスクリプト。
/// Serverや他のClientには無効化されます。
/// </summary>
public class PlayerOwnedCamera : NetworkBehaviour
{
  [Header("Assign in Prefab")]
  [Tooltip("プレイヤーの子オブジェクトにある XRCameraRig (またはCamera Rig) を指定")]
  public GameObject xrCameraRig;

  [Tooltip("上記Rig内の Camera コンポーネント")]
  public Camera xrCamera;

  [Tooltip("上記Rig内の AudioListener (あれば)")]
  public AudioListener xrAudio;

  private void Awake()
  {
    // 念のため全員無効状態からスタート
    SetXRActive(false);
  }

  public override void OnNetworkSpawn()
  {
    base.OnNetworkSpawn();

    // 所有者ならカメラを有効化
    SetXRActive(IsOwner);
  }

  public override void OnNetworkDespawn()
  {
    base.OnNetworkDespawn();

    // Despawn時に確実に無効化
    SetXRActive(false);
  }

  /// <summary>
  /// 所有権を獲得した瞬間に呼ばれる（Netcode 1.x〜2.5.x共通）
  /// </summary>
  public override void OnGainedOwnership()
  {
    base.OnGainedOwnership();
    SetXRActive(true);
  }

  /// <summary>
  /// 所有権を失った瞬間に呼ばれる（Netcode 1.x〜2.5.x共通）
  /// </summary>
  public override void OnLostOwnership()
  {
    base.OnLostOwnership();
    SetXRActive(false);
  }

  /// <summary>
  /// カメラRigとAudioListenerを有効/無効にする共通処理
  /// </summary>
  private void SetXRActive(bool active)
  {
    if (xrCameraRig != null)
      xrCameraRig.SetActive(active);

    if (xrCamera != null)
      xrCamera.enabled = active;

    if (xrAudio != null)
      xrAudio.enabled = active;
  }
}
