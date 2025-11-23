using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talent.Logic.HSM;

namespace bearloga.addons.Ursula.Modules.LogicInjector
{
    public class InjectorStateEventOverride
    {
        private string eventName;
        private List<InjectorStateCommandOverride> commandOverrides;

        public InjectorStateEventOverride(string eventName, List<InjectorStateCommandOverride> commandOverrides)
        {
            this.eventName = eventName;
            this.commandOverrides = commandOverrides;
        }

        public void TryApply(Event hsmEvent)
        {
            if (hsmEvent.GetName() == eventName)
            {
                TryApplyInternal(hsmEvent.GetCommand());
            }
        }

        public void TryApplyEnter(List<Command> commands)
        {
            if (eventName == "Enter")
            {
                TryApplyInternal(commands);
            }
        }

        public void TryApplyExit(List<Command> commands)
        {
            if (eventName == "Exit")
            {
                TryApplyInternal(commands);
            }
        }

        private void TryApplyInternal(IEnumerable<Command> commands)
        {
            foreach (Command command in commands)
            {
                foreach (InjectorStateCommandOverride commandOverride in commandOverrides)
                {
                    commandOverride.TryApply(command);
                }
            }
        }
    }
}
