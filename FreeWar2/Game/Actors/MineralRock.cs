using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DEngine;

namespace FactionsGame.Actors
{
    public class MineralRock : Actor
    {
        new FactionsGame _engine;
        protected long _minerals;
        protected long _mineralsMax = 50000;
        protected List<Drone> _droneClients = new List<Drone>();

        #region Public Properties
        public long Minerals
        {
            get { return _minerals; }
            set { _minerals = value; }
        }
        public List<Drone> DroneClients
        {
            get { return _droneClients; }
        }
        #endregion


        public MineralRock(FactionsGame engine, string name)
            : base(engine, name)
        {
            _engine = engine;
            _minerals = _mineralsMax;
        }

        protected override void LoadContent()
        {
            base.LoadContent();
        }


        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            base.Update(gameTime);

            Tile t = _engine.TileExistenceCheckByGridRef(this.GridReference);
            if (t != null)
                t.Solid = true;


            // Perform breakup of rock
            if (_minerals <= 0)
            {
                SpawnSubRocks();

                _engine.ActorQuadTree.Remove(this);
                _engine.SceneGraph.RemoveNode(this);
                _engine.Actors.Remove(this);
                _engine.CurrentLevel.Actors.Remove(this);
                if (t != null)
                    t.Solid = false;
                this.Dispose();
            }
        }



        void SpawnSubRocks()
        {
            Random rand = new Random();

            MineralRock rock = null;
            int subRocks = 0;
            int rockShatterDistance = 50;
            switch (this.Name)
            {
                case "GemRockHuge":
                    subRocks = 3;
                    break;
                case "GemRockLarge":
                    subRocks = 4;
                    break;
                default:
                    break;
            }

            
                // Gimme rocks
            for (int i = 0; i < subRocks; i++)
            {
                switch (this.Name)
                {
                    case "GemRockHuge":
                        rock = (GemRockLarge)_engine.GetTemplateActorByName("GemRockLarge").Clone();
                        rockShatterDistance = 100;
                        break;
                    case "GemRockLarge":
                        rock = (GemRockSmall)_engine.GetTemplateActorByName("GemRockSmall").Clone();
                        rockShatterDistance = 50;
                        break;
                    default:
                        break;
                }

                if (rock != null)
                {
                    rock.SpriteIndex = rand.Next(rock.Sprites.Count - 1);
                    rock.Position = new Vector2(this.Position.X + (rand.Next(rockShatterDistance) - (rockShatterDistance / 2)), this.Position.Y + (rand.Next(rockShatterDistance) - (rockShatterDistance / 2)));
                    rock.Initialize();
                    _engine.ActorQuadTree.Insert(rock);
                    _engine.ActorsSceneNode.Children.Add(rock);
                    _engine.Actors.Add(rock);
                    _engine.CurrentLevel.Actors.Add(rock);
                    rock = null;
                }
            }
        }

    }
}
