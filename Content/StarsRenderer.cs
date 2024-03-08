using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RealisticSky.Common.DataStructures;
using RealisticSky.Core.CrossCompatibility.Inbound;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
using SpecialStar = RealisticSky.Common.DataStructures.Star;

namespace RealisticSky.Content
{
    public class StarsRenderer : ModSystem
    {
        /// <summary>
        /// The set of all stars in the sky.
        /// </summary>
        internal static SpecialStar[] Stars;

        /// <summary>
        /// The vertex buffer that contains all star information.
        /// </summary>
        internal static VertexBuffer StarVertexBuffer;

        /// <summary>
        /// The index buffer that contains all vertex pointers for <see cref="StarVertexBuffer"/>.
        /// </summary>
        internal static IndexBuffer StarIndexBuffer;

        /// <summary>
        /// The basic shader responsible for rendering the contents of the <see cref="StarVertexBuffer"/>.
        /// </summary>
        internal static BasicEffect StarShader;

        /// <summary>
        /// The minimum brightness that a star can be at as a result of twinkling.
        /// </summary>
        public const float MinTwinkleBrightness = 0.2f;

        /// <summary>
        /// The maximum brightness that a star can be at as a result of twinkling.
        /// </summary>
        public const float MaxTwinkleBrightness = 3.37f;

        /// <summary>
        /// The identifier key for the sky's star shader.
        /// </summary>
        public const string StarShaderKey = "RealisticSky:StarShader";

        public override void OnModLoad()
        {
            // Initialize the star shader.
            GameShaders.Misc[StarShaderKey] = new MiscShaderData(new(ModContent.Request<Effect>("RealisticSky/Assets/Effects/StarPrimitiveShader", AssetRequestMode.ImmediateLoad).Value), "AutoloadPass");

            // Generate stars.
            GenerateStars(RealisticSkyConfig.Instance.NightSkyStarCount);
        }

        internal static void GenerateStars(int starCount)
        {
            Stars = new SpecialStar[starCount];
            if (starCount <= 0)
                return;

            for (int i = 0; i < Stars.Length; i++)
            {
                StarProfile profile = new(Main.rand);
                Color color = StarProfile.TemperatureToColor(profile.Temperature);
                color.A = 0;

                float latitude = Main.rand.NextFloat(-MathHelper.PiOver2, MathHelper.PiOver2) * MathF.Sqrt(Main.rand.NextFloat());
                float longitude = Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi);
                float radius = profile.Scale * 2.5f;
                Stars[i] = new(latitude, longitude, color * MathF.Pow(radius / 6f, 1.5f), radius);
            }

            Main.QueueMainThreadAction(RegenerateBuffers);
        }

        internal static void RegenerateBuffers()
        {
            RegenerateVertexBuffer();
            RegenerateIndexBuffer();
        }

        internal static void RegenerateVertexBuffer()
        {
            // Initialize the star buffer if necessary.
            StarVertexBuffer?.Dispose();
            StarVertexBuffer = new VertexBuffer(Main.instance.GraphicsDevice, VertexPositionColorTexture.VertexDeclaration, Stars.Length * 4, BufferUsage.WriteOnly);

            // Generate vertex data.
            VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[Stars.Length * 4];
            for (int i = 0; i < Stars.Length; i++)
            {
                // Acquire vertices for the star.
                Stars[i].GenerateVertices(1.2f, out var topLeft, out var topRight, out var bottomLeft, out var bottomRight);

                int bufferIndex = i * 4;
                vertices[bufferIndex] = topLeft;
                vertices[bufferIndex + 1] = topRight;
                vertices[bufferIndex + 2] = bottomRight;
                vertices[bufferIndex + 3] = bottomLeft;
            }

            // Send the vertices to the buffer.
            StarVertexBuffer.SetData(vertices);
        }

        internal static void RegenerateIndexBuffer()
        {
            // Initialize the star buffer if necessary.
            StarIndexBuffer?.Dispose();
            StarIndexBuffer = new(Main.instance.GraphicsDevice, IndexElementSize.ThirtyTwoBits, Stars.Length * 6, BufferUsage.WriteOnly);

            // Generate index data.
            int[] indices = new int[Stars.Length * 6];
            for (int i = 0; i < Stars.Length; i++)
            {
                int bufferIndex = i * 6;
                int vertexIndex = i * 4;
                indices[bufferIndex] = vertexIndex;
                indices[bufferIndex + 1] = vertexIndex + 1;
                indices[bufferIndex + 2] = vertexIndex + 2;
                indices[bufferIndex + 3] = vertexIndex + 2;
                indices[bufferIndex + 4] = vertexIndex + 3;
                indices[bufferIndex + 5] = vertexIndex;
            }

            StarIndexBuffer.SetData(indices);
        }

