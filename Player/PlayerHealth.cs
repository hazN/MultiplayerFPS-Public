using FishNet.Connection;
using FishNet.Object;
using FPS.GameManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace FPS.Player
{
    public class PlayerHealth : NetworkBehaviour
    {
        public static Dictionary<int, PlayerHealth> Players = new Dictionary<int, PlayerHealth>();
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int currentHealth;
        private void Awake()
        {
            currentHealth = maxHealth;
        }
        public override void OnStartClient()
        {
            base.OnStartClient();
            Players.Add(OwnerId, this);

            if (!base.IsOwner)
            {
                enabled = false;
                return;
            }

            GameUIManager.SetHealthText(maxHealth.ToString());
        }
        public override void OnStopClient()
        {
            base.OnStopClient();
            Players.Remove(OwnerId);
        }
        [ServerRpc(RequireOwnership = false)]
        public void ResetHealth()
        {
            currentHealth = maxHealth;
            LocalSetHealth(Owner, currentHealth);
        }
        [ServerRpc(RequireOwnership = false)]
        public void TakeDamage(int damage, int attackerID)
        {
            currentHealth -= damage;


            if (currentHealth <= 0)
            {
                currentHealth = 0;
                Die(attackerID);
            }

            LocalSetHealth(Owner, currentHealth);
        }

        private void Die(int attackerID)
        {
            PlayerController.TogglePlayer(OwnerId, false);
            PlayerManager.PlayerDied(OwnerId, attackerID);
        }
        [TargetRpc]
        private void LocalSetHealth(NetworkConnection conn, int newHealth)
        {
            GameUIManager.SetHealthText(newHealth.ToString());
        }
    }
}