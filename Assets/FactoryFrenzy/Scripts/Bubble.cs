using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class Bubble : MonoBehaviour
{
    //Add animations for player when stuck and for when bubble pop
    public float stopDuration = 3f;

    public float respawnDelay = 4f;
    // private bool playerTrapped = false;

    private BubbleSpawner spawner;
    AudioSource audioSource;

    private void Start(){
        spawner = FindObjectOfType<BubbleSpawner>();
        audioSource = gameObject.GetComponent<AudioSource>();
    }

    /*private void OnTriggerEnter(Collider collider){
        if(collider.gameObject.CompareTag("Player") && !playerTrapped){

            Rigidbody player = collider.gameObject.GetComponent<Rigidbody>();
            ThirdPersonController playerMove = collider.GetComponent<ThirdPersonController>();
            if(playerMove != null){
                audioSource.Play();
                StartCoroutine(StopPlayer(player, playerMove,  collider.transform)); 
            }
        }
    }

    private IEnumerator StopPlayer(Rigidbody rbPlayer, ThirdPersonController player,  Transform playerPosition)
    {
        playerTrapped = true;
        // Stop the player's velocity
        rbPlayer.velocity *= 0f;
        player.DisableMovement();
        
        playerPosition.position = transform.position + new UnityEngine.Vector3(0f, -0.75f, 0f);

        StartCoroutine(FloatingEffect(playerPosition));
        StartCoroutine(BubblePulsingEffect());

        Animator playerAnimator = rbPlayer.GetComponent<Animator>();
        if(playerAnimator != null){
            playerAnimator.SetTrigger("StuckBubble");
        }
        
        // Wait for the bubble to pop
        yield return new WaitForSeconds(stopDuration);

        // Return the player's velocity to the original speed
        player.EnableMovement();
        playerTrapped = false;
        if(playerAnimator != null){
            playerAnimator.ResetTrigger("StuckBubble");
        }
        
        spawner.StartRespawn(gameObject);
        
    }

    private IEnumerator FloatingEffect(Transform playerTransform){
        float floatAmplitude = 0.2f;
        float floatFrequency = 2f;
        UnityEngine.Vector3 startPosition = playerTransform.position;

        while(playerTrapped){
            float offsetY = Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
            playerTransform.position = startPosition + new UnityEngine.Vector3(0, offsetY, 0);
            yield return null;
        }
    }

    private IEnumerator BubblePulsingEffect(){
        float pulseSpeed = 1.6f;
        float pulseScale = 0.1f;
        UnityEngine.Vector3 originalScale = transform.localScale;

        while(playerTrapped){
            float scaleOffset = Mathf.Sin(Time.time * pulseSpeed) * pulseScale;
            transform.localScale = originalScale + new UnityEngine.Vector3(scaleOffset, scaleOffset, scaleOffset);
            yield return null;
        }

        transform.localScale = originalScale;
    }*/
}
