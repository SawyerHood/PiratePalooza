using System;
using System.Collections.Generic;
using CocosDenshion;
using CocosSharp;
using System.Linq;

#if NETFX_CORE

#endif

using Box2D.Common;
using Box2D.Dynamics;
using Box2D.Collision.Shapes;

namespace PiratePalooza
{
	public class NewGameLayer : CCLayerColor
	{
		const int PTM_RATIO = 100;
		const float CANNON_FORCE = 1000;

		float elapsedTime;
		b2World world;
		CCSpriteBatchNode spriteBatch;
		CCSpriteSheet spriteSheet;
		CCSpriteFrame blockFrame;
		CCSpriteFrame ballFrame;
		CCSpriteFrame cannonFrame;
		Cannon cannonObj;



		public NewGameLayer ()
		{
			var touchListener = new CCEventListenerTouchAllAtOnce ();
			touchListener.OnTouchesEnded = touchFunction;
			AddEventListener (touchListener, this);
			spriteSheet = new CCSpriteSheet ("texture.plist");
			Color = new CCColor3B (CCColor4B.White);
			Opacity = 255;
			spriteBatch = new CCSpriteBatchNode ("texture.png", 100);
			blockFrame = spriteSheet.Frames.Find(x => x.TextureFilename == "block.png");
			ballFrame = spriteSheet.Frames.Find(x => x.TextureFilename == "cannonball.png");
			cannonFrame = spriteSheet.Frames.Find(x => x.TextureFilename == "cannon.png");
			cannonObj = new Cannon (cannonFrame);
			cannonObj.Position = new CCPoint (300, 100);
			AddChild (spriteBatch, 1, 1);
			AddChild (cannonObj);


			StartScheduling ();
		}

		void touchFunction(List<CCTouch> touches, CCEvent touchEvent) {
			//Color = CCColor3B.Black;
			var location = touches [0].LocationOnScreen;
			location = WorldToScreenspace (location);  //Layer.WorldToScreenspace(location); 
			float angle = cannonObj.AimCannon (location);
			AddBall (cannonObj.Position, angle);
		}

		void StartScheduling() {
			Schedule (t => {
				elapsedTime += t;
				//AddBlock();
			}, 1.0f);

			Schedule (t => {
				world.Step (t, 8, 1);

				foreach (CCPhysicsSprite sprite in spriteBatch.Children) {
					if (sprite.Visible && sprite.PhysicsBody.Position.x < 0f || sprite.PhysicsBody.Position.x * PTM_RATIO > ContentSize.Width) { //or should it be Layer.VisibleBoundsWorldspace.Size.Width
						world.DestroyBody (sprite.PhysicsBody);
						sprite.Visible = false;
						sprite.RemoveFromParent ();
					} else {
						sprite.UpdateTransformLocation();
					}
				}
			});
		}

		void StackSomeBlocks() {
			List<CCPoint> points = new List<CCPoint> ();
			points.Add (new CCPoint(1000f, 0f));
			points.Add (new CCPoint(1000f, 100f));
			points.Add (new CCPoint(1000f, 200f));
			points.Add (new CCPoint(1000f, 300f));
			points.Add (new CCPoint(1000f, 400f));
			points.Add (new CCPoint(1000f, 500f));
			points.Add (new CCPoint(1000f, 600f));
			points.Add (new CCPoint(1200f, 0f));
			points.Add (new CCPoint(1200f, 100f));
			points.Add (new CCPoint(1200f, 200f));
			points.Add (new CCPoint(1200f, 300f));
			points.Add (new CCPoint(1200f, 400f));
			points.Add (new CCPoint(1200f, 500f));
			points.Add (new CCPoint(1200f, 600f));
			points.Add (new CCPoint(1050f, 700f));
			points.Add (new CCPoint(1150f, 700f));

			foreach (var point in points) {
				AddBlock (point);
			}
		}

