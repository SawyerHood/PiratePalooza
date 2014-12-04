using System;
using CocosSharp;


namespace PiratePalooza
{
	public enum EntityType {Block, Pirate, Cannon, Ball};

	public class MapEntity
	{
		public EntityType type;
		public float x;
		public float y;
		public int playerSide { get; set;}

		public MapEntity (EntityType type, float x, float y, int playerSide)
		{
			this.type = type;
			this.x = x;
			this.y = y;
			this.playerSide = playerSide;
		}
	}
}

