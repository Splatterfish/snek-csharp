using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;

namespace snek
{
    internal class Program
    {

        static void Main(string[] args)
        {
            string gameVersion = "2.2";
            int windowX = 42;
            int windowY = 27;
            int bodycount = 0;
            int fruitTimer;
            List<Coordinate> coordinates = new List<Coordinate>();
            List<Coordinate> fruitList = new List<Coordinate>(); 
            Random random = new Random();
            ConsoleKeyInfo keyInfo;
            ConsoleKey moveInput;
            bool walls;
            bool gameOn = true;
            int gameSpeed;

            // Set the console window size (width, height)
            Console.SetWindowSize(windowX, windowY);
            Console.SetBufferSize(windowX, windowY);
            Console.Title = $"snek v{gameVersion}";

            // Clear the console
            Console.Clear();

            //invoke the init/splash
            SplashScreen();

            //as long as you don't press ESC...
            //or break either of collision rules...
            //the game loops. otherwise it ends
            do
            {
                while (!Console.KeyAvailable)
                {
                    if ((WallCheck(coordinates[0].x, coordinates[0].y) == false) && (SnakeCheck(coordinates[0], coordinates) == false))
                    {
                        Update();
                        Thread.Sleep(gameSpeed);
                    }
                    else 
                    {
                        gameOn = false;
                        break;
                    }
                }
                if (gameOn)
                {
                    keyInfo = Console.ReadKey(true);
                }
                else
                {
                    break;
                }
            } while (keyInfo.Key != ConsoleKey.Escape);

            GameOver();

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ 

            // Init Function
            void SplashScreen()
            {
                walls = true;

                fruitTimer = 0;

                //draw a splash screen
                Console.Clear();
                WriteLineAt(@"                      __    ", 5, 4);
                WriteLineAt(@"  ______ ____   ____ |  | __", 5, 5);
                WriteLineAt(@" /  ___//    \_/ __ \|  |/ /", 5, 6);
                WriteLineAt(@" \___ \|   |  \  ___/|    < ", 5, 7);
                WriteLineAt(@"/____  >___|  /\___  >__|_ \", 5, 8);
                WriteLineAt(@"     \/     \/     \/     \/", 5, 9);
                WriteLineAt($"v{gameVersion} -- by Jason Morejon", 13, 10);
                WriteLineAt(" ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~", 3, 11);
                WriteLineAt("Press any key to begin!", 5, 22);

                Walls();
                keyInfo = Console.ReadKey(true);
                Console.Clear();

                Walls();
                WriteLineAt("step on fruit.", 12, 12);
                Thread.Sleep(1000);
                Console.Clear();

                Walls();
                WriteLineAt("no step on snek.", 12, 12);
                Thread.Sleep(1000);
                Console.Clear();

                // PHASE 2 IDEA- WALLS ARE OPTIONAL
                // add Console.Readkey() to determine if walls
                // if !walls, you'll have to add wraparound logic into UpdatePostion(), which should really just be "if (posX==1){posX=screenX}; if (posY==screenY){posY=1}; etc
                
                if (walls)
                {
                    Walls();
                    WriteLineAt("no step on walls.", 12, 12);
                    Thread.Sleep(1000);
                    Console.Clear();
                }

                Walls();
                PutCharAt('3', 21, 12);
                Thread.Sleep(1000);
                PutCharAt('2', 21, 12);
                Thread.Sleep(1000);
                PutCharAt('1', 21, 12);
                Thread.Sleep(1000);


                bodycount = 0;
                moveInput = ConsoleKey.W;
                coordinates.Add(new Coordinate(21, 12));
            }

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

            // The Game Loop
            void Update()
            {
                // increase game speed based on body length up until the point when it would break, then plateau
                if (bodycount > 48)
                {
                    gameSpeed = 10;
                }
                else
                {
                    int bodytens = bodycount * 5;     
                    gameSpeed = 250 - bodytens;
                }

                // fresh start
                Console.Clear();

                // draw walls (if applicable)
                Walls();
                
                // draw snake
                RenderSnake(bodycount, coordinates);

                // find new direction if applicable
                InputCheck(keyInfo, ref moveInput);

                // apply movement by adding future position of snake head into list 
                UpdatePosition(keyInfo, moveInput, coordinates);
                
                // look for fruit collision and handle that
                FruitCheck(coordinates[0], ref fruitList, ref bodycount);

                // new fruit generation
                if (fruitTimer < 20)
                {
                    fruitTimer++;
                }
                else
                {
                    RandomFruit((1 + (windowX - 1)), ((windowY - 1) - 1), ref fruitList);
                    fruitTimer = 0;
                }

                // draw fruit 
                RenderFruit(fruitList);
                
            }

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~


            // Function to update movement input, with 180 reject logic
            void InputCheck(ConsoleKeyInfo keyInfo, ref ConsoleKey moveInput)
            {

                if ((keyInfo.Key == ConsoleKey.W) && (moveInput != ConsoleKey.S)) 
                {
                    moveInput = ConsoleKey.W;
                }
                else if ((keyInfo.Key == ConsoleKey.A) && (moveInput != ConsoleKey.D))
                {
                    moveInput = ConsoleKey.A;
                }
                else if ((keyInfo.Key == ConsoleKey.S) && (moveInput != ConsoleKey.W))
                {
                    moveInput = ConsoleKey.S;
                }
                else if ((keyInfo.Key == ConsoleKey.D) && (moveInput != ConsoleKey.A))
                {
                    moveInput = ConsoleKey.D;
                }

            }

            // Insert a new Coordinate into the List in the first/0 position with the updated coordinates
            void UpdatePosition(ConsoleKeyInfo keyinfo, ConsoleKey moveInput, List<Coordinate> coordinates)
            {
                if (moveInput == ConsoleKey.W)
                {
                    coordinates.Insert(0, new Coordinate(coordinates[0].x, (coordinates[0].y - 1)));
                }
                else if (moveInput == ConsoleKey.A)
                {
                    coordinates.Insert(0, new Coordinate((coordinates[0].x - 1), coordinates[0].y));
                }
                else if (moveInput == ConsoleKey.S)
                {
                    coordinates.Insert(0, new Coordinate(coordinates[0].x, (coordinates[0].y + 1)));
                }
                else if (moveInput == ConsoleKey.D)
                {
                    coordinates.Insert(0, new Coordinate((coordinates[0].x + 1), coordinates[0].y));
                }
            }

            // Function to place the snake on the map
            void RenderSnake(int bodycount, List<Coordinate> coordinates)
            {
                for (int body = bodycount; body >= 0; body--)
                {
                    int posX = coordinates[body].x;
                    int posY = coordinates[body].y;
                    PutCharAt('@', posX, posY);
                }
            }

            // Function to check for collision with an existing bodypart 
            bool SnakeCheck(Coordinate coordinate, List<Coordinate> coordinates)
            {
                bool eaten = false;
                int tempX = coordinate.x;
                int tempY = coordinate.y;

                for (int i = 1; i <= bodycount; i++)
                {

                    if ((coordinates[i].x == tempX) && (coordinates[i].y == tempY))
                    {
                        eaten = true;
                        break;
                    }
                }
                return eaten;
            }

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~


    // in order to improve: 
    //    make sure that fruit can only spawns in an area that is not already occupied by either fruit or body
    //    IFF you do that, figure out maximum possible bodycount and set Win Condition


            // Function to generate a piece of fruit in a random location and add to the fruit list
            void RandomFruit(int x, int y, ref List<Coordinate> coordinates)
            {
                int randomX = random.Next(2,x-1);
                int randomY = random.Next(2,y-1);
                coordinates.Add(new Coordinate(randomX, randomY));
            }

            // Function to render all fruit on the fruit list
            void RenderFruit(List<Coordinate> coordinates)
            {
                foreach (Coordinate coord in coordinates) 
                {
                    int posX = coord.x;
                    int posY = coord.y;
                    PutCharAt('%', posX, posY);
                }
            }

            // Function to check for collision with a fruit on the list
            void FruitCheck(Coordinate coordinate, ref List<Coordinate> fruitList, ref int bodycount)
            {
                bool eaten = false;
                int tempX = coordinate.x;
                int tempY = coordinate.y;

                foreach (Coordinate fruit in fruitList)
                {
                    if ((fruit.x == tempX) && (fruit.y == tempY))
                    {
                        eaten = true;
                        break;
                    }
                }
                if (eaten)
                {
                    bodycount++;
                    fruitList.RemoveAll(c => c.x == tempX && c.y == tempY);
                }
            }

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

            // Function to place a character at a specific (x, y) coordinate
            void PutCharAt(char c, int x, int y)
            {
                // Set the cursor position
                Console.SetCursorPosition(x, y);
                // Write the character
                Console.Write(c);
                //hide the blinking cursor afterwards
                Console.SetCursorPosition((windowX-1), (windowY-1));
            }

            // Function to write a line of text starting at a specific (x, y) coordinate
            void WriteLineAt(string s, int x, int y)
            {
                // Set the cursor position
                Console.SetCursorPosition(x, y);
                // Write the character
                Console.Write(s);
                //hide the blinking cursor afterwards
                Console.SetCursorPosition((windowX-1), (windowY-1));
            }

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

            // Function that draws a box
            void Walls()
            {
                if (walls)
                {
                    WriteLineAt("#########################################", 1, 2);
                    for (int i = 3; i < (26); i++)
                    {
                        PutCharAt('#', 1, i);
                        PutCharAt('#', (windowX - 1), i);
                    }
                    WriteLineAt("#########################################", 1, 26);
                }
            }

            // Function to check if you're colliding with the wall
            bool WallCheck(int x, int y)
            {
                if (walls)
                {
                    if ((x == 1) || (x == (windowX - 1)) || (y == 1) || (y == 25))
                    {                        
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

            // Function that ends the game
            void GameOver()
            {
                Console.Clear();
                Thread.Sleep(10);
                PutCharAt('G', 16, 12);
                Walls();
                WriteLineAt("GAME OVER!!", 16, 11);
                WriteLineAt($"Score: {bodycount}", 16, 12);
                Console.ReadKey();
                Console.Clear();
            }
        }
    }
}

