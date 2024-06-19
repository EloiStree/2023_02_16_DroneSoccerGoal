using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class GoalPointTriggerZoneMono : GoalPointTriggerZoneMonoGenetic<GoalFlagTagMono>
{ 

}

public class GoalPointTriggerZoneMonoGenetic<T> : MonoBehaviour where T : MonoBehaviour
{
    public Transform m_goalDirection;
    public Dictionary<T, DroneCollisionWithEntry> m_dronesInCollision = new Dictionary<T, DroneCollisionWithEntry>();
    public List<DroneCollisionWithEntry> m_debugCollisionList = new List<DroneCollisionWithEntry>();

    public int          m_pointCountForDebug;
    public UnityEvent   m_onValideGoal;
    public Eloi.PrimitiveUnityEvent_Int m_onPointChanged;

    [System.Serializable]
    public class DroneCollisionWithEntry {
        public T m_drone;
        public Vector3 m_rootWhenEnterPosition;
        public Vector3 m_rootWhenExitPosition;

        public DroneCollisionWithEntry(){}
        public DroneCollisionWithEntry(T drone) { m_drone = drone; }
    }
    public void OnTriggerEnter(Collider other)
    {
        T script = other.GetComponentInChildren<T>();
        if (script != null)
        {
            if (!m_dronesInCollision.ContainsKey(script)) { 
                m_dronesInCollision.Add(script, new DroneCollisionWithEntry(script));
                m_dronesInCollision[script].m_rootWhenEnterPosition = script.transform.position;
                Debug.DrawLine(m_goalDirection.position,
                    m_dronesInCollision[script].m_rootWhenEnterPosition,Color.blue, 5);
            }
            RefreshList();
        }
    }
    private void OnTriggerExit(Collider other)
    {

        T script = other.GetComponentInChildren<T>();
        if (script != null)
        {
            if (!m_dronesInCollision.ContainsKey(script))
                return;

            m_dronesInCollision[script].m_rootWhenExitPosition = script.transform.position;
            RefreshList();

            DroneCollisionWithEntry record = m_dronesInCollision[script];
            bool wasFrontEntry = m_goalDirection.InverseTransformPoint(record.m_rootWhenEnterPosition).z > 0;
            bool wasBackExit = m_goalDirection.InverseTransformPoint(record.m_rootWhenExitPosition).z <= 0;
            bool isValide = wasFrontEntry && wasBackExit;
            if (isValide) {
                m_pointCountForDebug++;
                m_onValideGoal.Invoke();
                m_onPointChanged.Invoke(m_pointCountForDebug);
            }
            Debug.DrawLine(m_goalDirection.position, record.m_rootWhenExitPosition, isValide?Color.green:Color.red, 5);
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
