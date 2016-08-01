using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace DeferredPipeline
{
	public class GBufferEffectMaterialContent : MaterialContent
	{
		public const string TextureKey = "Texture";
		public const string NormalMapKey = "NormalMap";
		public const string SpecularMapKey = "SpecularMap";

		public ExternalReference<TextureContent> Texture
		{
			get { return GetTexture(TextureKey); }
			set { SetTexture(TextureKey, value); }
		}

		public ExternalReference<TextureContent> NormalMap
		{
			get { return GetTexture(NormalMapKey); }
			set { SetTexture(NormalMapKey, value); }
		}

		public ExternalReference<TextureContent> SpecularMap
		{
			get { return GetTexture(SpecularMapKey); }
			set { SetTexture(SpecularMapKey, value); }
		}
	}
}
