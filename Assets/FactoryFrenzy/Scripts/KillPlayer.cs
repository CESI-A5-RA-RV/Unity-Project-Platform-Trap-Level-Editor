using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillPlayer : MonoBehaviour
{
    public Vector3 lastCheckpoint;
    private GameObject player;
    private Rigidbody rbPlayer;
    // Start is called before the first frame update
    public void Kill(Collider other)
    {
                // get player that entered the zone
                player = other.gameObject;
                rbPlayer = player.GetComponent<Rigidbody>();
                Animator playerAnimator = other.GetComponent<Animator>();
                if(playerAnimator != null && rbPlayer != null){
                    rbPlayer.velocity = Vector3.zero;
                    rbPlayer.isKinematic = true;
                    playerAnimator.SetTrigger("Death");
                }
                Invoke(nameof(RespawnPlayer), 1.5f);
    }

    private void RespawnPlayer()
    {
        player.transform.position = lastCheckpoint;
        rbPlayer.isKinematic = false;
    }
}
