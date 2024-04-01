using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace FPS.Weapons
{
    public class GroundWeapon : NetworkBehaviour
    {
        public int weaponIndex = -1;
        public bool despawnOnPickup = true;
        public int PickupWeapon()
        {
            if (despawnOnPickup)
                DespawnWeapon();
            return weaponIndex;
        }
        [ServerRpc(RequireOwnership = false)]
        private void DespawnWeapon()
        {
            ServerManager.Despawn(gameObject);
        }
    }
}