using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Platformer.Screens
{
    class MainMenuScreen : MenuScreen
    {
        public MainMenuScreen()
            : base("Platformer")
        {
            MenuEntry topic0MenuEntry = new MenuEntry("Level1");
            MenuEntry topic1MenuEntry = new MenuEntry("Level2");
            MenuEntry topic2MenuEntry = new MenuEntry("Level3");
            MenuEntry topic3MenuEntry = new MenuEntry("Level4");
            MenuEntry topic4MenuEntry = new MenuEntry("Level5");
            MenuEntry exitMenuEntry = new MenuEntry("Exit");

            topic0MenuEntry.Selected += TopicMenuEntrySelected;
            topic1MenuEntry.Selected += TopicMenuEntrySelected;
            topic2MenuEntry.Selected += TopicMenuEntrySelected;
            topic3MenuEntry.Selected += TopicMenuEntrySelected;
            topic4MenuEntry.Selected += TopicMenuEntrySelected;
            exitMenuEntry.Selected += ExitMenuEntrySelected;

            MenuEntries.Add(topic0MenuEntry);
            MenuEntries.Add(topic1MenuEntry);
            MenuEntries.Add(topic2MenuEntry);
            MenuEntries.Add(topic3MenuEntry);
            MenuEntries.Add(topic4MenuEntry);
            MenuEntries.Add(exitMenuEntry);
        }

        void ExitMenuEntrySelected(object sender, SelectedIndexEventArgs e)
        {
            this.OnCancel();
        }

        void TopicMenuEntrySelected(object sender, SelectedIndexEventArgs e)
        {
            ScreenManager.AddScreen(new GamePlayScreen(e.Index));
            //LoadingScreen.Load(ScreenManager, false, new GamePlayScreen(e.Index));
        }

        protected override void OnCancel()
        {
            const string message = "Exit";
            MessageBoxScreen confirmExitMessageBox = new MessageBoxScreen(message);
            confirmExitMessageBox.Accepted += ConfirmExitMessageBoxAccepted;
            ScreenManager.AddScreen(confirmExitMessageBox);
        }

        void ConfirmExitMessageBoxAccepted(object sender, EventArgs e)
        {
            ScreenManager.Game.Exit();
        }
    }
}
