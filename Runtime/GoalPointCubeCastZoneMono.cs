using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class GoalPointCubeCastZoneMono : GoalPointCubeCastZoneMonoGenetic<GoalFlagTagMono>
{ 

}

public class GoalPointCubeCastZoneMonoGenetic<T> : MonoBehaviour where T : MonoBehaviour
{
    public Transform m_goalDirection;
    public Transform m_cubeCastZone;
    public Transform m_cubeCastHalfExtendAnchor;
    public Dictionary<T, DroneCollisionWithEntry> m_dronesInCollision = new Dictionary<T, DroneCollisionWithEntry>();
    public List<DroneCollisionWithEntry> m_debugCollisionList = new List<DroneCollisionWithEntry>();

    public GameObject [] m_currentHitCastAll;

    public int          m_pointCountForDebug;
    public UnityEvent   m_onValideGoal;
    public Eloi.PrimitiveUnityEvent_Int m_onPointChanged;


    public DroneCollisionWithEntry m_lastValide;
    public DroneCollisionWithEntry m_lastFailed;

    [System.Serializable]
    public class DroneCollisionWithEntry {
        public T m_drone;
        public Vector3 m_rootWhenEnterPosition;
        public Vector3 m_rootCurrentPosition;
        public Vector3 m_rootWhenExitPosition;

        public DroneCollisionWithEntry(){}
        public DroneCollisionWithEntry(T drone) { m_drone = drone; }
    }


    public LayerMask m_maskToUse = ~1;



    public List<T> m_previousList= new List<T>();
    public List<T> m_currentList= new List<T>();
    public List<T> m_addedToList= new List<T>();
    public List<T> m_removeFromList= new List<T>();

    public void Update()
    {


        Vector3 half = m_cubeCastZone.InverseTransformPoint(m_cubeCastHalfExtendAnchor.position);
        RaycastHit [] hits = Physics.BoxCastAll(
            m_cubeCastZone.position, 
            half,
            m_cubeCastZone.forward, m_cubeCastZone.rotation,0, m_maskToUse);

        m_previousList = m_currentList;
        m_currentList = hits.Select(a => Get(a.collider.gameObject)).Where(a => a!=null).ToList();
        m_currentHitCastAll= hits.Select(a => a.collider.gameObject).ToArray();
        m_addedToList = m_currentList.Except(m_previousList).ToList();
        m_removeFromList = m_previousList.Except(m_currentList).ToList();

       
        foreach (T script in m_addedToList)
        {
            DetectedAsEntered(script);
        }
        foreach (T script in m_removeFromList)
        {
            DetectedAsExit(script);
        }

    }

    private T Get(GameObject gameObject)
    {
        if (gameObject == null) 
            return null;
        Rigidbody r = gameObject.GetComponent<Rigidbody>();
        if (r == null)
            r = gameObject.GetComponentInParent < Rigidbody> ();
        if (r == null)
            return null;


        T find = r.GetComponent<T>();
        if (find != null) return find;
        find = r.GetComponentInChildren<T>();
        if (find != null) return find;
        return r.GetComponentInParent<T>();
    }

    public void DetectedAsEntered(T script)
    {
        if (script != null)
        {
            if (!m_dronesInCollision.ContainsKey(script)) { 
                m_dronesInCollision.Add(script, new DroneCollisionWithEntry(script));
                m_dronesInCollision[script].m_rootWhenEnterPosition = script.transform.position;
                Debug.DrawLine(m_cubeCastZone.position, script.transform.position, Color.blue, 5);

            }
            RefreshList();
        }
    }
    private void DetectedAsExit(T script)
    {


        if (script != null)
        {
            if (!m_dronesInCollision.ContainsKey(script))
            {
                Debug.LogWarning("Not in list. ", script);
                //m_dronesInCollision.Add(script, new DroneCollisionWithEntry(script));
                //m_dronesInCollision[script].m_rootWhenExitPosition = script.transform.position;
                //Debug.DrawLine(m_cubeCastZone.position, script.transform.position, Color.blue, 5);

            }
            else { 
            
                m_dronesInCollision[script].m_rootWhenExitPosition = script.transform.position;
            
            }

            DroneCollisionWithEntry record = m_dronesInCollision[script];
            bool wasFrontEntry = m_goalDirection.InverseTransformPoint(record.m_rootWhenEnterPosition).z > 0;
            bool wasBackExit = m_goalDirection.InverseTransformPoint(record.m_rootWhenExitPosition).z <= 0;
            bool isValideGoal  = wasFrontEntry && wasBackExit;
            if (isValideGoal) {
                m_pointCountForDebug++;
                m_onValideGoal.Invoke();
                m_onPointChanged.Invoke(m_pointCountForDebug);
            }
            if( isValideGoal)
                m_lastValide = record;
            else 
                m_lastFailed = record;

            Debug.Log("Left goal", script);
            Debug.DrawLine(m_cubeCastZone.position, script.transform.position, isValideGoal?Color.green: Color.red, 10f);
            m_dronesInCollision.Remove(script);
            RefreshList();
        }
    }


    private void Reset()
    {
        m_goalDirection = transform;
    }
    private void RefreshList()
    {
        m_debugCollisionList = m_dronesInCollision.Values.ToList() ;
    }

}
