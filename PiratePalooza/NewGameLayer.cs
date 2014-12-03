using System;
using System.Collections.Generic;
using CocosDenshion;
using CocosSharp;
using System.Linq;
using System.Diagnostics;
using Newtonsoft.Json;

#if NETFX_CORE

#endif

using Box2D.Common;
using Box2D.Dynamics;
using Box2D.Collision.Shapes;

namespace PiratePalooza
{
	public class NewGameLayer : CCLayerColor
	{

		const int PTM_RATIO = 32;
		const float CANNON_FORCE = 1000;

		float elapsedTime;
		b2World world;
		CCSpriteBatchNode spriteBatch;
		CCSpriteSheet spriteSheet;
		CCSpriteFrame blockFrame;
		CCSpriteFrame ballFrame;
		CCSpriteFrame cannonFrame;
		List<Cannon> cannons;
		int playerTurn;
		Stopwatch timeSinceFire;
		CCLabelTtf turnLabel;
		string map;




		public NewGameLayer (string map)
		{
			this.map = map;
			timeSinceFire = new Stopwatch ();
			timeSinceFire.Start ();
			playerTurn = 0;
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
			AddChild (spriteBatch, 1, 1);
			InitCannons ();
			InitLabel ();
			StartScheduling ();
		}

		void InitCannons() {
			cannons = new List<Cannon> ();
			Cannon cannonObj;
			cannonObj = new Cannon (cannonFrame);
			cannonObj.Position = new CCPoint (400, 100);
			Cannon cannonObj2 = new Cannon (cannonFrame);
			cannonObj2.Position = new CCPoint (880, 100);
			cannons.Add (cannonObj);
			cannons.Add (cannonObj2);
			AddChild (cannonObj);
			AddChild (cannonObj2);
		}

		void InitLabel() {
			turnLabel = new CCLabelTtf("Player 1's Turn", "arial", 22) {
				Position = new CCPoint(VisibleBoundsWorldspace.Center.X + 600, VisibleBoundsWorldspace.Center.Y + 600),
				Color = CCColor3B.Green,
				HorizontalAlignment = CCTextAlignment.Center,
				VerticalAlignment = CCVerticalTextAlignment.Center,
				AnchorPoint = CCPoint.AnchorMiddle
			};

			AddChild (turnLabel);
		}


		void touchFunction(List<CCTouch> touches, CCEvent touchEvent) {
			//Color = CCColor3B.Black;
			if (timeSinceFire.ElapsedMilliseconds < 1000) {
				return;
			}
			timeSinceFire.Restart ();
			var currCannon = cannons[playerTurn];
			var location = touches [0].LocationOnScreen;
			location = WorldToScreenspace (location);  //Layer.WorldToScreenspace(location); 
			float angle = currCannon.AimCannon (location);
			AddBall (currCannon.Position, angle);
			playerTurn += 1;
			if (playerTurn >= cannons.Count) {
				playerTurn = 0;
			}
			turnLabel.Text = "Player " + (playerTurn + 1) + "'s Turn";
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
			//List<CCPoint> points = new List<CCPoint> ();
			/*
			points.Add (new CCPoint(500f, 0f));
			points.Add (new CCPoint(500f, 100f));
			points.Add (new CCPoint(500f, 200f));
			points.Add (new CCPoint(500f, 300f));
			points.Add (new CCPoint(500f, 400f));
			points.Add (new CCPoint(500f, 500f));
			points.Add (new CCPoint(1000f, 600f));
			points.Add (new CCPoint(1200f, 0f));
			points.Add (new CCPoint(1200f, 100f));
			points.Add (new CCPoint(1200f, 200f));
			points.Add (new CCPoint(1200f, 300f));
			points.Add (new CCPoint(1200f, 400f));
			points.Add (new CCPoint(1200f, 500f));
			points.Add (new CCPoint(1200f, 600f));
			points.Add (new CCPoint(1050f, 700f));
			points.Add (new CCPoint(1150f, 700f));*/

			/*for (int i = 0; i < points.Count; i++) {
				//AddBlock (point);
				entities.Add (new MapEntity (EntityType.Block, points[i].X, points[i].Y, 1));

			}*/

			List<MapEntity> entities = JsonConvert.DeserializeObject <List<MapEntity>> (map);
			LoadMapFromEntityList (entities);
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

		void LoadMapFromEntityList (List<MapEntity> list) {
			foreach (var entity in list) {
				Console.WriteLine (entity.x);
				if (entity.type == EntityType.Block) {
					AddBlock (new CCPoint(entity.x, entity.y));
				} 
			}
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

		public static CCScene GameScene (CCWindow mainWindow, string map)
		{
			var scene = new CCScene (mainWindow);
			var layer = new NewGameLayer (map);

			scene.AddChild (layer);

			return scene;
		}

	}
}

