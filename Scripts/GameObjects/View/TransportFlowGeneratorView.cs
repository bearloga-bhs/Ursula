using bearloga.addons.Ursula.Scripts.NavigationGraph.Controller;
using bearloga.addons.Ursula.Scripts.NavigationGraph.Controller.ModelPlacement;
using Fractural.Tasks;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ursula.Core.DI;
using Ursula.GameObjects.Model;
using Ursula.GameObjects.View;

namespace ursula.addons.Ursula.Scripts.GameObjects.View
{
    public partial class TransportFlowGeneratorView : Control, IInjectable
    {
        [Export]
        private GameObjectAssetInfoView RoadCrossPrefab;
        [Export]
        private GameObjectAssetInfoView RoadTPrefab;
        [Export]
        private GameObjectAssetInfoView RoadStraightPrefab;
        [Export]
        private GameObjectAssetInfoView RoadTurnPrefab;

        [Export]
        private GameObjectAssetInfoView TrafficLightGreenPrefab;
        [Export]
        private GameObjectAssetInfoView TrafficLightRedPrefab;

        [Export]
        private GameObjectAssetInfoView CarPrefab;

        [Export]
        private SliderShowValue SliderScale;
        [Export]
        private SliderShowValue SliderCarsCount;
        [Export]
        private SliderShowValue SliderMinTrafficLightsGreenTime;
        [Export]
        private SliderShowValue SliderMaxTrafficLightsGreenTime;


        [Export]
        private Button ButtonGenerate;
        [Export]
        private Button ButtonClear;

        private float scale;
        private int carsCount;
        private float minTrafficLightsGreenTime;
        private float maxTrafficLightsGreenTime;

        [Inject]
        private ISingletonProvider<GameObjectCollectionModel> _gameObjectCollectionModelProvider;
        private GameObjectCollectionModel _gameObjectCollectionModel;

        public override void _Ready()
        {
            base._Ready();
            _ = SubscribeEvent();

            RoadCrossPrefab.clickItemEvent += OnRoadCrossPrefabClickItemEvent;
            RoadTPrefab.clickItemEvent += OnRoadTPrefabClickItemEvent;
            RoadStraightPrefab.clickItemEvent += OnRoadStraightPrefabClickItemEvent;
            RoadTurnPrefab.clickItemEvent += OnRoadTurnPrefabClickItemEvent;
            
            TrafficLightGreenPrefab.clickItemEvent += OnTrafficLightGreenPrefabClickItemEvent;
            TrafficLightRedPrefab.clickItemEvent += OnTrafficLightRedPrefabClickItemEvent;
            
            CarPrefab.clickItemEvent += OnCarPrefabClickItemEvent;

            SliderScale.ValueChanged += OnSliderScaleValueChanged;
            SliderCarsCount.ValueChanged += OnSliderCarsCountValueChanged;
            SliderMinTrafficLightsGreenTime.ValueChanged += OnSliderMinTrafficLightsGreenTimeValueChanged;
            SliderMaxTrafficLightsGreenTime.ValueChanged += OnSliderMaxTrafficLightsGreenTimeValueChanged;

            ButtonGenerate.ButtonDown += OnButtonGenerateClick;
            ButtonClear.ButtonDown += OnButtonClearClick;
        }

        private void OnButtonGenerateClick()
        {
            GD.Print("Generate Transport Flow");
            GD.Print($"scale - {scale}, carsCount - {carsCount}, minTrafficLightsGreenTime - {minTrafficLightsGreenTime}, maxTrafficLightsGreenTime - {maxTrafficLightsGreenTime}");

            // Init
            NavGraphModelPlacer.Instance.Init(
                RoadCrossPrefab.GameObjectAssetInfo, 
                RoadTPrefab.GameObjectAssetInfo, 
                RoadStraightPrefab.GameObjectAssetInfo, 
                RoadTurnPrefab.GameObjectAssetInfo, 
                TrafficLightGreenPrefab.GameObjectAssetInfo, 
                TrafficLightRedPrefab.GameObjectAssetInfo, 
                CarPrefab.GameObjectAssetInfo
                );
            NavGraphManager.Instance.Init(scale, carsCount, minTrafficLightsGreenTime, maxTrafficLightsGreenTime);
            
            // Generate
            TerrainManager terrainManager = VoxLib.terrainManager;
            float height = terrainManager.GetTerrainHeight(terrainManager.size / 2, terrainManager.size / 2);
           _ = NavGraphManager.Instance.Generate(terrainManager.countBlock, height);
        }
        private void OnButtonClearClick()
        {
            GD.Print("Clear Transport Flow");
            // Логика очистки транспортного потока
        }

        private void OnSliderMaxTrafficLightsGreenTimeValueChanged(double value)
        {
            maxTrafficLightsGreenTime = Convert.ToSingle(SliderMaxTrafficLightsGreenTime.Value);
        }

        private void OnSliderMinTrafficLightsGreenTimeValueChanged(double value)
        {
            minTrafficLightsGreenTime = Convert.ToSingle(SliderMinTrafficLightsGreenTime.Value);
        }

        private void OnSliderCarsCountValueChanged(double value)
        {
            carsCount = Convert.ToInt32(SliderCarsCount.Value);
        }

        private void OnSliderScaleValueChanged(double value)
        {
            scale = Convert.ToSingle(SliderScale.Value);
        }

        private async GDTask SubscribeEvent()
        {
            _gameObjectCollectionModel = await _gameObjectCollectionModelProvider.GetAsync();
        }

        private void OnCarPrefabClickItemEvent(GameObjectAssetInfo info)
        {
            CarPrefab.Invalidate(_gameObjectCollectionModel.AssetSelected);
        }

        private void OnTrafficLightRedPrefabClickItemEvent(GameObjectAssetInfo info)
        {
            TrafficLightRedPrefab.Invalidate(_gameObjectCollectionModel.AssetSelected);
        }

        private void OnTrafficLightGreenPrefabClickItemEvent(GameObjectAssetInfo info)
        {
            TrafficLightGreenPrefab.Invalidate(_gameObjectCollectionModel.AssetSelected);
        }

        private void OnRoadTurnPrefabClickItemEvent(GameObjectAssetInfo info)
        {
            RoadTurnPrefab.Invalidate(_gameObjectCollectionModel.AssetSelected);
        }

        private void OnRoadStraightPrefabClickItemEvent(GameObjectAssetInfo info)
        {
            RoadStraightPrefab.Invalidate(_gameObjectCollectionModel.AssetSelected);
        }

        private void OnRoadTPrefabClickItemEvent(GameObjectAssetInfo info)
        {
            RoadTPrefab.Invalidate(_gameObjectCollectionModel.AssetSelected);
        }

        private void OnRoadCrossPrefabClickItemEvent(GameObjectAssetInfo info)
        {
            RoadCrossPrefab.Invalidate(_gameObjectCollectionModel.AssetSelected);
        }

        public void OnDependenciesInjected()
        {
        }
    }
}
