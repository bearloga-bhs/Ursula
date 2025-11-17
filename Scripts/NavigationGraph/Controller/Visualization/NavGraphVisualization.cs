using bearloga.addons.Ursula.Scripts.NavigationGraph.Model;
using Godot;
using System.Collections.Generic;

namespace bearloga.addons.Ursula.Scripts.NavigationGraph.Controller.Visualization
{
    public class NavGraphVisualization
    {
        private List<MeshInstance3D> points = new List<MeshInstance3D>();
        private List<MeshInstance3D> lines = new List<MeshInstance3D>();

        public void Draw(NavGraph graph, Node parent)
        {
            foreach (NavGraphVertex vertex in graph.vertices)
            {
                MeshInstance3D meshInstance = NavGraphVertexVisualization.InstantiateMeshInstance3D(vertex, parent);
                points.Add(meshInstance);
            }

            foreach (NavGraphEdge edge in graph.edges)
            {
                MeshInstance3D meshInstance = NavGraphEdgeVisualization.InstantiateMeshInstance3D(edge, parent);
                lines.Add(meshInstance);
            }
        }

        public void Clear()
        {
            foreach (MeshInstance3D point in points)
            {
                point.QueueFree();
            }

            foreach (MeshInstance3D line in lines)
            {
                line.QueueFree();
            }

            points.Clear();
            lines.Clear();
        }
    }
}
