using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Bumper : MonoBehaviour
{
    [SerializeField] float bounceForce;
    private Animator animator;
    private AudioSource audioSource;

    private void Start(){
        animator = GetComponentInParent<Animator>();
        audioSource = GetComponentInParent<AudioSource>();
    }

    private void OnTriggerEnter(Collider collider){
        if(collider.gameObject.CompareTag("Player")){
            Vector3 bounceDirection = transform.forward;
            bounceDirection.Normalize();

            Rigidbody player = collider.gameObject.GetComponent<Rigidbody>();

            if(player != null){
                animator.SetTrigger("ActivateBumper");
                audioSource.Play();
                player.AddForce(bounceDirection * bounceForce, ForceMode.Impulse);
            }
        }
    }

    private void OnTriggerStay(Collider collider){
        if(collider.gameObject.CompareTag("Player")){
            Vector3 bounceDirection = transform.forward;
            bounceDirection.Normalize();

            Rigidbody player = collider.gameObject.GetComponent<Rigidbody>();

            if(player != null){
                player.AddForce(bounceDirection * bounceForce, ForceMode.Impulse);
            }
        }
    }
}
