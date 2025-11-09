using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct OctreeObject
{
    public Bounds bounds;
    public GameObject go;

    public OctreeObject(GameObject gameObject)
    {
        go = gameObject;
        bounds = go.GetComponent<Collider>().bounds;
    }
}

/// <summary>
/// 八叉树节点
/// </summary>
public class OctreeNode
{
    // 节点信息
    public int id;
    public float minSize;
    public Bounds bounds = new();
    public Bounds[] childBounds = null;
    
    // 父亲节点信息
    public OctreeNode parent;
    
    // 孩子节点信息
    public bool isContainedChild = false;
    public OctreeNode[] children = null;
    
    // 世界物体包含信息
    public List<OctreeObject> containedObjects = new();         // 充当阻挡

    public OctreeNode(Bounds bounds, float minSize, OctreeNode parent)
    {
        this.id = Utils.idInt++;
        this.parent = parent;
        this.bounds = bounds;
        this.minSize = minSize;
        BuildChildBounds();
    }

    private void BuildChildBounds()
    {
        float quarter = bounds.size.x / 4f;
        Vector3 childSize = new Vector3(bounds.size.x / 2f, bounds.size.x / 2f, bounds.size.x / 2f);
        childBounds = new[]
        {
            // 4 2 1
            new Bounds(bounds.center + new Vector3(-quarter, -quarter, -quarter), childSize),     // 0
            new Bounds(bounds.center + new Vector3(-quarter, -quarter, quarter), childSize),      // 1    
            new Bounds(bounds.center + new Vector3(-quarter, quarter, -quarter), childSize),      // 2
            new Bounds(bounds.center + new Vector3(-quarter, quarter, quarter), childSize),       // 3
            new Bounds(bounds.center + new Vector3(quarter, -quarter, -quarter), childSize),      // 4
            new Bounds(bounds.center + new Vector3(quarter, -quarter, quarter), childSize),       // 5
            new Bounds(bounds.center + new Vector3(quarter, quarter, -quarter), childSize),       // 6
            new Bounds(bounds.center + new Vector3(quarter, quarter, quarter), childSize),        // 7
        };
    }

    /// <summary>
    /// 八叉树核心，向下递归创建节点
    /// </summary>
    public void DivideAndAdd(GameObject worldObject)
    {
        OctreeObject octObj = new OctreeObject(worldObject);
        // 最底层，直接添加
        if (bounds.size.x <= minSize)
        {
            octObj.go.name = $"otn_go_in_{id}";
            containedObjects.Add(octObj);           
            return;
        }

        // 内部分割
        if (children == null)
            children = new OctreeNode[8];
        for (int i = 0; i < 8; i++)
        {
            if (children[i] == null) 
                children[i] = new OctreeNode(childBounds[i], minSize, this);
            if (children[i].bounds.Intersects(octObj.bounds))
            {
                isContainedChild = true;
                children[i].DivideAndAdd(worldObject);
            }
        }
        if (!isContainedChild)
        {
            children = null;           // 清理节点；
        }
    }

    public void DrawDebug()
    {
        // draw my bounds
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
        // draw contain cube
        if (containedObjects.Count > 0)
        {
            Gizmos.color = new Color(0, 0, 1, 0.75f);
            Gizmos.DrawCube(bounds.center, bounds.size);

            foreach (var obj in containedObjects)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube(obj.bounds.center, obj.bounds.size);
            }
        }
        // draw child bounds
        if (children != null)
        {
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i] != null)
                {
                    children[i].DrawDebug();
                }
            }
        }
    }
}