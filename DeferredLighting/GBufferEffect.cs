using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredLighting
{
	public class GBufferEffect : Effect
	{
		EffectParameter textureParam;
		EffectParameter normalParam;
		EffectParameter specularParam;

		public Texture2D Texture
		{
			get { return textureParam.GetValueTexture2D(); }
			set { textureParam.SetValue(value); }
		}

		public Texture2D Normal
		{
			get { return normalParam.GetValueTexture2D(); }
			set { normalParam.SetValue(value); }
		}

		public Texture2D Specular
		{
			get { return specularParam.GetValueTexture2D(); }
			set { specularParam.SetValue(value); }
		}

		static readonly byte[] Bytecode = LoadEffectResource("DeferredLighting.Resources.RenderGBuffer.dx11.mgfxo");

		static byte[] LoadEffectResource(string name)
		{
			var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
			using (var ms = new MemoryStream())
			{
				stream.CopyTo(ms);
				return ms.ToArray();
			}
		}

		/// <summary>
		/// Creates a new BasicEffect with default parameter settings.
		/// </summary>
		public GBufferEffect(GraphicsDevice device) : base(device, Bytecode)
		{
			CacheEffectParameters(null);
		}

		void CacheEffectParameters(GBufferEffect cloneSource)
		{
			textureParam = Parameters["Texture"];
			normalParam = Parameters["NormalMap"];
			specularParam = Parameters["SpecularMap"];
		}
	}
}
