using Godot;
using System.Collections.Generic;

namespace bearloga.addons.Ursula.Scripts.NavigationGraph.Model
{
    public class NavGraphVertex
    {
        public Vector3 position;
        public NavGraphVertexShedule vertexShedule;
        public List<NavGraphEdge> edges;

        public bool ContainsShedule => vertexShedule != null;

        public NavGraphVertex(Vector3 position, NavGraphVertexShedule vertexShedule = null)
        {
            this.position = position;
            this.vertexShedule = vertexShedule;
            edges = new List<NavGraphEdge>();
        }
    }
}
