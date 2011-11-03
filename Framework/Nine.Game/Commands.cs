#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

#endregion

namespace Nine
{
    /// <summary>
    /// Defines a game command that can be executed in order.
    /// </summary>
    public interface IGameCommand
    {
        /// <summary>
        /// Executes this command with the specified context.
        /// </summary>
        /// <returns>
        /// Whether this command has finished executing.
        /// </returns>
        bool Execute(IGameObjectContainer context);
    }

    /// <summary>
    /// Defines an executor that executes a queue of commands in order.
    /// </summary>
    public class GameCommandExecutor
    {
        Queue<IGameCommand> commands = new Queue<IGameCommand>();

        /// <summary>
        /// Occurs when a new command is added to the game command queue.
        /// </summary>
        public event Action<object, IGameCommand> NewCommand;

        /// <summary>
        /// Executes the specified command.
        /// </summary>
        public void Enqueue(IGameCommand command)
        {
            if (command == null)
                throw new ArgumentNullException("command");            
            commands.Enqueue(command);
            if (NewCommand != null)
                NewCommand(this, command);
        }

        internal void Update(IGameObjectContainer context, TimeSpan elapsedTime)
        {
            while (commands.Count > 0)
            {
                var currentCommand = commands.Peek();
                if (currentCommand == null || currentCommand.Execute(context))
                {
                    commands.Dequeue();
                }
            }
        }
    }
}
