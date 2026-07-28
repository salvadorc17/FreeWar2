/*
 * Project: DEngine
 * File: GoldenRatio.cs
 * Author: David Wilson
 * Date: 30/12/2008
 * 
 * Summary:
 * 
 * Simple, static class to obtain the golden ratio.
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

namespace DEngine
{
    public static class GoldenRatio
    {
        public static float LongFromShort(float value)
        {
            value *= (float)((1 + Math.Sqrt(5)) / 2);
            return value;
        }

        public static float ShortFromLong(float value)
        {
            value /= (float)((1 + Math.Sqrt(5)) / 2);
            return value;
        }
    }
}
