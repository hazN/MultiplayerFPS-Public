using FishNet.Example.ColliderRollbacks;
using FishNet.Object;
using FPS.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FPS.GameManager
{
    public class GameUIManager : NetworkBehaviour
    {
        private static GameUIManager instance;
        [SerializeField] private TMPro.TextMeshProUGUI healthText;
        [SerializeField] private TMPro.TextMeshProUGUI ammoText;
        [SerializeField] private GameObject scoreboard;
        [SerializeField] private GameObject pauseMenu;
        [SerializeField] private PlayerCard playerCardPrefab;
        [SerializeField] private Transform playerCardParent;
        
        private Dictionary<int, PlayerCard> playerCards = new Dictionary<int, PlayerCard>();
        [SerializeField] private InputActionReference tabAction, escAction;

        private void Awake()
        {
            //if (instance != null)
            //{
            //    Destroy(this);
            //    return;
            //}
            instance = this;
        }
        private void Update()
        {
            if (tabAction.action.IsInProgress())
            {
                scoreboard.SetActive(true);
            }
            else
            {
                scoreboard.SetActive(false);
            }
            if (escAction.action.IsInProgress())
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                pauseMenu.SetActive(true);
            }
            foreach (KeyValuePair<int, PlayerCard> playerCard in playerCards)
            {
                Debug.Log(instance.playerCards[playerCard.Key].GetName());
                if (playerCard.Value.GetName() != "")
                    instance.SetUsernameServer(playerCard.Key, instance.playerCards[playerCard.Key].GetName());
            }
        }
        public void QuitGame()
        {
            Application.Quit();
        }
        public void Resume()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            pauseMenu.SetActive(false);
        }
        public static void PlayerJoined(int clientID, string username)
        {
            PlayerCard newCard = Instantiate(instance.playerCardPrefab, instance.playerCardParent);
            instance.playerCards.Add(clientID, newCard);
            newCard.Initialize(username);
        }
        public static void PlayerLeft(int clientID)
        {
            Debug.Log("Player left: " + clientID);
            instance.PlayerLeftServer(clientID);
        }
        public static void SetHealthText(string hpText)
        {
            instance.healthText.text = hpText;
        }
        public static void SetAmmoText(string ammo)
        {
            instance.ammoText.text = ammo;
        }
        public static void SetUsername(int clientID, string username)
        {
            instance.SetUsernameServer(clientID, username);
        }

        public static void SetKills(int clientID, int kills)
        {
            instance.SetKillsServer(clientID, kills);
        }
        public static void SetDeaths(int clientID, int deaths)
        {
            instance.SetDeathsServer(clientID, deaths);
        }
        [ServerRpc(RequireOwnership = false)]
        private void PlayerLeftServer(int clientID)
        {
            Debug.Log("Player left Server: " + clientID);
            PlayerLeftObservers(clientID);
        }
        [ServerRpc(RequireOwnership = false)]
        private void SetUsernameServer(int clientID, string username)
        {
            SetUsernameObservers(clientID, username);
        }
        [ServerRpc(RequireOwnership = false)]
        private void SetKillsServer(int clientID, int kills)
        {
            SetKillsObservers(clientID, kills);
        }
        [ServerRpc(RequireOwnership = false)]
        private void SetDeathsServer(int clientID, int deaths)
        {
            SetDeathsObservers(clientID, deaths);
        }
        [ObserversRpc]
        private void PlayerLeftObservers(int clientID)
        {
            Debug.Log("Player left Observer: " + clientID);
            Destroy(instance.playerCards[clientID].gameObject);
        }
        [ObserversRpc]
        private void SetUsernameObservers(int clientID, string username)
        {
            instance.playerCards[clientID].SetName(username);
        }
        [ObserversRpc]
        private void SetKillsObservers(int clientID, int kills)
        {
            instance.playerCards[clientID].SetKills(kills);
        }
        [ObserversRpc]
        private void SetDeathsObservers(int clientID, int deaths)
        {
            instance.playerCards[clientID].SetDeaths(deaths);
        }
    }
}