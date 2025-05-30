using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class BulletController : NetworkBehaviour{

    private float damage;
    [SerializeField] private AudioClip hitmarkerSound;

    public void SetDamage(float damage)
    {
        this.damage = damage;
    }   
    
    private void OnCollisionEnter(Collision collision){
        Runner.Despawn(GetComponent<NetworkObject>());

        var rootParent = RootParent.GetRootParent(collision.gameObject);
        // Debug.Log("hit on: " + rootParent);
        if(rootParent.CompareTag("Enemy")){
            ZombieController zombie = rootParent.transform.GetComponent<ZombieController>();
            if(zombie.IsAlive){
                SoundFXManager.instance.PlaySoundFXClip(hitmarkerSound, GameObject.FindWithTag("Player").transform, 1f);
            }
            zombie.RpcDecreaseHealthBy(damage);
        }else if(rootParent.layer == 3){ //Terrain
            CreateBulletImpact(collision);
        }
    }

    private void CreateBulletImpact(Collision collision){
        ContactPoint contact = collision.contacts[0];

        GameObject hole = Instantiate(
            GlobalReferencesManager.Instance.bulletImpactPrefab,
            contact.point,
            Quaternion.LookRotation(contact.normal)
        );
        
        hole.transform.position = new Vector3(hole.transform.position.x, hole.transform.position.y, hole.transform.position.z - 0.1f);

        Destroy(hole, 10);
    }
}
