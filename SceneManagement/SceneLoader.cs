using FishNet;
using FishNet.Managing.Logging;
using FishNet.Managing.Scened;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FPS.SceneManagement
{
    public class SceneLoader : MonoBehaviour
    {
        public string SCENE_NAME = "NewScene";
        [SerializeField] private NetworkObject[] persistentNetworkObjects = new NetworkObject[] { };

        [Server(Logging = LoggingType.Off)]
        private void OnTriggerEnter(Collider other)
        {
            NetworkObject nob = other.GetComponent<NetworkObject>();
            if (nob == null)
                return;
            LoadScene(nob);
        }

        public void LoadScene(NetworkObject nob)
        {
            if (!nob.Owner.IsActive)
                return;

            SceneLoadData sld = new SceneLoadData(SCENE_NAME);
            sld.MovedNetworkObjects = new NetworkObject[] { nob };
            sld.ReplaceScenes = ReplaceOption.All;
            InstanceFinder.SceneManager.LoadConnectionScenes(nob.Owner, sld);
        }
        public void LoadScene()
        {
            if (!InstanceFinder.NetworkManager.IsServer)
                return;

            //Load options.
            LoadOptions loadOptions = new LoadOptions
            {
                AutomaticallyUnload = true,
            };

            //Make scene data.
            SceneLoadData sld = new SceneLoadData(SCENE_NAME);
            sld.PreferredActiveScene = sld.SceneLookupDatas[0];
            sld.ReplaceScenes = ReplaceOption.All;
            sld.Options = loadOptions;
            sld.MovedNetworkObjects = persistentNetworkObjects;
            InstanceFinder.SceneManager.LoadGlobalScenes(sld);
        }
    }
}