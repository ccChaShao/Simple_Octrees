using System.Collections.Generic;
using UnityEngine;

public class Graph
{
    public List<GraphNode> nodeList = new();
    public List<GraphEdge> edgeList = new();

    private Ray m_CacheRay = new ();
    private List<Vector3> m_SixDirs = new()
    {
        Vector3.forward,
        Vector3.back,
        Vector3.left,
        Vector3.right,
        Vector3.up,
        Vector3.down
    };

    public void AddNode(OctreeNode otn)
    {
        if (FindNode(otn.id) == null)
        {
            GraphNode graphNode = new GraphNode(otn);           // 一个树节点 = 一个路径节点
            nodeList.Add(graphNode);
        }
    }

    public GraphNode FindNode(int otnId)
    {
        for (int i = 0; i < nodeList.Count; i++)
        {
            if (nodeList[i].octreeNode.id == otnId)
            {
                return nodeList[i];
            }
        }

        return null;
    }

    public void AddEdge(OctreeNode fromOtn, OctreeNode toOtn)
    {
        if (FindEdge(fromOtn, toOtn) != null)
        {
            return;
        }
        
        GraphNode from = FindNode(fromOtn.id);
        GraphNode to = FindNode(toOtn.id);
        if (from != null && to != null)
        {
            // 正向
            GraphEdge graphEdge = new(from, to);
            edgeList.Add(graphEdge);
            from.edgeList.Add(graphEdge);
            
            // 反向
            GraphEdge reverseGraphEdge = new(to, from);
            edgeList.Add(reverseGraphEdge);
            to.edgeList.Add(reverseGraphEdge);
        }
    }

    public GraphEdge FindEdge(OctreeNode fromOtn, OctreeNode toOtn)
    {
        GraphNode from = FindNode(fromOtn.id);
        GraphNode to = FindNode(toOtn.id);
        if (from != null && to != null)
        {
            for (int i = 0; i < from.edgeList.Count; i++)
            {
                var element = from.edgeList[i];
                if (element.EndGraphNode.octreeNode.id == toOtn.id)
                {
                    return element;
                }
            }
        }
        
        return null;
    }

    public void DrawDebug()
    {
        // 边界球体绘制
        foreach (var node in nodeList)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(node.octreeNode.bounds.center, 0.25f);
        }
        
        // 边界路径绘制
        for (int i = 0; i < edgeList.Count; i++)
        {
            Debug.DrawLine(
                edgeList[i].StartGraphNode.octreeNode.bounds.center,
                edgeList[i].EndGraphNode.octreeNode.bounds.center,
                Color.red
            );
        }
    }

    /// <summary>
    /// 连接所有相邻节点
    /// </summary>
    public void ConnectNodeNodeNeighbours()
    {
        for (int i = 0; i < nodeList.Count; i++)                // 一层循环
        {
            for (int j = 0; j < nodeList.Count; j++)            // 二层循环
            {
                if (i == j)
                {
                    continue;
                }
                // 六方向检查
                for (int k = 0; k < m_SixDirs.Count; k++)
                {
                    var nodeI = nodeList[i];
                    var nodeJ = nodeList[j];
                    m_CacheRay.origin = nodeI.octreeNode.bounds.center;
                    m_CacheRay.direction = m_SixDirs[k];
                    float maxLength = nodeI.octreeNode.bounds.size.x / 2.0f + 0.01f;
                    // 检查是否跟这个射线有交叉
                    if (nodeJ.octreeNode.bounds.IntersectRay(m_CacheRay, out float hitLength))
                    {
                        // 仅连接相邻邻居（最短）
                        if (hitLength <= maxLength)
                        {
                            AddEdge(nodeI.octreeNode, nodeJ.octreeNode);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// A*寻路
    /// </summary>
    /// <param name="startNode"></param>
    /// <param name="endNode"></param>
    /// <param name="pathList"></param>
    /// <returns></returns>
    public bool AStar(OctreeNode startNode, OctreeNode endNode, ref List<GraphNode> pathList)
    {
        GraphNode start = FindNode(startNode.id);
        GraphNode end = FindNode(endNode.id);

        if (start == null || end == null)
        {
            return false;
        }

        // 一开始就是终点
        if (start.octreeNode.id == end.octreeNode.id)
        {
            end.cameFrom = start;
            ReconstructPath(start, end, ref pathList);
            return true;
        }

        // HashSet<GraphNode> openHasSet = new ();
        // HashSet<GraphNode> closedHasSet = new ();
        List<GraphNode> openList = new();            // open（记录即将要走的路径）
        List<GraphNode> closeList = new();           // close（记录的是走过的路径）

        // 代价计算
        start.g = 0;
        start.h = Vector3.SqrMagnitude(endNode.bounds.center - startNode.bounds.center);
        
        openList.Add(start);
        while (openList.Count > 0)
        {
            int thisI = GetLowestFIndex(openList);
            GraphNode thisN = openList[thisI];
            
            // 到达终点则结束
            if (thisN.octreeNode.id == endNode.id)
            {
                ReconstructPath(start, end, ref pathList);
                return true;
            }
            
            // 数列更新
            openList.RemoveAt(thisI);         // 待考察的节点
            closeList.Add(thisN);             // 已经考察过的节点
            
            // 查找附近所有临近点
            foreach (GraphEdge edge in thisN.edgeList)
            {
                GraphNode edgeEndGraphNode = edge.EndGraphNode;

                if (closeList.IndexOf(edgeEndGraphNode) > -1)
                {
                    continue; 
                }

                bool updateNode = false;
                float newG = thisN.g + Vector3.SqrMagnitude
                (
                    edgeEndGraphNode.octreeNode.bounds.center -
                    thisN.octreeNode.bounds.center
                );

                // 首次被发现
                if (openList.IndexOf(edgeEndGraphNode) <= -1)
                {
                    openList.Add(edgeEndGraphNode);
                    updateNode = true;
                }
                // 有更近的入口点
                else if (newG <= edgeEndGraphNode.g)
                {
                    updateNode = true;
                }
                
                // 数据更新
                if (updateNode)
                {
                    edgeEndGraphNode.cameFrom = thisN;                // 更新来源点
                    edgeEndGraphNode.g = newG;                        // 已消耗代价更新
                    edgeEndGraphNode.h = Vector3.SqrMagnitude(        // 启发代价更新
                        endNode.bounds.center -
                        edgeEndGraphNode.octreeNode.bounds.center
                    );
                }
            }
        }
        
        return false;
    }

    /// <summary>
    /// 找到最低总代价（f）的节点
    /// </summary>
    private int GetLowestFIndex(List<GraphNode> openList)
    {
        float lowestF = -9999;
        int lowestIndex = 0;
        for (int i = 0; i < openList.Count; i++)
        {
            if (openList[i].f <= lowestF)
            {
                lowestF = openList[i].f;
                lowestIndex = i;
            }
        }

        return lowestIndex;
    }

    /// <summary>
    ///  路径回溯
    /// </summary>
    private void ReconstructPath(GraphNode startGraphNode, GraphNode endGraphNode, ref List<GraphNode> pathList)
    {
        pathList.Clear();
        pathList.Add(endGraphNode);

        var from = endGraphNode.cameFrom;
        while (from != null && from != startGraphNode)
        {
            pathList.Insert(0, from);          // 添加到数列首部
            from = from.cameFrom;
        }

        pathList.Insert(0, startGraphNode);
    }
}
