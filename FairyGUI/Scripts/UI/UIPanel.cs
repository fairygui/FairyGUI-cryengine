using CryEngine;
using CryEngine.Common;
using System.IO;
using System.Runtime.InteropServices;
/*
namespace FairyGUI
{
	[EntityComponentAttribute(category: "FairyGUI")]
	[Guid("66463B63-6ABB-4982-804A-0727FCA4458D")]
	public class UIPanel : EntityComponent
	{
		/// <summary>
		/// 
		/// </summary>
		public Container container { get; private set; }

		string _packagePath;
		string _componentName;
		bool _touchDisabled;
		GComponent _ui;

		bool _setTextureOp;

		[EntityProperty(EntityPropertyType.AnyFile, "Package description file(.bytes)")]
		public string packagePath
		{
			get
			{
				return _packagePath;
			}
			set
			{
				_packagePath = value;
			}
		}

		[EntityProperty(Description = "Component Name")]
		public string componentName
		{
			get
			{
				return _componentName;
			}
			set
			{
				_componentName = value;
			}
		}

		[EntityProperty(Description = "Touch Disabled")]
		public bool touchDisabled
		{
			get
			{
				return _touchDisabled;
			}
			set
			{
				_touchDisabled = value;
			}
		}

		protected override void OnInitialize()
		{
			base.OnInitialize();

			this.container = new Container();
			this.container.touchable = !_touchDisabled;
			this.container.hitArea = new MeshColliderHitTest(Entity);

			if (!Engine.IsSandbox)
				Start();
		}

		protected override void OnGameplayStart()
		{
			base.OnGameplayStart();

			Start();
		}

		protected override void OnEditorGameModeChange(bool enterGame)
		{
			base.OnEditorGameModeChange(enterGame);

			if (!enterGame)
				Destroy();
		}

		protected override void OnUpdate(float frameTime)
		{
			if (_setTextureOp)
			{
				if (container.paintingGraphics.texture == null)
					return;

				_setTextureOp = false;
				IMaterial mat = Entity.Material;
				if (mat != null)
					mat.SetTexture(container.paintingGraphics.texture.nativeTexture.ID);
			}
		}

		void Start()
		{
			Stage.inst.AddChildAt(container, 0);

			CreateUI();
			this.container.EnterPaintingMode(16, null);
			this.container.paintingGraphics.enabled = false;
			_setTextureOp = true;
		}

		void Destroy()
		{
			Stage.inst.RemoveChild(container);

			if (_ui != null)
			{
				_ui.Dispose();
				_ui = null;
			}
			IMaterial mat = Entity.Material;
			if (mat != null)
				mat.SetTexture(0);
			this.container.LeavePaintingMode(16);
			_setTextureOp = false;
		}

		public GComponent ui
		{
			get
			{
				return _ui;
			}
		}

		void CreateUI()
		{
			if (Engine.IsSandbox && !Engine.IsSandboxGameMode)
				return;

			if (_ui != null)
			{
				_ui.Dispose();
				_ui = null;
			}

			if (!string.IsNullOrEmpty(packagePath))
			{
				string path = Path.ChangeExtension(packagePath, "");
				path = path.Substring(0, path.Length - 1);
				UIPackage pkg = UIPackage.AddPackage(path);

				if (_componentName.Length > 0)
				{
					_ui = pkg.CreateObject(componentName).asCom;
					if (_ui != null)
					{
						container.AddChild(_ui.displayObject);
						container.SetSize(_ui.width, ui.height);
					}
				}
			}
		}
	}
}
*/