using FishNet.Managing;
using FishNet.Transporting.UTP;
using FPS.SceneManagement;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace FPS.GameManager
{
    public class ConnectionManager : MonoBehaviour
    {
        [SerializeField] private TMP_InputField connectionInput;
        [SerializeField] private NetworkManager _networkManager;
        [SerializeField] private List<GameObject> disableList = new List<GameObject>();
        private string joinCode;
        private static ConnectionManager instance;
        private FishyUnityTransport utp;
        private string playerId;
        public bool loadedIn = false;
        private void Awake()
        {
            if (instance != null)
            {
                Destroy(this);
                return;
            }
            instance = this;
            UnityServices.InitializeAsync();
            DontDestroyOnLoad(this);
        }

        public void UIStartHost()
        {
            Debug.Log("UIStartHost");
            StartHost();
        }

        public void UIStartClient()
        {
            Debug.Log("UIStartClient");
            StartClient(connectionInput.text);
        }

        public async Task StartHost()
        {
            await OnSignIn();
            Debug.Log("Starting Host");
            try
            {
                utp = (FishyUnityTransport)_networkManager.TransportManager.Transport;
            }
            catch (System.Exception e)
            {
                Debug.Log(e);
            }
            Debug.Log("Setup Relay Server");
            // Setup HostAllocation
            Allocation hostAllocation = null;
            try
            {
                hostAllocation = await RelayService.Instance.CreateAllocationAsync(4);
            }
            catch (System.Exception e)
            {
                Debug.Log(e);
            }
            try
            {
                var serverdata = new RelayServerData(hostAllocation, "dtls");
                utp.SetRelayServerData(serverdata);
            }
            catch (System.Exception e)
            {
                Debug.Log(e);
            }
            Debug.Log("Setup Relay Server Done");
            // Start Server Connection
            _networkManager.ServerManager.StartConnection();
            // Start Client Connection
            _networkManager.ClientManager.StartConnection();
            joinCode = await RelayService.Instance.GetJoinCodeAsync(hostAllocation.AllocationId);
            DisableGameObjects();
            Debug.Log("Starting Host Done");
            //GetComponent<SceneLoader>().LoadScene();

        }

        public async Task StartClient(string joinCode)
        {
            await OnSignIn();
            utp = (FishyUnityTransport)_networkManager.TransportManager.Transport;
            JoinAllocation joinAllocation = null;
            joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            try
            {
                var serverdata = new RelayServerData(joinAllocation, "dtls");
                utp.SetRelayServerData(serverdata);
            }
            catch (System.Exception e)
            {
                Debug.Log(e);
            }
            // Start Client Connection
            _networkManager.ClientManager.StartConnection();
            DisableGameObjects();
            GetComponent<SceneLoader>().LoadScene();
        }

        public static string GetJoinCode()
        {
            return instance.joinCode;
        }

        public async Task OnSignIn()
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            playerId = AuthenticationService.Instance.PlayerId;

            Debug.Log($"Signed in. Player ID: {playerId}");
        }

        private void DisableGameObjects()
        {
            foreach (GameObject go in disableList)
            {
                go.SetActive(false);
            }
        }
    }
}