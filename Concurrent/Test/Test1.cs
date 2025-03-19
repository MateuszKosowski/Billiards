using Microsoft.VisualStudio.TestTools.UnitTesting;
using Data;
using Logic;
using System.Threading;

namespace Test
{
    [TestClass]
    public sealed class Test1
    {
        private TestContext testContextInstance;

        // Getters and setters for the test context 
        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }


        [TestMethod]
        public void PoolProcessorTest()
        {
            PoolTable poolTable = new PoolTable(100, 100);
            PoolProcessor poolProcessor = new PoolProcessor(poolTable);
            Ball ball1 = new Ball(1, "red", 2, 1, 0, 1, 1);
            Ball ball2 = new Ball(1, "blue", 5, 3, 2, 2, 2);
            poolProcessor.AddBall(ball1);
            poolProcessor.AddBall(ball2);

            // Timer działa asynchronicznie, więc testy muszą czekać na jego wykonanie.
            // Zamiast tego można ręcznie wywołać metodę Update, ale to nie jest zalecane.Tylko na potrzeby testów.
            for (int i = 0; i < 100; i++)
            {
                poolProcessor.Update(null, null);
                Thread.Sleep(10); // Symulacja odstępu czasowego 20ms
            }


            Assert.IsTrue(ball1.PositionX > 1 && ball1.PositionY > 0);
            Assert.IsTrue(ball2.PositionX > 3 && ball2.PositionY > 2);

        }
    }
}
