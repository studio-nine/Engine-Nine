#region Copyright 2010 (c) Nightin Games
//=============================================================================
//
//  Copyright 2010 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Isles.Navigation
{
    #region IGraphNode
    /// <summary>
    /// Interface for a node in the graph.
    /// </summary>
    public interface IGraphNode
    {
        /// <summary>
        /// Unique identifier of this node. Should be non-negetive.
        /// </summary>
        int ID { get; }
    }
    #endregion

    #region GraphEdge
    /// <summary>
    /// Represents a graph edge.
    /// </summary>
    public struct GraphEdge<TGraphNode> where TGraphNode : IGraphNode
    {
        /// <summary>
        /// Gets an index representing where the edge is from.
        /// </summary>
        public TGraphNode From;

        /// <summary>
        /// Gets an index representing where the edge leads to.
        /// </summary>
        public TGraphNode To;

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
    public interface IGraph<TGraphNode> where TGraphNode : IGraphNode
    {
        /// <summary>
        /// Gets the total number of nodes in the graph
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Gets all the out-going edges of a given node.
        /// </summary>
        IEnumerable<GraphEdge<TGraphNode>> GetEdges(TGraphNode node);

        /// <summary>
        /// Gets the heuristic value between two nodes used in A* algorithm.
        /// </summary>
        /// <param name="currentIndex">Index to the current node.</param>
        /// <param name="endIndex">Index to the end/target node.</param>
        /// <returns>A heuristic value between the two nodes.</returns>
        float GetHeuristicValue(TGraphNode current, TGraphNode end);
    }
    #endregion
    
    #region GraphSearch
    /// <summary>
    /// Performs an A* graph search on a given graph.
    /// </summary>
    public sealed class GraphSearch<TGraphNode> where TGraphNode : struct, IGraphNode
    {
        /// <summary>
        /// A list holding the path information.
        /// For a given node index, the value at that index is the parent
        /// (or the previous step) index.
        /// </summary>
        TGraphNode[] path;

        /// <summary>
        /// Contains the real accumulative cost to that node
        /// </summary>
        float[] costs;

        /// <summary>
        /// Current length of path or costs (Node count)
        /// </summary>
        int length = 0;

        /// <summary>
        /// Create an priority queue to store node indices.
        /// </summary>
        PriorityQueue<TGraphNode> queue;

        /// <summary>
        /// Perform a graph search on a graph, find a best path from start to end.
        /// </summary>
        /// <param name="graph">The graph to be searched.</param>
        /// <param name="start">Start node.</param>
        /// <param name="end">End node</param>
        /// <returns>The result path from end node to start node.</returns>
        public IEnumerable<TGraphNode> Search(IGraph<TGraphNode> graph, TGraphNode start, TGraphNode end)
        {
            int newLength = graph.Count;    

            if (newLength > length)
            {
                length = newLength;

                path = new TGraphNode[length];
                costs = new float[length];
                queue = new PriorityQueue<TGraphNode>(length);
            }

            // Clear everthing
            Array.Clear(costs, 0, length);

            // Reset the queue
            queue.Clear();

            // Add the start node on the queue
            queue.Add(start.ID, graph.GetHeuristicValue(start, end), start);
            
            // While the queue is not empty
            while (!queue.IsEmpty)
            {
                // Get the next node with the lowest cost
                // and removes it from the queue
                PriorityQueue<TGraphNode>.Entry top = queue.Pop();

                // If we reached the end, everything is done
                if (end.ID == top.Key)
                {
                    // Build result path
                    TGraphNode current = end;

                    while (current.ID != start.ID && current.ID > 0)
                    {
                        yield return current;

                        current = path[current.ID];
                    }

                    // Do not forget to return start
                    yield return start;
                }

                // Otherwise test all node adjacent to this one
                foreach (GraphEdge<TGraphNode> edge in graph.GetEdges(top.Value))
                {
                    // Calculate the heuristic cost from this node to the target (H)                       
                    float HCost = graph.GetHeuristicValue(edge.To, end);

                    // Calculate the 'real' cost to this node from the source (G)
                    float GCost = costs[top.Key] + edge.Cost;

                    // If the node is discoverted for the first time,
                    // Setup it's cost then add it to the priority queue.
                    if (queue.Index[edge.To.ID] - 1 < 0)
                    {
                        path[edge.To.ID] = top.Value;
                        costs[edge.To.ID] = GCost;

                        queue.Add(edge.To.ID, GCost + HCost, edge.To);
                    }

                    // If the node has already been visited, but we have found a
                    // new path with a lower cost, then replace the existing path
                    // and update the cost.
                    else if (queue.Index[edge.To.ID] - 1 > 0 && GCost < costs[edge.To.ID])
                    {
                        path[edge.To.ID] = top.Value;
                        costs[edge.To.ID] = GCost;

                        // Reset node cost
                        queue.IncreasePriority(edge.To.ID, GCost + HCost, edge.To);
                    }
                }
            }
        }
    }
    #endregion
}
