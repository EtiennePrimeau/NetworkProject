using UnityEngine;

public class TriggerForPlayer : MonoBehaviour
{
    [field: SerializeField] public E_TriggerTypes TriggerType { get; set; }

    private void OnTriggerEnter(Collider other)
    {
        var runnerSM = other.transform.root.gameObject.GetComponentInChildren<RunnerSM>();

        //Debug.Log(runnerSM);
        if (runnerSM != null)
        {
            GameObject player = other.gameObject.transform.root.gameObject;
            //Debug.Log("sending cmd");
            NetworkManagerCustom.Instance.MatchManager.CMD_SendPlayerAndTrigger(player, TriggerType);
        }       
    }
}
