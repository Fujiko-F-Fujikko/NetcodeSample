using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class NGOHud : MonoBehaviour
{
  [Header("Transport")]
  public string address = "127.0.0.1";
  public ushort port = 7777;

  [Header("UI")]
  public int width = 260;
  public int lineH = 26;
  public int pad = 10;

  string _status = "OFFLINE";
  string _reason = "";

  void Start()
  {
    var nm = NetworkManager.Singleton;
    if (nm == null) { _status = "No NetworkManager"; return; }

    // Transport 設定を反映
    if (nm.NetworkConfig.NetworkTransport is UnityTransport utp)
    {
      utp.ConnectionData.Address = address;
      utp.ConnectionData.Port = port;
    }

    // 状態変更イベント
    nm.OnServerStarted += UpdateStatus;
    nm.OnServerStopped += _ => UpdateStatus();
    nm.OnClientConnectedCallback += _ => UpdateStatus();
    nm.OnClientDisconnectCallback += _ =>
    {
      // 切断理由（NGO 2.x 以降）
      _reason = string.IsNullOrEmpty(nm.DisconnectReason) ? "Unknown" : nm.DisconnectReason;
      UpdateStatus();
    };

    UpdateStatus();
  }

  void OnDestroy()
  {
    var nm = NetworkManager.Singleton;
    if (nm == null) return;
    nm.OnServerStarted -= UpdateStatus;
    nm.OnServerStopped -= _ => UpdateStatus();
    nm.OnClientConnectedCallback -= _ => UpdateStatus();
    nm.OnClientDisconnectCallback -= _ => UpdateStatus();
  }

  void UpdateStatus()
  {
    var nm = NetworkManager.Singleton;
    if (nm == null) { _status = "No NetworkManager"; return; }

    if (!nm.IsClient && !nm.IsServer) { _status = "OFFLINE"; return; }

    if (nm.IsHost) { _status = nm.IsListening ? "HOST (listening)" : "HOST (starting)"; return; }
    if (nm.IsServer) { _status = nm.IsListening ? "SERVER (listening)" : "SERVER (starting)"; return; }

    // Client
    if (nm.IsConnectedClient) _status = $"CLIENT (connected) LocalId:{nm.LocalClientId}";
    else _status = "CLIENT (connecting...)";
  }

  void OnGUI()
  {
    int x = pad, y = pad;
    GUI.Box(new Rect(x - 6, y - 6, width + 12, 200), GUIContent.none);

    GUI.Label(new Rect(x, y, width, lineH), $"Status: {_status}");
    y += lineH + 4;
    GUI.Label(new Rect(x, y, width, lineH), $"Addr: {address}:{port}");
    y += lineH + 10;

    var nm = NetworkManager.Singleton;
    if (nm == null)
    {
      if (GUI.Button(new Rect(x, y, width, lineH), "No NetworkManager")) { }
      return;
    }

    // オフライン時：起動ボタン
    if (!nm.IsClient && !nm.IsServer)
    {
      if (GUI.Button(new Rect(x, y, width, lineH), "Start Host")) nm.StartHost();
      y += lineH + 4;
      if (GUI.Button(new Rect(x, y, width, lineH), "Start Client")) nm.StartClient();
      y += lineH + 4;
      if (GUI.Button(new Rect(x, y, width, lineH), "Start Server")) nm.StartServer();
      y += lineH + 4;
    }
    else
    {
      // オンライン時：停止/再試行
      if (GUI.Button(new Rect(x, y, width, lineH), "Shutdown")) nm.Shutdown();
      y += lineH + 6;

      // Client で未接続 → 理由表示 + 再試行
      if (nm.IsClient && !nm.IsConnectedClient)
      {
        if (!string.IsNullOrEmpty(_reason))
        {
          GUI.Label(new Rect(x, y, width, lineH), $"Last reason: {_reason}");
          y += lineH + 4;
        }
        if (GUI.Button(new Rect(x, y, width, lineH), "Retry Connect"))
        {
          _reason = "";
          nm.Shutdown();
          nm.StartClient();
        }
      }
    }
  }
}
