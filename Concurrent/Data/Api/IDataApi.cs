﻿using Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Api
{
    public interface IDataApi
    {
        IBall CreateBall(string color, int radius, int number, float vx, float vy);

        void UpdateBall(IBall ball, float x, float y, float? vx, float? vy);

        void CreatePoolTable(float width, float height);

        void AddBallToTable(IBall ball);

        void DeleteBallFromTable(IBall ball);

        Vector2 GetTableSize();

        IEnumerable<IBall> GetAllBallsFromTable();
    }
}
