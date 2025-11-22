using System;
using Ursula.GameObjects.Model;

namespace ursula.addons.Ursula.Scripts.GameObjects.Model
{
    [Serializable]
    public class EcosystemGeneratorAssetInfo : GameObjectAssetInfo
    {
        public string Type;
        public string Sex;
        public int PopulationCount;
        public int Famine;
        public int ChildCount;

        public EcosystemGeneratorAssetInfo(string name, string providerId, GameObjectTemplate template) : base(name, providerId, template)
        {
        }

        public EcosystemGeneratorAssetInfo(GameObjectAssetInfo info) : base(info.Name, info.ProviderId, info.Template)
        {
        }
    }
}
