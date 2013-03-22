namespace Nine.Graphics.UI
{
    interface ISealable
    {
        bool CanSeal { get; }
        bool IsSealed { get; }

        void Seal();
    }
}
