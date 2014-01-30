using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Diagnostics;
using System.Threading;
using Platformer.Libz;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Platformer.Screens
{
    class QuestionBoxScreen : MenuScreen
    {
        Texture2D overlay;

        public DataTable Table { get; private set; }

        public int RowIndex { get; private set; }

        public string Question { get; private set; }

        public int NrLinesInQuestion { get; private set; }

        public QuestionBoxScreen()
            : base("")
        {
            ExitScreen();
        }

        public QuestionBoxScreen(DataTable dt, int rowIndex)
            : base("")
        {
            Table = dt;
            RowIndex = rowIndex;

            List<int> answerNumbers = new List<int>(){2, 3, 4};
            List<int> cellIndices = new List<int>();
            Random rand = new Random(325456431);

            while(answerNumbers.Count > 0)
            {
                int index = rand.Next(0, answerNumbers.Count);
                cellIndices.Add(answerNumbers[index]);
                answerNumbers.RemoveAt(index);
            }
            cellIndices.Shuffle();

            MenuEntry answer1MenuEntry = new MenuEntry(dt.Rows[rowIndex].ItemArray[cellIndices[0]].ToString());
            MenuEntry answer2MenuEntry = new MenuEntry(dt.Rows[rowIndex].ItemArray[cellIndices[1]].ToString());
            MenuEntry answer3MenuEntry = new MenuEntry(dt.Rows[rowIndex].ItemArray[cellIndices[2]].ToString());

            answer1MenuEntry.Selected += AnswerSelected;
            answer2MenuEntry.Selected += AnswerSelected;
            answer3MenuEntry.Selected += AnswerSelected;

            MenuEntries.Add(answer1MenuEntry);
            MenuEntries.Add(answer2MenuEntry);
            MenuEntries.Add(answer3MenuEntry);

            Question = dt.Rows[rowIndex].ItemArray[1].ToString();

            IsPopUp = true;
            TransitionOnTime = TimeSpan.FromSeconds(0.2);
            TransitionOffTime = TimeSpan.FromSeconds(0.2);

            NrLinesInQuestion = 1;
        }

        void AnswerSelected(object sender, SelectedIndexEventArgs e)
        {
            MenuEntry menuEntry = (MenuEntry)sender;
            if (menuEntry.Text == Table.Rows[RowIndex].ItemArray[2].ToString())
            {
                MessageBoxScreen passMsgBoxScreen = new MessageBoxScreen("Congratz!", false);
                Level.Score += 100;
                passMsgBoxScreen.Accepted += ConfirmMessageBox;
                ScreenManager.AddScreen(passMsgBoxScreen);
            }
            else
            {
                MessageBoxScreen failMsgBoxScreen = new MessageBoxScreen("Sorry, the correct answer was: \n" + Table.Rows[RowIndex].ItemArray[2].ToString(), false);
                failMsgBoxScreen.Accepted += ConfirmMessageBox;
                ScreenManager.AddScreen(failMsgBoxScreen);
            }
            ExitScreen();
        }

        void ConfirmMessageBox(object sender, EventArgs e) 
        {
            ScreenManager.RemoveScreen((MessageBoxScreen)sender);
        }

        public override void Activate(bool instancePreserved)
        {
            if (!instancePreserved)
                overlay = ScreenManager.Game.Content.Load<Texture2D>("Overlays/blankOverlay");
        }

        public override void Draw(GameTime gameTime)
        {
            //ScreenManager.FadeBackBufferToBlack(TransitionAlpha * 2 / 3);

            //Vector2 viewportSize = new Vector2(ScreenManager.GraphicsDevice.Viewport.Width, ScreenManager.GraphicsDevice.Viewport.Height);
            //Vector2 textSize = ScreenManager.FontQ.MeasureString(Question);

            //string questionToDraw = ParseText(Question, ScreenManager.FontQ);
            //Vector2 textPosition = (viewportSize - ScreenManager.FontQ.MeasureString(questionToDraw)) / 2;
            //textPosition.Y = 130;

            //const int hPad = 32;
            //const int vPad = 16;

            //int sumLines = NrLinesInQuestion;
            //foreach (MenuEntry entry in MenuEntries)
            //{
            //    sumLines += entry.NrLines;
            //}

            //Rectangle backgroundRectangle = new Rectangle(- hPad, (int)textPosition.Y - vPad, ScreenManager.GraphicsDevice.Viewport.Width + hPad * 2
            //    , (int)textSize.Y * (sumLines + 4) + vPad * 2);
            //Color color = Color.White * TransitionAlpha;


            //ScreenManager.SpriteBatch.Begin();
            //ScreenManager.SpriteBatch.Draw(overlay, backgroundRectangle, color);
            //ScreenManager.SpriteBatch.DrawString(ScreenManager.FontQ, questionToDraw, textPosition, color);
            //ScreenManager.SpriteBatch.End();

            //base.Draw(gameTime);
        }

        private string ParseText(string text, SpriteFont font)
        {
            string line = String.Empty;
            string returnString = String.Empty;
            string[] wordArray = text.Split(' ');
            NrLinesInQuestion = 1;

            foreach (String word in wordArray)
            {
                if (font.MeasureString(line + word).Length() > 600)
                {
                    returnString = returnString + line + '\n';
                    NrLinesInQuestion++;
                    line = String.Empty;
                }

                line = line + word + ' ';
            }

            return returnString + line;
        }
    }
}
