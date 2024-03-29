﻿/*
 * ObjectDestroyListener.cs 
 * Authors Sawyer Hood, Max Miller, Victoria Deen, Gaby Llave
 */

using System;
using Box2D.Collision;
using Box2D.Dynamics;
using Box2D.Dynamics.Contacts;

namespace PiratePalooza
{
	public class ObjectDestroyListener : b2ContactListener
	{
		public const float BREAK_FORCE = 10;
		NewGameLayer game;
		public ObjectDestroyListener (NewGameLayer game)
		{
			this.game = game;
		}
			
		override public void PreSolve (b2Contact contact, b2Manifold oldManifold){}

		override public void  PostSolve (b2Contact contact, ref b2ContactImpulse impulse)
		{
			if(impulse.normalImpulses[0] > 1f)

			if (impulse.normalImpulses [0] > BREAK_FORCE) {
				if (contact.FixtureA.Body.UserData is CCPhysicsSprite) {
					CCPhysicsSprite spriteA = (CCPhysicsSprite)contact.FixtureA.Body.UserData;
					if (spriteA != null && (spriteA.type == EntityType.Block)) {
						game.AddToRemoveList (spriteA);
					}
				}

				if (contact.FixtureB.Body.UserData is CCPhysicsSprite) {
					CCPhysicsSprite spriteB = (CCPhysicsSprite)contact.FixtureA.Body.UserData;

					if (spriteB != null && (spriteB.type == EntityType.Block)) {
						game.AddToRemoveList (spriteB);
					}
				}
			}
		}


	}
}

