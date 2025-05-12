using Data.Entities;
using Logic;
using Abstractions;

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
            Assert.IsNotNull(ball);
            Assert.IsInstanceOfType(ball, typeof(Ball));
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
            IPoolProcessor poolProcessor = new PoolProcessor();
            poolProcessor.CreateTable(20.0f, 10.0f);
            poolProcessor.AddBalls(2);

            List<float> startPosX = new List<float>();
            List<float> startPosY = new List<float>();
            foreach (var ball in poolProcessor.GetAllBallsFromTable())
            {
                startPosX.Add(ball.Position[0]);
                startPosY.Add(ball.Position[1]);
            }


            poolProcessor.Start();

            for (int i = 0; i < 100; i++)
            {
                Thread.Sleep(10); // Symulacja odstępu czasowego 10ms
            }

            poolProcessor.Stop();

            int j = 0;
            foreach (var ball in poolProcessor.GetAllBallsFromTable())
            {
                Assert.IsTrue(ball.Position[0] != startPosX[j] && ball.Position[1] != startPosY[j]);
                j++;
            }


        }

        [TestMethod]
        public void BallHitTableWallTest()
        {
            IPoolProcessor poolProcessor = new PoolProcessor();
            poolProcessor.CreateTable(4.0f, 4.0f);
            poolProcessor.AddBalls(1);

            bool wallCollisionDetected = false;

            // Subskrypcja - czyli event handler na konkretne zdarzenie
            // += oznacza dodanie nowego event handlera, który czeka na ten event
            //poolProcessor.WallsCollision += (sender, args) =>
            //{
            //    wallCollisionDetected = true;
            //    Console.WriteLine($"Ball {args.Ball.Color} hit wall at {args.CollisionTime}");
            //};

            poolProcessor.Start();

            // Max czas na czekanie
            var maxWaitTime = TimeSpan.FromSeconds(5);
            var startTime = DateTime.Now;

            while (!wallCollisionDetected && (DateTime.Now - startTime) < maxWaitTime)
            {
                Thread.Sleep(10);
            }

            poolProcessor.Stop();

            Assert.IsTrue(wallCollisionDetected);
        }

        //[TestMethod]
        //public void IsAnotherBallCollidingTest()
        //{
        //    PoolTable poolTable = new PoolTable(10, 10);
        //    PoolProcessor poolProcessor = new PoolProcessor(poolTable);
        //    Ball ball1 = new Ball(1, "pink", 9, 2, 2, 10, 0);
        //    Ball ball2 = new Ball(1, "pruple", 9, 5, 2, 0, 0);
        //    poolProcessor.AddBall(ball1);
        //    poolProcessor.AddBall(ball2);

        //    bool collisionDetected = false;

        //    poolProcessor.BallsCollision += (sender, args) =>
        //    {
        //        collisionDetected = true;
        //    };

        //    var maxWaitTime = TimeSpan.FromSeconds(3);
        //    var timeout = new CancellationTokenSource(maxWaitTime);

        //    poolProcessor.Start();

        //    while (!timeout.Token.IsCancellationRequested && !collisionDetected)
        //    {
        //        Thread.Sleep(10);
        //    }

        //    poolProcessor.Stop();

        //    Assert.IsTrue(collisionDetected, $"Kule powinny zderzyć się w ciągu {maxWaitTime.TotalSeconds} sekund");
        //}

        //[TestMethod]
        //public void IsAnotherBallCollidingTestFail()
        //{
        //    PoolTable poolTable = new PoolTable(10, 10);
        //    PoolProcessor poolProcessor = new PoolProcessor(poolTable);
        //    Ball ball1 = new Ball(1, "pink", 9, 2, 2, 0.05, 0);
        //    Ball ball2 = new Ball(1, "pruple", 9, 5, 2, 0, 0);
        //    poolProcessor.AddBall(ball1);
        //    poolProcessor.AddBall(ball2);

        //    bool collisionDetected = false;

        //    poolProcessor.BallsCollision += (sender, args) =>
        //    {
        //        collisionDetected = true;
        //        Console.WriteLine("Czas kolizji między bilami: " + args.CollisionTime);
        //    };

        //    var maxWaitTime = TimeSpan.FromMilliseconds(100);
        //    var timeout = new CancellationTokenSource(maxWaitTime);

        //    poolProcessor.Start();

        //    while (!timeout.Token.IsCancellationRequested && !collisionDetected)
        //    {
        //        Thread.Sleep(10);
        //    }

        //    poolProcessor.Stop();

        //    Assert.IsFalse(collisionDetected);
        //}
    }
}
