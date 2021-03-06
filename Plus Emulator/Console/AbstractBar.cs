﻿/*
 * Created by SharpDevelop.
 * User: claudio.santoro
 * Date: 02/10/2014
 * Time: 16:55
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;

namespace Plus
{
    public abstract class AbstractBar
    {
        /// <summary>
        /// Prints a simple message 
        /// </summary>
        /// <param name="msg">Message to print</param>
        public void PrintMessage(string msg)
        {
            Console.Write("  {0}", msg);
            Console.Write("\r".PadLeft(Console.WindowWidth - Console.CursorLeft - 1));
        }

        public abstract void Step();
    }
}