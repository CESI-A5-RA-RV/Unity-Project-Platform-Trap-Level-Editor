using UnityEngine;

public class PressTrap : MonoBehaviour
{
    
    public Transform trapBottom;


    private KillPlayer killPlayer;

    private void Start(){
        killPlayer = FindObjectOfType<KillPlayer>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody rbPLayer = other.gameObject.GetComponent<Rigidbody>();
            if(rbPLayer.transform.position.y < trapBottom.transform.position.y){
                killPlayer.Kill(other);
            }
            
        }
    }

}
