using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.ResourceManagement
{
    /// <summary>
    /// Reference to a javascript file.
    /// </summary>
    [ResourceConfigurationCollectionName("scriptModules")]
    public class ScriptModuleResource : LinkResourceBase, IPreloadResource, IDeferableResource
    {
        /// <summary> Location of a fallback script for the case that the browser does not support ES6 modules. May be null, if the fallback is not needed. There is no way to put a CDN fallback nor integrity hash, so it should simply point to a local resource </summary>
        public IResourceLocation NomoduleLocation { get; }
        /// <summary> If `defer` attribute should be used. </summary>
        public bool Defer { get; }

        public ScriptModuleResource(IResourceLocation location, IResourceLocation nomoduleLocation = null, bool defer = true)
            : base(defer ? ResourceRenderPosition.Head : ResourceRenderPosition.Body, "text/javascript", location)
        {
            this.NomoduleLocation = nomoduleLocation;
            this.Defer = defer;
        }

        public override void RenderLink(IResourceLocation location, IHtmlWriter writer, IDotvvmRequestContext context, string resourceName)
        {
            AddSrcAndIntegrity(writer, context, location.GetUrl(context, resourceName), "src");
            writer.AddAttribute("type", "module");
            if (Defer)
                writer.AddAttribute("defer", null);
            writer.RenderBeginTag("script");
            writer.RenderEndTag();

            if (NomoduleLocation is object)
            {
                writer.AddAttribute("nomodule", null);
                writer.AddAttribute("src", location.GetUrl(context, resourceName));
                if (Defer)
                    writer.AddAttribute("defer", null);
                writer.RenderBeginTag("script");
                writer.RenderEndTag();
            }
        }

        public void RenderPreloadLink(IHtmlWriter writer, IDotvvmRequestContext context, string resourceName)
        {
            writer.AddAttribute("rel", "preload");
            writer.AddAttribute("href", Location.GetUrl(context, resourceName));
            writer.AddAttribute("as", "script");

            writer.RenderBeginTag("link");
            writer.RenderEndTag();
        }
    }
}
