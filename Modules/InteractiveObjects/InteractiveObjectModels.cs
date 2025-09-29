using Godot;
using System;
using System.Reflection;
using System.Xml.Linq;
using Fractural.Tasks;
using Modules.HSM;
using Ursula.Core.DI;
using Ursula.GameObjects.Model;

public partial class InteractiveObjectModels : Node, IInjectable
{
	[Inject]
	private ISingletonProvider<GameObjectLibraryManager> _gameObjectLibraryManagerProvider;
	
	private InteractiveObject _interactiveObject;
	
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
	
	public InteractiveObject interactiveObject
	{
		get
		{
			if (_interactiveObject == null )
				_interactiveObject = GetParent().GetNode("InteractiveObject") as InteractiveObject;

			return _interactiveObject;
		}
	}

	public void ChangeModel(string modelName)
	{
		var data = _gameObjectLibraryManager.GetItemInfo(modelName);
		if (!_gameObjectLibraryManager.TryGetItem(data.Id, out IGameObjectAsset asset))
			return ;
		if (asset.Model3d == null)
			return ;

		Node3D newRoot = ((asset.Model3d) as Node3D).Duplicate() as Node3D;
		if (newRoot == null)
			return ;

		var oldRoot = GetParent<Node3D>();
		if (oldRoot == null)
			return ;

		var grandParent = oldRoot.GetParent();
		if (grandParent == null)
			return ;

		foreach (var child in newRoot.GetChildren())
		{
			if (child is Node n)
			{
				string nName = n.Name;
				if (nName.StartsWith("InteractiveObject") || nName.StartsWith("ItemPropsScript"))
				{
					newRoot.RemoveChild(n);
					n.QueueFree();
				}
			}
		}

		foreach (var child in oldRoot.GetChildren())
		{
			if (child is Node n)
			{
				string nName = n.Name;
				if (nName.StartsWith("InteractiveObject") || nName.StartsWith("ItemPropsScript"))
				{
					oldRoot.RemoveChild(n);
					newRoot.AddChild(n, true);
				}
			}
		}

		newRoot.Name = oldRoot.Name; 

		newRoot.Transform = oldRoot.Transform;

		grandParent.RemoveChild(oldRoot);
		oldRoot.QueueFree();

		grandParent.AddChild(newRoot, true);
	}
	
	public void OnDependenciesInjected() { }
}
