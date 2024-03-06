using Mirror;
using System;
using UnityEngine;

public class NetworkSpawner : NetworkBehaviour
{
    //[SerializeField] private bool m_isTesting = false;

    [SerializeField] private bool m_spawnPlatform = false;
    [SerializeField] private GameObject m_platformPrefab;
    
    //[SerializeField] private bool m_spawnDummy = false;
    //[SerializeField] private GameObject m_dummyPrefab;
    //[SerializeField] private Vector3 m_dummyPrefabPosition;
    
    //[SerializeField] private bool m_spawnCubes = false;
    //[SerializeField] private GameObject m_cubesPrefab;

    [SerializeField] private bool m_spawnMapBounds = false;
    [SerializeField] private GameObject m_verticalMapBoundsTrigger;

    [SerializeField] private bool m_spawnWinTrigger = false;
    [SerializeField] private GameObject m_winBoundTrigger;

    //[SerializeField] private bool m_spawnRotatingPlatform = false;
    //[SerializeField] private GameObject m_rotatingPlatformPrefab;
    //[SerializeField] private Vector3 m_rotatingPlatformPosition;


    [SerializeField] private Identifier m_identifier;


    //Works for testing

    /*public override void OnStartClient() // Was OnStartClient
    {
         base.OnStartClient();




         if (!m_isTesting)
         {
             return;

         }

         if (m_spawnPlatform)
         {
             GameObject platformInstance = Instantiate(m_platformPrefab, transform);
             //NetworkServer.Spawn(platformInstance);

         }
         if (m_spawnDummy)
         {
             GameObject dummyInstance = Instantiate(m_dummyPrefab, m_dummyPrefabPosition, Quaternion.identity, transform);
             //NetworkServer.Spawn(dummyInstance);

         }

         if (m_spawnCubes)
         {
             GameObject cubesInstance = Instantiate(m_cubesPrefab, transform);
             //NetworkServer.Spawn(cubesInstance);
         }

         if (m_spawnMapBounds)
         {
             GameObject mapBounds = Instantiate(m_verticalMapBoundsTrigger, transform);

             var boundsDetection = m_verticalMapBoundsTrigger.GetComponent<TriggerForPlayer>();
             if (boundsDetection != null)
             {
                 boundsDetection.TriggerType = E_TriggerTypes.OutOfVerticalMapBounds;
             }

             //NetworkServer.Spawn(mapBounds);

         }

         if (m_spawnWinTrigger)
         {
             GameObject winTrigger = Instantiate(m_winBoundTrigger, transform);
             //NetworkServer.Spawn(winTrigger);
         }

         if (m_spawnRotatingPlatform)
         {
             GameObject rotatingPlatform = Instantiate(m_rotatingPlatformPrefab, m_rotatingPlatformPosition, Quaternion.identity, transform);
             //NetworkServer.Spawn(rotatingPlatform);
         }


         m_identifier.AssignAllIds(transform);

    }*/

    //[Command(requiresAuthority = false)]
    public void Spawn()
    {
        if (!isServer)
        {
            return;
        }



        if (m_spawnPlatform)
        {
            Debug.Log("Instantiated at " + DateTime.Now.TimeOfDay);
            
            GameObject platformInstance = Instantiate(m_platformPrefab, transform);
            NetworkServer.Spawn(platformInstance);

        }
        if (m_spawnMapBounds)
        {
            GameObject mapBounds = Instantiate(m_verticalMapBoundsTrigger, transform);
            
            var boundsDetection = m_verticalMapBoundsTrigger.GetComponent<TriggerForPlayer>();
            if (boundsDetection != null)
            {
                boundsDetection.TriggerType = E_TriggerTypes.OutOfVerticalMapBounds;
            }

            NetworkServer.Spawn(mapBounds);

        }
        
        if (m_spawnWinTrigger)
        {
            GameObject winTrigger = Instantiate(m_winBoundTrigger, transform);
            //NetworkServer.Spawn(winTrigger);
        }

        m_identifier.AssignAllIds(transform);
    }
}
