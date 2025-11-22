using Fractural.Tasks;
using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ursula.addons.Ursula.Scripts.GameObjects.Model;
using Ursula.Core.DI;
using Ursula.GameObjects.Model;

namespace ursula.addons.Ursula.Scripts.GameObjects.View
{
    public partial class EcosystemGeneratorView : Control, IInjectable
    {
        [Inject]
        protected ISingletonProvider<GameObjectCollectionModel> _gameObjectCollectionModelProvider;
        protected GameObjectCollectionModel _gameObjectCollectionModel;


        [Export]
        Array<EcosystemGeneratorAssetView> prefabs;

        List<Action<EcosystemGeneratorAssetView>> actions = new();

        public override void _Ready()
        {
            base._Ready();
            _ = SubscribeEvent();

            for (int i = 0; i < prefabs.Count; i++)
            {
                actions.Add(prefabs[i].clickItemEvent);
                actions[i] += OnItemClickEvent;
                GD.Print($"actions - {i}");
            }
        }

        private async GDTask SubscribeEvent()
        {
            _gameObjectCollectionModel = await _gameObjectCollectionModelProvider.GetAsync();
        }

        private void OnItemClickEvent(EcosystemGeneratorAssetView info)
        {
            info.Invalidate(_gameObjectCollectionModel.AssetSelected);
        }

        public void OnDependenciesInjected()
        {
            throw new NotImplementedException();
        }
    }
}
