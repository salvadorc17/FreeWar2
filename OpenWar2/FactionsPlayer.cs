using System;
using System.Collections.Generic;
using System.Text;
using DEngine;

namespace FactionsGame
{
    public delegate void FactionsPlayerLongChangedHandler(long value);

    public class FactionsPlayer : Player
    {
        FactionsGame _engine;
        long _resources;

        public event FactionsPlayerLongChangedHandler OnResourcesChanged;

        #region Public Properties
        public long Resources
        {
            get { return _resources; }
            set 
            { 
                _resources = value;
                if (OnResourcesChanged != null)
                    OnResourcesChanged(value);
            }
        }
        #endregion

        public FactionsPlayer(FactionsGame engine)
            : base(engine)
        {
            _engine = engine;
        }
    }
}
