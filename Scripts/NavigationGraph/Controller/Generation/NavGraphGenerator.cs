using bearloga.addons.Ursula.Scripts.NavigationGraph.Model;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Talent.Graphs;

namespace bearloga.addons.Ursula.Scripts.NavigationGraph.Controller.Generation
{
    public static class NavGraphGenerator
    {
        public static NavGraph Generate(float range, float height, float dx, float dy, float connectionProbability)
        {
            if (range < 0 || !float.IsNormal(range))
                throw new ArgumentOutOfRangeException(nameof(range));

            if (float.IsInfinity(height) || float.IsNaN(height))
                throw new ArgumentOutOfRangeException(nameof(height));

            if (dx < 0 || !float.IsNormal(dx))
                throw new ArgumentOutOfRangeException(nameof(dx));

            if (dy < 0 || !float.IsNormal(dy))
                throw new ArgumentOutOfRangeException(nameof(dy));

            if (connectionProbability < 0 || connectionProbability > 1)
                throw new ArgumentOutOfRangeException(nameof(connectionProbability));

            NavGraph navGraph = CreateBaseForm(range, height, dx, dy, connectionProbability);
            navGraph = EraseIlands(navGraph);
            navGraph = Subdivide(navGraph);
            navGraph = ApplyDirections(navGraph, (dx + dx) / 16);

            return navGraph;
        }

        private static NavGraph EraseIlands(NavGraph navGraph)
        {
            List<NavGraph> connectedSubgraphs = new List<NavGraph>();
            foreach (NavGraphVertex vertex in navGraph.vertices)
            {
                if (connectedSubgraphs.Exists(graph => graph.vertices.Contains(vertex)))
                    continue;

                connectedSubgraphs.Add(GetConnectedSubgraph(vertex));
            }

            return connectedSubgraphs.MaxBy(graph => graph.vertices.Count);
        }

        // Depth search that returns connected subgraph that contains startVertex
        private static NavGraph GetConnectedSubgraph(NavGraphVertex startVertex)
        {
            List<NavGraphEdge> edges = new List<NavGraphEdge>();
            List<NavGraphVertex> vertices = new List<NavGraphVertex>();
            Queue<NavGraphVertex> vertexQueue = new Queue<NavGraphVertex>();
            vertexQueue.Enqueue(startVertex);

            while (vertexQueue.Count != 0)
            {
                NavGraphVertex vertex = vertexQueue.Dequeue();
                vertices.Add(vertex);

                foreach (NavGraphEdge edge in vertex.edges)
                {
                    if (!edges.Contains(edge))
                        edges.Add(edge);

                    NavGraphVertex next;
                    if (edge.v1 == vertex)
                    {
                        next = edge.v2;
                    }
                    else
                    {
                        next = edge.v1;
                    }

                    if (!vertexQueue.Contains(next) && !vertices.Contains(next))
                        vertexQueue.Enqueue(next);
                }
            }

            return new NavGraph(edges, vertices);
        }

