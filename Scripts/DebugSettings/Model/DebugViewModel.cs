using Core.UI.Constructor;
using Ursula.Core.DI;

namespace ursula.addons.Ursula.Scripts.DebugSettings.Model
{
    public partial class DebugViewModel : ConstructorViewModel, IInjectable
    {
        private bool _physicsOn;
        private bool _shapeVisibility;
        private bool _navGraphVisibility;
        private bool _logCompressOn;

        public void SetPhysicsOn(bool value)
        {
            _physicsOn = value;
        }

        public void SetShapeVisibility(bool value)
        {
            _shapeVisibility = value;
        }

        public void SetNavGraphVisibility(bool value)
        {
            _navGraphVisibility = value;
        }

        public void SetLogCompressOn(bool value)
        {
            _logCompressOn = value;
        }

        public void OnDependenciesInjected()
        {

        }
    }
}
