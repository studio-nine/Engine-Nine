#region Copyright 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;

#endregion

namespace Nine.Navigation
{
    #region GraphEdge
    /// <summary>
    /// Represents a graph edge.
    /// </summary>
    public struct GraphEdge
    {
        /// <summary>
        /// Gets an index representing where the edge is from.
        /// </summary>
        public int From;

        /// <summary>
        /// Gets an index representing where the edge leads to.
        /// </summary>
        public int To;

        /// <summary>
        /// Gets a non-negtive cost associated to the edge.
        /// </summary>
        public float Cost;

        /// <summary>
        /// Gets the string representation of this instance.
        /// </summary>
        public override string ToString()
        {
            return "[" + From.ToString() + "] => [" + To.ToString() + "] (" + Cost + ")";
        }
    }
    #endregion
    
    #region IGraph
    /// <summary>
    /// Interface for a directed graph.
    /// </summary>
    public interface IGraph
    {
        /// <summary>
        /// Gets the total number of nodes in the graph
        /// </summary>
        int NodeCount { get; }

        /// <summary>
        /// Gets the max count of edges for each node.
        /// </summary>
        int MaxEdgeCount { get; }

        /// <summary>
        /// Gets all the out-going edges of a given node.
        /// </summary>
        /// <returns>
        /// Returns the count of edges.
        /// </returns>
        int GetEdges(int node, GraphEdge[] edges, int startIndex);

        /// <summary>
        /// Gets the heuristic value between two nodes used in A* algorithm.
        /// </summary>
        /// <returns>A heuristic value between the two nodes.</returns>
        float GetHeuristicValue(int current, int end);
    }
    #endregion
    
    #region GraphSearch
    /// <summary>
    /// Performs an A* graph search on a given graph.
    /// </summary>
    public class GraphSearch
    {
        /// <summary>
        /// A list holding the path information.
        /// For a given node index, the value at that index is the parent
        /// (or the previous step) index.
        /// </summary>
        int[] path;

        /// <summary>
        /// Contains the real accumulative cost from the start to that node
        /// </summary>
        float[] costs;

        /// <summary>
        /// Current length of path or costs (Node count)
        /// </summary>
        int nodeCount = 0;
        
        /// <summary>
        /// Create an priority queue to store node indices.
        /// </summary>
        PriorityQueue queue;

        /// <summary>
        /// A list holding the edges during search.
        /// </summary>
        GraphEdge[] edges;

        /// <summary>
        /// Perform a graph search on a graph, find a best path from start to end.
        /// </summary>
        /// <param name="graph">The graph to be searched.</param>
        /// <param name="start">Start node.</param>
        /// <param name="end">End node</param>
        /// <param name="result">The result path from end node to start node.</param>
        public void Search(IGraph graph, int start, int end, ICollection<int> result)
        {
            int newNodeCount = graph.NodeCount;
            if (newNodeCount > nodeCount)
            {
                nodeCount = newNodeCount;

                Array.Resize(ref path, nodeCount);
                Array.Resize(ref costs, nodeCount);

                queue = new PriorityQueue(nodeCount);
            }

            if (edges == null || edges.Length < graph.MaxEdgeCount)
            {
                edges = new GraphEdge[graph.MaxEdgeCount];
            }

            // Reset cost
            costs[start] = 0;

            // Reset the queue
            queue.Clear();

            // Add the start node on the queue
            queue.Push(start, graph.GetHeuristicValue(start, end));
            
            // While the queue is not empty
            while (queue.Count > 0)
            {
                // Get the next node with the lowest cost
                // and removes it from the queue
                int top = queue.Pop();

                // If we reached the end, everything is done
                if (end == top)
                {
                    // Build result path
                    int current = end;
                    while (current != start && current > 0)
                    {
                        result.Add(current);
                        current = path[current];
                    }

                    // Do not forget to return start
                    result.Add(start);
                    return;
                }

                // Otherwise test all node adjacent to this one
                int edgeCount = graph.GetEdges(top, edges, 0);

                for (int i = 0; i < edgeCount; i++)
                {
                    var edge = edges[i];

                    // Calculate the heuristic cost from this node to the target (H)                       
                    float HCost = graph.GetHeuristicValue(edge.To, end);

                    // Calculate the 'real' cost to this node from the source (G)
                    float GCost = costs[top] + edge.Cost;

                    // If the node is discovered for the first time,
                    // Setup it's cost then add it to the priority queue.
                    if (queue.Index[edge.To] <= 0)
                    {
                        costs[edge.To] = GCost;

                        path[edge.To] = top;

                        queue.Push(edge.To, GCost + HCost);
                    }

                    // If the node has already been visited, but we have found a
                    // new path with a lower cost, then replace the existing path
                    // and update the cost.
                    else if (queue.Index[edge.To] > 1 && GCost < costs[edge.To])
                    {
                        costs[edge.To] = GCost;

                        path[edge.To] = top;                     

                        // Reset node cost
                        queue.IncreasePriority(edge.To, GCost + HCost);
                    }
                }
            }
        }
    }
    #endregion
}
