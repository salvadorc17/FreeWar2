using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Collision;
using DEngine;

namespace FactionsGame.Actors
{
    /// <summary>
    /// Anti-infantry structure.
    /// </summary>
    public class Pillbox : RTSActor
    {
        // Point on circle away from this actor to create bullet/effect
        Vector2 barrelVector = new Vector2(64, 64);
        protected Actor projectileTemplate;
        float _turretRotation = 0f;



        public Pillbox(FactionsGame game)
            : base(game, "Pillbox")
        {
            _engine = game;
            _movable = false;

            _fireRate = 10;
            _damage = 10;
            _health = 200;
            _useBurst = true;
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        #region LoadContent
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();
            this.OnBurstStart += new RTSActorEventHandler(Pillbox_OnBurstStart);
            this.OnAttackStart += new RTSActorTargetEventHandler(Pillbox_OnAttackStart);
        }

        protected override void UnloadContent()
        {
            this.OnBurstStart -= Pillbox_OnBurstStart;
            this.OnAttackStart -= Pillbox_OnAttackStart;
            base.UnloadContent();
        }

        void Pillbox_OnAttackStart(RTSActor target)
        {
            float adjustedDamage = _damage;
            switch (target.Armor)
            {
                case ArmorType.Light:
                    adjustedDamage *= 0.8f;
                    break;
                case ArmorType.Medium:
                    adjustedDamage *= 0.4f;
                    break;
                case ArmorType.Heavy:
                    adjustedDamage *= 0.2f;
                    break;
                case ArmorType.Building:
                    adjustedDamage *= 0.5f;
                    break;
                case ArmorType.Super:
                    adjustedDamage *= 0.05f;
                    break;
                default:
                    break;
            }

            target.Health -= (int)adjustedDamage;

            // Make a bullet/blood splat impact effect
            if (target.Name == "Soldier")
            {
                Actor templateActor = _engine.GetTemplateActorByName("BloodSplat1");
                BloodSplat1 impactEffect = (BloodSplat1)templateActor.Clone();

                // Randomize location of bullet impact slightly
                Random rand = new Random();
                float randomX, randomY;
                randomX = (float)((rand.NextDouble() * inaccuracyValue) - (inaccuracyValue / 2)); // 10 is inaccuracy value
                randomY = (float)((rand.NextDouble() * inaccuracyValue) - (inaccuracyValue / 2)); // 10 is inaccuracy value

                impactEffect.Position
                    = new Vector2(target.Position.X + ((target.Size.X * target.Scale) / 2) + randomX,
                                  target.Position.Y + ((target.Size.Y * target.Scale) / 2) + randomY);
                //impactEffect.Scale = 0.6f;
                impactEffect.TintColor = new Color(255, 255, 255, 200);
                impactEffect.Initialize();

                // Attach the node to the graph
                _engine.EffectsSceneNode1.Children.Add(impactEffect);
            }
            else
            {
                Actor templateActor = _engine.GetTemplateActorByName("BulletImpact");
                BulletImpact impactEffect = (BulletImpact)templateActor.Clone();

                // Randomize location of bullet impact slightly
                Random rand = new Random();
                float randomX, randomY;
                randomX = (float)((rand.NextDouble() * inaccuracyValue) - (inaccuracyValue / 2)); // 10 is inaccuracy value
                randomY = (float)((rand.NextDouble() * inaccuracyValue) - (inaccuracyValue / 2)); // 10 is inaccuracy value

                impactEffect.Position = new Vector2(target.Position.X + randomX, target.Position.Y + randomY);
                //impactEffect.Scale = 0.6f;
                impactEffect.TintColor = new Color(255, 255, 255, 200);
                impactEffect.Initialize();

                // Attach the node to the graph
                _engine.EffectsSceneNode1.Children.Add(impactEffect);
                //engine.Actors.Add(impactEffect);

            }
        }

        void Pillbox_OnBurstStart()
        {
            _engine.PlaySound("LightMG3");
        }
        #endregion


        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }



    }
}
