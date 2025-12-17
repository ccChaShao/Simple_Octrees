public class GraphEdge
{
    public GraphNode StartGraphNode;
    public GraphNode EndGraphNode;
    
    public GraphEdge(GraphNode from, GraphNode to)
    {
        StartGraphNode = from;
        EndGraphNode = to;
    }
}
