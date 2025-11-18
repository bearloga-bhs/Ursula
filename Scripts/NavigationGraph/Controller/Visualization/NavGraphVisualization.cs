using bearloga.addons.Ursula.Scripts.NavigationGraph.Model;
using Godot;
using System.Collections.Generic;

namespace bearloga.addons.Ursula.Scripts.NavigationGraph.Controller.Visualization
{
    public class NavGraphVisualization
    {
        private Dictionary<NavGraphVertex, MeshInstance3D> points = new Dictionary<NavGraphVertex, MeshInstance3D>();
        private List<MeshInstance3D> lines = new List<MeshInstance3D>();

        public void Draw(NavGraph graph, Node parent)
        {
            foreach (NavGraphVertex vertex in graph.vertices)
            {
                MeshInstance3D meshInstance = NavGraphVertexVisualization.InstantiateMeshInstance3D(vertex, parent);
                points[vertex] = (meshInstance);
            }

            foreach (NavGraphEdge edge in graph.edges)
            {
                MeshInstance3D meshInstance = NavGraphEdgeVisualization.InstantiateMeshInstance3D(edge, parent);
                lines.Add(meshInstance);
            }
        }

        public void Update(float time)
        {
            foreach (NavGraphVertex vertex in points.Keys)
            {
                if (vertex.ContainsShedule)
                {
                    if (vertex.shedule.IsOpen(time))
                    {
                        UpdateColor(vertex, new Color(0, 1, 0, 1));
                    }
                    else
                    {
                        UpdateColor(vertex, new Color(1, 0, 0, 1));
                    }
                }
            }
        }

        private void UpdateColor(NavGraphVertex vertex, Color color)
        {
            MeshInstance3D meshInstance = points[vertex];
            Material material = meshInstance.GetActiveMaterial(0);
            OrmMaterial3D ormMaterial = material as OrmMaterial3D;
            if (ormMaterial is null)
                throw new System.Exception($"Couldn't access OrmMaterial of meshInstance {meshInstance.Name}");
            ormMaterial.AlbedoColor = color;
        }

        public void Clear()
        {
            foreach (MeshInstance3D point in points.Values)
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
