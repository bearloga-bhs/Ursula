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
using Ursula.GameProjects.Model;

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
        private ISingletonProvider<GameProjectLibraryManager> _commonLibraryProvider;

        [Inject]
        private ISingletonProvider<GameProjectCollectionViewModel> _gameProjectCollectionViewModelProvider;

        void IInjectable.OnDependenciesInjected()
        {
        }

        public override void _Ready()
        {
            base._Ready();

            //ButtonClickAsset.ButtonDown += OnItemClickEvent;
            ButtonGenerate.ButtonDown += OnItemClickEvent;
        }

        public override void _ExitTree()
        {
            base._ExitTree();
            ButtonGenerate.ButtonDown -= OnItemClickEvent;
        }

        private void OnItemClickEvent()
        {
            int entitiesCount = Convert.ToInt32(TextEditEntitiesCount.Text);
            float percent = Convert.ToSingle(TextEditPercent.Text);
            float coefficient = Convert.ToSingle(TextEditCoefficient.Text);
            float radius = Convert.ToSingle(TextEditRadius.Text);
            int protectionCount = Convert.ToInt32(TextEditProtectionCount.Text);
            GD.Print($"Generate Simulation with params: entitiesCount={entitiesCount}, percent={percent}, coefficient={coefficient}, radius={radius}, protectionCount={protectionCount}");
        }
    }
}
