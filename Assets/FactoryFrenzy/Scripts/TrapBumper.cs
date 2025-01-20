using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapBumper : MonoBehaviour
{
    [SerializeField] float bounceForce;
    [SerializeField] float bounceMultiplier;
    private AudioSource audioSource;

    private void Start(){
        audioSource = GetComponentInParent<AudioSource>();
    }

    /*private void OnCollisionEnter(Collision collision){
        if(collision.gameObject.CompareTag("Player")){
          
            Vector3 bounceDirection = collision.transform.position - transform.position;
            bounceDirection.Normalize();

            ThirdPersonController playerControl = collision.collider.GetComponent<ThirdPersonController>();
            Rigidbody player = collision.gameObject.GetComponent<Rigidbody>();
            float finalForce = bounceForce;
            if(player != null && playerControl.isGrounded){
                finalForce *= bounceMultiplier;
                
            }
            audioSource.Play();
            player.AddForce(bounceDirection * finalForce, ForceMode.Impulse);
        }
    }*/

}