        internal static Matrix CalculatePerspectiveMatrix()
        {
            float height = Main.instance.GraphicsDevice.Viewport.Height / (float)Main.instance.GraphicsDevice.Viewport.Width;
            Matrix rotation = Matrix.CreateRotationZ(DaysCounterSystem.DayCounter * -2.3f);
            Matrix projection = Matrix.CreateOrthographicOffCenter(-1f, 1f, height, -height, -1f, 0f);
            Matrix screenStretch = Matrix.CreateScale(1.1f, 1.1f, 1f);
            return rotation * projection * screenStretch;
        }

        public static void Render(float opacity, Matrix backgroundMatrix)
        {
            // Make vanilla's stars disappear. They are not needed.
            // This only applies if the player is on the surface so that the shimmer stars are not interfered with.
            // TODO -- Consider making custom stars for the Aether?
            SkyPlayerSnapshot player = SkyPlayerSnapshot.TakeSnapshot();
            for (int i = 0; i < Main.maxStars; i++)
            {
                if (Main.star[i] is null)
                    continue;

                Main.star[i].hidden = player.Center.Y <= player.WorldSurface * 16f && !CalamityModCompatibility.InAstralBiome(Main.LocalPlayer);
            }

            // Calculate the star opacity. If it's zero, don't waste resources rendering anything.
            float skyBrightness = (Main.ColorOfTheSkies.R + Main.ColorOfTheSkies.G + Main.ColorOfTheSkies.B) / 765f;
            float starOpacity = MathHelper.Clamp(MathF.Pow(1f - Main.atmo, 3f) + MathF.Pow(1f - skyBrightness, 5f), 0f, 1f) * opacity;
            if (starOpacity <= 0f)
                return;

            // Since this can render on the mod screen it's important that the shader be checked for if it's disposed or not.
            if (!GameShaders.Misc.TryGetValue(StarShaderKey, out MiscShaderData s))
                return;
            Effect starShader = s.Shader;
            if (starShader.IsDisposed)
                return;

            // Don't waste resources rendering anything if there are no stars to draw.
            if (RealisticSkyConfig.Instance.NightSkyStarCount <= 0)
                return;

            // Prepare the star shader.
            Vector2 screenSize = Vector2.Transform(new Vector2(Main.instance.GraphicsDevice.Viewport.Width, Main.instance.GraphicsDevice.Viewport.Height), backgroundMatrix);
            starShader.Parameters["opacity"]?.SetValue(starOpacity);
            starShader.Parameters["projection"]?.SetValue(CalculatePerspectiveMatrix());
            starShader.Parameters["globalTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 5f);
            starShader.Parameters["sunPosition"]?.SetValue(Main.dayTime ? SunPositionSaver.SunPosition : Vector2.One * 50000f);
            starShader.Parameters["minTwinkleBrightness"]?.SetValue(MinTwinkleBrightness);
            starShader.Parameters["maxTwinkleBrightness"]?.SetValue(MaxTwinkleBrightness);
            starShader.Parameters["distanceFadeoff"]?.SetValue(Main.eclipse ? 0.11f : 1f);
            starShader.Parameters["screenSize"]?.SetValue(screenSize);
            starShader.CurrentTechnique.Passes[0].Apply();

            // Request the atmosphere target.
            AtmosphereRenderer.AtmosphereTarget.Request();

            Main.instance.GraphicsDevice.Textures[1] = ModContent.Request<Texture2D>("RealisticSky/Assets/ExtraTextures/BloomCircle").Value;
            Main.instance.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
            Main.instance.GraphicsDevice.Textures[2] = !AtmosphereRenderer.AtmosphereTarget.IsReady ? TextureAssets.MagicPixel.Value : AtmosphereRenderer.AtmosphereTarget.GetTarget();
            Main.instance.GraphicsDevice.SamplerStates[2] = SamplerState.LinearClamp;
            Main.instance.GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            // Render the stars.
            Main.instance.GraphicsDevice.Indices = StarIndexBuffer;
            Main.instance.GraphicsDevice.SetVertexBuffer(StarVertexBuffer);
            Main.instance.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, StarVertexBuffer.VertexCount, 0, StarIndexBuffer.IndexCount / 3);
            Main.instance.GraphicsDevice.SetVertexBuffer(null);
            Main.instance.GraphicsDevice.Indices = null;
        }
    }
}
