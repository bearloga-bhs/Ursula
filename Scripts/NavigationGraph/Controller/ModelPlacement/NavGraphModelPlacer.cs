using bearloga.addons.Ursula.Scripts.NavigationGraph.Controller.ModelPlacement.Placers;
using bearloga.addons.Ursula.Scripts.NavigationGraph.Model;
using Fractural.Tasks;
using Godot;
using System;
using System.Drawing;
using Ursula.Core.DI;
using Ursula.GameObjects.Model;
using Ursula.MapManagers.Setters;
using Ursula.Water.Model;

namespace bearloga.addons.Ursula.Scripts.NavigationGraph.Controller.ModelPlacement
{
    public partial class NavGraphModelPlacer : Node, IInjectable
    {
        [Inject]
        private ISingletonProvider<GameObjectLibraryManager> _commonLibraryProvider;
        GameObjectLibraryManager gameObjectLibraryManager;

        //[Inject]
        //private ISingletonProvider<MapManagerItemSetter> _MapManagerItemSetterProvider;
        //private MapManagerItemSetter mapManagerItemSetter;

        [Inject]
        private ISingletonProvider<GameObjectCreateItemsModel> _gameObjectCreateItemsModelProvider;
        private GameObjectCreateItemsModel gameObjectCreateItemsModel;

        [Inject]
        private ISingletonProvider<GameObjectCollectionModel> _gameObjectCollectionModelProvider;
        private GameObjectCollectionModel gameObjectCollectionModel;

        private IGameObjectAsset roadCross;
        private IGameObjectAsset roadT;
        private IGameObjectAsset roadStraight;
        private IGameObjectAsset roadTurn;

        private IGameObjectAsset trafficLightGreen;
        private IGameObjectAsset trafficLightRed;

        private IGameObjectAsset car;

        public static NavGraphModelPlacer Instance { get; private set; }

        private RandomNumberGenerator rng = new RandomNumberGenerator();

        public override void _Ready()
        {
            base._Ready();
            Instance = this;
            _ = SubscribeEvent();
        }

        private async GDTask SubscribeEvent()
        {
            gameObjectLibraryManager = await _commonLibraryProvider.GetAsync();
            //mapManagerItemSetter = await _MapManagerItemSetterProvider.GetAsync();
            gameObjectCreateItemsModel = await _gameObjectCreateItemsModelProvider.GetAsync();
            gameObjectCollectionModel = await _gameObjectCollectionModelProvider.GetAsync();
        }

        private void LoadRoadAssets()
        {
            string roadCrossId = $"{GameObjectAssetsEmbeddedSource.LibId}.road_cross";
            string roadTId = $"{GameObjectAssetsEmbeddedSource.LibId}.road_t";
            string roadStraightId = $"{GameObjectAssetsEmbeddedSource.LibId}.road_straight";
            string roadTurnId = $"{GameObjectAssetsEmbeddedSource.LibId}.road_turn";

            roadCross = GetEmbeddedAsset(roadCrossId);
            roadT = GetEmbeddedAsset(roadTId);
            roadStraight = GetEmbeddedAsset(roadStraightId);
            roadTurn = GetEmbeddedAsset(roadTurnId);
        }

        private void LoadTrafficLightsAssets()
        {
            string trafficLightGreenId = $"{GameObjectAssetsEmbeddedSource.LibId}.traffic_light_green";
            string trafficLightRedId = $"{GameObjectAssetsEmbeddedSource.LibId}.traffic_light_red";

            trafficLightGreen = GetEmbeddedAsset(trafficLightGreenId);
            trafficLightRed = GetEmbeddedAsset(trafficLightRedId);
        }

        private void LoadCarsAssets()
        {
            string CarId = $"{GameObjectAssetsEmbeddedSource.LibId}.Cow";

            car = GetEmbeddedAsset(CarId);
        }

        private float GetScale(IGameObjectAsset asset, float size)
        {
            Aabb aabb = NavGraphPlacerUtils.GetNodeAABB(asset);
            float modelRadius = NavGraphPlacerUtils.GetRadiusFromAABB(aabb);
            return size / modelRadius / 2;
        }

        public async GDTask GenerateRoads(NavGraph navGraph, float size, float heightOffset)
        {
            if (navGraph is null)
            {
                throw new ArgumentNullException(nameof(navGraph));
            }

            LoadRoadAssets();

            float roadStraightScale = GetScale(roadStraight, size);
            float roadTurnScale = GetScale(roadTurn, size);
            float roadTScale = GetScale(roadT, size);
            float roadCrossScale = GetScale(roadCross, size);

            NavGraphRoadPlacer roadPlacer = new NavGraphRoadPlacer(gameObjectCollectionModel, gameObjectCreateItemsModel);
            foreach (NavGraphVertex vertex in navGraph.vertices)
            {
                switch (vertex.edges.Count)
                {
                    case 1:
                        roadPlacer.PlaceRoadStraight(roadStraight.Info, vertex, roadStraightScale, heightOffset);
                        break;
                    case 2:
                        roadPlacer.PlaceRoadStraightOrTurn(roadStraight.Info, roadTurn.Info, vertex, roadStraightScale, roadTurnScale, heightOffset);
                        break;
                    case 3:
                        roadPlacer.PlaceRoadT(roadT.Info, vertex, roadTScale, heightOffset);
                        break;
                    case 4:
                        roadPlacer.PlaceRoadCross(roadCross.Info, vertex, roadCrossScale, heightOffset);
                        break;
                    default:
                        throw new Exception("Encountered vertex with wrong number of edges connected.");
                }

                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            }
        }

        public async GDTask GenerateTrafficLights(NavGraph navGraph, float size, Vector3 offset)
        {
            if (navGraph is null)
            {
                throw new ArgumentNullException(nameof(navGraph));
            }

            LoadTrafficLightsAssets();

            float trafficLightRedScale = GetScale(trafficLightRed, size);

            NavGraphTrafficLightsPlacer trafficLightsPlacer = new NavGraphTrafficLightsPlacer(gameObjectCollectionModel, gameObjectCreateItemsModel);
            foreach (NavGraphEdge edge in navGraph.edges)
            {
                if (edge.v2.shedule != null)
                {
                    trafficLightsPlacer.PlaceTrafficLights(trafficLightRed.Info, edge, trafficLightRedScale, offset);
                    await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                }
            }
        }

        public async GDTask GenerateCars(NavGraph navGraph, int carCount, float modelHegihtOffset)
        {
            if (navGraph is null)
            {
                throw new ArgumentNullException(nameof(navGraph));
            }

            LoadCarsAssets();

            Aabb aabb = NavGraphPlacerUtils.GetNodeAABB(car);
            float modelRadius = NavGraphPlacerUtils.GetRadiusFromAABB(aabb);

            NavGraphCarPlacer carPlacer = new NavGraphCarPlacer(gameObjectCollectionModel, gameObjectCreateItemsModel, modelRadius);
            carPlacer.AssignEdgesUniform(navGraph, carCount, rng);
            foreach (NavGraphEdge edge in navGraph.edges)
            {
                if (carPlacer.PlaceCars(car.Info, edge, 1, modelHegihtOffset))
                    await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            }
        }

        private IGameObjectAsset GetEmbeddedAsset(string id)
        {
            if (gameObjectLibraryManager.TryGetItem(id, out IGameObjectAsset asset))
            {
                return asset;
            }
            else
            {
                throw new Exception($"Couldn't find asset {id} in embedded lib");
            }
        }

        public void OnDependenciesInjected()
        {
            
        }
    }
}
