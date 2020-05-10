using System;

namespace Newmazon.Model
{
    public class NewmazonEventArgs : EventArgs
    {
        private double gameTime;
        private bool gameOver;

        public double GameTime { get { return gameTime; } }

        public bool GameOver { get { return gameOver; } }


        public NewmazonEventArgs(bool _gameOver, double _gameTime)
        {
            gameOver = _gameOver;
            gameTime = _gameTime;
        }
    }
}
