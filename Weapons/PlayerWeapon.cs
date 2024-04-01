using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using FPS.GameManager;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace FPS.Weapons
{
    public class PlayerWeapon : NetworkBehaviour
    {
        [SerializeField] private List<APlayerWeapon> weapons = new List<APlayerWeapon>();
        [SerializeField] private APlayerWeapon currentWeapon;
        [SyncVar(Channel = Channel.Unreliable, OnChange = nameof(OnCurrentWeaponIndexChanged))]
        private int currentWeaponIndex = -1;
        private bool isFiring = false;
        [SerializeField] private InputActionReference fireAction, reloadAction;
        [SerializeField] private Transform rightHandTarget, leftHandTarget;
        public override void OnStartClient()
        {
            base.OnStartClient();

            if (!base.IsOwner)
            {
                enabled = false;
                return;
            }
        }
        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Alpha1))
            {
                InitializeWeapon(0);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                InitializeWeapon(1);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                InitializeWeapon(2);
            }
            if (fireAction.action.IsInProgress())
            {
                FireWeapon();
            }
            if (reloadAction.action.triggered)
            {
                Reload();
            }
            if (currentWeapon == null)
                return;
            rightHandTarget.SetPositionAndRotation(currentWeapon.rightHandIKTarget.position, currentWeapon.rightHandIKTarget.rotation);
            leftHandTarget.SetPositionAndRotation(currentWeapon.leftHandIKTarget.position, currentWeapon.leftHandIKTarget.rotation);
        }
        public void FireWeapon()
        {
            if (currentWeapon == null)
                return;
            if (!base.IsOwner)
                return;
            currentWeapon.Fire();
        }
        public void Reload()
        {
            if (currentWeapon == null)
                return;
            if (!base.IsOwner)
                return;
            currentWeapon.Reload();
        }
        public void InitializeWeapons(Transform parentOfWeapons)
        {
            //for (int i = 0; i < weapons.Count; i++)
            //{
            //    weapons[i].transform.SetParent(parentOfWeapons);
            //}
            InitializeWeapon(0);
        }
        public void InitializeWeapon(int weaponIndex)
        {
            SetWeaponIndex(weaponIndex);
        }
        public void InitializeEnemyWeapons(Transform parentOfWeapons)
        {
            for (int i = 0; i < weapons.Count; i++)
            {
                weapons[i].transform.SetParent(parentOfWeapons);
            }
        }
        [ServerRpc] private void SetWeaponIndex(int weaponIndex)
        {
            currentWeaponIndex = weaponIndex;
        }
        private void OnCurrentWeaponIndexChanged(int oldIndex, int newIndex, bool asServer)
        {
            for (int i = 0; i < weapons.Count; i++)
            {
                weapons[i].gameObject.SetActive(false);
            }

            if (weapons.Count > newIndex)
            {
                currentWeapon = weapons[newIndex];
                currentWeapon.gameObject.SetActive(true);
                GameUIManager.SetAmmoText($"{currentWeapon.GetAmmo()}/{currentWeapon.maxAmmo}");
            }
        }

    }
}