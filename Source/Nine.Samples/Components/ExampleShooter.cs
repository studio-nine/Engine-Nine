namespace Nine.Samples
{
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Input;
    using Nine;

    public class ExampleShooter : Component
    {
        private Input input;

        public string BulletTemplate { get; set; }
        public Keys Trigger { get; set; }

        protected override void OnAdded(Group parent)
        {
            input = new Input();
            input.KeyDown += new EventHandler<KeyboardEventArgs>(OnKeyDown);
        }

        private void OnKeyDown(object sender, KeyboardEventArgs e)
        {
            if (e.Key == Trigger)
            {
                var bullet = new Instance(BulletTemplate);
                bullet.Transform = Parent.Transform;
                Scene.Add(bullet);
            }
        }
    }

    public class ExampleBullet : Component
    {
        private float lifetime;

        public float Speed { get; set; }
        public TimeSpan Duration { get; set; }

        protected override void Update(float elapsedTime)
        {
            lifetime += elapsedTime;
            if (lifetime > Duration.TotalSeconds)
            {
                Scene.Remove(Parent);
                return;
            }

            var transform = Parent.Transform;
            transform.Translation += transform.Forward * Speed * elapsedTime;
            Parent.Transform = transform;
        }
    }
}