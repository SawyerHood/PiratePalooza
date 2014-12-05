/*
 * NewGameLayer.cs 
 * Authors Sawyer Hood, Max Miller, Victoria Deen, Gaby Llave
 */

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using CocosDenshion;
using CocosSharp;
using System.Linq;
using System.Diagnostics;
using Newtonsoft.Json;
using Box2D.Common;
using Box2D.Dynamics;
using Box2D.Collision.Shapes;

namespace PiratePalooza
{
	public class NewGameLayer : CCLayerColor
	{

		const int PTM_RATIO = 32;
		const float CANNON_FORCE = 15000;
		b2World world;
		CCSpriteBatchNode spriteBatch;
		CCSpriteSheet spriteSheet;
		CCSpriteFrame blockFrame;
		CCSpriteFrame ballFrame;
		CCSpriteFrame cannonFrame;
		CCSpriteFrame pirateFrame;
		List<Cannon> cannons;
		List<int> pirateCounts;
		List<MapEntity> entities;
		ConcurrentStack<CCPhysicsSprite> toRemove;
		int playerTurn;
		Stopwatch timeSinceFire;
		CCLabelTtf turnLabel;
		string map;
		bool gameOver;

		public NewGameLayer (string map)
		{
			gameOver = false;
			this.map = map;
			entities = JsonConvert.DeserializeObject <List<MapEntity>> (map); 
			toRemove = new ConcurrentStack<CCPhysicsSprite> (); //So we don't remove sprites while BOX2d steps.
			timeSinceFire = new Stopwatch (); // Cool down for firing
			timeSinceFire.Start ();
			playerTurn = 0;
			var touchListener = new CCEventListenerTouchAllAtOnce ();
			touchListener.OnTouchesEnded = touchFunction;
			AddEventListener (touchListener, this);
			spriteSheet = new CCSpriteSheet ("texture.plist");
			Color = new CCColor3B (CCColor4B.Aquamarine);
			Opacity = 255;
			spriteBatch = new CCSpriteBatchNode ("texture.png", 100);
			blockFrame = spriteSheet.Frames.Find(x => x.TextureFilename == "block.png");
			ballFrame = spriteSheet.Frames.Find(x => x.TextureFilename == "cannonball.png");
			cannonFrame = spriteSheet.Frames.Find(x => x.TextureFilename == "cannon.png");
			pirateFrame = spriteSheet.Frames.Find(x => x.TextureFilename == "pirate.png");
			AddChild (spriteBatch, 1, 1);
			InitCannons ();
			InitLabel ();
			StartScheduling ();
		}

		//Creates the Cannons for each player. 
		void InitCannons() {
			cannons = new List<Cannon> ();
			Cannon cannonObj;
			cannonObj = new Cannon (cannonFrame);
			cannonObj.Position = new CCPoint (442, 100);
			Cannon cannonObj2 = new Cannon (cannonFrame);
			cannonObj2.Position = new CCPoint (742, 100);
			cannons.Add (cannonObj);
			cannons.Add (cannonObj2);
			AddChild (cannonObj);
			AddChild (cannonObj2);
			pirateCounts = new List<int> ();
			for (int i = 0; i < 2; i++) {

				pirateCounts.Add (entities.FindAll(x => (x.type == EntityType.Pirate && x.playerSide == i)).Count);
				Console.WriteLine ("Pirate count for player " + i + ": " + pirateCounts[i]);
			}
		}

		//This Label is used to state whose turn it is.
		void InitLabel() {
			turnLabel = new CCLabelTtf("Player 1's Turn", "arial", 22) {
				Position = new CCPoint(VisibleBoundsWorldspace.Center.X + 600, VisibleBoundsWorldspace.Center.Y + 600),
				Color = CCColor3B.Black,
				HorizontalAlignment = CCTextAlignment.Center,
				VerticalAlignment = CCVerticalTextAlignment.Center,
				AnchorPoint = CCPoint.AnchorMiddle
			};

			AddChild (turnLabel);
		}

