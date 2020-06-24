
using UnityEngine;
using Nullspace;
using System.Collections;

public partial class TestNetwork : MonoBehaviour
{
    private static AbstractNetworkClient mClient = null;

    private string mIP = "127.0.0.1";
    private short mPort = 9898;

    private void Awake()
    {
        NetworkCommandHandler.Instance.Initialize();
        StartCoroutine(Connect());
    }

    private void Update()
    {

    }


    private IEnumerator Connect()
    {
        yield return new WaitForSeconds(1);
#if UNITY_EDITOR
        mClient = new NetworkSynClient(mIP, mPort);
#else
        mClient = new NetworkCppClient(mIP, mPort);
#endif
        mClient.Start();
        yield return null;
    }

}
