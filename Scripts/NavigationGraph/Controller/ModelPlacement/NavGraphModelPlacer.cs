using bearloga.addons.Ursula.Scripts.NavigationGraph.Controller.ModelPlacement.Placers;
using bearloga.addons.Ursula.Scripts.NavigationGraph.Model;
using Fractural.Tasks;
using Godot;
using System;
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

        private GameObjectAssetInfo roadCross;
        private GameObjectAssetInfo roadT;
        private GameObjectAssetInfo roadStraight;
        private GameObjectAssetInfo roadTurn;

        private GameObjectAssetInfo trafficLightGreen;
        private GameObjectAssetInfo trafficLightRed;

        public static NavGraphModelPlacer Instance { get; private set; }

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

            roadCross = GetEmbeddedAsset(roadCrossId).Info;
            roadT = GetEmbeddedAsset(roadTId).Info;
            roadStraight = GetEmbeddedAsset(roadStraightId).Info;
            roadTurn = GetEmbeddedAsset(roadTurnId).Info;
        }

        private void LoadTrafficLightsAssets()
        {
            string trafficLightGreenId = $"{GameObjectAssetsEmbeddedSource.LibId}.traffic_light_green";
            string trafficLightRedId = $"{GameObjectAssetsEmbeddedSource.LibId}.traffic_light_red";

            trafficLightGreen = GetEmbeddedAsset(trafficLightGreenId).Info;
            trafficLightRed = GetEmbeddedAsset(trafficLightRedId).Info;
        }

        public async GDTask GenerateRoads(NavGraph navGraph, float scale, float heightOffset)
        {
            if (navGraph is null)
            {
                throw new ArgumentNullException(nameof(navGraph));
            }

            LoadRoadAssets();
            NavGraphRoadPlacer roadPlacer = new NavGraphRoadPlacer(gameObjectCollectionModel, gameObjectCreateItemsModel);
            foreach (NavGraphVertex vertex in navGraph.vertices)
            {
                switch (vertex.edges.Count)
                {
                    case 1:
                        roadPlacer.PlaceRoadStraight(roadStraight, vertex, scale, heightOffset);
                        break;
                    case 2:
                        roadPlacer.PlaceRoadStraightOrTurn(roadStraight, roadTurn, vertex, scale, heightOffset);
                        break;
                    case 3:
                        roadPlacer.PlaceRoadT(roadT, vertex, scale, heightOffset);
                        break;
                    case 4:
                        roadPlacer.PlaceRoadCross(roadCross, vertex, scale, heightOffset);
                        break;
                    default:
                        throw new Exception("Encountered vertex with wrong number of edges connected.");
                }

                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            }
        }

        public async GDTask GenerateTrafficLights(NavGraph navGraph, float scale, Vector3 offset)
        {
            if (navGraph is null)
            {
                throw new ArgumentNullException(nameof(navGraph));
            }

            LoadTrafficLightsAssets();
            NavGraphTrafficLightsPlacer trafficLightsPlacer = new NavGraphTrafficLightsPlacer(gameObjectCollectionModel, gameObjectCreateItemsModel);
            foreach (NavGraphEdge edge in navGraph.edges)
            {
                if (edge.v2.shedule != null)
                {
                    trafficLightsPlacer.PlaceTrafficLights(trafficLightRed, edge, scale, offset);
                    await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                }
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
