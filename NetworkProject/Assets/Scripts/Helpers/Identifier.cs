using System.Collections.Generic;
using UnityEngine;

public class Identifier : MonoBehaviour
{
    private int s_id;
    [SerializeField] private int m_instanceID;

    public List<Object> m_objectsList = new List<Object>();

    private int ReceiveID()
    {
        m_instanceID = s_id;
        s_id++;
        return m_instanceID;
    }
    
    public void AssignSingleId(Transform root)
    {
        Object obj = new Object(ReceiveID(), root.gameObject);
        m_objectsList.Add(obj);
        //Debug.Log(obj.gameObject.name + "   " + obj.uniqueID);

    }

    public void AssignAllIds(Transform root)
    {
        foreach (Transform child in root)
        {
            if (child.GetComponent<TriggerForPlayer>())
            {
                continue;
            }
            
            
            Object obj = new Object(ReceiveID(), child.gameObject);
            m_objectsList.Add(obj);
            Debug.Log(obj.gameObject.name + "   " + obj.uniqueID);

            if (child.childCount != 0)
            {
                AssignAllIds(child);
            }
        }
    }
    
    public int GetIndex(GameObject go)
    {
        foreach (var obj in m_objectsList)
        {
            if (obj.gameObject == go)
            {
                return obj.uniqueID;
            }
        }
        //Debug.Log("Didnt find ID");
        return -1;
    }

    public GameObject GetObjectAtIndex(int index)
    {
        if (index == -1)
        {
            //Debug.Log("Incorrect index");
            return null;
        }

        foreach (var obj in m_objectsList)
        {
            if (obj.uniqueID == index)
            {
                return obj.gameObject;
            }
        }
        //Debug.Log("Didnt find Object");
        return null;
    }
}

//[Serializable]
public struct Object
{
    public Object(int _uniqueID, GameObject _gameObject){
        gameObject = _gameObject;
        uniqueID = _uniqueID;
        }

    public int uniqueID;
    public GameObject gameObject;
}