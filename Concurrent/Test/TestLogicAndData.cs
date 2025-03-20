using Microsoft.VisualStudio.TestTools.UnitTesting;
using Data;
using Logic;
using System.Threading;

namespace Test
{
    [TestClass]
    public sealed class TestLogicAndData
    {
        [TestMethod]
        public void BallTest()
        {
            Ball ball = new Ball(1, "red", 2, 1, 0, 1, 1);
            Assert.AreEqual(1, ball.Radius);
            Assert.AreEqual("red", ball.Color);
            Assert.AreEqual(2, ball.Number);
            Assert.AreEqual(1, ball.PositionX);
            Assert.AreEqual(0, ball.PositionY);
            Assert.AreEqual(1, ball.VelocityX);
            Assert.AreEqual(1, ball.VelocityY);
        }

        [TestMethod]
        public void PoolTableTest()
        {
            PoolTable poolTable = new PoolTable(100, 100);
            Assert.AreEqual(100, poolTable.Width);
            Assert.AreEqual(100, poolTable.Height);
        }

        [TestMethod]
        public void MovingBallsTest()
        {
            PoolTable poolTable = new PoolTable(100, 100);
            PoolProcessor poolProcessor = new PoolProcessor(poolTable);
            Ball ball1 = new Ball(1, "red", 2, 4, 3, 1, 1);
            Ball ball2 = new Ball(1, "blue", 7, 6, 6, 2, 2);
            poolProcessor.AddBall(ball1);
            poolProcessor.AddBall(ball2);

            // Timer działa asynchronicznie, więc testy muszą czekać na jego wykonanie.
            // Zamiast tego można ręcznie wywołać metodę Update, ale to nie jest zalecane.Tylko na potrzeby testów.
            for (int i = 0; i < 100; i++)
            {
                poolProcessor.Update(null, null);
                Thread.Sleep(10); // Symulacja odstępu czasowego 20ms
            }


            Assert.IsTrue(ball1.PositionX > 1 && ball1.PositionY > 0.5);
            Assert.IsTrue(ball2.PositionX > 3 && ball2.PositionY > 2);

        }

        [TestMethod]
        public void BallHitTabelWallTest()
        {
            PoolTable poolTable = new PoolTable(10, 10);
            PoolProcessor poolProcessor = new PoolProcessor(poolTable);
            Ball ball1 = new Ball(1, "green", 9, 2, 2, -10, -10);
            poolProcessor.AddBall(ball1);

            for (int i = 0; i < 15; i++)
            {
                poolProcessor.Update(null, null);
                Thread.Sleep(10); // Symulacja odstępu czasowego 10ms
            }

            Assert.IsTrue(ball1.VelocityX > 0 && ball1.VelocityY > 0);

        }

        [TestMethod]
        public void IsAnotherBallCollidingTest()
        {
            PoolTable poolTable = new PoolTable(10, 10);
            PoolProcessor poolProcessor = new PoolProcessor(poolTable);
            Ball ball1 = new Ball(1, "pink", 9, 2, 2, 10, 0);
            Ball ball2 = new Ball(1, "pruple", 9, 5, 2, 0, 0);
            poolProcessor.AddBall(ball1);
            poolProcessor.AddBall(ball2);

            bool collisionDetected = false;

            poolProcessor.BallCollision += (sender, args) =>
            {
                collisionDetected = true;
            };

            var maxWaitTime = TimeSpan.FromSeconds(3);
            var timeout = new CancellationTokenSource(maxWaitTime);

            poolProcessor.Start();

            while (!timeout.Token.IsCancellationRequested && !collisionDetected)
            {
                Thread.Sleep(10);
            }

            poolProcessor.Stop();

            Assert.IsTrue(collisionDetected, $"Kule powinny zderzyć się w ciągu {maxWaitTime.TotalSeconds} sekund");
        }

        [TestMethod]
        public void IsAnotherBallCollidingTestFail()
        {
            PoolTable poolTable = new PoolTable(10, 10);
            PoolProcessor poolProcessor = new PoolProcessor(poolTable);
            Ball ball1 = new Ball(1, "pink", 9, 2, 2, 0.05, 0);
            Ball ball2 = new Ball(1, "pruple", 9, 5, 2, 0, 0);
            poolProcessor.AddBall(ball1);
            poolProcessor.AddBall(ball2);

            bool collisionDetected = false;

            poolProcessor.BallCollision += (sender, args) =>
            {
                collisionDetected = true;
            };

            var maxWaitTime = TimeSpan.FromMilliseconds(100);
            var timeout = new CancellationTokenSource(maxWaitTime);

            poolProcessor.Start();

            while (!timeout.Token.IsCancellationRequested && !collisionDetected)
            {
                Thread.Sleep(10);
            }

            poolProcessor.Stop();

            Assert.IsFalse(collisionDetected);
        }
    }
}
