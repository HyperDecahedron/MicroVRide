using UnityEngine;

public class FinishTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("[Finish Trigger] Called Finish Ride");
            GameManager.Instance.FinishRide();
        }
    }
}
