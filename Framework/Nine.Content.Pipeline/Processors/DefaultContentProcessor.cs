#region Copyright 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
using System.Diagnostics;
#endregion

namespace Nine.Content.Pipeline.Processors
{
    /// <summary>
    /// Processes each property of the input object graph using the processor specified by 
    /// DefaultContentProcessorAttribute or the method marked with SelfProcessAttribute.
    /// </summary>
    [ContentProcessor(DisplayName = "Content - Engine Nine")]
    public class DefaultContentProcessor : ContentProcessor<object, object>
    {
        /// <summary>
        /// Gets or sets a value indicating whether a debug intermediate xml file will be created.
        /// </summary>
        [DefaultValue(false)]
        public bool Debug { get; set; }

        /// <summary>
        /// A stack holding the current processor context.
        /// </summary>
        private Stack<ContentProcessorContext> contextStack = new Stack<ContentProcessorContext>();

        /// <summary>
        /// Processes the specified input data and returns the result.
        /// </summary>
        public override object Process(object input, ContentProcessorContext context)
        {
            try
            {
                contextStack.Push(context);

                object output = Process(input);
                if (output == input)
                    ObjectGraph.TraverseProperties(input, Process);
                else
                    input = output;
            }
            finally
            {
                contextStack.Pop();
            }

            SerializeOutput(input, context);
            return input;
        }
        
        private object Process(object input)
        {
            IContentProcessor processor = FindDefaultProcessor(input);
            if (processor != null)
            {
                var context = contextStack.Peek();
                if (processor.InputType.IsAssignableFrom(input.GetType()))
                {
                    context.Logger.LogImportantMessage("Processing {0} using {1}", input.GetType().Name, processor.GetType().Name);
                    return processor.Process(input, context);
                }
                else
                {
                    context.Logger.LogWarning(null, null, "Processor type mismatch {0} -> {1} -> {2} -> {4}", input.GetType().Name, processor.InputType.Name, processor.OutputType.Name);
                }
            }
            return input;
        }
        
        private void SerializeOutput(object input, ContentProcessorContext context)
        {
            if (Debug)
            {
                var debugOutput = Path.Combine(context.IntermediateDirectory, "DefaultContent-" + Guid.NewGuid().ToString("B").ToUpperInvariant()) + ".xml";
                using (var writer = XmlWriter.Create(debugOutput))
                {
                    IntermediateSerializer.Serialize(writer, input, null);
                }
            }
        }

        #region FindDefaultProcessor
        /// <summary>
        /// Finds the default processor for the given input object
        /// </summary>
        private static IContentProcessor FindDefaultProcessor(object input)
        {
            if (input == null)
                return null;

            var inputType = input.GetType();

            // Create processor from DefaultContentProcessorAttribute on the content type
            var defaultProcessorAttribute = inputType.GetCustomAttributes(typeof(DefaultContentProcessorAttribute), false).OfType<DefaultContentProcessorAttribute>().FirstOrDefault();
            if (defaultProcessorAttribute != null)
                return (IContentProcessor)Activator.CreateInstance(Type.GetType(defaultProcessorAttribute.DefaultProcessor));

            // Create processor from SelfProcessAttribute
            var selfProcessMethod = inputType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                                             .Where(m => m.GetCustomAttributes(typeof(SelfProcessAttribute), false).Any())
                                             .FirstOrDefault();

            if (selfProcessMethod != null)
                return new SelfProcessor(input, selfProcessMethod);

            return DefaultProcessors.FirstOrDefault(p => p.InputType.IsAssignableFrom(inputType));
        }

        static List<IContentProcessor> DefaultProcessors;

        static DefaultContentProcessor()
        {
            DefaultProcessors = new List<IContentProcessor>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.FullName.StartsWith("System") ||
                    assembly.FullName.StartsWith("mscorlib") ||
                   (assembly.FullName.StartsWith("Microsoft.") && !assembly.FullName.StartsWith("Microsoft.Xna.Framework.Content.Pipeline")))
                {
                    continue;
                }

                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (type.GetInterface(typeof(IContentProcessor).Name) != null &&
                            type.GetCustomAttributes(false).OfType<DefaultContentProcessorAttribute>().Any())
                        {
                            try
                            {
                                DefaultProcessors.Add((IContentProcessor)Activator.CreateInstance(type));
                            }
                            catch (Exception e)
                            {
                                Trace.Write("Failed loading content processor ");
                                Trace.WriteLine(type.FullName);
                                Trace.WriteLine(e);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Trace.Write("Failed loading assembly ");
                    Trace.WriteLine(assembly.FullName);
                    Trace.WriteLine(e);
                }
            }
        }

        class SelfProcessor : ContentProcessor<object, object>
        {
            private object input;
            private MethodInfo processMethod;

            public SelfProcessor(object input, MethodInfo processMethod)
            {
                this.input = input;
                this.processMethod = processMethod;
            }

            public override object Process(object input, ContentProcessorContext context)
            {
                return processMethod.Invoke(input, new object[] { input, context });
            }
        }
        #endregion
    }
}
