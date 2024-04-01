using FishNet.Object;
using FishNet.Object.Synchronizing;
using FPS.Player;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
namespace FPS.GameManager
{
    public class PlayerManager : NetworkBehaviour
    {
        public static PlayerManager instance;
        private Dictionary<int, Player> players = new Dictionary<int, Player>();
        private List<Player> deadPlayers = new List<Player>();

        [SerializeField] private float respawnTime = 3f;
        [SerializeField] private List<Transform> spawnPoints = new List<Transform>();

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
            if (!IsServerInitialized)
                return;
            for (int i = 0; i < deadPlayers.Count; i++)
            {
                if (players[deadPlayers[i].clientID].deathTime < Time.time - respawnTime)
                {
                    RespawnPlayer(deadPlayers[i].clientID);
                    deadPlayers.RemoveAt(i);
                    return;
                }
            }
        }
        private void RespawnPlayer(int clientID)
        {
            PlayerController.SetPlayerPosition(clientID, spawnPoints[Random.Range(0, spawnPoints.Count)].position);
            PlayerController.TogglePlayer(clientID, true);
            if (PlayerHealth.Players.TryGetValue(clientID, out PlayerHealth playerHealth))
            {
                playerHealth.ResetHealth();
            }
        }
        public static async void InitializeNewPlayer(int clientID, string username)
        {
            Debug.Log("Player joined: " + username);
            instance.players.Add(clientID, new Player() { clientID = clientID, playerName = username});
            instance.RespawnPlayer(clientID);
        }
        public static void PlayerDisconnected(int clientID)
        {
            instance.players.Remove(clientID);
        }
        public static void PlayerDied(int playerID, int killerID)
        {
            if (instance.players.TryGetValue(killerID, out Player killerPlayer))
            {
                killerPlayer.kills++;
            }
            if (instance.players.TryGetValue(playerID, out Player player))
            {
                player.deaths++;
                player.deathTime = Time.time;
            }
            GameUIManager.SetKills(killerID, killerPlayer.kills);
            GameUIManager.SetDeaths(playerID, player.deaths);
            instance.deadPlayers.Add(player);
        }
        public static void ForceRespawn(int playerID)
        {
            instance.RespawnPlayer(playerID);
        }
        public static bool IsInitialized()
        {
            return instance != null;
        }

        public class Player
        {
            [SyncVar]
            public string playerName = "";
            public int clientID = -1;
            public int kills = 0;
            public int deaths = 0;
            public float deathTime = -99;
        }
    }
}