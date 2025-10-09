using Unity.Netcode;
using UnityEngine;

public class KitchenSetSpawner : NetworkBehaviour
{
  [SerializeField] private NetworkObject kitchenPrefab; // Kitchen_set_net を割り当て

  public override void OnNetworkSpawn()
  {
    if (!IsServer) return;

    // 位置はお好みで
    var spawnPos = new Vector3(0f, -10f, 30f);
    var rot = new Vector3(90f, 0f, 0f);
    var scale = new Vector3(0.1f, 0.1f, 0.1f);
    var obj = Instantiate(kitchenPrefab, spawnPos, Quaternion.Euler(rot));
    obj.transform.localScale = scale;
    obj.Spawn(true); // 全クライアントへ出現
  }
}
