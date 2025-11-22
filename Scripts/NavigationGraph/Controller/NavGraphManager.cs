using bearloga.addons.Ursula.Scripts.NavigationGraph.Controller.Generation;
using bearloga.addons.Ursula.Scripts.NavigationGraph.Controller.ModelPlacement;
using bearloga.addons.Ursula.Scripts.NavigationGraph.Controller.Visualization;
using bearloga.addons.Ursula.Scripts.NavigationGraph.Model;
using Godot;
using Ursula.Core.DI;
using System;
using bearloga.addons.Ursula.Scripts.NavigationGraph.Controller.PathFinding;
using System.Collections.Generic;

namespace bearloga.addons.Ursula.Scripts.NavigationGraph.Controller
{
    public partial class NavGraphManager : Node, IInjectable
    {
        NavGraph navGraph;
        NavGraphVisualization visualization;
        NavGraphVisualization pathVisualization;
        RandomNumberGenerator rng;

        public static NavGraphManager Instance { get; private set; }

        public override void _Ready()
        {
            base._Ready();
            rng = new RandomNumberGenerator();
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
            float modelHegihtOffset = 0.01f;
            Vector3 offset = new Vector3(subdivisionOffset * dx - directionsOffset, modelHegihtOffset, 0);

            // Create undirected graph
            navGraph = NavGraphGenerator.Generate(range, height, dx, dy, connectionProbability);
            // Place road models
            _ = NavGraphModelPlacer.Instance.GenerateRoads(navGraph, dx, modelHegihtOffset);
            // Create directed graph and assign shedules
            navGraph = NavGraphGenerator.PostProcess(navGraph, subdivisionOffset, directionsOffset);
            // Place traffic lights models
            _ = NavGraphModelPlacer.Instance.GenerateTrafficLights(navGraph, dx / 8, offset);

            //visualization = new NavGraphVisualization();
            //visualization.Draw(navGraph, this, modelHegihtOffset);

            //List<NavGraphVertex> path = BuildPath(navGraph.vertices[0].position, navGraph.vertices[navGraph.vertices.Count - 1].position);
            //List<NavGraphEdge> pathEdges = new List<NavGraphEdge>(); 
            //for (int i = 0; i < path.Count - 2; i++)
            //{
            //    pathEdges.Add(new NavGraphEdge(path[i], path[i + 1], true));
            //}
            //NavGraph pathGraph = new NavGraph(pathEdges, path);

            //pathVisualization = new NavGraphVisualization();
            //pathVisualization.Draw(pathGraph, this, modelHegihtOffset);
        }

        public Queue<Vector3> BuildPath(Vector3 from, Vector3 to)
        {
            float vertexTolerance = 1f;
            NavGraphVertex fromVertex = NavGraphVertexFinder.GetVertex(navGraph, from, vertexTolerance);
            NavGraphVertex toVertex = NavGraphVertexFinder.GetVertex(navGraph, to, vertexTolerance);
            List<NavGraphVertex> path = NavGraphPathFinder.GetPath(fromVertex, toVertex);
            // TODO:
            Queue<Vector3> pathPoints = new Queue<Vector3>();
            foreach (NavGraphVertex vertex in path)
            {
                pathPoints.Enqueue(vertex.position);
            }
            return pathPoints;
        }

        public Vector3 GetRandomPoint()
        {
            if (navGraph == null)
            {
                throw new InvalidOperationException("Couldn't get navigation point. Navigation graph is not initialized.");
            }

            int idx = rng.RandiRange(0, navGraph.vertices.Count - 1);
            return navGraph.vertices[idx].position;
        }

        public void OnDependenciesInjected()
        {
            
        }
    }
}
