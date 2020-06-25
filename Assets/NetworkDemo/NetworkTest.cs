
using UnityEngine;

namespace Nullspace
{
    public class NetworkTest : MonoBehaviour // : Singleton<NetworkTest>
    {
        private AbstractNetworkClient ClientSocket;
        private bool IsConnect = false;

        private void Awake()
        {
            ClientSocket = new NetworkSynClient("127.0.0.1", 9898);
            NetworkCommandHandler.Instance.Initialize();
        }


        private void Update()
        {
            NetworkCommandHandler.Instance.Update();
        }
        private void OnGUI()
        {
            GUILayout.Label("ConnectState: " + EnumUtils.EnumToString(ClientSocket.ConnectState));
            if (!IsConnect && GUILayout.Button("Connect Server"))
            {
                IsConnect = true;
                ClientSocket.Start();
            }
            else if (IsConnect && GUILayout.Button("Close Client"))
            {
                IsConnect = false;
                ClientSocket.Stop();
                ClientSocket = new NetworkSynClient("127.0.0.1", 9898);
            }
        }

        private void OnDestroy()
        {
            if (ClientSocket.ConnectState == ClientConnectState.Connectted)
            {
                ClientSocket.Stop();
            }
        }
    }
}

