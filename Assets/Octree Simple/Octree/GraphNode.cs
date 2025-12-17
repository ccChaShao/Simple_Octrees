using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphNode
{
    public OctreeNode octreeNode;
    public List<GraphEdge> edgeList = new ();            // 以本体为起点的所有链接路径
    
    // 单次A*的计算结果
    public float g, h;          // g：已用代价；h：启发函数代价；
    public float f => g + h;    // f：总代价
    public GraphNode cameFrom;       // 入口节点
    
    public GraphNode(OctreeNode octreeNode)
    {
        this.octreeNode = octreeNode;
    }
}
