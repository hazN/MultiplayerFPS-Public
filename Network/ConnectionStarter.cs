using FishNet;
using FishNet.Transporting;
using FishNet.Transporting.Tugboat;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ConnectionStarter : MonoBehaviour
{
#if UNITY_EDITOR
    private Tugboat _tugboat;
    private void OnEnable()
    {
        InstanceFinder.ClientManager.OnClientConnectionState += OnClientConnectionState;
    }
    private void OnDisable()
    {
        InstanceFinder.ClientManager.OnClientConnectionState -= OnClientConnectionState;
    }
    private void OnClientConnectionState(ClientConnectionStateArgs args)
    {
        if (args.ConnectionState == LocalConnectionState.Stopping)
        {
            UnityEditor.EditorApplication.isPlaying = false;
        }
    }
    private void Start()
    {
        if (TryGetComponent(out Tugboat _t))
        {
            _tugboat = _t;
        }
        else
        {
            Debug.LogError("No Tugboat found on " + gameObject.name);
            return;
        }

        if (ParrelSync.ClonesManager.IsClone())
        {
            _tugboat.StartConnection(false);
        }
        else
        {
            _tugboat.StartConnection(true);
            _tugboat.StartConnection(false);
        }
    }
#endif
}
