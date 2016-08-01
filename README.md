# Deferred Rendering in monogame Source Code

　XNA4.0向けに書かれたDeferred Renderingのサンプルコードをmonogameで動くように改造しました。  
　monogameでは、サンプルで使っている下記ExternalReferenceのコードがうまく機能しないため、オリジナルのままでは動きません。  

`            EffectMaterialContent deferredShadingMaterial = new EffectMaterialContent();`  
`            deferredShadingMaterial.Effect = new ExternalReference<EffectContent>("RenderGBuffer.fx");`

　対処法として、GBufferEffectを新たに作成し、アセンブラに組み込んだRenderGBuffer.fx(RenderGBuffer.dx11.mgfxo)を内部で動かしています。これが正しい対処法であるのかは分かりません。  
　monogame自体を修正できれば良いのですが、ソース読んだ限り簡単に修正できそうではなかったです。  
　また、LoadContentメソッドが呼び出されるタイミングが、XNAとmonogameでは異なるため、QuadRenderComponentのコンストラクタを暫定的に修正しています。  
　vertsとibが2回もnewされるので、実装を真似る際は注意してください。  

---
XNA4.0(Original)  
http://roy-t.nl/2010/12/28/deferred-rendering-in-xna4-0-source-code.html  
XNA2.0  
http://www.catalinzima.com/xna/tutorials/deferred-rendering-in-xna/
