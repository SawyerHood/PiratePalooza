/*
 *GameStartLayer.cs 
 * Authors Sawyer Hood, Max Miller, Victoria Deen, Gaby Llave
 */

using System;
using CocosSharp;

namespace PiratePalooza
{
	public class GameStartLayer : CCLayerColor
	{
		public string map;
		public GameStartLayer () : base ()
		{
			var touchListener = new CCEventListenerTouchAllAtOnce ();
			//touchListener.OnTouchesEnded = (touches, ccevent) => Window.DefaultDirector.ReplaceScene (GameLayer.GameScene (Window));
			touchListener.OnTouchesEnded = (touches, ccevent) => {
				Color = CCColor3B.Blue; //This is here for debugging purposes.
				Window.DefaultDirector.ReplaceScene (NewGameLayer.GameScene (Window, map)); //Switches scenes for the application.
			};

			AddEventListener (touchListener, this);

			Color = CCColor3B.Black;
			Opacity = 255;
		}

		//This is run when the Scene is first loaded.
		protected override void AddedToScene ()
		{
			base.AddedToScene ();

			//Add the label
			//TODO Change to an image.
			var label = new CCLabelTtf("Tap Screen to start a game!", "arial", 22) {
				Position = VisibleBoundsWorldspace.Center,
				Color = CCColor3B.Green,
				HorizontalAlignment = CCTextAlignment.Center,
				VerticalAlignment = CCVerticalTextAlignment.Center,
				AnchorPoint = CCPoint.AnchorMiddle
			};

			AddChild (label);
		}


		public static CCScene GameStartLayerScene (CCWindow mainWindow, string map)
		{

			var scene = new CCScene (mainWindow);
			var layer = new GameStartLayer ();
			layer.map = map;

			scene.AddChild (layer);

			return scene;
		}
	}
}

