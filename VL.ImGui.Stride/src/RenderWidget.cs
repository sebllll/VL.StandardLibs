﻿using ImGuiNET;
using Stride.Core.Mathematics;
using Stride.Input;
using Stride.Rendering;
using VL.Core;
using Viewport = Stride.Graphics.Viewport;

namespace VL.ImGui.Widgets
{
    using ImGui = ImGuiNET.ImGui;

    [GenerateNode(Category = "ImGui.Widgets.Internal", IsStylable = false)]
    public sealed partial class RenderWidget : Widget, IDisposable
    {
        private RenderLayerWithViewPort? renderLayer;
        private IContextWithRenderer? strideContext;

        public NodeContext? nodeContext { private get; set; }

        public IGraphicsRendererBase? Layer { private get; set ; }

        public RenderView? RenderView { private get; set; } = new RenderView();

        public Vector2 Size { private get; set; } = new Vector2(1f, 1f);

        public IInputSource? InputSource => renderLayer?.MappedInputSource; 

        internal override void UpdateCore(Context context)
        {
            if (context is StrideContext strideContext)
            {
                this.strideContext = strideContext;

                if (renderLayer == null)
                    renderLayer = new RenderLayerWithViewPort();

                renderLayer.HasFocus = false;

                if (Layer is null)
                {
                    this.strideContext.RemoveRenderer(renderLayer);
                    return;
                }
               
                if (ImGui.BeginChild("##RenderWidget__" + nodeContext?.Path.Stack.Peek().ToString() , Size.FromHectoToImGui(), ImGuiChildFlags.None, ImGuiWindowFlags.ChildWindow))
                {
                    if (renderLayer.RenderView != RenderView)
                        renderLayer.RenderView = RenderView;

                    if (renderLayer.Layer != Layer)
                        renderLayer.Layer = Layer;

                    var id = this.strideContext.AddRenderer(renderLayer);

                    var pos = ImGui.GetWindowPos();
                    var size = ImGui.GetWindowSize();

                    ImGui.InvisibleButton($"{GetHashCode()}", size, ImGuiButtonFlags.None);

                    renderLayer.HasFocus = ImGui.IsWindowFocused();
                    renderLayer.Viewport = new Viewport(pos.X, pos.Y, size.X, size.Y);

                    var drawList = ImGui.GetWindowDrawList();
                    drawList.AddCallback(id, IntPtr.Zero);
                }
                ImGui.EndChild();
            }
        }

        public void Dispose()
        {
            if (renderLayer != null) 
            {
                this.strideContext?.RemoveRenderer(renderLayer);
            }
        }
    }
}
