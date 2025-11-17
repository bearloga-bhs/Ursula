using Godot;
using System;

namespace bearloga.addons.Ursula.Scripts.NavigationGraph.Model
{
    public class NavGraphVertexShedule
    {
        public float timeOpen;
        public float timeClosed;

        public NavGraphVertexShedule(float timeOpen, float timeClosed)
        {
            if (timeOpen <= 0 || !float.IsNormal(timeOpen))
                throw new ArgumentOutOfRangeException(nameof(timeOpen));

            if (timeClosed <= 0 || !float.IsNormal(timeClosed))
                throw new ArgumentOutOfRangeException(nameof(timeClosed));

            this.timeOpen = timeOpen;
            this.timeClosed = timeClosed;
        }

        public bool IsOpen(float currentTime)
        {
            currentTime = currentTime % (timeOpen + timeClosed);
            if (currentTime <= timeOpen)
                return true;
            return false;
        }
    }
}
