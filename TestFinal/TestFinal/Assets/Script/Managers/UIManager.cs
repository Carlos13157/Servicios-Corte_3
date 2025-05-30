using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [SerializeField] private List<TMP_Text> texts = new();
    public void UpdateCurrentBullets(int value){
        texts[0].text = $"Curent ammo: {value}";
    }
    
    public void UpdateCurrentReserve(int value){
        texts[1].text = $"Reserves: {value}";
    }
    
    public void UpdateAmmoType(string value){
        texts[2].text = $"Ammo type: {value}";
    }
}
