using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;

namespace RealisticSky.Content
{
    public class AtmosphereTargetContent : ARenderTargetContentByRequest
    {
        protected override void HandleUseReqest(GraphicsDevice device, SpriteBatch spriteBatch)
        {
            // Initialize the underlying render target if necessary.
            Vector2 size = new(device.Viewport.Width, device.Viewport.Height);
            PrepareARenderTarget_WithoutListeningToEvents(ref _target, Main.instance.GraphicsDevice, (int)size.X, (int)size.Y, RenderTargetUsage.PreserveContents);

            device.SetRenderTarget(_target);
            device.Clear(Color.Transparent);

            // Draw the host's contents to the render target.
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Matrix.Identity);
            AtmosphereRenderer.RenderToTarget();
            Main.spriteBatch.End();

            device.SetRenderTarget(null);

            // Mark preparations as completed.
            _wasPrepared = true;
        }
    }
}
