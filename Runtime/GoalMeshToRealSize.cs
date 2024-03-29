using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalMeshToRealSize : MonoBehaviour
{

    public Transform m_innerBorder;
    public Transform m_outterBorder;

    public float m_thicknessInCm = 20;
    public float m_innerInCm=80;
    public float m_outerInCm=100;

    public float m_m_thicknessAdjustement = 1;
    public float m_meshInnerAdjustement=1;
    public float m_meshOuterAdjustement=1;

    [ContextMenu("RandomSet_10x20")]
    public void RandomSet_10x20()
    {
        m_innerInCm = 60 + Random.value * (80 - 60);
        m_outerInCm = 100 + Random.value * (120 - 100);
        Refresh();
    }
    [ContextMenu("RandomSet_3x6")]
    public void RandomSet_3x6()
    {
        m_innerInCm = (60 + Random.value * (40 - 20) );
        m_outerInCm = (100 + Random.value * (60 - 40) );
        Refresh();
    }

    private void OnValidate()
    {
        Refresh();
    }

    private void Refresh()
    {
        Vector3 tmp = m_innerInCm * 0.01f * m_meshInnerAdjustement * Vector3.one;
        tmp.y = m_thicknessInCm*0.01f* m_m_thicknessAdjustement;
        m_innerBorder.localScale = tmp;

        tmp = m_outerInCm * 0.01f * m_meshOuterAdjustement * Vector3.one;
        tmp.y = m_thicknessInCm * 0.01f * m_m_thicknessAdjustement;
        m_outterBorder.localScale = tmp;
    }
}

public enum GoalType { Goal_3x6, Goal_10x20 }
