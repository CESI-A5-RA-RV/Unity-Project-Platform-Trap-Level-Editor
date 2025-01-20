using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleSpawner : MonoBehaviour
{
    public GameObject bubblePrefab;
    public float respawnDelay = 4f;
    // Start is called before the first frame update
    public void StartRespawn(GameObject bubble)
    {
        bubble.SetActive(false);
        StartCoroutine(RespawnBubble(bubble));
    }

    private IEnumerator RespawnBubble(GameObject bubble){
        yield return new WaitForSeconds(respawnDelay);
        bubble.SetActive(true);
    }
}
