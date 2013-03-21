namespace Nine.Graphics
{
#if MDX    
    using System;
    using System.IO;
    using System.Windows.Forms;
    using Microsoft.DirectX;
    using Microsoft.DirectX.Direct3D;

    class ProceduralTextureGenerator
    {
        public string CreateTextureContentFromShader(string effectFilename, string functionName, int width, int height, string outputPath)
        {
            // Adopted from Nvidia DXSas Sample
            string errors;
            ConstantTable constants;

            // We may be here for the first creation of the function texture, or we may be here
            // because the function texture is associated with the viewport size, and it needs to be rebuilt.
            string effectText = File.ReadAllText(effectFilename);

            GraphicsStream functionStream = ShaderLoader.CompileShader(effectText, functionName, null, null, "tx_1_0", 0, out errors, out constants);
            if (functionStream == null)
                throw new InvalidOperationException("Couldn't find texture function: " + functionName);

            TextureShader shader = new TextureShader(functionStream);
            if (shader == null)
                throw new InvalidOperationException("Couldn't create texture shader from function stream for: " + functionName);

            Texture texture = new Texture(Device, (int)width, (int)height, 1, 0, Format.A8R8G8B8, Pool.Managed);
            TextureLoader.FillTexture(texture, shader);

            outputPath = Path.Combine(outputPath, Path.ChangeExtension(Path.GetFileName(effectFilename), functionName + ".dds"));
            TextureLoader.Save(outputPath, ImageFileFormat.Dds, texture);
            return outputPath;
        }

        static Device device;
        static public Device Device
        {
            get { return device ?? (device = CreateDevice()); }
        }

        private static Device CreateDevice()
        {
            Form dummy = new Form();

            PresentParameters parameters = new PresentParameters();
            parameters.Windowed = true;
            parameters.SwapEffect = SwapEffect.Copy;
            parameters.PresentationInterval = PresentInterval.Immediate;
            parameters.AutoDepthStencilFormat = DepthFormat.D24X8;
            parameters.BackBufferCount = 1;
            parameters.EnableAutoDepthStencil = true;
            parameters.BackBufferWidth = 1;
            parameters.BackBufferHeight = 1;
            parameters.ForceNoMultiThreadedFlag = true;
            parameters.DeviceWindowHandle = dummy.Handle;
            parameters.BackBufferFormat = Format.X8R8G8B8;

            return new Device(0, DeviceType.Hardware, dummy.Handle, CreateFlags.HardwareVertexProcessing | CreateFlags.PureDevice, parameters);
        }
    }

#endif
}
