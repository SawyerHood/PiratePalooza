/*
 * NewGameLayer.cs 
 * Authors Sawyer Hood, Max Miller, Victoria Deen, Gaby Llave
 */

using System;
using CocosSharp;

namespace PiratePalooza
{
	public class Cannon : CCSprite
	{
		public Cannon (CCSpriteFrame frame) : base(frame)
		{
		}

		public float AimCannon(CCPoint whereToAim)
		{
			CCVector2 vec = whereToAim - this.Position;
			var angle = Math.Atan2 (vec.Y, vec.X);
			var rot = (float) ( - angle * 57.2957795f);
			this.Rotation = rot;
			Console.WriteLine (rot);
			return rot;

		}
	}
}

