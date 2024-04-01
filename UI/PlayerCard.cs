using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
namespace FPS.UI
{
    public class PlayerCard : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText, killsText, deathsText;
        public void Initialize(string name)
        {
            nameText.text = name;
            killsText.text = "0";
            deathsText.text = "0";
        }
        public void SetName(string name)
        {
            nameText.text = name;
        }
        public string GetName()
        {
            return nameText.text;
        }
        public void SetKills(int kills)
        {
            killsText.text = kills.ToString();
        }
        public void SetDeaths(int deaths)
        {
            deathsText.text = deaths.ToString();
        }
    }
}