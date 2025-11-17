using bearloga.addons.Ursula.Scripts.NavigationGraph.Model;
using Godot;


namespace bearloga.addons.Ursula.Scripts.NavigationGraph.Controller.Visualization
{
    public static class NavGraphVertexVisualization
    {
        public static MeshInstance3D InstantiateMeshInstance3D(NavGraphVertex vertex, Node parent, float radius = 0.5f, Color color = default)
        {
            RandomNumberGenerator rng = new RandomNumberGenerator();
            MeshInstance3D meshInstance = new MeshInstance3D();
            SphereMesh mesh = new SphereMesh();
            OrmMaterial3D material = new OrmMaterial3D();

            meshInstance.Mesh = mesh;
            meshInstance.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
            
            mesh.Radius = radius;
            mesh.Height = radius * 2;
            mesh.Material = material;

            if (color == default)
                color = new Color(0, 1, 0, 1); // Green

            material.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
            material.AlbedoColor = color;

            parent.AddChild(meshInstance);
            meshInstance.GlobalPosition = vertex.position + new Vector3(0, rng.Randf(), 0);

            return meshInstance;
        }
    }
}
