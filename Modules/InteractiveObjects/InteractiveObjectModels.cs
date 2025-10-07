using Godot;
using System.Linq;
using Fractural.Tasks;
using Ursula.Core.DI;
using Ursula.GameObjects.Model;

public partial class InteractiveObjectModels : Node, IInjectable
{
	[Inject]
	private ISingletonProvider<GameObjectLibraryManager> _gameObjectLibraryManagerProvider;
	
	GameObjectLibraryManager _gameObjectLibraryManager;
	
	public override void _Ready()
	{
		base._Ready();
		_ = SubscribeEvent();
	}

	private async GDTask SubscribeEvent()
	{
		_gameObjectLibraryManager = await _gameObjectLibraryManagerProvider.GetAsync();
	}

	public void ChangeModel(string modelName)
	{
		if (_gameObjectLibraryManager == null)
			return;

		if (!_gameObjectLibraryManager.TryGetItem(modelName, out var asset))
			return;
		if (asset?.Model3d is not Node3D model)
			return;

		var newRoot = model.Duplicate() as Node3D;
		if (newRoot == null) return;

		var oldRoot = GetParent<Node3D>();
		var grandParent = oldRoot?.GetParent();
		if (grandParent == null) return;

		var toRemove = newRoot.GetChildren()
			.Where(n =>
			{
				string name = n.Name;
				return name != null && (name.StartsWith("InteractiveObject") || name.StartsWith("ItemPropsScript"));
			})
			.ToList();

		foreach (var n in toRemove)
		{
			newRoot.RemoveChild(n);
			n.QueueFree();
		}

		var toMove = oldRoot.GetChildren()
			.Where(n =>
			{
				string name = n.Name;
				return name != null && (name.StartsWith("InteractiveObject") || name.StartsWith("ItemPropsScript"));
			})
			.ToList();

		foreach (var n in toMove)
		{
			oldRoot.RemoveChild(n);
			newRoot.AddChild(n, true);
		}

		newRoot.Name = oldRoot.Name;
		newRoot.Transform = oldRoot.Transform;

		grandParent.RemoveChild(oldRoot);
		oldRoot.QueueFree();

		grandParent.AddChild(newRoot, true);
	}
	
	public void OnDependenciesInjected() { }
}
