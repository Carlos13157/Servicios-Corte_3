using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalReferencesManager : MonoBehaviour{

    public static GlobalReferencesManager Instance {get; set;}

    public GameObject bulletImpactPrefab;

    private void Awake(){
        if(Instance != null && Instance != this){
            Destroy(gameObject);
        }else{
            Instance = this;
        }
    }
    
}
