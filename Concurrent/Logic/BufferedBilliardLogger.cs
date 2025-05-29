using Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic
{
    public class BufferedBilliardLogger : IDisposable
    {
        private readonly ConcurrentQueue<string> _logBuffer;
        private readonly Timer _flushTimer;
        private readonly string _logFilePath;
        private readonly object _fileLock = new();
        private int _positionLogCounter = 0;
        private bool _disposed = false;

        // Konfiguracja bufora
        private const int FlushIntervalMs = 1000; // Zrzuć bufor co 1 sekundę
        private const int MaxBufferSize = 1000;   // Maksymalny rozmiar bufora

        public BufferedBilliardLogger(string logFilePath = "logs/billiards.txt")
        {
            _logBuffer = new ConcurrentQueue<string>();
            _logFilePath = logFilePath;

            // Utwórz katalog jeśli nie istnieje
            var directory = Path.GetDirectoryName(_logFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Wyczyść plik logu na początku nowej sesji
            if (File.Exists(_logFilePath))
            {
                File.WriteAllText(_logFilePath, string.Empty);
            }

            // Timer do automatycznego zrzucania bufora
            _flushTimer = new Timer(FlushBufferToFile, null, FlushIntervalMs, FlushIntervalMs);
        }

        private void AddToBuffer(string logEntry)
        {
            if (_disposed) return;

            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var formattedEntry = $"{timestamp} | {logEntry}";

            _logBuffer.Enqueue(formattedEntry);

            // Jeśli bufor jest pełny, wymuś zrzut
            if (_logBuffer.Count >= MaxBufferSize)
            {
                Task.Run(() => FlushBufferToFile(null));
            }
        }

        private void FlushBufferToFile(object state)
        {
            if (_disposed || _logBuffer.IsEmpty) return;

            lock (_fileLock)
            {
                try
                {
                    using (var writer = new StreamWriter(_logFilePath, append: true))
                    {
                        while (_logBuffer.TryDequeue(out string logEntry))
                        {
                            writer.WriteLine(logEntry);
                        }
                        writer.Flush();
                    }
                }
                catch (Exception ex)
                {
                    // W przypadku błędu zapisu, dodaj informację o błędzie do bufora
                    Console.WriteLine($"Logger Error: {ex.Message}");
                }
            }
        }

        public void LogBallPosition(IBall ball)
        {
            _positionLogCounter++;
            // Loguj pozycję co 60 tików 
            if (_positionLogCounter % 60 == 0)
            {
                var speed = Math.Sqrt(ball.Velocity.X * ball.Velocity.X + ball.Velocity.Y * ball.Velocity.Y);
                var logEntry = $"POSITION | Ball {ball.Number} ({ball.Color}): " +
                              $"X={ball.Position.X:F2}, Y={ball.Position.Y:F2}, " +
                              $"VX={ball.Velocity.X:F2}, VY={ball.Velocity.Y:F2}, Speed={speed:F2}";

                AddToBuffer(logEntry);
            }
        }

        public void LogBallCollision(IBall ball1, IBall ball2, DateTime collisionTime)
        {
            var speed1 = Math.Sqrt(ball1.Velocity.X * ball1.Velocity.X + ball1.Velocity.Y * ball1.Velocity.Y);
            var speed2 = Math.Sqrt(ball2.Velocity.X * ball2.Velocity.X + ball2.Velocity.Y * ball2.Velocity.Y);

            var logEntry = $"COLLISION | Ball {ball1.Number} ({ball1.Color}) <-> Ball {ball2.Number} ({ball2.Color}) | " +
                          $"Position: ({ball1.Position.X:F2}, {ball1.Position.Y:F2}) | " +
                          $"Speed1: {speed1:F2}, Speed2: {speed2:F2} | " +
                          $"CollisionTime: {collisionTime:HH:mm:ss.fff}";

            AddToBuffer(logEntry);
        }

        public void LogWallCollision(IBall ball, DateTime collisionTime)
        {
            var speed = Math.Sqrt(ball.Velocity.X * ball.Velocity.X + ball.Velocity.Y * ball.Velocity.Y);

            var logEntry = $"WALL_HIT | Ball {ball.Number} ({ball.Color}) hit wall | " +
                          $"Position: ({ball.Position.X:F2}, {ball.Position.Y:F2}) | " +
                          // Nazwa ściany U, D, L, R w zależności od kierunku
                           $"Wall name: {(ball.Velocity.X > 0 ? "R" : ball.Velocity.X < 0 ? "L" : ball.Velocity.Y > 0 ? "D" : "U")} | " +
                          $"Speed: {speed:F2} | CollisionTime: {collisionTime:HH:mm:ss.fff}";

            AddToBuffer(logEntry);
        }

        public void LogSimulationStart(int ballCount)
        {
            var logEntry = $"=== SIMULATION STARTED === Balls: {ballCount}";
            AddToBuffer(logEntry);
        }

        public void LogSimulationStop()
        {
            var logEntry = "=== SIMULATION STOPPED ===";
            AddToBuffer(logEntry);

            // Natychmiast zrzuć bufor przy zatrzymaniu symulacji
            FlushBufferToFile(null);
        }

        // Metoda do manualnego zrzucenia bufora
        public void ForceFlush()
        {
            FlushBufferToFile(null);
        }

        // Właściwość do sprawdzenia rozmiaru bufora
        public int BufferSize => _logBuffer.Count;

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;

            // Zatrzymaj timer
            _flushTimer?.Dispose();

            // Zrzuć pozostałe logi
            FlushBufferToFile(null);
        }
    }
}