        // Makes directed graph.
        // Splits every edge into right and left handed directions.
        private static NavGraph ApplyDirections(NavGraph navGraph, float offset)
        {
            List<NavGraphVertex> vertices = new List<NavGraphVertex>();
            List<NavGraphEdge> edges = new List<NavGraphEdge>();
            // Maps old vertices to new ones
            Dictionary<NavGraphVertex, List<NavGraphVertex>> vertexMap = new Dictionary<NavGraphVertex, List<NavGraphVertex>>();

            foreach (NavGraphEdge edge in navGraph.edges)
            {
                Vector3 direction = edge.v2.position - edge.v1.position;
                direction = direction.Normalized();

                Vector3 rightDirection = Vector3.Up.Cross(direction);
                Vector3 leftDirection = -rightDirection;

                // Determine left and right vertices for the first vertex from the edge
                NavGraphVertex rightV1;
                NavGraphVertex leftV1;
                if (!vertexMap.ContainsKey(edge.v1))
                {
                    // Create new vertices if map record is empty
                    rightV1 = new NavGraphVertex(edge.v1.position + rightDirection * offset);
                    leftV1 = new NavGraphVertex(edge.v1.position + leftDirection * offset);
                    vertexMap[edge.v1] = new List<NavGraphVertex>();
                    vertexMap[edge.v1].Add(leftV1);
                    vertexMap[edge.v1].Add(rightV1);
                }
                else
                {
                    // Determine left and right vertices if map record is not empty
                    if (IsLeft(edge.v1.position, edge.v2.position, vertexMap[edge.v1][0].position))
                    {
                        leftV1 = vertexMap[edge.v1][0];
                        rightV1 = vertexMap[edge.v1][1];
                    }
                    else
                    {
                        leftV1 = vertexMap[edge.v1][1];
                        rightV1 = vertexMap[edge.v1][0];
                    }
                }

                // Determine left and right vertices for the second vertex from the edge
                NavGraphVertex rightV2;
                NavGraphVertex leftV2;
                if (!vertexMap.ContainsKey(edge.v2))
                {
                    // Create new vertices if map record is empty
                    rightV2 = new NavGraphVertex(edge.v2.position + rightDirection * offset);
                    leftV2 = new NavGraphVertex(edge.v2.position + leftDirection * offset);
                    vertexMap[edge.v2] = new List<NavGraphVertex>();
                    vertexMap[edge.v2].Add(leftV2);
                    vertexMap[edge.v2].Add(rightV2);
                }
                else
                {
                    // Determine left and right vertices if map record is not empty
                    if (IsLeft(edge.v1.position, edge.v2.position, vertexMap[edge.v2][0].position))
                    {
                        leftV2 = vertexMap[edge.v2][0];
                        rightV2 = vertexMap[edge.v2][1];
                    }
                    else
                    {
                        leftV2 = vertexMap[edge.v2][1];
                        rightV2 = vertexMap[edge.v2][0];
                    }
                }

                NavGraphEdge left = new NavGraphEdge(leftV2, leftV1);
                NavGraphEdge right = new NavGraphEdge(rightV1, rightV2);

                edges.Add(left);
                edges.Add(right);

                // Connect right and left directions in dead ends
                if (edge.v1.edges.Count == 1)
                {
                    NavGraphEdge deadEnd = new NavGraphEdge(leftV1, rightV1);
                    edges.Add(deadEnd);
                }

                if (edge.v2.edges.Count == 1)
                {
                    NavGraphEdge deadEnd = new NavGraphEdge(rightV2, leftV2);
                    edges.Add(deadEnd);
                }
            }

            return new NavGraph(edges, vertices);
        }

        private static bool IsLeft(Vector3 edgeV1, Vector3 edgeV2, Vector3 p)
        {
            // cross product
            return (edgeV2.X - edgeV1.X) * (p.Z - edgeV1.Z) - (edgeV2.Z - edgeV1.Z) * (p.X - edgeV1.X) > 0;
        }

        // Creates points at 0.25 and 0.75 of every edge
        // Connects edges similar to the original graph
        private static NavGraph Subdivide(NavGraph navGraph)
        {
            List<NavGraphVertex> vertices = new List<NavGraphVertex>();
            List<NavGraphEdge> edges = new List<NavGraphEdge>();
            // maps old vertices to new ones
            Dictionary<NavGraphVertex, List<NavGraphVertex>> vertexMap = new Dictionary<NavGraphVertex, List<NavGraphVertex>>(); 

            // Create new vertices and fill the vertexMap
            foreach (NavGraphEdge edge in navGraph.edges)
            {
                Vector3 source = edge.v1.position;
                Vector3 direction = edge.v2.position - edge.v1.position;

                // Create new vertices
                NavGraphVertex v1 = new NavGraphVertex(source + 0.25f * direction);
                NavGraphVertex v2 = new NavGraphVertex(source + 0.75f * direction);
                // Create new edge in place of the old one but twice as short
                NavGraphEdge newEdge = new NavGraphEdge(v1, v2); 
                vertices.Add(v1);
                vertices.Add(v2);
                edges.Add(newEdge);

                // Fill vertexMap
                if (!vertexMap.ContainsKey(edge.v1))
                    vertexMap[edge.v1] = new List<NavGraphVertex>();
                vertexMap[edge.v1].Add(v1);

                if (!vertexMap.ContainsKey(edge.v2))
                    vertexMap[edge.v2] = new List<NavGraphVertex>();
                vertexMap[edge.v2].Add(v2);
            }

            // Create new edges where old ones intersect
            foreach (NavGraphVertex vertex in navGraph.vertices)
            {
                List<NavGraphVertex> toConnect = vertexMap[vertex];
                for (int i = 0; i < toConnect.Count; i++)
                {
                    for (int j = i + 1; j < toConnect.Count; j++)
                    {
                        NavGraphEdge newEdge = new NavGraphEdge(toConnect[i], toConnect[j]);
                        edges.Add(newEdge);
                    }
                }
            }

            return new NavGraph(edges, vertices);
        }

