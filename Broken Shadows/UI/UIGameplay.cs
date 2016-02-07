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

        public override void Update(float fDeltaTime)
        {
            base.Update(fDeltaTime);
        }

        public override void Draw(float fDeltaTime, SpriteBatch DrawBatch)
        {
            base.Draw(fDeltaTime, DrawBatch);
        }

        public override void KeyboardInput(SortedList<eBindings, BindInfo> binds)
        {
            GameState g = GameState.Get();
            if (binds.ContainsKey(eBindings.UI_Exit))
            {
                g.ShowPauseMenu();
                binds.Remove(eBindings.UI_Exit);
            }

            base.KeyboardInput(binds);
        }
    }
}
