using bearloga.addons.Ursula.Scripts.NavigationGraph.Model;
using Ursula.GameObjects.Model;
using Godot;
using Talent.Graphs;
using System;

namespace bearloga.addons.Ursula.Scripts.NavigationGraph.Controller.ModelPlacement.Placers
{
    public class NavGraphTrafficLightsPlacer
    {
        private GameObjectCollectionModel gameObjectCollectionModel;
        private GameObjectCreateItemsModel gameObjectCreateItemsModel;

        public NavGraphTrafficLightsPlacer(GameObjectCollectionModel gameObjectCollectionModel, GameObjectCreateItemsModel gameObjectCreateItemsModel)
        {
            this.gameObjectCollectionModel = gameObjectCollectionModel;
            this.gameObjectCreateItemsModel = gameObjectCreateItemsModel;
        }

        public void PlaceTrafficLights(GameObjectAssetInfo trafficLights, NavGraphEdge edge, float scale, Vector3 offset)
        {
            if (edge.v2.shedule == null)
                throw new ArgumentException($"Edge second vertex should be containing shedule");

            gameObjectCollectionModel.SetGameObjectAssetSelected(trafficLights);
            
            Vector3 dir = (edge.v2.position - edge.v1.position).Normalized();
            Quaternion rotation = new Quaternion(Vector3.Forward, dir);
            offset = rotation * offset;
            Vector3 pos = edge.v2.position + offset;

            gameObjectCreateItemsModel.SetGameObjectCreateItem(pos, scale, NavGraphPlacerUtils.EncodeDirection(-dir));
        }
    }
}
