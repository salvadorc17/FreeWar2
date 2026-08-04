using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Microsoft.Xna.Framework.Storage;

using FarseerPhysics.Dynamics;
using FarseerPhysics.Collision;
using FarseerPhysics.Factories;

using Path = System.IO.Path;

namespace DEngine
{
    // Relative file addresses for use with XNA content
    public static class FileAccess
    {
        // Directory constants
        const string ACTORS_DIR = "actors";
        const string ACTOR_IMAGES_DIR = "images";
        const string TILES_DIR = "tiles";
        const string TILE_TRANSITIONS_DIR = "transitions";
        const string LEVELS_DIR = "levels";
        const string BACKGROUNDS_DIR = "backgrounds";
        const string SOUNDS_DIR = "sounds";

        static string contentDir = "Content";

        public static string GetBackgroundsDir()
        {
            return Path.Combine(contentDir, BACKGROUNDS_DIR);
        } 

        public static string GetTilesDir()
        {
            return Path.Combine(contentDir,TILES_DIR);
        }

        public static string GetActorsDir()
        {
            return Path.Combine(contentDir, ACTORS_DIR);
        }

        public static string GetLevelsDir()
        {
            return Path.Combine(contentDir, LEVELS_DIR);
        }

        public static string GetSoundsDir()
        {
            return Path.Combine(contentDir, SOUNDS_DIR);
        }

        public static string GetTileTransitionsDir()
        {
            return Path.Combine(GetTilesDir(), TILE_TRANSITIONS_DIR);
        }

        public static string GetActorDir(string actorName)
        {
            string actorsDir = Path.Combine(GetActorsDir(),actorName);
            if (Directory.Exists(actorsDir))
            {
                return actorsDir;
            }
            return null;
        }



        // Actor folder names
        public static string[] GetAllActorNames()
        {
            string[] actors = null;
            string actorsDir = Path.Combine(Environment.CurrentDirectory, GetActorsDir());
            if (Directory.Exists(actorsDir))
            {
                try
                {
                    actors = Directory.GetDirectories(actorsDir);
                    for (int i = 0; i < actors.Length; i++)
                    {
                        actors[i] = actors[i].Replace(actorsDir + @"\", "");
                    }
                }
                catch (Exception e)
                {
                    Log.Message("Could not get actor directories: " + e.Message);
                }
            }

            return actors;
        }


        // Read the Lua file!
        /*public static string GetActorScript(string actorName)
        {
            string actorDir = GetActorDir(actorName);
            string luaFile = Path.Combine(actorDir,actorName + ".lua");
            string script = null;

            if (File.Exists(luaFile))
            {
                StreamReader sr = new StreamReader(luaFile);
                script = sr.ReadToEnd();
            }
            else
            {
                Log.Message("Couldn't find actor script for actor: " + actorName);
                script = ""; // give it an empty script
            }
            return script;
        }*/

        // Get the XML filename
        public static string GetActorXmlFile(string actorName)
        {
            string actorDir = GetActorDir(actorName);
            string xmlFile = Path.Combine(actorDir,actorName + ".xml");

            if (File.Exists(xmlFile))
            {
                return xmlFile;
            }
            return null;
        }


        // Get images dir
        public static string GetActorImagesDir(string actorName)
        {
            string actorDir = GetActorDir(actorName);
            string imagesDir = Path.Combine(actorDir,ACTOR_IMAGES_DIR);

            if (Directory.Exists(imagesDir))
            {
                return imagesDir;
            }
            return null;
        }


        // take a file path like C:\temp\file.txt
        // and return just "file"
        public static string GetFileNameFromPath(string path)
        {
            if (path != null && path.Length > 0)
            {
                //path = path.Remove(path.LastIndexOf('.')); // clip extension
                path = path.Remove(0, path.LastIndexOf(@"\")+1);                
            }
            return path;
        }

    }
}
