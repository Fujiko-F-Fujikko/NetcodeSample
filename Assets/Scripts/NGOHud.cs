using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class NGOHud : MonoBehaviour
{
  string ip;
  ushort port;

  UnityTransport transport => NetworkManager.Singleton.GetComponent<UnityTransport>();

  void Awake()
  {
    ip = PlayerPrefs.GetString("NGO_IP", "127.0.0.1");
    port = (ushort)PlayerPrefs.GetInt("NGO_Port", 7777);
  }

  // 入力は使わないので Update は不要（削除してOK）

  void OnGUI()
  {
    const int w = 260;
    GUILayout.BeginArea(new Rect(10, 10, w, 220), GUI.skin.box);

    var nm = NetworkManager.Singleton;
    string status = "OFFLINE";
    if (nm.IsHost) status = $"HOST  (localId:{nm.LocalClientId})";
    else if (nm.IsServer) status = "SERVER (listening)";
    else if (nm.IsClient) status = $"CLIENT (connected) localId:{nm.LocalClientId}";
    GUILayout.Label($"Status: {status}");

    GUILayout.Space(6);
    GUILayout.Label($"Addr: {ip}:{port}");
    GUILayout.BeginHorizontal();
    GUILayout.Label("IP", GUILayout.Width(25));
    ip = GUILayout.TextField(ip, GUILayout.Width(130));
    GUILayout.Label("Port", GUILayout.Width(35));
    var portStr = GUILayout.TextField(port.ToString(), GUILayout.Width(50));
    ushort.TryParse(portStr, out port);
    GUILayout.EndHorizontal();

    GUILayout.Space(6);

    if (!nm.IsListening)
    {
      if (GUILayout.Button("Start Host")) { ApplyConnection(ip, port, true); nm.StartHost(); }
      if (GUILayout.Button("Start Client")) { ApplyConnection(ip, port, false); nm.StartClient(); }
      if (GUILayout.Button("Start Server")) { ApplyConnection(ip, port, true); nm.StartServer(); }
    }
    else
    {
      if (GUILayout.Button("Shutdown")) nm.Shutdown();
    }

    GUILayout.EndArea();
  }

  void ApplyConnection(string addr, ushort prt, bool isServer)
  {
    PlayerPrefs.SetString("NGO_IP", addr);
    PlayerPrefs.SetInt("NGO_Port", prt);
    PlayerPrefs.Save();

    if (transport == null) { Debug.LogError("UnityTransport not found."); return; }

    if (isServer) transport.SetConnectionData("0.0.0.0", prt); // 全NICで待受け
    else transport.SetConnectionData(addr, prt);      // 指定先に接続

    Debug.Log($"[NGOHud] Set {addr}:{prt} (server={isServer})");
  }
}
