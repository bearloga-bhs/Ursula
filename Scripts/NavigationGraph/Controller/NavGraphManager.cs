using bearloga.addons.Ursula.Scripts.NavigationGraph.Controller.Generation;
using bearloga.addons.Ursula.Scripts.NavigationGraph.Controller.Visualization;
using bearloga.addons.Ursula.Scripts.NavigationGraph.Model;
using Godot;
namespace bearloga.addons.Ursula.Scripts.NavigationGraph.Controller
{
    public partial class NavGraphManager : Node
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
            navGraph = NavGraphGenerator.Generate(range, height, 10, 10, 0.6f);
            visualization = new NavGraphVisualization();
            visualization.Draw(navGraph, this);
        }
    }
}
