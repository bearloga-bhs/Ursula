using bearloga.addons.Ursula.Scripts.NavigationGraph.Controller.Generation.Stages;
using bearloga.addons.Ursula.Scripts.NavigationGraph.Model;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Talent.Graphs;
using static Godot.Control;

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

            NavGraph navGraph = NavGraphUndirectedStage.CreateBaseForm(range, height, dx, dy, connectionProbability);
            navGraph = NavGraphIslandStage.EraseIslands(navGraph);
            return navGraph;
        }

        public static NavGraph PostProcess(NavGraph navGraph, float directionsOffset)
        {
            if (navGraph == null)
                throw new ArgumentNullException(nameof(navGraph));

            if (float.IsNaN(directionsOffset) || float.IsInfinity(directionsOffset))
                throw new ArgumentOutOfRangeException(nameof(directionsOffset));

            navGraph = NavGraphSubdivisionStage.Subdivide(navGraph);
            navGraph = NavGraphDirectedStage.ApplyDirections(navGraph, directionsOffset);
            NavGraphSheduleStage.CreateShedules(navGraph);

            return navGraph;
        }
    }
}
