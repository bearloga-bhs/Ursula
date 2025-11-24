using bearloga.addons.Ursula.Scripts.NavigationGraph.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bearloga.addons.Ursula.Scripts.NavigationGraph.Controller.Generation.Stages
{
    public static class NavGraphSheduleStage
    {
        public static void CreateShedules(NavGraph navGraph)
        {
            // Go through shedule group and generate shedules for each vertex in shedule group
            foreach (NavGraphVertex vertex in navGraph.vertices)
            {
                if (!vertex.ContainsShedule && vertex.sheduleGroup != null)
                {
                    CreateGroupShedule(vertex.sheduleGroup);
                }
            }
        }

        private static void CreateGroupShedule(List<NavGraphVertex> sheduleGroup)
        {
            float delta = 5f;
            float totalTime = sheduleGroup.Count * delta;
            float timeOpen = delta;
            float timeClosed = totalTime - timeOpen;

            // Generate shedules for each vertex in shedule group
            for (int i = 0; i < sheduleGroup.Count; i++)
            {
                float offset = i * delta;
                NavGraphVertex vertex = sheduleGroup[i];
                NavGraphVertexShedule shedule = new NavGraphVertexShedule(timeOpen, timeClosed, offset);
                vertex.shedule = shedule;
            }
        }

    }
}
