using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KVA_Task_7
{
    internal class GameBoard
    {
        public static bool IsCut(Point point)
        {
            // Проверка, попадает ли точка в вырез
            return
                ((point.X == 0 || point.X == 1) && (point.Y == 0 || point.Y == 1)) ||
                ((point.X == 5 || point.X == 6) && (point.Y == 5 || point.Y == 6)) ||
                ((point.X == 0 || point.X == 1) && (point.Y == 5 || point.Y == 6)) ||
                ((point.X == 5 || point.X == 6) && (point.Y == 0 || point.Y == 1));
        }

        public static bool IsValidMove(Point from, Point to)
        {
            // Проверка на допустимость хода
            if (Math.Abs(from.X - to.X) + Math.Abs(from.Y - to.Y) == 1)
            {
                return true;
            }
            return false;
        }
    }
}
