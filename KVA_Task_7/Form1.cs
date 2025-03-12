namespace KVA_Task_7
{
    public partial class Form1 : Form
    {
        private const int GridSize = 7; // Размер поля 7x7
        private const int CellSize = 90; // Размер клетки
        private Button[,] grid; // Сетка для игрового поля
        private List<Point> chickens; 
        private List<Point> foxes; 
        private Point selectedChicken; 
        private bool isChickensTurn = true; // Флаг, чей сейчас ход
        private int currentFoxIndex = 0; // Индекс лисы, которая ходит

        private Image chickenImage;
        private Image foxImage;
        public Form1()
        {
            InitializeComponent();
            this.Size = new Size(GridSize * CellSize + 50, GridSize * CellSize + 100);
            InitializeGame();
        }

        /// <summary>
        /// создание поля
        /// </summary>
        private void InitializeGame()
        {
            grid = new Button[GridSize, GridSize];
            chickens = new List<Point>();
            foxes = new List<Point>();
            chickenImage = Image.FromFile("chicken.jpg");
            foxImage = Image.FromFile("fox.png");
            chickenImage = ResizeImage(chickenImage, CellSize, CellSize);
            foxImage = ResizeImage(foxImage, CellSize, CellSize);

            // Инициализация куриц (в нижней части поля, начиная с ряда 6 и ниже)
            for (int i = 1; i < 21; i++)
            {
                chickens.Add(new Point(i % GridSize, GridSize - 1 - (i / GridSize)));
            }

            // Инициализация лис (в верхней части поля)

            foxes.Add(new Point(2, 2)); // Первая лиса
            foxes.Add(new Point(4, 2)); // Вторая лиса

            for (int i = 0; i < GridSize; i++)
            {
                for (int j = 0; j < GridSize; j++)
                {
                    // Условия для вырезов
                    if (
                        ((i == 0 || i == 1) && (j == 0 || j == 1)) ||
                        ((i == 5 || i == 6) && (j == 5 || j == 6)) ||
                        ((i == 0 || i == 1) && (j == 5 || j == 6)) ||
                        ((i == 5 || i == 6) && (j == 0 || j == 1))
                    )
                    {
                        continue;
                    }

                    var button = new Button
                    {
                        Size = new Size(CellSize, CellSize),
                        Location = new Point(i * CellSize + 20, j * CellSize),
                        Tag = new Point(i, j),
                        FlatStyle = FlatStyle.Flat,
                        BackColor = Color.White,
                        Text = "",
                        ImageAlign = ContentAlignment.MiddleCenter,
                        BackgroundImageLayout = ImageLayout.Center,
                    };

                    button.Click += GridButton_Click;
                    grid[i, j] = button;
                    this.Controls.Add(button);
                }
            }

            UpdateBoard();
        }

        /// <summary>
        /// обновление игрового поля
        /// </summary>
        private void UpdateBoard()
        {
            foreach (var button in grid)
            {
                if (button is not null)
                {
                    var point = (Point)button.Tag;
                    button.Enabled = true;
                    button.BackColor = Color.White;
                    button.Text = "";
                    button.Image = null;
                }
            }

            foreach (var chicken in chickens)
            {
                if (grid[chicken.X, chicken.Y] is not null)
                {
                    grid[chicken.X, chicken.Y].BackColor = Color.LightGreen;
                    grid[chicken.X, chicken.Y].Image = chickenImage;
                }
            }

            foreach (var fox in foxes)
            {
                if (grid[fox.X, fox.Y] is not null)
                {
                    grid[fox.X, fox.Y].BackColor = Color.Orange;
                    grid[fox.X, fox.Y].Image = foxImage;
                }
            }

            if (isChickensTurn)
            {
                // Курицам нужно выбирать ход
            }
            else
            {
                // Логика для хода лис
                MoveFox();
                isChickensTurn = true;
                currentFoxIndex = (currentFoxIndex + 1) % foxes.Count; // Переход к следующей лисе
                UpdateBoard();
            }
        }

        private void GridButton_Click(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var point = (Point)button.Tag;
            if (isChickensTurn)
            {
                if (chickens.Contains(point))
                {
                    // Пользователь выбрал курицу для движения
                    if (selectedChicken != Point.Empty)
                    {
                        // Перемещение курицы
                        if (GameBoard.IsValidMove(selectedChicken, point))
                        {
                            MoveChicken(selectedChicken, point);
                            isChickensTurn = false;
                            UpdateBoard();
                        }
                        else
                        {
                            MessageBox.Show("Неверный ход для курицы!");
                        }
                    }
                    else
                    {
                        selectedChicken = point;
                    }
                }
                else if (selectedChicken != Point.Empty)
                {
                    // Пользователь пытается переместить курицу на клетку, не содержащую курицу
                    if (GameBoard.IsValidMove(selectedChicken, point))
                    {
                        MoveChicken(selectedChicken, point);
                        isChickensTurn = false;
                        selectedChicken = Point.Empty;
                        UpdateBoard();
                    }
                    else
                    {
                        MessageBox.Show("Неверный ход для курицы!");
                    }
                }
            }
        }


        /// <summary>
        /// перемещение курицы. Удаляет курицу с from, добавляет на to.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        private void MoveChicken(Point from, Point to)
        {
            // Перемещаем курицу
            chickens.Remove(from);
            chickens.Add(to);
        }

        /// <summary>
        /// ход лисы
        /// </summary>
        private void MoveFox()
        {
            Point currentFox = foxes[currentFoxIndex];

            // Попытка съесть курицу
            Point? chickenToEat = FindChickenToEat(currentFox);

            if (chickenToEat.HasValue)
            {
                // Лиса съедает курицу
                EatChicken(currentFox, chickenToEat.Value);
            }
            else
            {
                // Лиса не может съесть курицу, она просто двигается
                MoveFoxRandom(currentFox);
            }
        }

        /// <summary>
        /// поиск курицы для съедения
        /// </summary>
        /// <param name="fox"></param>
        /// <returns></returns>
        private Point? FindChickenToEat(Point fox)
        {
            // Найти курицу, которую лиса может съесть (проверка на возможный прыжок через курицу)
            foreach (var chicken in chickens)
            {
                if (CanEatFox(fox, chicken))
                {
                    return chicken;
                }
            }

            return null;
        }

        
        private bool CanEatFox(Point fox, Point chicken)
        {
            // Лиса может съесть курицу, если она стоит рядом (слева, справа, сверху, снизу)
            // и за курицей есть свободная клетка для перепрыгивания
            if (fox.X == chicken.X && Math.Abs(fox.Y - chicken.Y) == 1)
            {
                // Лиса сверху или снизу
                int newY = fox.Y + (chicken.Y - fox.Y) * 2;
                if (newY >= 0 && newY < GridSize && !chickens.Contains(new Point(fox.X, newY)) && !foxes.Contains(new Point(fox.X, newY)))
                    return true;
            }

            if (fox.Y == chicken.Y && Math.Abs(fox.X - chicken.X) == 1)
            {
                // Лиса справа или слева
                int newX = fox.X + (chicken.X - fox.X) * 2;
                if (newX >= 0 && newX < GridSize && !chickens.Contains(new Point(newX, fox.Y)) && !foxes.Contains(new Point(newX, fox.Y)))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// лиса съедает курицу
        /// </summary>
        /// <param name="fox"></param>
        /// <param name="chicken"></param>
        private void EatChicken(Point fox, Point chicken)
        {
            // Лиса съедает курицу
            foxes.Remove(fox);
            chickens.Remove(chicken);

            // Лиса становится на место, откуда была съедена курица
            foxes.Add(new Point(fox.X + (chicken.X - fox.X) * 2, fox.Y + (chicken.Y - fox.Y) * 2));
        }

        /// <summary>
        /// случайный ход лисы
        /// </summary>
        /// <param name="fox"></param>
        private void MoveFoxRandom(Point fox)
        {
            // Логика перемещения лисы, если она не может съесть курицу
            List<Point> possibleMoves = new List<Point>
            {
                new Point(fox.X - 1, fox.Y), // Влево
                new Point(fox.X + 1, fox.Y), // Вправо
                new Point(fox.X, fox.Y - 1), // Вверх
                new Point(fox.X, fox.Y + 1)  // Вниз
            };

            // Фильтруем допустимые ходы (находятся внутри поля, не заняты другими объектами, и не в вырезах)
            possibleMoves = possibleMoves
                .Where(p => p.X >= 0 && p.X < GridSize && p.Y >= 0 && p.Y < GridSize) // Проверка на границы поля
                .Where(p => !chickens.Contains(p) && !foxes.Contains(p)) // Проверка, чтобы клетка не была занята курицей или лисой
                .Where(p => !GameBoard.IsCut(p)) // Проверка, чтобы клетка не была в вырезе
                .ToList();

            // Выбираем случайный допустимый ход для лисы
            if (possibleMoves.Count > 0)
            {
                Random rand = new Random();
                Point newFoxPosition = possibleMoves[rand.Next(possibleMoves.Count)];
                foxes.Remove(fox);
                foxes.Add(newFoxPosition);
            }
        }


        private void Form1_Load(object sender, EventArgs e)
        {

        }
        private Image ResizeImage(Image img, int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(img, 0, 0, width, height);
            }
            return bmp;
        }
    }
}
