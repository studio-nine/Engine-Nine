namespace Nine.Physics
{
    using System;
    using System.Collections.Generic;
    using BEPUphysics.Collidables.MobileCollidables;
    using BEPUphysics.CollisionShapes;
    using BEPUphysics.Entities;
    using BEPUphysics.Entities.Prefabs;
    using BEPUphysics.MathExtensions;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;

    class PhysicsComponentReader : ContentTypeReader<PhysicsComponent>
    {
        protected override PhysicsComponent Read(ContentReader input, PhysicsComponent existingInstance)
        {
            var entity = input.ReadObject<Entity>();
            var physicsComponent = new PhysicsComponent(entity);
            if (entity.Position.LengthSquared() > 1E-8F ||
               (entity.Orientation - Quaternion.Identity).LengthSquared() > 1E-8F)
            {
                var transformBias = entity.WorldTransform;
                Matrix.Invert(ref transformBias, out transformBias);
                physicsComponent.TransformBias = transformBias;
            }
            return physicsComponent;
        }
    }

    abstract class ShapeReader<T> : ContentTypeReader
    {
        protected ShapeReader() : base(typeof(T)) { }

        protected override sealed object Read(ContentReader input, object existingInstance)
        {
            var physicsType = input.ReadString();
            var staticFriction = input.ReadSingle();
            var dynamicFriction = input.ReadSingle();
            var restitution = input.ReadSingle();
            var mass = input.ReadSingle();
            var position = input.ReadVector3();
            var orientation = input.ReadQuaternion();

            var entity = ReadEntity(input);

            entity.Position = position;
            entity.Orientation = orientation;

            entity.Material.StaticFriction = staticFriction;
            entity.Material.KineticFriction = dynamicFriction;
            entity.Material.Bounciness = restitution;

            if (physicsType == "Dynamic")
                entity.BecomeDynamic(mass);
            else if (physicsType == "Kinematic")
                entity.BecomeKinematic();
            else
                throw new NotSupportedException("Physics type not supported: " + physicsType);

            return entity;
        }

        protected abstract Entity ReadEntity(ContentReader input);
    }

    class CompoundShapeReader : ContentTypeReader<CompoundBody>
    {
        protected override CompoundBody Read(ContentReader input, CompoundBody existingInstance)
        {
            int count = input.ReadInt32();
            if (count <= 0)
                throw new ContentLoadException("Compound body must contain at least 1 children");
            
            float mass = 0;
            bool isDynamic = false;
            var children = new List<CompoundChildData>(count);
            
            for (int i = 0; i < count; i++)
            {
                var entity = input.ReadObject<Entity>();
                if (i == 0)
                    isDynamic = entity.IsDynamic;
                else if (isDynamic != entity.IsDynamic)
                    throw new ContentLoadException("The children of the compound body must be all dynamic or kinematic");
                
                mass += entity.Mass;
                var child = new CompoundChildData();
                child.Entry = new CompoundShapeEntry(entity.CollisionInformation.Shape,
                              new RigidTransform(entity.Position, entity.Orientation));
                child.Material = entity.Material;
                children.Add(child);
            }
            if (isDynamic)
                return new CompoundBody(children, mass);
            return new CompoundBody(children);
        }
    }

    class BoxShapeReader : ShapeReader<Box>
    {
        protected override Entity ReadEntity(ContentReader input)
        {
            var size = input.ReadVector3();
            return new Box(Vector3.Zero, size.X, size.Y, size.Z);
        }
    }

    class SphereShapeReader : ShapeReader<Sphere>
    {
        protected override Entity ReadEntity(ContentReader input)
        {
            return new Sphere(Vector3.Zero, input.ReadSingle());
        }
    }

    class CylinderShapeReader : ShapeReader<Cylinder>
    {
        protected override Entity ReadEntity(ContentReader input)
        {
            return new Cylinder(Vector3.Zero, input.ReadSingle(), input.ReadSingle());
        }
    }

    class CapsuleShapeReader : ShapeReader<Capsule>
    {
        protected override Entity ReadEntity(ContentReader input)
        {
            return new Capsule(Vector3.Zero, input.ReadSingle(), input.ReadSingle());
        }
    }

    class ConeShapeReader : ShapeReader<Cone>
    {
        protected override Entity ReadEntity(ContentReader input)
        {
            return new Cone(Vector3.Zero, input.ReadSingle(), input.ReadSingle());
        }
    }    
}