		//Sets up a solid ground
		//Taken from Xamarin Tutorial.
		void InitPhysics ()
		{
			CCSize s = Layer.VisibleBoundsWorldspace.Size;

			var gravity = new b2Vec2 (0.0f, -10.0f);
			world = new b2World (gravity);

			world.SetAllowSleeping (true);
			world.SetContinuousPhysics (true);

			var def = new b2BodyDef ();
			def.allowSleep = true;
			def.position = b2Vec2.Zero;
			def.type = b2BodyType.b2_staticBody;
			b2Body groundBody = world.CreateBody (def);
			groundBody.SetActive (true);

			b2EdgeShape groundBox = new b2EdgeShape ();
			groundBox.Set (b2Vec2.Zero, new b2Vec2 (s.Width / PTM_RATIO, 0));
			b2FixtureDef fd = new b2FixtureDef ();
			fd.shape = groundBox;
			fd.friction = 1f;
			groundBody.CreateFixture (fd);
		}

		void AddBlock (CCPoint p) {

			var sprite = new CCPhysicsSprite (blockFrame, PTM_RATIO);
			spriteBatch.AddChild (sprite);

			sprite.Position = new CCPoint (p.X, p.Y);

			var def = new b2BodyDef ();
			def.position = new b2Vec2 (p.X / PTM_RATIO, p.Y / PTM_RATIO);
			def.linearVelocity = new b2Vec2 (0.0f, -1.0f);
			def.type = b2BodyType.b2_dynamicBody;
			b2Body body = world.CreateBody (def);

			var rect = new b2PolygonShape ();
			rect.SetAsBox (.5f, .5f);

			var fd = new b2FixtureDef ();
			fd.shape = rect;
			fd.density = 1f;
			fd.restitution = 0f;
			fd.friction = 1f;
			body.CreateFixture (fd);

			sprite.PhysicsBody = body;

			#if !NETFX_CORE
			Console.WriteLine ("sprite batch node count = {0}", spriteBatch.ChildrenCount);
			#else

			#endif

		}

		void AddBall (CCPoint p, float angle) {

			var sprite = new CCPhysicsSprite (ballFrame, PTM_RATIO);
			spriteBatch.AddChild (sprite);

			sprite.Position = new CCPoint (p.X, p.Y);

			var def = new b2BodyDef ();
			def.position = new b2Vec2 (p.X / PTM_RATIO, p.Y / PTM_RATIO);
			def.linearVelocity = new b2Vec2 (0.0f, 0.0f);
			def.type = b2BodyType.b2_dynamicBody;
			b2Body body = world.CreateBody (def);

			var circle = new b2CircleShape();
			circle.Radius = .5f;


			var fd = new b2FixtureDef ();
			fd.shape = circle;
			fd.density = 1f;
			fd.restitution = 0f;
			fd.friction = 1f;
			body.CreateFixture (fd);

			sprite.PhysicsBody = body;
			var angleInRadians = (-angle) * Math.PI / 180.0;
			body.ApplyForceToCenter (new b2Vec2((float)Math.Cos (angleInRadians) * CANNON_FORCE, (float)Math.Sin (angleInRadians) * CANNON_FORCE));

			#if !NETFX_CORE
			Console.WriteLine ("sprite batch node count = {0}", spriteBatch.ChildrenCount);
			#else

			#endif

		}
			

		CCPoint GetRandomPosition (CCSize spriteSize)
		{
			double rnd = CCRandom.NextDouble ();
			double randomX = (rnd > 0) 
				? rnd * VisibleBoundsWorldspace.Size.Width - spriteSize.Width / 2 
				: spriteSize.Width / 2;

			return new CCPoint ((float)randomX, VisibleBoundsWorldspace.Size.Height - spriteSize.Height / 2);
		}

		protected override void AddedToScene ()
		{
			base.AddedToScene ();

			Scene.SceneResolutionPolicy = CCSceneResolutionPolicy.NoBorder;

		}

		public override void OnEnter()
		{
			base.OnEnter ();
			InitPhysics ();
			StackSomeBlocks ();
		}

		public static CCScene GameScene (CCWindow mainWindow)
		{
			var scene = new CCScene (mainWindow);
			var layer = new NewGameLayer ();

			scene.AddChild (layer);

			return scene;
		}

	}
}

