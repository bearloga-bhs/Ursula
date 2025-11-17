using bearloga.addons.Ursula.Scripts.NavigationGraph.Model;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bearloga.addons.Ursula.Scripts.NavigationGraph.Controller.Visualization
{
    public static class NavGraphEdgeVisualization
    {
        public static MeshInstance3D InstantiateMeshInstance3D(NavGraphEdge edge, Node parent, Color color = default)
        {
            RandomNumberGenerator rng = new RandomNumberGenerator();
            MeshInstance3D meshInstance = new MeshInstance3D();
            ImmediateMesh mesh = new ImmediateMesh();
            OrmMaterial3D material = new OrmMaterial3D();

            meshInstance.Mesh = mesh;
            meshInstance.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;

            mesh.SurfaceBegin(Mesh.PrimitiveType.Lines, material);
            mesh.SurfaceAddVertex(edge.v1.position + new Vector3(0, rng.Randf(), 0));
            mesh.SurfaceAddVertex(edge.v2.position + new Vector3(0, rng.Randf(), 0));
            mesh.SurfaceEnd();

            Vector3 direction = edge.v2.position - edge.v1.position;
            direction = direction.Normalized();
            direction = (direction + Vector3.One) / 2;
            if (color == default)
                color = new Color(direction.X, direction.Y, direction.Z, 1);

            material.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
            material.AlbedoColor = color;

            parent.AddChild(meshInstance);
            meshInstance.GlobalPosition = Vector3.Zero;

            return meshInstance;
        }
    }
}
