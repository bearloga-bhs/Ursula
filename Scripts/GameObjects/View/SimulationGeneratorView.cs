using Fractural.Tasks;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ursula.addons.Ursula.Scripts.GameObjects.Controller;
using Ursula.Core.DI;
using Ursula.GameObjects.Model;
using Ursula.GameObjects.View;
using Ursula.GameProjects.Model;
using Ursula.Terrain.Model;
using Ursula.Water.Model;
using static Godot.TileSet;

namespace ursula.addons.Ursula.Scripts.GameObjects.View
{
    public partial class SimulationGeneratorView : Control, IInjectable
    {
        [Export]
        private Label LabelEntitiesCount;
        [Export]
        private TextEdit TextEditEntitiesCount;

        [Export]
        private Label LabelPetcent;
        [Export]
        private TextEdit TextEditPercent;

        [Export]
        private Label LabelCoefficient;
        [Export]
        private TextEdit TextEditCoefficient;

        [Export]
        private Label LabelRadius;
        [Export]
        private TextEdit TextEditRadius;

        [Export]
        private Label LabelProtectionCount;
        [Export]
        private TextEdit TextEditProtectionCount;

        [Export]
        private Button ButtonGenerate;

        [Export]
        private GameObjectAssetInfoView view1;
        [Export]
        private GameObjectAssetInfoView view2;


        [Inject]
        protected ISingletonProvider<SimulationGeneratorController> _simulationGeneratorControllerProvider;
        protected SimulationGeneratorController _simulationGeneratorController;


        [Inject]
        protected ISingletonProvider<GameObjectCollectionModel> _gameObjectCollectionModelProvider;
        protected GameObjectCollectionModel _gameObjectCollectionModel;


        [Inject]
        protected ISingletonProvider<GameObjectCreateItemsModel> _gameObjectCreateItemsModelProvider;
        protected GameObjectCreateItemsModel _gameObjectCreateItemsModel;

        [Inject]
        protected ISingletonProvider<TerrainModel> _terrainModelProvider;
        protected TerrainModel _terrainModel;

        [Inject]
        protected ISingletonProvider<TerrainManager> _terrainManagerProvider;
        protected TerrainManager _terrainManager;

        [Inject]
        protected ISingletonProvider<WaterModel> _waterModelProvider;
        protected WaterModel _waterModel;


        void IInjectable.OnDependenciesInjected()
        {
        }

        public override void _Ready()
        {
            base._Ready();
            _ = SubscribeEvent();

            //ButtonClickAsset.ButtonDown += OnItemClickEvent;
            ButtonGenerate.ButtonDown += OnButtonGenerateClickEvent;
            view1.clickItemEvent += OnView1ClickEvent;
            view2.clickItemEvent += OnView2ClickEvent;
        }

        private async GDTask SubscribeEvent()
        {
            _simulationGeneratorController = await _simulationGeneratorControllerProvider.GetAsync();
            _gameObjectCollectionModel = await _gameObjectCollectionModelProvider.GetAsync();
            
            _gameObjectCreateItemsModel = await _gameObjectCreateItemsModelProvider.GetAsync();
            _terrainModel = await _terrainModelProvider.GetAsync();
            _terrainManager = await _terrainManagerProvider.GetAsync();
            _waterModel = await _waterModelProvider.GetAsync();

            _simulationGeneratorController.Init(
                _gameObjectCreateItemsModel,
                _gameObjectCollectionModel,
                _terrainModel,
                _terrainManager,
                _waterModel);
        }

        public override void _ExitTree()
        {
            base._ExitTree();
            ButtonGenerate.ButtonDown -= OnButtonGenerateClickEvent;
        }

        private void OnButtonGenerateClickEvent()
        {
            int entitiesCount = Convert.ToInt32(TextEditEntitiesCount.Text);
            float percent = Convert.ToSingle(TextEditPercent.Text);
            float coefficient = Convert.ToSingle(TextEditCoefficient.Text);
            float radius = Convert.ToSingle(TextEditRadius.Text);
            int protectionCount = Convert.ToInt32(TextEditProtectionCount.Text);
            GD.Print($"Generate Simulation with params: entitiesCount={entitiesCount}, percent={percent}, coefficient={coefficient}, radius={radius}, protectionCount={protectionCount}");
            _simulationGeneratorController.GenerateSimulationItems(view1.GameObjectAssetInfo, view2.GameObjectAssetInfo, entitiesCount, percent, coefficient, radius, protectionCount);
        }

        private void OnView1ClickEvent(GameObjectAssetInfo assetInfo)
        {
            view1.Invalidate(_gameObjectCollectionModel.AssetSelected);

            GD.Print($"Set assetInfo1 {view1.GameObjectAssetInfo} {view1.GameObjectAssetInfo.Name}  {view1.GameObjectAssetInfo.GetGraphXmlPath()}");
        }

        private void OnView2ClickEvent(GameObjectAssetInfo assetInfo)
        {
            view2.Invalidate(_gameObjectCollectionModel.AssetSelected);

            GD.Print($"Set assetInfo2 {view2.GameObjectAssetInfo} {view2.GameObjectAssetInfo.Name} {view2.GameObjectAssetInfo.GetGraphXmlPath()}");
        }
    }
}
