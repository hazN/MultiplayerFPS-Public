using FPS.GameManager;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ConnectedHostIDSetter : MonoBehaviour
{
    [SerializeField] private GameObject canvas;
    private TextMeshProUGUI text;
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        text = GetComponent<TextMeshProUGUI>();
    }
    private void Start()
    {
        SetHostID();
    }
    public void SetHostID()
    {
        text.text = ConnectionManager.GetJoinCode();
    }
    private void Update()
    {
        SetHostID();
    }
}
