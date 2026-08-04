/*
 * Project: DEngine
 * File: Log.cs
 * Author: David Wilson
 * Date: 30/12/2008
 * 
 * Summary:
 * 
 * Static error logging class.
 * Logs info to a text file.
 * 
 * License information:

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.

 * Credits:
 * 
 * Scene Graph by mdx4ever (with modifications)
 * http://www.ziggyware.com/readarticle.php?article_id=130&rowstart=0
 * 
 * Physics simulation by Farseer Physics:
 * http://www.codeplex.com/FarseerPhysics
 */


using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace DEngine
{
    // Delegate functions (for communicating with other classes)
    public delegate void LogMessageHandler(string msg);

    public static class Log
    {
        static string filename = "DEngine.log";  // Filename only
        static string path = "";               // File path
        static bool relativePath = true;       // If relative to current directory

        static string fullPath = null;

        // Log message event!
        public static event LogMessageHandler LogMessage;

        static StreamWriter writer;
        static StreamReader reader;

        static Log()
        {
            // Trim leading and trailing slashes off path.
            /*
            if (path.Length > 0)
            {
                // Trim off the leading
                if (relativePath == true && path[0] == Convert.ToChar(@"\"))
                    path = path.Substring(1);

                // Add a trailing
                if (path[path.Length - 1] != Convert.ToChar(@"\"))
                    path = path.Remove(path.Length - 1);
            }

            if (relativePath)
                fullPath = Directory.GetCurrentDirectory() + path + filename;
            else
                fullPath = path + filename;
            */
        }

        // Dump the message to the log!
        public static void Message(string msg)
        {
            //writer = new StreamWriter(fullPath,true);
            try
            {
                //writer.WriteLine(msg);
                //writer.Close();

                if ( LogMessage != null)
                    LogMessage(msg);
                // Send out the event to any listeners!
            }
            catch 
            { 
            }
        }

        public static void PurgeLog()
        {
            /*
            if (File.Exists(fullPath))
            {
                try
                {
                    File.Delete(fullPath);
                }
                catch (Exception ex)
                {
                    Message("Could not purge log: " + ex.Message);
                }
            }*/
        }

        public static string GetLog()
        {
            string dataFileContents = string.Empty;
            /*if (File.Exists(fullPath))
            {
                try
                {
                    reader = new StreamReader(fullPath);
                    dataFileContents = reader.ReadToEnd();
                }
                finally
                {
                    reader.Dispose();
                }
            }*/
            return dataFileContents;
        }


    }
}
