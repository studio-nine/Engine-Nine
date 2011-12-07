#region Copyright 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
#endregion

namespace Nine.Content.Pipeline.Processors
{
    /// <summary>
    /// Processes each property of the input object graph using the processor specified by DefaultContentProcessorAttribute.
    /// </summary>
    [ContentProcessor(DisplayName = "Content - Engine Nine")]
    public class DefaultContentProcessor : ContentProcessor<object, object>
    {
        /// <summary>
        /// Gets or sets a value indicating whether a debug intermediate xml file will be created.
        /// </summary>
        [DefaultValue(false)]
        public bool Debug { get; set; }

        ContentProcessorContext context;

        static List<IContentProcessor> DefaultProcessors;

        public override object Process(object input, ContentProcessorContext context)
        {
            if (input == null)
                return null;

            if (DefaultProcessors == null)
            {
                DefaultProcessors = AppDomain.CurrentDomain.GetAssemblies().SelectMany(asm => asm.GetTypes().Where(
                    t => typeof(IContentProcessor).IsAssignableFrom(t) && t.GetCustomAttributes(typeof(DefaultContentProcessorAttribute), false).Count() > 0))
                    .Select(t => Activator.CreateInstance(t)).OfType<IContentProcessor>().ToList();
            }

            this.context = context;
            input = Process(input.GetType(), input);
            ObjectGraph.ForEachProperty(input, Process);

            if (Debug)
            {
                var debugOutput = Path.Combine(context.IntermediateDirectory, "DefaultContent-" + Guid.NewGuid().ToString("B").ToUpperInvariant()) + ".xml";
                var settings = new XmlWriterSettings { Indent = true, NewLineChars = Environment.NewLine };
                using (var writer = XmlWriter.Create(debugOutput, settings))
                {
                    IntermediateSerializer.Serialize(writer, input, ".");
                }
            }
            return input;
        }
        
        private object Process(Type type, object input)
        {
            if (input == null)
                return null;

            IContentProcessor processor = null;
            var defaultProcessorAttribute = input.GetType().GetCustomAttributes(typeof(DefaultContentProcessorAttribute), false)
                                                           .OfType<DefaultContentProcessorAttribute>().FirstOrDefault();

            if (defaultProcessorAttribute != null)
                processor = (IContentProcessor)Activator.CreateInstance(Type.GetType(defaultProcessorAttribute.DefaultProcessor));

            if (processor == null)
            {
                var selfProcessMethod = input.GetType().GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                                                       .Where(m => m.GetCustomAttributes(typeof(SelfProcessAttribute), false).Any())
                                                       .FirstOrDefault();
                if (selfProcessMethod != null)
                    processor = new SelfProcessor(input, selfProcessMethod);
            }

            if (processor == null)
                processor = DefaultProcessors.FirstOrDefault(p => p.InputType.IsAssignableFrom(input.GetType()));
            if (processor == null)
                return input;

            if (processor.InputType.IsAssignableFrom(input.GetType())/* && type.IsAssignableFrom(processor.OutputType)*/)
            {
                context.Logger.LogImportantMessage("Processing {0} using {1}", input.GetType().Name, processor.GetType().Name);
                return processor.Process(input, context);
            }
            else
            {
                context.Logger.LogWarning(null, null, "Processor type mismatch {0} -> {1} -> {2} -> {4}", input.GetType().Name, processor.InputType.Name, processor.OutputType.Name, type.Name);
            }
            return input;
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
    }
}
