using AlkaronEngine.Graphics3D;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Veldrid;

namespace AlkaronEngine.Gui
{
    public class GuiCore
    {
        private ImGuiRenderer guiRenderer;

        public GuiCore()
        {
            
        }

        public void Initialize(int setWidth, int setHeight)
        {
            guiRenderer = new ImGuiRenderer(AlkaronCoreGame.Core.GraphicsDevice, AlkaronCoreGame.Core.GraphicsDevice.MainSwapchain.Framebuffer.OutputDescription,
                setWidth, setHeight);
        }

        public void Resize(int newWidth, int newHeight)
        {

        }

        public void Update(double deltaTime, InputSnapshot inputSnapshot)
        {
            guiRenderer.Update((float)deltaTime, inputSnapshot);

            //ImGui.SetNextWindowSize();// new Vector2(100, 100));
            ImGui.Begin("Test", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoMove);

            ImGui.Text("Hello World!");
            if (ImGui.IsItemFocused())
            {
                ImGui.Text("Blubb");
            }

            if (ImGui.Button("Test", new Vector2(100, 20)))
            {
                ImGui.Text("Bla");
            }

            ImGui.End();
        }

        public void RenderOnScreen(RenderContext renderContext)
        {
            guiRenderer.Render(renderContext.GraphicsDevice, renderContext.CommandList);
        }
    }
}