		//Called everytime the screen is touched.
		void touchFunction(List<CCTouch> touches, CCEvent touchEvent) {
			if (timeSinceFire.ElapsedMilliseconds < 1000) {
				return;
			}
			if (gameOver) {
				Window.DefaultDirector.ReplaceScene (NewGameLayer.GameScene (Window, map)); //Restart the game.
			} else { //Fire a cannon.
				timeSinceFire.Restart ();
				var currCannon = cannons [playerTurn];
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
		}

		//This is run when an entity is removed from the game.
		void TryRemoveEntity (CCPhysicsSprite sprite) {
			if (sprite == null) {
				return;
			}

			if (sprite.type == EntityType.Pirate && sprite.Visible) { //Reduce the pirate counts for each player.
				pirateCounts [sprite.playerSide] -= 1;
				Console.WriteLine ("Pirate count for player " + sprite.playerSide + ": " + pirateCounts [sprite.playerSide]);
				if (pirateCounts [sprite.playerSide] <= 0) {
					gameOver = true;
					turnLabel.Text = "PLAYER  " + (((sprite.playerSide + 1) % 2) + 1) + " WINS. TAP TO PLAY AGAIN.";
				}
			}

				sprite.PhysicsBody.World.DestroyBody (sprite.PhysicsBody);
				sprite.Visible = false;
				sprite.RemoveFromParent ();


		}

		public void AddToRemoveList (CCPhysicsSprite sprite) {
			toRemove.Push(sprite);
		}

		//These are the events that run periodically.
		void StartScheduling() {

			Schedule (t => {
				world.Step (t, 8, 1); //Update physics

				foreach (CCPhysicsSprite sprite in spriteBatch.Children) { //Remove entities that are outside of the world bounds.
					if (sprite.Visible && sprite.PhysicsBody.Position.x < 0f || sprite.PhysicsBody.Position.x * PTM_RATIO > ContentSize.Width) { //or should it be Layer.VisibleBoundsWorldspace.Size.Width
						AddToRemoveList(sprite);
					} else {
						sprite.UpdateTransformLocation();
					}
				}

				while (toRemove.Count > 0) { 
					CCPhysicsSprite s = null;

					if(toRemove.TryPop(out s)) {
						TryRemoveEntity (s);
					}
				}

			});
				
		}
			

		//Sets up a solid ground
		//Adapted from Xamarin Tutorial.
		void InitPhysics ()
		{
			CCSize s = Layer.VisibleBoundsWorldspace.Size;

			var gravity = new b2Vec2 (0.0f, -10.0f);
			world = new b2World (gravity);

			world.SetAllowSleeping (true);
			world.SetContinuousPhysics (true);
			//world.SetContactListener (new ObjectDestroyListener(this));

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

		//Set up the map from the entity file.
		void LoadMapFromEntityList (List<MapEntity> list) {
			foreach (var entity in list) {
				if (entity.type == EntityType.Block) {
					AddBlock (new CCPoint (entity.x, entity.y));
				} else if (entity.type == EntityType.Pirate) {
					AddPirate(new CCPoint (entity.x, entity.y), entity.playerSide);
				}
			}

		}

		//Adds a block to the game.
		void AddBlock (CCPoint p) {

			var sprite = new CCPhysicsSprite (blockFrame, PTM_RATIO);
			sprite.type = EntityType.Block;
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
			fd.density = .5f;
			fd.restitution = 0f;
			fd.friction = 1f;
			body.CreateFixture (fd);
			body.UserData = sprite;

			sprite.PhysicsBody = body;


		}



		//Adds a cannon ball to the game.
		void AddBall (CCPoint p, float angle) {

			var sprite = new CCPhysicsSprite (ballFrame, PTM_RATIO);
			sprite.type = EntityType.Ball;
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
			fd.density = 10f;
			fd.restitution = 0f;
			fd.friction = 1f;
			body.CreateFixture (fd);
			body.UserData = sprite;

			sprite.PhysicsBody = body;
			var angleInRadians = (-angle) * Math.PI / 180.0;
			body.ApplyForceToCenter (new b2Vec2((float)Math.Cos (angleInRadians) * CANNON_FORCE, (float)Math.Sin (angleInRadians) * CANNON_FORCE));

	

		}

		//Adds a pirate to the game.
		void AddPirate (CCPoint p, int playerSide) {

			var sprite = new CCPhysicsSprite (pirateFrame, PTM_RATIO);
			sprite.type = EntityType.Pirate;
			spriteBatch.AddChild (sprite);
			sprite.playerSide = playerSide;
			if (sprite.playerSide == 1) {
				sprite.Color = CCColor3B.Green;
			}

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
			fd.density = .5f;
			fd.restitution = 0f;
			fd.friction = 1f;
			body.CreateFixture (fd);
			body.UserData = sprite;

			sprite.PhysicsBody = body;

		}
			

		protected override void AddedToScene ()
		{
			base.AddedToScene ();

			Scene.SceneResolutionPolicy = CCSceneResolutionPolicy.NoBorder;

		}

		//Once the scene is entered some setup is done.
		public override void OnEnter()
		{
			base.OnEnter ();
			InitPhysics ();
			LoadMapFromEntityList (entities);

		}

		//Initialization function.
		public static CCScene GameScene (CCWindow mainWindow, string map)
		{
			var scene = new CCScene (mainWindow);
			var layer = new NewGameLayer (map);

			scene.AddChild (layer);

			return scene;
		}

	}
}

