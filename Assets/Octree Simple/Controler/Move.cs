using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class Move : MonoBehaviour
{
    public Vector2 speedRange = new (1f, 20f); 
    public float speed = 5.0f;
    public float accuray = 0.5f;            // 位置精度
    
    // 实时导航数据
    private int m_CurWayPoint = 0;
    private OctreeNode m_Move2Node;

    // 八叉树导航数据
    private Octree m_Octree;
    private Graph m_Graph;
    private List<GraphNode> m_AStarPathList = new();
    
    public OctreeNode move2Node => m_Move2Node;

    void Update()
    {
        if (m_Octree == null  || m_Graph == null)
        {
            return;
        }

        if (m_CurWayPoint >= GetAStarPathCount())
        {
            ReStartMove(m_Move2Node, null);
        }
        else
        {
            float distance = Vector3.Distance(GetAstarPathNode(m_CurWayPoint).octreeNode.bounds.center, transform.position);
            if (distance <= accuray)
            {
                m_CurWayPoint++;
            }

            if (m_CurWayPoint < GetAStarPathCount())
            {
                m_Move2Node = GetAstarPathNode(m_CurWayPoint).octreeNode;
                Vector3 direction = m_Move2Node.bounds.center - transform.position;
                transform.Translate(direction.normalized * speed * Time.deltaTime, Space.World);
            }
        }
    }

    public void DataInit(Octree octree)
    {
        m_Octree = octree;
        m_Graph = octree.navigationGraph;

        ReStartMove(null, null);
    }

    public void ReStartMove(OctreeNode _startNode, OctreeNode _endNode)
    {
        OctreeNode startNode = _startNode != null
            ? _startNode
            : m_Graph.nodeList[Random.Range(0, m_Graph.nodeList.Count)].octreeNode;
        OctreeNode endNode = _endNode != null
            ? _endNode
            : m_Graph.nodeList[Random.Range(0, m_Graph.nodeList.Count)].octreeNode;
        
        bool randomSuc = m_Graph.AStar(startNode, endNode, ref m_AStarPathList);
        if (randomSuc)
        {
            m_CurWayPoint = 0;
            speed = Random.Range(speedRange.x, speedRange.y);
            transform.position = startNode.bounds.center;
        }
        else
        {
            Debug.LogError("[ReStartMove] : a* fail !");
        }
    }

    private int GetAStarPathCount()
    {
        return m_AStarPathList.Count;
    }

    private GraphNode GetAstarPathNode(int index)
    {
        return m_AStarPathList[index];
    }
}