        // Creates undirected graph on a rectangular grid
        private static NavGraph CreateBaseForm(float range, float height, float dx, float dy, float connectionProbability)
        {
            RandomNumberGenerator rng = new RandomNumberGenerator();

            int gridSizeX = (int)(range / dx) - 1;
            int gridSizeY = (int)(range / dy) - 1;

            // Create grid filled with placeholders
            NavGraphVertex[] vertices = new NavGraphVertex[gridSizeX * gridSizeY];
            List<NavGraphEdge> edges = new List<NavGraphEdge>();

            // Iterate through every vertex
            for (int i = 0; i < gridSizeX; i++)
            {
                for (int j = 0; j < gridSizeY; j++)
                {
                    // Determine if edge to the right neighbor should be created
                    float prob = rng.Randf();
                    if (i != gridSizeX - 1 && prob < connectionProbability)
                    {
                        // Create current vertex or check it's position if already exists
                        int index1 = GetGridIndex(i, j, gridSizeX);
                        Vector3 position1 = GetPosition(i, j, height, dx, dy);
                        CheckCreateVertex(index1, position1, vertices);

                        // Create neighbor vertex or check it's position if already exists
                        int index2 = GetGridIndex(i + 1, j, gridSizeX);
                        Vector3 position2 = GetPosition(i + 1, j, height, dx, dy);
                        CheckCreateVertex(index2, position2, vertices);

                        // Create edge
                        NavGraphVertex v1 = vertices[index1];
                        NavGraphVertex v2 = vertices[index2];
                        NavGraphEdge edge = new NavGraphEdge(v1, v2); // Adds reference for this edge into v1 and v2
                        edges.Add(edge);
                    }

                    // Determine if edge to the down neighbor should be created
                    prob = rng.Randf();
                    if (j != gridSizeY - 1 && prob < connectionProbability)
                    {
                        // Create current vertex or check it's position if already exists
                        int index1 = GetGridIndex(i, j, gridSizeX);
                        Vector3 position1 = GetPosition(i, j, height, dx, dy);
                        CheckCreateVertex(index1, position1, vertices);

                        // Create neighbor vertex or check it's position if already exists
                        int index2 = GetGridIndex(i, j + 1, gridSizeX);
                        Vector3 position2 = GetPosition(i, j + 1, height, dx, dy);
                        CheckCreateVertex(index2, position2, vertices);

                        // Create edge
                        NavGraphVertex v1 = vertices[index1];
                        NavGraphVertex v2 = vertices[index2];
                        NavGraphEdge edge = new NavGraphEdge(v1, v2); // Adds reference for this edge into v1 and v2
                        edges.Add(edge);
                    }
                }
            }

            // Clear all placeholders
            List<NavGraphVertex> resultVertices = new List<NavGraphVertex>();
            foreach (NavGraphVertex vertex in vertices)
            {
                if (vertex != null)
                    resultVertices.Add(vertex);
            }

            return new NavGraph(edges, resultVertices);
        }

        private static void CheckCreateVertex(int index, Vector3 position, NavGraphVertex[] vertices)
        {
            if (vertices[index] is null)
                vertices[index] = new NavGraphVertex(position);
            else if (vertices[index].position != position)
                throw new Exception("Couldn't generate navigation graph vertex. Space is already occupied.");
        }

        private static int GetGridIndex(int x, int y, int gridSizeX)
        {
            return y * gridSizeX + x;
        }

        private static Vector3 GetPosition(int x, int y, float height, float dx, float dy)
        {
            Vector3 origin = new Vector3(dx, height, dy);
            Vector3 offset = new Vector3(x * dx, 0, y * dy);
            return origin + offset;
        }
    }
}
