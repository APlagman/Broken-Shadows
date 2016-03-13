using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Broken_Shadows.UI
{
    public class UIOverWorld : UIScreen
    {
        public UIOverWorld(ContentManager Content) :
            base(Content)
        {
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
        }

        public override void Draw(float deltaTime, SpriteBatch DrawBatch)
        {
            base.Draw(deltaTime, DrawBatch);
        }

        public override void KeyboardInput(SortedList<Binding, BindInfo> binds)
        {
            StateHandler g = StateHandler.Get();
            if (binds.ContainsKey(Binding.UI_Exit))
            {
                g.ShowPauseMenu();
                binds.Remove(Binding.UI_Exit);
            }

            base.KeyboardInput(binds);
        }
    }
}
