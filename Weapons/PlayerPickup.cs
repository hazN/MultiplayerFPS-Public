using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace FPS.Weapons
{
    public class PlayerPickup : NetworkBehaviour
    {
        [SerializeField] private float pickupRange = 3f;
        [SerializeField] private LayerMask pickupLayer;
        private PlayerWeapon playerWeapon;
        private Camera playerCamera;
        public override void OnStartClient()
        {
            base.OnStartClient();
            if (!base.IsOwner)
            {
                enabled = false;
                return;
            }
            if (TryGetComponent(out PlayerWeapon playerWeapon))
                this.playerWeapon = playerWeapon;
            else Debug.LogError("PlayerWeapon not found on PlayerPickup");
            // Camera
            playerCamera = GetComponentInChildren<Camera>();
        }
        // Called through input system event
        public void Pickup()
        {
            if (!base.IsOwner)
                return;
            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, pickupRange, pickupLayer))
            {
                if (hit.collider.TryGetComponent(out GroundWeapon groundWeapon))
                {
                    playerWeapon.InitializeWeapon(groundWeapon.PickupWeapon());
                }
            }
        }
    }
}