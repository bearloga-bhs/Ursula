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

        private void LoadAssets()
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

        public async GDTask GenerateRoads(NavGraph navGraph, float scale, float heightOffset)
        {
            if (navGraph is null)
            {
                throw new ArgumentNullException(nameof(navGraph));
            }

            LoadAssets();
            foreach (NavGraphVertex vertex in navGraph.vertices)
            {
                switch (vertex.edges.Count)
                {
                    case 1:
                        PlaceStraightRoad(vertex, scale, heightOffset);
                        break;
                    case 2:
                        if (IsRoadStraight(vertex))
                            PlaceStraightRoad(vertex, scale, heightOffset);
                        else
                            PlaceRoadTurn(vertex, scale, heightOffset);
                        break;
                    case 3:
                        PlaceRoadT(vertex, scale, heightOffset);
                        break;
                    case 4:
                        PlaceRoadCross(vertex, scale, heightOffset);
                        break;
                    default:
                        throw new Exception("Encountered vertex with wrong number of edges connected.");
                }

                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            }
        }

        private bool IsRoadStraight(NavGraphVertex vertex)
        {
            if (vertex.edges.Count != 2)
            {
                throw new ArgumentException($"Vertex should be connected to 2 edges");
            }

            NavGraphEdge e1 = vertex.edges[0];
            NavGraphEdge e2 = vertex.edges[1];

            Vector3 dir1 = (e1.v2.position - e1.v1.position).Normalized();
            Vector3 dir2 = (e2.v2.position - e2.v1.position).Normalized();

            float value = Mathf.Abs(dir1.Dot(dir2));
            if (value > 0.5f)
                return true;
            return false;
        }

        private void PlaceStraightRoad(NavGraphVertex vertex, float scale, float heightOffset)
        {
            gameObjectCollectionModel.SetGameObjectAssetSelected(roadStraight);

            Vector3 pos = vertex.position + Vector3.Up * heightOffset;
            NavGraphEdge edge = vertex.edges[0];
            Vector3 dir = edge.v2.position - edge.v1.position;

            if (Mathf.Abs(dir.Z) > Mathf.Abs(dir.X))
                gameObjectCreateItemsModel.SetGameObjectCreateItem(pos, scale, (byte)GameItemRotation.forward);
            else
                gameObjectCreateItemsModel.SetGameObjectCreateItem(pos, scale, (byte)GameItemRotation.right);
        }

        private void PlaceRoadTurn(NavGraphVertex vertex, float scale, float heightOffset)
        {
            gameObjectCollectionModel.SetGameObjectAssetSelected(roadTurn);

            Vector3 pos = vertex.position + Vector3.Up * heightOffset;
            NavGraphEdge edge1 = OrientFromVertex(vertex.edges[0], vertex);
            NavGraphEdge edge2 = OrientFromVertex(vertex.edges[1], vertex);

            Vector3 dir1 = edge1.v2.position - edge1.v1.position;
            Vector3 dir2 = edge2.v2.position - edge2.v1.position;
            Vector3 normal = dir1.Cross(dir2);

            Vector3 mainDir;
            if (normal.Y < 0)
                mainDir = dir1;
            else
                mainDir = dir2;

            gameObjectCreateItemsModel.SetGameObjectCreateItem(pos, scale, EncodeDirection(mainDir));
        }

        private void PlaceRoadT(NavGraphVertex vertex, float scale, float heightOffset)
        {
            gameObjectCollectionModel.SetGameObjectAssetSelected(roadT);

            Vector3 pos = vertex.position + Vector3.Up * heightOffset;
            NavGraphEdge edge1 = OrientFromVertex(vertex.edges[0], vertex);
            NavGraphEdge edge2 = OrientFromVertex(vertex.edges[1], vertex);
            NavGraphEdge edge3 = OrientFromVertex(vertex.edges[2], vertex);
            Vector3 dir1 = edge1.v2.position - edge1.v1.position;
            Vector3 dir2 = edge2.v2.position - edge2.v1.position;
            Vector3 dir3 = edge3.v2.position - edge3.v1.position;

            Vector3 mainDir;
            if (Mathf.Abs(dir1.Dot(dir3)) > 0.5f)
                mainDir = dir2;
            else if (Mathf.Abs(dir1.Dot(dir2)) > 0.5f)
                mainDir = dir3;
            else
                mainDir = dir1;

            gameObjectCreateItemsModel.SetGameObjectCreateItem(pos, scale, EncodeDirection(mainDir));
        }

        private void PlaceRoadCross(NavGraphVertex vertex, float scale, float heightOffset)
        {
            gameObjectCollectionModel.SetGameObjectAssetSelected(roadCross);
            Vector3 pos = vertex.position + Vector3.Up * heightOffset;
            gameObjectCreateItemsModel.SetGameObjectCreateItem(pos, scale, 0);
        }

        private NavGraphEdge OrientFromVertex(NavGraphEdge edge, NavGraphVertex vertex)
        {
            if (edge.v1 == vertex)
                return edge;
            else
                return new NavGraphEdge(edge.v2, edge.v1, temp: true);
        }

        private byte EncodeDirection(Vector3 direction)
        {
            if (direction == Vector3.Zero)
                throw new ArgumentException(nameof(direction));

            if (direction.Dot(Vector3.Forward) >= 0.5f)
                return (byte)GameItemRotation.forward;
            if (direction.Dot(Vector3.Right) >= 0.5f)
                return (byte)GameItemRotation.right;
            if (direction.Dot(Vector3.Back) >= 0.5f)
                return (byte)GameItemRotation.backward;
            if (direction.Dot(Vector3.Left) >= 0.5f)
                return (byte)GameItemRotation.left;

            throw new Exception($"Couldn't encode model direction from vector {direction}");
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
