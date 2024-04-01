using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Connection;
using FishNet.Object;
using FPS.Weapons;
using System.Runtime.CompilerServices;
using FPS.GameManager;
using TMPro;
using FPS.Player;
using FishNet.Object.Synchronizing;
using FishNet;
using Unity.VisualScripting;
using Cinemachine;

public class PlayerController : NetworkBehaviour
{
    public static Dictionary<int, PlayerController> Players = new Dictionary<int, PlayerController>();
    [Header("Base setup")]
    public float walkingSpeed = 7.5f;
    public float runningSpeed = 11.5f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;
    [SerializeField] public int playerSelfLayer = 7;
    [SerializeField] private List<GameObject> playerModels = new List<GameObject>();
    CharacterController characterController;
    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;

    [HideInInspector]
    public bool canMove = true;

    [SerializeField] private float cameraYOffset = 0.4f;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Animator anim;
    [SyncVar][SerializeField] private string username;
    public override void OnStartClient()
    {
        base.OnStartClient();
        Players.Add(OwnerId, this);

        if (base.IsOwner)
        {
            // Setup username
            PlayerUsername[] usernames = FindObjectsByType<PlayerUsername>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (usernames.Length == 0)
            {
                username = OwnerId.ToString();
                Debug.LogError("No username text GameObject found");
            }
            else username = usernames[0].GetComponent<TextMeshProUGUI>().text;
        }
        PlayerManager.InitializeNewPlayer(OwnerId, username);
        GameUIManager.PlayerJoined(OwnerId, username);

        if (base.IsOwner)
        {
            playerCamera.transform.position = new Vector3(transform.position.x, transform.position.y + cameraYOffset, transform.position.z);
            //playerCamera.transform.SetParent(transform);

            if (TryGetComponent(out PlayerWeapon playerWeapon))
            {
                playerWeapon.InitializeWeapons(playerCamera.transform);
            }
            gameObject.layer = playerSelfLayer;
            foreach (var obj in playerModels)
            {
                obj.layer = playerSelfLayer;
                foreach (Transform child in obj.transform)
                {
                    child.gameObject.layer = playerSelfLayer;
                }
            }
        }
        else
        {
            var enemyCamera = gameObject.GetComponentInChildren<Camera>();
            enemyCamera.enabled = false;
            gameObject.GetComponent<PlayerController>().enabled = false;
            gameObject.GetComponentInChildren<AudioListener>().enabled = false;
   
        }
    }
    public override void OnStopClient()
    {
        base.OnStopClient();
        Players.Remove(OwnerId);
        PlayerManager.PlayerDisconnected(OwnerId);
        GameUIManager.PlayerLeft(OwnerId);
    }

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }
    void Start()
    {
        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (!canMove)
            return;

        bool isRunning = false;

        // Press Left Shift to run
        isRunning = Input.GetKey(KeyCode.LeftShift);

        // We are grounded, so recalculate move direction based on axis
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        float curSpeedX = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpSpeed;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // Move the controller
        characterController.Move(moveDirection * Time.deltaTime);
        
        // Animator
        Vector3 localVelocity = transform.InverseTransformDirection(characterController.velocity);
        anim.SetFloat("VelocityX", localVelocity.x);
        anim.SetFloat("VelocityZ", localVelocity.z);
        // Player and Camera rotation
        if (canMove && playerCamera != null)
        {
            // If mouse button down add some random recoil to the rotation 
            if (Input.GetMouseButtonDown(0))
            {
                rotationX += Random.Range(-0.5f, 0.5f);
            }
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
    }
    public void SetUsername(string username)
    {
        this.username = username;
    }
    public static void SetPlayerPosition(int clientID, Vector3 position)
    {
        if (!Players.TryGetValue(clientID, out PlayerController player))
            return;

        player.SetPlayerPositionServer(position);
    }
    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerPositionServer(Vector3 position)
    {
        SetPlayerPositionTarget(Owner, position);
    }
    [TargetRpc]
    private void SetPlayerPositionTarget(NetworkConnection conn, Vector3 position)
    {
        transform.position = position;
    }
    public static void TogglePlayer(int clientID, bool toggle)
    {
        if (!Players.TryGetValue(clientID, out PlayerController player))
            return;
        player.TogglePlayerServer(toggle);
    }
    [ServerRpc(RequireOwnership = false)]
    private void TogglePlayerServer(bool toggle)
    {
        DisablePlayerObserver(toggle);
    }
    [ObserversRpc]
    private void DisablePlayerObserver(bool toggle)
    {
        canMove = toggle;
        if(TryGetComponent(out Renderer playerRenderer))
        {
            playerRenderer.enabled = toggle;
        }
        if (TryGetComponent(out CapsuleCollider collider))
        {
            collider.enabled = toggle;
        }
        characterController.enabled = toggle;
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach(Renderer renderer in renderers)
        {
            renderer.enabled = toggle;

        }
    }
}
