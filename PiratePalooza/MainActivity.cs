﻿using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using CocosSharp;
using Microsoft.Xna.Framework;


namespace PiratePalooza
{
	//This sets all of the settings for the android app.
	[Activity(
		Label = "PiratePalooza",
		AlwaysRetainTaskState = true,
		//Icon = "@drawable/ic_launcher",
		Theme = "@android:style/Theme.NoTitleBar",
		ScreenOrientation = ScreenOrientation.Landscape,
		LaunchMode = LaunchMode.SingleInstance,
		MainLauncher = true,
		ConfigurationChanges =  ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden)
	]
	public class MainActivity : AndroidGameActivity
	{
		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
			var application = new CCApplication(); //Creates a new Cocos2d app
			application.ApplicationDelegate = new PiratePaloozaApplicationDelegate(); //The delegate is the entry point.
			SetContentView(application.AndroidContentView);
			application.StartGame(); 
		}
	}
}