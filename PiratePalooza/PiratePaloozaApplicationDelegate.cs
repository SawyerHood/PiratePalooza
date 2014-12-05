/*
 * Authors Sawyer Hood, Max Miller, Victoria Deen, Gaby Llave
 */
using System;
using CocosSharp;
using CocosDenshion;

namespace PiratePalooza
{
	public class PiratePaloozaApplicationDelegate :  CCApplicationDelegate
	{
		string map;

		public PiratePaloozaApplicationDelegate(string map) : base() {
			this.map = map;
		}
		public override void ApplicationDidFinishLaunching (CCApplication application, CCWindow mainWindow)
		{
			application.PreferMultiSampling = false;
			application.ContentRootDirectory = "Content";

			application.ContentSearchPaths.Add("hd"); //This is where Cocos will look for images and files

			CCSimpleAudioEngine.SharedEngine.PreloadEffect ("Sounds/tap"); //Cache this sound
			CCSize winSize = mainWindow.WindowSizeInPixels;
			mainWindow.SetDesignResolutionSize(winSize.Width, winSize.Height, CCSceneResolutionPolicy.ExactFit);
			CCScene scene = GameStartLayer.GameStartLayerScene(mainWindow, map);
			mainWindow.RunWithScene (scene);
		}

		public override void ApplicationDidEnterBackground (CCApplication application)
		{

			application.Paused = true;

			CCSimpleAudioEngine.SharedEngine.PauseBackgroundMusic ();
		}

		public override void ApplicationWillEnterForeground (CCApplication application)
		{
			application.Paused = false;

			 
			CCSimpleAudioEngine.SharedEngine.ResumeBackgroundMusic ();
		}
	}
}

