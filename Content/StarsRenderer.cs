using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
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
        /// The identifier key for the sky's star shader.
        /// </summary>
        public const string StarShaderKey = "RealisticSky:StarShader";

        public override void OnModLoad()
        {
            // Initialize the star shader.
            GameShaders.Misc[StarShaderKey] = new MiscShaderData(new(ModContent.Request<Effect>("RealisticSky/Assets/Effects/StarPrimitiveShader", AssetRequestMode.ImmediateLoad).Value), "AutoloadPass");

            // Generate stars.
            GenerateStars(16000);
        }

        internal static void GenerateStars(int starCount)
        {
            Stars = new SpecialStar[starCount];
            for (int i = 0; i < Stars.Length; i++)
            {
                // Calculate the position of the star on the screen. The Y position is biased a bit towards the top by making the random interpolant generally favor being closer to 0.
                float xPositionRatio = Main.rand.NextFloat(-0.05f, 1.05f);
                float yPositionRatio = MathHelper.Lerp(-0.05f, 1f, MathF.Pow(Main.rand.NextFloat(), 1.5f));

                // Calculate the star color.
                Color color = Color.Lerp(Color.Wheat, Color.LightGoldenrodYellow, Main.rand.NextFloat());
                if (Main.rand.NextBool(10))
                    color = Color.Lerp(color, Color.Cyan, Main.rand.NextFloat(0.67f));
                color.A = 0;

                // Calculate the star's radius. These are harshly biased towards being tiny.
                float radius = MathHelper.Lerp(2f, 4.3f, MathF.Pow(Main.rand.NextFloat(), 9f));
                if (Main.rand.NextBool(30))
                    radius *= 1.3f;
                if (Main.rand.NextBool(50))
                    radius *= 1.3f;
                if (Main.rand.NextBool(50))
                    radius *= 1.45f;

                Stars[i] = new(xPositionRatio, yPositionRatio, color * MathF.Pow(radius / 6f, 1.5f), radius, Main.rand.NextFloat(MathHelper.TwoPi));
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
                Stars[i].GenerateVertices(1f, out var topLeft, out var topRight, out var bottomLeft, out var bottomRight);

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
            StarIndexBuffer = new(Main.instance.GraphicsDevice, IndexElementSize.SixteenBits, Stars.Length * 6, BufferUsage.WriteOnly);

            // Generate index data.
            short[] indices = new short[Stars.Length * 6];
            for (int i = 0; i < Stars.Length; i++)
            {
                int bufferIndex = i * 6;
                short vertexIndex = (short)(i * 4);
                indices[bufferIndex] = vertexIndex;
                indices[bufferIndex + 1] = (short)(vertexIndex + 1);
                indices[bufferIndex + 2] = (short)(vertexIndex + 2);
                indices[bufferIndex + 3] = (short)(vertexIndex + 2);
                indices[bufferIndex + 4] = (short)(vertexIndex + 3);
                indices[bufferIndex + 5] = vertexIndex;
            }
            StarIndexBuffer.SetData(indices);
        }

        public static void Render(float opacity)
        {
            // Make vanilla's stars disappear. They are not needed.
            for (int i = 0; i < Main.numStars; i++)
                Main.star[i].hidden = true;

            // Draw custom stars.
            float skyBrightness = (Main.ColorOfTheSkies.R + Main.ColorOfTheSkies.G + Main.ColorOfTheSkies.B) / 765f;
            float starOpacity = MathHelper.Clamp(MathF.Pow(1f - Main.atmo, 3f) + MathF.Pow(1f - skyBrightness, 5f), 0f, 1f) * opacity;
            if (starOpacity <= 0f)
                return;

            // Calculate the star matrix.
            Matrix projection = Matrix.CreateOrthographicOffCenter(0f, 1f, 1f, 0f, -100f, 100f);

            // Prepare the star shader.
            Effect starShader = GameShaders.Misc[StarShaderKey].Shader;
            starShader.Parameters["opacity"]?.SetValue(starOpacity);
            starShader.Parameters["projection"]?.SetValue(projection);
            starShader.Parameters["globalTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 8f);
            starShader.Parameters["sunPosition"]?.SetValue(SunPositionSaver.SunPosition);
            starShader.CurrentTechnique.Passes[0].Apply();

            Main.instance.GraphicsDevice.Textures[1] = ModContent.Request<Texture2D>("RealisticSky/Assets/ExtraTextures/BloomCircle").Value;
            Main.instance.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
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
