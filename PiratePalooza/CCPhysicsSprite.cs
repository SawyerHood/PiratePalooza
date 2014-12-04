//Class taken from the Xamarin CocosSharp Tutorial

using System;
using Box2D.Common;
using Box2D.Dynamics;
using CocosSharp;

namespace PiratePalooza
{
	public class CCPhysicsSprite : CCSprite
	{
		readonly float ptmRatio;

		public CCPhysicsSprite (CCTexture2D f, CCRect r, float ptmRatio) : base (f, r)
		{
			this.ptmRatio = ptmRatio;
			playerSide = 0;
		}

		public CCPhysicsSprite(CCSpriteFrame frame, float ptmRatio) : base(frame)
		{
			this.ptmRatio = ptmRatio;
			playerSide = 0;
		}



		public b2Body PhysicsBody { get; set; }
		public EntityType type;
		public int playerSide;

		public void UpdateTransformLocation()
		{
			if (PhysicsBody != null)
			{
				b2Vec2 pos = PhysicsBody.Position;

				float x = pos.x * ptmRatio;
				float y = pos.y * ptmRatio;

				if (IgnoreAnchorPointForPosition) 
				{
					x += AnchorPointInPoints.X;
					y += AnchorPointInPoints.Y;
				}
					
				float radians = PhysicsBody.Angle;

				Position = new CCPoint(x, y);
				Rotation = -radians * 57.2957795f;
			}
		}
	}
}