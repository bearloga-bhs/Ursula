using bearloga.addons.Ursula.Scripts.NavigationGraph.Controller.Generation;
using bearloga.addons.Ursula.Scripts.NavigationGraph.Controller.ModelPlacement;
using bearloga.addons.Ursula.Scripts.NavigationGraph.Controller.Visualization;
using bearloga.addons.Ursula.Scripts.NavigationGraph.Model;
using Fractural.Tasks;
using Godot;
using Ursula.Core.DI;
using Ursula.GameObjects.Model;
using Ursula.MapManagers.Setters;
namespace bearloga.addons.Ursula.Scripts.NavigationGraph.Controller
{
    public partial class NavGraphManager : Node, IInjectable
    {
        NavGraph navGraph;
        NavGraphVisualization visualization;

        public static NavGraphManager Instance { get; private set; }

        public override void _Ready()
        {
            base._Ready();
            Instance = this;
        }

        public override void _Process(double delta)
        {
            base._Process(delta);
            if (visualization != null)
            {
                visualization.Update(Time.GetTicksMsec() / 1000f);
            }
        }

        public void Generate(float range, float height)
        {
            float dx = 10;
            float dy = 10;
            float connectionProbability = 0.6f;
            float directionsOffset = (dx + dy) / 16;
            float subdivisionOffset = 0.3f;
            float modelHegihtOffset = 1f;
            Vector3 offset = new Vector3(subdivisionOffset * dx - directionsOffset, modelHegihtOffset, 0);

            // Create undirected ghraph
            navGraph = NavGraphGenerator.Generate(range, height, dx, dy, connectionProbability);
            // Place road models
            _ = NavGraphModelPlacer.Instance.GenerateRoads(navGraph, dx, modelHegihtOffset);
            // Create directed graph and assign shedules
            navGraph = NavGraphGenerator.PostProcess(navGraph, subdivisionOffset, directionsOffset);
            // Place traffic lights models
            _ = NavGraphModelPlacer.Instance.GenerateTrafficLights(navGraph, dx / 8, offset);

            visualization = new NavGraphVisualization();
            visualization.Draw(navGraph, this, modelHegihtOffset);
            
        }

        public void OnDependenciesInjected()
        {
            throw new System.NotImplementedException();
        }
    }
}
