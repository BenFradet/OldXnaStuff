using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Data;
using GameStateManagement;
using Platformer.Libz;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Media;

namespace Platformer.Screens
{
    class GamePlayScreen : GameScreen
    {
        ContentManager content;

        private Texture2D winOverlay;
        private Texture2D loseOverlay;
        private Texture2D dieOverlay;

        private int levelIndex;
        private Level level;
        private bool wasContinuePressed = false;

        Song backgroundSong;

        DataTable questionsDT;
        List<int> questionsIndices;
        List<string> levelNames = new List<string>();
        int questionNumber = 0;

        private static readonly TimeSpan WarningTime = TimeSpan.FromSeconds(30);

        private const int NumberOfLevels = 5;

        public GamePlayScreen(int index)
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.0);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            this.levelIndex = index - 1;
        }

        public override void Activate(bool instancePreserved)
        {
            if (!instancePreserved)
            {
                if (content == null)
                    content = new ContentManager(ScreenManager.Game.Services, "Content");

                winOverlay = content.Load<Texture2D>("Overlays/you_win");
                loseOverlay = content.Load<Texture2D>("Overlays/you_lose");
                dieOverlay = content.Load<Texture2D>("Overlays/you_died");

                //questionsDT = ExcelImport.GetExcelData(@"Content\Questions.xlsx").Tables[0];
                //Dictionary<string, List<int>> questionsDict = ExcelImport.GetSpreadSheetStructure(questionsDT);
                //levelNames = questionsDict.Keys.ToList();
                //levelNames = levelNames.OrderBy((s) => s).ToList();

                levelNames = new List<string>() { "Level1", "Level2", "Level3", "Level4", "Level5" };

                //Random rand = new Random(37879456);

                //for (int i = 0; i < questionsDT.Rows.Count; i++)
                //    questionNumbers.Add(i);

                //for (int i = 0; i < NumberOfQuestions; i++)
                //{
                //    int index = rand.Next(0, questionNumbers.Count);
                //    rowIndices.Add(questionNumbers[index]);
                //    questionNumbers.RemoveAt(index);
                //}

                LoadNextLevel();

                //questionsIndices = questionsDict[level.Name];
                //if (questionsIndices == null || questionsIndices.Count == 0)
                //    throw new NotSupportedException("No questions for this level");
                //questionsIndices.Shuffle();

                ScreenManager.Game.ResetElapsedTime();

                backgroundSong = content.Load<Song>("Sounds/Ballet");
                MediaPlayer.Play(backgroundSong);
                MediaPlayer.IsRepeating = true;
            }
        }

        public override void Deactivate()
        {
            base.Deactivate();
        }

        public override void Unload()
        {
            MediaPlayer.Stop();
            content.Unload();
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);

            //if (level.Player.Question)
            //{
            //    ScreenManager.AddScreen(new QuestionBoxScreen());
            //    questionNumber++;
            //}

            //if (level.Player.Question && questionNumber < NumberOfQuestions)
            //{
            //    ScreenManager.AddScreen(new QuestionBoxScreen(questionsDT, rowIndices[questionNumber]));
            //    questionNumber++;
            //}
        }

        public override void HandleInput(GameTime gameTime, InputState inputState)
        {
            KeyboardState keyboardState = inputState.CurrentKeyboardState;
            KeyboardState lastKeyboardState = inputState.LastKeyboardState;

            level.Update(gameTime, inputState);

            //Debug.WriteLine(level.Player.Position.X.ToString());

            if (keyboardState.IsKeyDown(Keys.Escape) && !lastKeyboardState.IsKeyDown(Keys.Escape))
            {
                MessageBoxScreen quitMsgBoxScreen = new MessageBoxScreen("Sure you want to quit?");
                quitMsgBoxScreen.Accepted += QuitAccepted;
                quitMsgBoxScreen.Cancelled += QuitCancelled;
                ScreenManager.AddScreen(quitMsgBoxScreen);
            }

            bool continuePressed = keyboardState.IsKeyDown(Keys.Space) || keyboardState.IsKeyDown(Keys.W) || keyboardState.IsKeyDown(Keys.Up)
                || inputState.IsLeftMouseButtonPressed();

            if (!wasContinuePressed && continuePressed)
            {
                if (!level.Player.IsAlive)
                    level.StartNewLife();
                else if (level.TimeRemaining == TimeSpan.Zero)
                {
                    if (level.ReachedExit)
                    {
                        ScreenManager.RemoveScreen(this);
                        MessageBoxScreen linkMsgBoxScreen = new MessageBoxScreen("If you wanna learn more about this topic:");
                        linkMsgBoxScreen.Link += LinkToWebPage;
                        ScreenManager.AddScreen(new BackgroundScreen());
                        ScreenManager.AddScreen(new MainMenuScreen());
                        ScreenManager.AddScreen(linkMsgBoxScreen);                        
                    }
                    else
                        ReloadCurrentLevel();
                }
            }
            wasContinuePressed = continuePressed;
        }

        void QuitAccepted(object sender, EventArgs e)
        {
            ScreenManager.Game.Exit();
        }

        void QuitCancelled(object sender, EventArgs e)
        {
            ScreenManager.RemoveScreen((MessageBoxScreen)sender);
        }

        private void LoadNextLevel()
        {
            levelIndex = (levelIndex + 1) % NumberOfLevels;

            if (level != null)
                level.Dispose();

            string levelPath = string.Format("Content/Levels/{0}.txt", levelIndex);
            using (Stream fileStream = TitleContainer.OpenStream(levelPath))
            {
                level = new Level(ScreenManager.Game.Services, fileStream, levelIndex);
                level.Name = levelNames[levelIndex];
            }
        }

        public void ReloadCurrentLevel()
        {
            levelIndex--;
            LoadNextLevel();
        }

        public override void Draw(GameTime gameTime)
        {
            level.Draw(gameTime, ScreenManager.SpriteBatch);
            DrawHUD();
        }

        private void DrawHUD()
        {
            ScreenManager.SpriteBatch.Begin();

            Rectangle titleSafeArea = ScreenManager.GraphicsDevice.Viewport.TitleSafeArea;
            Vector2 hudLocation = new Vector2(titleSafeArea.X, titleSafeArea.Y);
            Vector2 center = new Vector2(titleSafeArea.X + titleSafeArea.Width / 2.0f, titleSafeArea.Y + titleSafeArea.Height / 2.0f);

            string timeString = "TIME: " + level.TimeRemaining.Minutes.ToString("00") + ":" + level.TimeRemaining.Seconds.ToString("00");
            Color timeColor;

            if (level.TimeRemaining > WarningTime || level.ReachedExit || (int)level.TimeRemaining.TotalSeconds % 2 == 0)
                timeColor = Color.Yellow;
            else
                timeColor = Color.Red;
            DrawShadowedString(ScreenManager.FontHUD, timeString, hudLocation, timeColor);

            float timeHeight = ScreenManager.FontHUD.MeasureString(timeString).Y;
            DrawShadowedString(ScreenManager.FontHUD, "SCORE: " + Level.Score.ToString(), hudLocation + new Vector2(0.0f, timeHeight * 1.2f), Color.Yellow);
            
            Texture2D status = null;
            if (level.TimeRemaining == TimeSpan.Zero)
            {
                if (level.ReachedExit)
                {
                    status = winOverlay;
                }
                else
                    status = loseOverlay;
            }
            else if (!level.Player.IsAlive)
                status = dieOverlay;

            if (status != null)
            {
                Vector2 statusSize = new Vector2(status.Width, status.Height);
                ScreenManager.SpriteBatch.Draw(status, center - statusSize / 2, Color.White);
            }

            ScreenManager.SpriteBatch.End();
        }

        private void DrawShadowedString(SpriteFont font, string text, Vector2 position, Color color)
        {
            ScreenManager.SpriteBatch.DrawString(font, text, position + new Vector2(1.0f, 1.0f), Color.Black);
            ScreenManager.SpriteBatch.DrawString(font, text, position, color);
        }

        void LinkToWebPage(object sender, LinkEventArgs e)
        {
            Process.Start(e.Link);
        }
    }
}
