using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.Collections;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

public class CreateOctree : MonoBehaviour
{
    public int minNodeSize;
    
    [Header("初始化阻挡")]
    public GameObject[] worldObjects;

    [Header("新增目标组")] 
    public GameObject[] targetObjects;
    
    [Header("移动组"), ReadOnly] 
    public Move[] movements;
    
    private Octree m_Octree;
    private Graph m_WayPointGraph;

    public Octree octree => m_Octree;
    public Graph wayPointGraph => m_WayPointGraph;

    private void Awake()
    {
        m_Octree = new Octree(worldObjects, minNodeSize, new Graph());
        m_WayPointGraph = m_Octree.navigationGraph;
        foreach (var movement in movements)
        {
            movement.DataInit(octree);
        }
    }

    #region OnGUI
    
    private Vector2 m_OnGUIPos = Vector2.zero; 
    private Vector2 m_OnGUISize = Vector2.zero;
    private string m_AddIndexStr = "0";
    private string m_ChangeIndexStr = "0";

    private void OnGUI()
    {
        m_OnGUIPos.Set(0, 0);
        m_OnGUISize.Set(80, 20);
        
        // 第一行
        m_OnGUIPos.y += m_OnGUISize.y;
        m_OnGUIPos.x = 0;
        GUI.Label(new Rect(m_OnGUIPos, m_OnGUISize),"修改目的地：");
        m_OnGUIPos.x = m_OnGUISize.x;
        m_AddIndexStr = GUI.TextField(new Rect(m_OnGUIPos, m_OnGUISize), m_AddIndexStr);
        
        // 第二行
        m_OnGUIPos.y += m_OnGUISize.y;
        m_OnGUIPos.x = 0;
        GUI.Label(new Rect(m_OnGUIPos, m_OnGUISize),"修改目标：");
        m_OnGUIPos.x = m_OnGUISize.x;
        m_ChangeIndexStr = GUI.TextField(new Rect(m_OnGUIPos, m_OnGUISize), m_ChangeIndexStr);
        
        // 第三行
        m_OnGUIPos.y += m_OnGUISize.y;
        m_OnGUIPos.x = 0;
        if (GUI.Button(new Rect(m_OnGUIPos, m_OnGUISize), "执行改变"))
        {
            GameObject obj = targetObjects[int.Parse(m_AddIndexStr)];
            Move move = movements[int.Parse(m_ChangeIndexStr)];
            
            if (obj != null && move != null)
            {
                int found = octree.FindEmptyLeafNode(octree.rootNode, obj.transform.position);
                GraphNode graphGraphNode = wayPointGraph.FindNode(found);
                if (graphGraphNode != null)
                {
                    move.ReStartMove(graphGraphNode.octreeNode, graphGraphNode.octreeNode);
                }
            }
        }
    }

    #endregion

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            return;
        }
        
        m_Octree.DrawDebug();
        m_Octree.rootNode.DrawDebug();
        m_Octree.navigationGraph.DrawDebug();
    }
}
