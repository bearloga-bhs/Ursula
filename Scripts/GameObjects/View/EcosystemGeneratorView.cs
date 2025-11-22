using Fractural.Tasks;
using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using ursula.addons.Ursula.Scripts.GameObjects.Model;
using ursula.addons.Ursula.Scripts.GameObjects.Controller;
using Ursula.Core.DI;
using Ursula.GameObjects.Model;

namespace ursula.addons.Ursula.Scripts.GameObjects.View
{
    public partial class EcosystemGeneratorView : Control, IInjectable
    {
        [Inject]
        protected ISingletonProvider<GameObjectCollectionModel> _gameObjectCollectionModelProvider;
        protected GameObjectCollectionModel _gameObjectCollectionModel;

        // Провайдер контроллера генерации
        [Inject]
        private ISingletonProvider<SimulationGeneratorController> _simulationGeneratorControllerProvider;
        private SimulationGeneratorController _simulationGeneratorController;

        [Export]
        public Array<EcosystemGeneratorAssetView> prefabs;

        [Export]
        public Button ButtonGenerate;

        [Export]
        public Button ButtonClear;

        private readonly List<Action<EcosystemGeneratorAssetView>> actions = new();

        public override void _Ready()
        {
            base._Ready();
            _ = SubscribeEvent();

            if (prefabs != null)
            {
                for (int i = 0; i < prefabs.Count; i++)
                {
                    if (prefabs[i] != null)
                        prefabs[i].clickItemEvent += OnItemClickEvent;
                }
            }

            if (ButtonGenerate != null)
                ButtonGenerate.ButtonDown += OnButtonGenerateClick;

            if (ButtonClear != null)
                ButtonClear.ButtonDown += OnButtonClearClick;
        }

        public override void _ExitTree()
        {
            base._ExitTree();

            if (prefabs != null)
            {
                for (int i = 0; i < prefabs.Count; i++)
                {
                    if (prefabs[i] != null)
                        prefabs[i].clickItemEvent -= OnItemClickEvent;
                }
            }

            if (ButtonGenerate != null)
                ButtonGenerate.ButtonDown -= OnButtonGenerateClick;

            if (ButtonClear != null)
                ButtonClear.ButtonDown -= OnButtonClearClick;
        }

        private void OnButtonClearClick()
        {
            GD.Print("Clear Ecosystem");
            // Здесь позже можно добавить реальную очистку с помощью модели/контроллера
        }

        /// <summary>
        /// Генерируем КАЖДЫЙ объект из prefabs, у которого есть GameObjectAssetInfo
        /// и PopulationCount > 0, как 100% один тип (asset1 == asset2, percent = 100f).
        /// </summary>
        private void OnButtonGenerateClick()
        {
            if (_simulationGeneratorController == null)
            {
                GD.PrintErr($"{nameof(EcosystemGeneratorView)}: {nameof(SimulationGeneratorController)} is not initialized.");
                return;
            }

            if (prefabs == null || prefabs.Count == 0)
            {
                GD.Print("EcosystemGeneratorView: no prefabs to generate.");
                return;
            }

            GD.Print("Generate Ecosystem");

            foreach (var view in prefabs)
            {
                if (view == null)
                    continue;

                var info = view.GameObjectAssetInfo;
                if (info == null)
                    continue;

                int count = Mathf.Max(0, info.PopulationCount);
                if (count <= 0)
                    continue;

                // Генерируем этот ассет как 100% один тип
                _simulationGeneratorController.GenerateSimulationItems(
                    info,   // asset1
                    info,   // asset2 тот же самый
                    count,  // количество объектов
                    100f,   // 100% первого (и единственного) типа
                    0f      // coefficient — пока 0, при необходимости можно прокинуть из UI
                );

                GD.Print($"EcosystemGeneratorView: generated {count} entities of {info.Name}.");
            }
        }

        private async GDTask SubscribeEvent()
        {
            if (_gameObjectCollectionModelProvider != null)
            {
                _gameObjectCollectionModel = await _gameObjectCollectionModelProvider.GetAsync();
            }
            else
            {
                GD.PrintErr($"{nameof(EcosystemGeneratorView)}: GameObjectCollectionModel provider is null.");
            }

            if (_simulationGeneratorControllerProvider != null)
            {
                _simulationGeneratorController = await _simulationGeneratorControllerProvider.GetAsync();
            }
            else
            {
                GD.PrintErr($"{nameof(EcosystemGeneratorView)}: SimulationGeneratorController provider is null.");
            }
        }

        private void OnItemClickEvent(EcosystemGeneratorAssetView view)
        {
            if (_gameObjectCollectionModel == null)
            {
                GD.PrintErr($"{nameof(EcosystemGeneratorView)}: GameObjectCollectionModel is not initialized.");
                return;
            }

            view.Invalidate(_gameObjectCollectionModel.AssetSelected);
        }

        // Реализация IInjectable; логика инициализации — в SubscribeEvent()
        public void OnDependenciesInjected()
        {
        }
    }
}
