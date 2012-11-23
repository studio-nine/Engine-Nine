namespace Test
{
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using Nine;
    using Nine.Components;
    using Nine.Graphics;
    using Nine.Graphics.Materials;
    using Nine.Graphics.Primitives;
    using Nine.Animations;

    public class SkinnedModelTest : ITestGame
    {
        public Scene CreateTestScene(GraphicsDevice graphics, ContentManager content)
        {
            var scene = new Scene();

            scene.Add(new ModelViewerCamera(graphics));
            scene.Add(new AmbientLight(graphics) { AmbientLightColor = new Vector3(0.5f, 0.5f, 0.5f) });
            scene.Add(new Nine.Graphics.DirectionalLight(graphics) { Direction = new Vector3(-1, -1, -1) });

            var model = content.Load<Microsoft.Xna.Framework.Graphics.Model>("Models/Peon");
            scene.Add(new Group(new Nine.Graphics.Model(model) { Material = new SkinnedMaterial(graphics), Transform = Matrix.CreateTranslation(-5, 0, 0) }, new AttackAndRun()));
            scene.Add(new Group(new Nine.Graphics.Model(model) { Material = new SkinnedMaterial(graphics), Transform = Matrix.CreateTranslation(5, 0, 0) }, new IdleRunCarryBlended()));
            
            return scene;
        }
    }

    class AttackAndRun : Component
    {
        protected override void OnAdded(Group parent)
        {
            var model = parent.Find<Nine.Graphics.Model>();
            var attack = new BoneAnimationController(model.Source.GetAnimation("Attack"));
            var run = new BoneAnimationController(model.Source.GetAnimation("Run"));
            run.Speed = 0.8f;

            var blended = new BoneAnimation(model.Skeleton);
            blended.Controllers.Add(run);
            blended.Controllers.Add(attack);

            blended.Controllers[run].Disable("Bip01_Pelvis", false);
            blended.Controllers[run].Disable("Bip01_Spine1", true);

            blended.Controllers[attack].Disable("Bip01", false);
            blended.Controllers[attack].Disable("Bip01_Spine", false);
            blended.Controllers[attack].Disable("Bip01_L_Thigh", true);
            blended.Controllers[attack].Disable("Bip01_R_Thigh", true);

            blended.KeyController = run;
            blended.IsSychronized = true;

            model.Animations.Play(blended);
        }
    }

    class IdleRunCarryBlended : Component
    {
        LookAtController lookAtController;
        ICamera camera;

        protected override void OnAdded(Group parent)
        {
            var scene = parent.FindRoot<Scene>();
            camera = scene.GetDrawingContext().Camera;

            var model = parent.Find<Nine.Graphics.Model>();            

            // Blend between 3 animation channels
            var idle = new BoneAnimationController(model.Source.GetAnimation("Idle"));
            var carry = new BoneAnimationController(model.Source.GetAnimation("Carry"));
            var run = new BoneAnimationController(model.Source.GetAnimation("Run"));

            var blended = new BoneAnimation(model.Skeleton);
            blended.Controllers.Add(idle);
            blended.Controllers.Add(run);
            blended.Controllers.Add(carry);

            lookAtController = new LookAtController(model.Skeleton, model.AbsoluteTransform, model.Skeleton.GetBone("Bip01_Head"));
            lookAtController.Up = Vector3.UnitX;
            lookAtController.Forward = -Vector3.UnitZ;
            lookAtController.HorizontalRotation = new Range<float>(-MathHelper.PiOver2, MathHelper.PiOver2);
            lookAtController.VerticalRotation = new Range<float>(-MathHelper.PiOver2, MathHelper.PiOver2);
            blended.Controllers.Add(lookAtController);

            // Give the look at controller a huge blend weight so it will dominate the other controllers.
            // All the weights will be normalized during blending.
            blended.Controllers[lookAtController].BlendWeight = 10;

            blended.KeyController = run;
            blended.IsSychronized = true;

            model.Animations.Play(blended);
        }

        protected override void Update(float elapsedTime)
        {
            Matrix view, projection;
            if (camera.TryGetViewFrustum(out view, out projection))
                lookAtController.Target = Matrix.Invert(view).Translation;
        }
    }
}
