using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FactionsGame
{
    /// <summary>
    /// Settings for the Factions game.
    /// </summary>
    public class FactionsGameSettings : ApplicationSettingsBase
    {
        [UserScopedSetting()]
        [DefaultSettingValue("Player")]
        public string PlayerName
        {
            get
            {
                return ((string)this["PlayerName"]);
            }
            set
            {
                this["PlayerName"] = (string)value;
            }
        }
        [UserScopedSetting()]
        [DefaultSettingValue("1")]
        public int Team
        {
            get
            {
                return ((int)this["Team"]);
            }
            set
            {
                this["Team"] = (int)value;
            }
        }
        [UserScopedSetting()]
        [DefaultSettingValue("1")]
        public int PlayerColor
        {
            get
            {
                return ((int)this["PlayerColor"]);
            }
            set
            {
                this["PlayerColor"] = (int)value;
            }
        }
        [UserScopedSetting()]
        [DefaultSettingValue("1280")]
        public int ScreenWidth
        {
            get
            {
                return ((int)this["ScreenWidth"]);
            }
            set
            {
                this["ScreenWidth"] = (int)value;
            }
        }
        [UserScopedSetting()]
        [DefaultSettingValue("960")]
        public int ScreenHeight
        {
            get
            {
                return ((int)this["ScreenHeight"]);
            }
            set
            {
                this["ScreenHeight"] = (int)value;
            }
        }
        [UserScopedSetting()]
        [DefaultSettingValue("False")]
        public bool Fullscreen
        {
            get
            {
                return ((bool)this["Fullscreen"]);
            }
            set
            {
                this["Fullscreen"] = (bool)value;
            }
        }
    }
}
