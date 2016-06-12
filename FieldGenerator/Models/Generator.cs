using EpPathFinding.cs;
using GAF;
using GAF.Operators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FieldGenerator.Models
{
    public class Generator
    {
        static int boardSize;

        public static int[,] Generate()
        {
            const int populationSize = 200;

            var population = new Population();

            Random random = new Random();

            boardSize = 4;
            int[,] board = new int[boardSize, boardSize];
            for (var i = 0; i < boardSize; i++)
            {
                for (var j = 0; j < boardSize; j++)
                {
                    board[i, j] = random.Next(0, 3);
                }
            }

            //start point and end point
            board[0, 0] = 2;
            board[boardSize - 1, boardSize - 1] = 0;

            for (var p = 0; p < populationSize; p++)
            {
                var chromosome = new Chromosome();
                foreach (var field in board)
                {
                    chromosome.Genes.Add(new Gene(field));
                }

                population.Solutions.Add(chromosome);
            }

            var elite = new Elite(10);

            var crossover = new Crossover(0.8)
            {
                CrossoverType = CrossoverType.SinglePoint
            };

            var mutate = new SwapMutate(0.1);

            var ga = new GeneticAlgorithm(population, CalculateFitness);

            ga.OnGenerationComplete += ga_OnGenerationComplete;
            ga.OnRunComplete += ga_OnRunComplete;

            ga.Operators.Add(elite);
            ga.Operators.Add(crossover);
            ga.Operators.Add(mutate);

            ga.Run(Terminate);

            return chromosome2Board(ga.Population.GetTop(1)[0]);
        }

        static void ga_OnRunComplete(object sender, GaEventArgs e)
        {
            var fittest = e.Population.GetTop(1)[0];

            var board = chromosome2Board(fittest);

            for (var i = 0; i < board.GetLength(0); i++)
            {
                for (var j = 0; j < board.GetLength(1); j++)
                {
                    Console.Write(string.Format("{0} ", board[i, j]));
                }
                Console.Write(Environment.NewLine);
            }
            Console.ReadLine();
        }

        private static void ga_OnGenerationComplete(object sender, GaEventArgs e)
        {
            var fittest = e.Population.GetTop(1)[0];
            Console.WriteLine("Generation {0}, Fitness {1}", e.Generation, fittest.Fitness);
        }

        public static double CalculateFitness(Chromosome chromosome)
        {
            double fitness = 0;

            var board = chromosome2Board(chromosome);

            if (board[0, 0] != 2) return 0;
            if (board[boardSize / 2 - 1, boardSize / 2 - 1] != 0) return 0;



            BaseGrid searchGrid = new StaticGrid(boardSize, boardSize);

            searchGrid.SetWalkableAt(0, 0, true);

            for (var i = 0; i < boardSize; i++)
            {
                for (var j = 0; j < boardSize; j++)
                {
                    if (board[i, j] != 1)
                        searchGrid.SetWalkableAt(i, j, true);
                }
            }

            GridPos startPos = new GridPos(0, 0);
            GridPos endPos;
            JumpPointParam jpParam;
            var resultPathLists = new List<List<GridPos>>();


            for (var i = 0; i < boardSize; i++)
            {
                for (var j = 0; j < boardSize; j++)
                {
                    if (board[i, j] != 1)
                    {
                        endPos = new GridPos(i, j);
                        jpParam = new JumpPointParam(searchGrid, startPos, endPos);
                        resultPathLists.Add(JumpPointFinder.FindPath(jpParam));
                    }
                }
            }

            var counter = 0;

            foreach (var list in resultPathLists)
            {
                if (list.Count > 0)
                    counter++;

            }

            fitness = ((double)counter) / ((double)resultPathLists.Count);

            if (!fieldProportions(board) && (fitness > 0.9))// && (fitness > 0.75))
                fitness = fitness / 2;// fitness/2;// fitness *3/4;

            return fitness;
        }

        private static bool fieldProportions(int[,] board)
        {
            var counter0 = 0;
            var counter1 = 0;
            var counter2 = 0;
            for (var i = 0; i < boardSize; i++)
            {
                for (var j = 0; j < boardSize; j++)
                {
                    if (board[i, j] == 0)
                        counter0++;
                    if (board[i, j] == 1)
                        counter1++;
                    if (board[i, j] == 2)
                        counter2++;
                }
            }

            if ((counter1 < (counter0 + counter2)))
            {
                if (counter0 < counter2)
                    return false;
                else
                    return true;
            }
            else
                return false;
        }

        private static int[,] chromosome2Board(Chromosome chromosome)
        {
            int[,] tempBoard = new int[boardSize, boardSize];


            var genesCount = chromosome.Genes.Count;
            var geneNumber = 0;

            for (var i = 0; i < boardSize; i++)
            {
                for (var j = 0; j < boardSize; j++)
                {
                    tempBoard[i, j] = (int)chromosome.Genes.ElementAt(geneNumber).ObjectValue;
                    geneNumber++;
                }
            }

            return tempBoard;

        }

        public static bool Terminate(Population population, int currentGeneration, long currentEvaluation)
        {
            return currentGeneration > 500;
        }

    }
}