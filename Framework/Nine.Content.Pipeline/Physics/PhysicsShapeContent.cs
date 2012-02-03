#region Copyright 2012 (c) Engine Nine
//=============================================================================
//
//  Copyright 2012 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System.Collections.Generic;
using System.Windows.Markup;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

#endregion

namespace Nine.Content.Pipeline.Physics
{
    public abstract class ShapeContent
    {
        public PhysicsType PhysicsType { get; set; }
        public float StaticFriction { get; set; }
        public float DynamicFriction { get; set; }
        public float Restitution { get; set; }
        public float Mass { get; set; }

        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public RotationOrder RotationOrder { get; set; }

        protected ShapeContent()
        {
            Mass = 1;
            StaticFriction = 0.8f;
            DynamicFriction = 0.6f;
            Restitution = 0;
            PhysicsType = PhysicsType.Dynamic;
        }

        internal void Write(ContentWriter writer)
        {
            var rotation = MatrixHelper.CreateRotation(new Vector3(MathHelper.ToRadians(Rotation.X)
                                                                 , MathHelper.ToRadians(Rotation.Y)
                                                                 , MathHelper.ToRadians(Rotation.Z))
                                                                 , RotationOrder);
            writer.Write(PhysicsType.ToString());
            writer.Write(StaticFriction);
            writer.Write(DynamicFriction);
            writer.Write(Restitution);
            writer.Write(Mass);
            writer.Write(Position);
            writer.Write(Quaternion.CreateFromRotationMatrix(rotation));
        }
    }
    
    [ContentProperty("Shapes")]
    public class CompoundShapeContent
    {
        public List<ShapeContent> Shapes { get; private set; }

        public CompoundShapeContent()
        {
            Shapes = new List<ShapeContent>();
        }

        public CompoundShapeContent(IEnumerable<ShapeContent> shapes)
        {
            Shapes = new List<ShapeContent>(shapes);
        }
    }

    [ContentTypeWriter]
    class CompoundShapeContentWriter : ContentTypeWriter<CompoundShapeContent>
    {
        protected override void Write(ContentWriter output, CompoundShapeContent value)
        {
            output.Write(value.Shapes.Count);
            for (int i = 0; i < value.Shapes.Count; i++)
                output.WriteObject(value.Shapes[i]);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "Nine.Physics.CompoundShapeReader, Nine.Physics, Version=1.2.0.0, Culture=neutral, PublicKeyToken=ed8336b5652212a9";
        }
    }

    public class BoxShapeContent : ShapeContent
    {
        public Vector3 Size { get; set; }
    }

    [ContentTypeWriter]
    class BoxShapeContentWriter : ContentTypeWriter<BoxShapeContent>
    {
        protected override void Write(ContentWriter output, BoxShapeContent value)
        {
            value.Write(output);
            output.Write(value.Size);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "Nine.Physics.BoxShapeReader, Nine.Physics, Version=1.2.0.0, Culture=neutral, PublicKeyToken=ed8336b5652212a9";
        }
    }

    public class SphereShapeContent : ShapeContent
    {
        public float Radius { get; set; }
    }

    [ContentTypeWriter]
    class SphereShapeContentWriter : ContentTypeWriter<SphereShapeContent>
    {
        protected override void Write(ContentWriter output, SphereShapeContent value)
        {
            value.Write(output);
            output.Write(value.Radius);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "Nine.Physics.SphereShapeReader, Nine.Physics, Version=1.2.0.0, Culture=neutral, PublicKeyToken=ed8336b5652212a9";
        }
    }

    public class CylinderShapeContent : ShapeContent
    {
        public float Height { get; set; }
        public float Radius { get; set; }
    }

    [ContentTypeWriter]
    class CylinderShapeContentWriter : ContentTypeWriter<CylinderShapeContent>
    {
        protected override void Write(ContentWriter output, CylinderShapeContent value)
        {
            value.Write(output);
            output.Write(value.Height);
            output.Write(value.Radius);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "Nine.Physics.CylinderShapeReader, Nine.Physics, Version=1.2.0.0, Culture=neutral, PublicKeyToken=ed8336b5652212a9";
        }
    }

    public class CapsuleShapeContent : ShapeContent
    {
        public float Height { get; set; }
        public float Radius { get; set; }
    }

    [ContentTypeWriter]
    class CapsuleShapeContentWriter : ContentTypeWriter<CapsuleShapeContent>
    {
        protected override void Write(ContentWriter output, CapsuleShapeContent value)
        {
            value.Write(output);
            output.Write(value.Height);
            output.Write(value.Radius);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "Nine.Physics.CapsuleShapeReader, Nine.Physics, Version=1.2.0.0, Culture=neutral, PublicKeyToken=ed8336b5652212a9";
        }
    }

    public class ConeShapeContent : ShapeContent
    {
        public float Height { get; set; }
        public float Radius { get; set; }
    }

    [ContentTypeWriter]
    class ConeShapeContentWriter : ContentTypeWriter<ConeShapeContent>
    {
        protected override void Write(ContentWriter output, ConeShapeContent value)
        {
            value.Write(output);
            output.Write(value.Height);
            output.Write(value.Radius);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "Nine.Physics.ConeShapeReader, Nine.Physics, Version=1.2.0.0, Culture=neutral, PublicKeyToken=ed8336b5652212a9";
        }
    }
}
