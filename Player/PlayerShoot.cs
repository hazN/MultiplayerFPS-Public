using FishNet.Object;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShoot : NetworkBehaviour
{
    [SerializeField] private int damage = 10;
    [SerializeField] private float fireRate = 0.3f;
    [SerializeField] private InputActionReference fireButton;

    private bool canShoot = true;
    WaitForSeconds shootWait;
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!base.IsOwner)
            return;
        shootWait = new WaitForSeconds(fireRate);
    }

    private void Update()
    {
        if (!base.IsOwner)
            return;

    }

    private void Shoot()
    {
        print("Shoot");
        StartCoroutine(ShootCooldown());
    }
    IEnumerator ShootCooldown()
    {
        canShoot = false;
        yield return shootWait;
        canShoot = true;
    }
}