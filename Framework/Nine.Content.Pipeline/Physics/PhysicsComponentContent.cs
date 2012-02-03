#region Copyright 2012 (c) Engine Nine
//=============================================================================
//
//  Copyright 2012 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Markup;
using System.Xaml;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

#endregion

namespace Nine.Content.Pipeline.Physics
{
    /// <summary>
    /// Defines the type of physics entity to be created.
    /// </summary>
    public enum PhysicsType
    {
        /// <summary>
        /// No physics entity will be created.
        /// </summary>
        None,

        /// <summary>
        /// A static physics entity will be created.
        /// </summary>
        Static,
        
        /// <summary>
        /// A dynamic physics entity will be created.
        /// </summary>
        Dynamic,

        /// <summary>
        /// A kinematic physics entity will be created.
        /// </summary>
        Kinematic,
    }

    /// <summary>
    /// Content model for PhysicsComponent.
    /// </summary>
    [ContentProperty("Shapes")]
    public class PhysicsComponentContent
    {
        public List<object> Shapes { get; set; }

        public PhysicsComponentContent()
        {
            Shapes = new List<object>();
        }

        #region Attached Properties
        static readonly AttachableMemberIdentifier PhysicsTypeProperty = new AttachableMemberIdentifier(typeof(PhysicsComponentContent), "PhysicsType");
        static readonly AttachableMemberIdentifier CollisionMeshProperty = new AttachableMemberIdentifier(typeof(PhysicsComponentContent), "CollisionMesh");
        static readonly AttachableMemberIdentifier StaticFrictionProperty = new AttachableMemberIdentifier(typeof(PhysicsComponentContent), "StaticFriction");
        static readonly AttachableMemberIdentifier DynamicFrictionProperty = new AttachableMemberIdentifier(typeof(PhysicsComponentContent), "DynamicFriction");
        static readonly AttachableMemberIdentifier RestitutionProperty = new AttachableMemberIdentifier(typeof(PhysicsComponentContent), "Restitution");
        static readonly AttachableMemberIdentifier MassProperty = new AttachableMemberIdentifier(typeof(PhysicsComponentContent), "Mass");

        /// <summary>
        /// Gets or sets the physics type of the physics entity.
        /// </summary>
        public static PhysicsType GetPhysicsType(object target)
        {
            string type = "None";
            AttachablePropertyServices.TryGetProperty(target, PhysicsTypeProperty, out type);
            return (PhysicsType)Enum.Parse(typeof(PhysicsType), type);
        }

        /// <summary>
        /// Gets or sets the physics type of the physics entity.
        /// </summary>
        public static void SetPhysicsType(object target, PhysicsType value)
        {
            AttachablePropertyServices.SetProperty(target, PhysicsTypeProperty, value.ToString());
        }

        /// <summary>
        /// Gets or sets the name of the collision mesh of the physics entity.
        /// </summary>
        public static string GetCollisionMesh(object target)
        {
            string type = "Collision";
            AttachablePropertyServices.TryGetProperty(target, CollisionMeshProperty, out type);
            return type;
        }

        /// <summary>
        /// Gets or sets the name of the collision mesh of the physics entity.
        /// </summary>
        public static void SetCollisionMesh(object target, string value)
        {
            AttachablePropertyServices.SetProperty(target, CollisionMeshProperty, value);
        }

        /// <summary>
        /// Gets or sets the static friction coefficient of the physics entity.
        /// </summary>
        public static float GetStaticFriction(object target)
        {
            float value = 0.8f;
            AttachablePropertyServices.TryGetProperty(target, StaticFrictionProperty, out value);
            return value;
        }

        /// <summary>
        /// Gets or sets the static friction coefficient of the physics entity.
        /// </summary>
        public static void SetStaticFriction(object target, float value)
        {
            AttachablePropertyServices.SetProperty(target, StaticFrictionProperty, value);
        }

        /// <summary>
        /// Gets or sets the dynamic friction coefficient of the physics entity.
        /// </summary>
        public static float GetDynamicFriction(object target)
        {
            float value = 0.6f;
            AttachablePropertyServices.TryGetProperty(target, DynamicFrictionProperty, out value);
            return value;
        }

        /// <summary>
        /// Gets or sets the dynamic friction coefficient of the physics entity.
        /// </summary>
        public static void SetDynamicFriction(object target, float value)
        {
            AttachablePropertyServices.SetProperty(target, DynamicFrictionProperty, value);
        }

        /// <summary>
        /// Gets or sets the restitution coefficient of the physics entity.
        /// </summary>
        public static float GetRestitution(object target)
        {
            float value = 0;
            AttachablePropertyServices.TryGetProperty(target, RestitutionProperty, out value);
            return value;
        }

        /// <summary>
        /// Gets or sets the restitution coefficient of the physics entity.
        /// </summary>
        public static void SetRestitution(object target, float value)
        {
            AttachablePropertyServices.SetProperty(target, RestitutionProperty, value);
        }

        /// <summary>
        /// Gets or sets the mass of the physics entity.
        /// </summary>
        public static float GetMass(object target)
        {
            float value = 1;
            AttachablePropertyServices.TryGetProperty(target, MassProperty, out value);
            return value;
        }

        /// <summary>
        /// Gets or sets the mass of the physics entity.
        /// </summary>
        public static void SetMass(object target, float value)
        {
            AttachablePropertyServices.SetProperty(target, MassProperty, value);
        }
        #endregion
    }

    [ContentTypeWriter]
    class PhysicsComponentContentWriter : ContentTypeWriter<PhysicsComponentContent>
    {
        protected override void Write(ContentWriter output, PhysicsComponentContent value)
        {
            if (value.Shapes.Count > 1)
                output.WriteObject(new CompoundShapeContent(value.Shapes.OfType<ShapeContent>()));
            else
                output.WriteObject(value.Shapes[0]);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "Nine.Physics.PhysicsComponentReader, Nine.Physics, Version=1.2.0.0, Culture=neutral, PublicKeyToken=ed8336b5652212a9";
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return "Nine.Physics.PhysicsComponent, Nine.Physics, Version=1.2.0.0, Culture=neutral, PublicKeyToken=ed8336b5652212a9";
        }
    }
}