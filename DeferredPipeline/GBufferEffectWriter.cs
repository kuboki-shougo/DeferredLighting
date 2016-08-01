using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace DeferredPipeline
{
	[ContentTypeWriter]
	class GBufferEffectWriter : ContentTypeWriter<GBufferEffectMaterialContent>
	{
		protected override void Write(ContentWriter output, GBufferEffectMaterialContent value)
		{
			output.WriteExternalReference(value.Textures.ContainsKey(GBufferEffectMaterialContent.TextureKey) ? value.Texture : null);
			output.WriteExternalReference(value.Textures.ContainsKey(GBufferEffectMaterialContent.NormalMapKey) ? value.NormalMap : null);
			output.WriteExternalReference(value.Textures.ContainsKey(GBufferEffectMaterialContent.SpecularMapKey) ? value.SpecularMap : null);
		}
		
		public override string GetRuntimeReader(TargetPlatform targetPlatform)
		{
			return "DeferredLighting.GBufferEffectReader,DeferredLighting,Version=1.0.0.0,Culture=neutral";
		}
	}
}
