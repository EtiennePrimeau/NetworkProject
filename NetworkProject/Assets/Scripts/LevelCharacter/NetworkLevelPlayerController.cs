using Mirror;
using UnityEngine;

public class NetworkLevelPlayerController : NetworkBehaviour
{

    [TargetRpc]
    public void TargetMovePlayerArrow(NetworkConnectionToClient target, int index, bool selectingBomb)
    {
        //Debug.Log("in rpc target");

        var manager = transform.root.GetComponentInChildren<ShooterUIManager>();

        if (manager == null)
        {
            Debug.Log("target manager null");
            return;
        }

        manager.MovePlayerArrow(index, selectingBomb);
    }

}
