using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;

namespace RealisticSky.Content.Clouds
{
    public class CloudsTargetContent : ARenderTargetContentByRequest
    {
        protected override void HandleUseReqest(GraphicsDevice device, SpriteBatch spriteBatch)
        {
            // Initialize the underlying render target if necessary.
            Vector2 size = new Vector2(device.Viewport.Width, device.Viewport.Height) * 0.5f;
            PrepareARenderTarget_WithoutListeningToEvents(ref _target, Main.instance.GraphicsDevice, (int)size.X, (int)size.Y, RenderTargetUsage.PreserveContents);

            device.SetRenderTarget(_target);
            device.Clear(Color.Transparent);

            // Draw the host's contents to the render target.
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer, null, Matrix.Identity);
            CloudsRenderer.RenderToTarget();
            Main.spriteBatch.End();

            device.SetRenderTarget(null);

            // Mark preparations as completed.
            _wasPrepared = true;
        }
    }
}
