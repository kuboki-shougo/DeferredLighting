using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace DeferredLighting
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class DeferredRenderer : DrawableGameComponent
    {
        private Camera camera;
        private QuadRenderComponent quadRenderer;
        private Scene scene;

        private RenderTarget2D colorRT; //color and specular intensity
        private RenderTarget2D normalRT; //normals + specular power
        private RenderTarget2D depthRT; //depth
        private RenderTarget2D lightRT; //lighting

        private Effect clearBufferEffect;
        private Effect directionalLightEffect;

        private Effect pointLightEffect;
        private Model sphereModel; //point ligt volume

        private Effect finalCombineEffect;

        private SpriteBatch spriteBatch;

        private Vector2 halfPixel;

        public DeferredRenderer(Game game)
            : base(game)
        {
            scene = new Scene(Game);
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here

            base.Initialize();
            camera = new Camera(Game);
            camera.CameraArc = -30;
            camera.CameraDistance = 50;
            quadRenderer = new QuadRenderComponent(Game);
            Game.Components.Add(camera);
            Game.Components.Add(quadRenderer);
        }

        protected override void LoadContent()
        {
            halfPixel = new Vector2()
            {
                X = 0.5f / (float)GraphicsDevice.PresentationParameters.BackBufferWidth,
                Y = 0.5f / (float)GraphicsDevice.PresentationParameters.BackBufferHeight
            };

            int backbufferWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
            int backbufferHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;

            colorRT = new RenderTarget2D(GraphicsDevice, backbufferWidth, backbufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth24);
            normalRT = new RenderTarget2D(GraphicsDevice, backbufferWidth, backbufferHeight, false, SurfaceFormat.Color, DepthFormat.None); 
            depthRT = new RenderTarget2D(GraphicsDevice, backbufferWidth, backbufferHeight, false, SurfaceFormat.Single, DepthFormat.None);
            lightRT = new RenderTarget2D(GraphicsDevice, backbufferWidth, backbufferHeight, false, SurfaceFormat.Color, DepthFormat.None);

            scene.InitializeScene();         

            clearBufferEffect = Game.Content.Load<Effect>("ClearGBuffer");
            directionalLightEffect = Game.Content.Load<Effect>("DirectionalLight");
            finalCombineEffect = Game.Content.Load<Effect>("CombineFinal");
            pointLightEffect = Game.Content.Load<Effect>("PointLight");
            sphereModel = Game.Content.Load<Model>(@"Models\sphere");
           
            spriteBatch = new SpriteBatch(GraphicsDevice);
            base.LoadContent();
        }

        private void SetGBuffer()
        {
            GraphicsDevice.SetRenderTargets(colorRT, normalRT, depthRT);
        }

        private void ResolveGBuffer()
        {
            GraphicsDevice.SetRenderTargets(null);            
        }

        private void ClearGBuffer()
        {            
            clearBufferEffect.Techniques[0].Passes[0].Apply();
            quadRenderer.Render(Vector2.One * -1, Vector2.One);            
        }

        private void DrawDirectionalLight(Vector3 lightDirection, Color color)
        {
            directionalLightEffect.Parameters["colorMap"].SetValue(colorRT);
            directionalLightEffect.Parameters["normalMap"].SetValue(normalRT);
            directionalLightEffect.Parameters["depthMap"].SetValue(depthRT);

            directionalLightEffect.Parameters["lightDirection"].SetValue(lightDirection);
            directionalLightEffect.Parameters["Color"].SetValue(color.ToVector3());

            directionalLightEffect.Parameters["cameraPosition"].SetValue(camera.Position);
            directionalLightEffect.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(camera.View * camera.Projection));

            directionalLightEffect.Parameters["halfPixel"].SetValue(halfPixel);

            directionalLightEffect.Techniques[0].Passes[0].Apply();
            quadRenderer.Render(Vector2.One * -1, Vector2.One);            
        }
      
        private void DrawPointLight(Vector3 lightPosition, Color color, float lightRadius, float lightIntensity)
        {            
            //set the G-Buffer parameters
            pointLightEffect.Parameters["colorMap"].SetValue(colorRT);
            pointLightEffect.Parameters["normalMap"].SetValue(normalRT);
            pointLightEffect.Parameters["depthMap"].SetValue(depthRT);

            //compute the light world matrix
            //scale according to light radius, and translate it to light position
            Matrix sphereWorldMatrix = Matrix.CreateScale(lightRadius) * Matrix.CreateTranslation(lightPosition);
            pointLightEffect.Parameters["World"].SetValue(sphereWorldMatrix);
            pointLightEffect.Parameters["View"].SetValue(camera.View);
            pointLightEffect.Parameters["Projection"].SetValue(camera.Projection);
            //light position
            pointLightEffect.Parameters["lightPosition"].SetValue(lightPosition);

            //set the color, radius and Intensity
            pointLightEffect.Parameters["Color"].SetValue(color.ToVector3());
            pointLightEffect.Parameters["lightRadius"].SetValue(lightRadius);
            pointLightEffect.Parameters["lightIntensity"].SetValue(lightIntensity);

            //parameters for specular computations
            pointLightEffect.Parameters["cameraPosition"].SetValue(camera.Position);
            pointLightEffect.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(camera.View * camera.Projection));
            //size of a halfpixel, for texture coordinates alignment
            pointLightEffect.Parameters["halfPixel"].SetValue(halfPixel);
            //calculate the distance between the camera and light center
            float cameraToCenter = Vector3.Distance(camera.Position, lightPosition);
            //if we are inside the light volume, draw the sphere's inside face
            if (cameraToCenter < lightRadius)
                GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;                
            else
                GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            GraphicsDevice.DepthStencilState = DepthStencilState.None;

            pointLightEffect.Techniques[0].Passes[0].Apply();
            foreach (ModelMesh mesh in sphereModel.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    GraphicsDevice.Indices = meshPart.IndexBuffer;
                    GraphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);
                    
                    GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, meshPart.StartIndex, meshPart.PrimitiveCount);
                }
            }            
            
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        }
        
        public override void Draw(GameTime gameTime)
        {            
            SetGBuffer();            
            ClearGBuffer(); 
            scene.DrawScene(camera, gameTime);
            ResolveGBuffer();
            DrawLights(gameTime);
            
            base.Draw(gameTime);
        }

        private void DrawLights(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(lightRT);
            GraphicsDevice.Clear(Color.Transparent);
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = DepthStencilState.None;

            Color[] colors = new Color[10];
            colors[0] = Color.Red; colors[1] = Color.Blue;
            colors[2] = Color.IndianRed; colors[3] = Color.CornflowerBlue;
            colors[4] = Color.Gold; colors[5] = Color.Green;
            colors[6] = Color.Crimson; colors[7] = Color.SkyBlue;
            colors[8] = Color.Red; colors[9] = Color.ForestGreen;
            float angle = (float)gameTime.TotalGameTime.TotalSeconds;
            int n = 15;

			for (int i = 0; i < n; i++)
			{
				Vector3 pos = new Vector3((float)Math.Sin(i * MathHelper.TwoPi / n + angle), 0.30f, (float)Math.Cos(i * MathHelper.TwoPi / n + angle));
				DrawPointLight(pos * 40, colors[i % 10], 15, 2);
				pos = new Vector3((float)Math.Cos((i + 5) * MathHelper.TwoPi / n - angle), 0.30f, (float)Math.Sin((i + 5) * MathHelper.TwoPi / n - angle));
				DrawPointLight(pos * 20, colors[i % 10], 20, 1);
				pos = new Vector3((float)Math.Cos(i * MathHelper.TwoPi / n + angle), 0.10f, (float)Math.Sin(i * MathHelper.TwoPi / n + angle));
				DrawPointLight(pos * 75, colors[i % 10], 45, 2);
				pos = new Vector3((float)Math.Cos(i * MathHelper.TwoPi / n + angle), -0.3f, (float)Math.Sin(i * MathHelper.TwoPi / n + angle));
				DrawPointLight(pos * 20, colors[i % 10], 20, 2);
			}

			DrawPointLight(new Vector3(0, (float)Math.Sin(angle * 0.8) * 40, 0), Color.Red, 30, 5);
			DrawPointLight(new Vector3(0, 25, 0), Color.White, 30, 1);
			DrawPointLight(new Vector3(0, 0, 70), Color.Wheat, 55 + 10 * (float)Math.Sin(5 * angle), 3);

			GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.None;            
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            GraphicsDevice.SetRenderTarget(null);
            
            //Combine everything
            finalCombineEffect.Parameters["colorMap"].SetValue(colorRT);
            finalCombineEffect.Parameters["lightMap"].SetValue(lightRT);
            finalCombineEffect.Parameters["halfPixel"].SetValue(halfPixel);

            finalCombineEffect.Techniques[0].Passes[0].Apply();            
            quadRenderer.Render(Vector2.One * -1, Vector2.One);

            //Output FPS and 'credits'
            double fps = (1000 / gameTime.ElapsedGameTime.TotalMilliseconds);
            fps = Math.Round(fps, 0);
            Game.Window.Title = "Deferred Rendering by Catalin Zima, converted to XNA4 by Roy Triesscheijn. Drawing " + (n * 4 + 3) + " lights at " + fps.ToString()  + " FPS";            
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            
            base.Update(gameTime);
        }
    }
}
