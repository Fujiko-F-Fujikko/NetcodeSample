using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;
using Unity.XR.CoreUtils; // XROrigin がここに定義されている


/// <summary>
/// Quest3 の左スティックで移動、右スティックで回転する。
/// 自分が所有しているPlayerだけを動かす。
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerMovementVR : NetworkBehaviour
{
  [Header("移動設定")]
  public float moveSpeed = 2.0f;
  public float turnSpeed = 60f;

  private CharacterController controller;
  private XROrigin xrOrigin;

  private void Awake()
  {
    controller = GetComponent<CharacterController>();
    xrOrigin = GetComponentInChildren<XROrigin>();
  }

  void Update()
  {
    if (!IsOwner) return; // 自分のPlayer以外は動かさない

    // 左右スティック入力をXR Inputから取得
    Vector2 moveInput = Vector2.zero;
    Vector2 turnInput = Vector2.zero;

    InputDevice leftHand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
    InputDevice rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

    leftHand.TryGetFeatureValue(CommonUsages.primary2DAxis, out moveInput);
    rightHand.TryGetFeatureValue(CommonUsages.primary2DAxis, out turnInput);

    // 移動方向をヘッドの向きに合わせる
    Transform head = xrOrigin.Camera.transform;
    Vector3 forward = new Vector3(head.forward.x, 0, head.forward.z).normalized;
    Vector3 right = new Vector3(head.right.x, 0, head.right.z).normalized;

    Vector3 move = (forward * moveInput.y + right * moveInput.x) * moveSpeed;

    // CharacterControllerで移動（重力はお好みで）
    controller.Move(move * Time.deltaTime);

    // 右スティックの横方向で回転
    if (Mathf.Abs(turnInput.x) > 0.2f)
    {
      transform.Rotate(Vector3.up, turnInput.x * turnSpeed * Time.deltaTime);
    }
  }
}
