using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum TurnType
{
    Moving,
    Building
}

enum OnTurn {
    HumanPlayer,
    AIPlayer
}

class Point
{
    public int row;
    public int col;

    public Point(int row, int col)
    {
        this.row = row;
        this.col = col;
    }

    public int DistanceFrom(Point point)
    {
        return Math.Max(Math.Abs(this.row - point.row), Math.Abs(this.col - point.col));
    }
}

class Move //treba da se modifikuje vrv
{
    public int result;
    public int figure; //index of figure

    public Point whereToMoveOn;
    public Point whereToBuildOn;

    public int treeDepth;

    public Move(int figure, Point whereToMoveOn, Point whereToBuildOn)
    {
        this.figure = figure;

        this.whereToMoveOn  = whereToMoveOn;
        this.whereToBuildOn = whereToBuildOn;
    }
}

class Tile
{
    public int  currentLevel;
    public bool isOccupied;

    public Tile(int currentLevel, bool isOccupied)
    {
        this.currentLevel = currentLevel;
        this.isOccupied   = isOccupied;
    }

    public bool CanMoveOnTo(Tile neighbouringTile)
    {
        return neighbouringTile.currentLevel < 4 && neighbouringTile.isOccupied == false &&
               neighbouringTile.currentLevel - currentLevel < 2;
    }

    public bool CanBuildOn(Tile neighbouringTile)
    {
        return neighbouringTile.currentLevel < 4 && neighbouringTile.isOccupied == false;
    }
}

public class AIController : MonoBehaviour
{
    public const int  RowCount = 5;
    public const int  ColCount = 5;

    public const long Easy   = 100000000;  //in nanoseconds, 0.1 second
    public const long Medium = 500000000;  //in nanoseconds, 0.5 second
    public const long Hard   = 1000000000; //in nanoseconds, 1 second

    public static long MaxMinimaxDuration = 1000000000; //in nanoseconds, 1 second

    public static Turn  aiPlayer;
    public static Turn  huPlayer;
    static GameObject[] aiFigures;
    static GameObject[] huFigures;

    static Point[] aiPosition;

    public GameObject[]   setBoard;
    static GameObject[][] board;

    static long minimaxStart;
    static long minimaxDuration;

    void Start()
    {
        if (GameModeController.IsHumanVsAI() == true)
        {
            if (GameModeController.IsEasy() == true)
                MaxMinimaxDuration = Easy;
            else if (GameModeController.IsMedium() == true)
                MaxMinimaxDuration = Medium;
            else if (GameModeController.IsHard() == true)
                MaxMinimaxDuration = Hard;
        }

        if (GameModeController.IsPlayer1AI() == true)
        {
            aiPlayer = Turn.Player1;
            huPlayer = Turn.Player2;
        }
        else if (GameModeController.IsPlayer2AI() == true)
        {
            aiPlayer = Turn.Player2;
            huPlayer = Turn.Player1;
        }

        if (GameModeController.IsAIVsAI() == true)
        {
            aiPlayer = Turn.Player1;
            huPlayer = Turn.Player2;
        }

        aiFigures = GameObject.FindGameObjectsWithTag(aiPlayer.ToString());
        huFigures = GameObject.FindGameObjectsWithTag(huPlayer.ToString());

        minimaxStart    = 0;
        minimaxDuration = 0;

        aiPosition = new Point[2];
        for (int i = 0; i < 2; i++)
            aiPosition[i] = new Point(0, 0);

        InitializeBoard();

        //ukoliko je Player1 AI treba da se odigra startup ovde
        if (GameModeController.IsHumanVsAI() == true && aiPlayer == Turn.Player1)
        {
            Startup();
        }
    }

    public static void CheckIfOnMove(bool isBuilding, States prevState)
    {
        if (GameModeController.IsHumanVsAI() == true)
        {
            if (isBuilding == true)
            {
                if (aiPlayer == GameController.onMove)
                {
                    Move();
                }
            }
            else //(isMoving == true)
            {
                if (aiPlayer == Turn.Player2 && GameController.currentState == States.StartupSecondFirst)
                {
                    Startup();
                }
                else if (aiPlayer == Turn.Player1 && prevState == States.StartupSecondSecond)
                {
                    Move();
                }
            }
        }
    }

    static Tile[][] SetupMockBoard()
    {
        Tile[][] mockBoard = new Tile[RowCount][];

        for (int i = 0; i < RowCount; i++)
        {
            mockBoard[i] = new Tile[ColCount];

            for (int j = 0; j < ColCount; j++)
            {
                TileController tileController = board[i][j].GetComponent<TileController>();

                mockBoard[i][j] = new Tile(tileController.currentLevel, tileController.isOccupied);
            }
        }

        return mockBoard;
    }


    static Point[] GetNeighbouringTiles(Tile[][] mockBoard, int row, int col) 
    {
        List<Point> neighbouringTiles = new List<Point>();

        for (int i = row - 1; i < row + 2; i++)
        {
            if (i > -1 && i < RowCount)
            {
                for (int j = col - 1; j < col + 2; j++)
                {
                    if (j > -1 && j < ColCount)
                    {
                        if (i != row || j != col)
                            neighbouringTiles.Add(new Point(i, j));
                    }
                }
            }
        }

        return neighbouringTiles.ToArray();
    }


    static bool IsGameOver(Tile[][] mockBoard, Point[] figures)
    {
        //proverava se samo u Move delu poteza, jer na osnovu pravila igre čim se igrač pomerio sa Tile na neki drugi susedni
        //znači da ima mogućnost da gradi barem na tom Tile-u na kom je bio pre pomeranja => na tom Tile-u nije bio roof
        //testiram da li je GameOver na osnovu blokiranja
        //proverava da li igrač koji je na potezu može da načini potez, ako ne može protivnik je pobedio

        for (int i = 0; i < figures.Length; i++)
        {
            if (IsMovingPossible(mockBoard, figures[i]) == true)
                return false;
        }

        return true;
    }


    static bool IsMovingPossible(Tile[][] mockBoard, Point figure)
    {
        Point[] neighbouringTiles = GetNeighbouringTiles(mockBoard, figure.row, figure.col);

        for (int i = 0; i < neighbouringTiles.Length; i++)
        {
            Tile  currentTile       = mockBoard[figure.row][figure.col];
            Point indices           = neighbouringTiles[i];
            Tile  neighbouringTile  = mockBoard[indices.row][indices.col];

            if (currentTile.CanMoveOnTo(neighbouringTile) == true)
                return true;
        }

        return false;
    }


    static void PrintBoard(ref Tile[][] board)
    {
        for (int i = 0; i < board.Length; i++)
        {
            for (int j = 0; j < board[i].Length; j++)
            {
                Tile tile = board[i][j];

                Debug.Log("Tile[" + i + "][" + j + "]: currentLevel - " + tile.currentLevel + "; isOccupied - " + tile.isOccupied);
            }
        }
    }


    static int Evaluate(ref Tile[][] board, Point[] aiFigures, Point[] huFigures, Point movedOn, Point builtOn)
    {
        //m - current level polja na koje sam se pomerio
        //l - current level - 1 polja na kojem sam gradio pomnožen razlikom rastojanja sopstvenih i protivničkih igrača od tog polja

        Tile tileMovedOn = board[movedOn.row][movedOn.col];
        Tile tileBuiltOn = board[builtOn.row][builtOn.col];

        int movedOnLevel = tileMovedOn.currentLevel;
        int builtOnLevel = tileBuiltOn.currentLevel - 1;

        int aiDistance = 0;
        for (int i = 0; i < aiFigures.Length; i++)
            aiDistance += aiFigures[i].DistanceFrom(builtOn);

        int huDistance = 0;
        for (int i = 0; i < huFigures.Length; i++)
            huDistance += huFigures[i].DistanceFrom(builtOn);

        //Debug.Log ispiši evaluaciju

        return movedOnLevel + builtOnLevel * (huDistance - aiDistance); //modifikovao sam na huDistance - aiDistance, po tekstu bi trebalo obrnuto
    }


    static Move Minimax(ref Tile[][] mockBoard, Point[] humanFigures, Point[] aiFigures, OnTurn onTurn, int treeDepth)
    {
        //TODO: kultiviši malo ovu funkciju, prevelika je
        Point[] figures      = (onTurn == OnTurn.AIPlayer ? aiFigures    : humanFigures);
        Point[] enemyFigures = (onTurn == OnTurn.AIPlayer ? humanFigures : aiFigures);

        List<Move> moves = new List<Move>();

        for (int i = 0; i < figures.Length; i++) //move deo poteza
        {
            Point   figure            = figures[i];
            Point   prevPos           = new Point(figure.row, figure.col);
            Tile    currentTile       = mockBoard[figure.row][figure.col];
            Point[] neighbouringTiles = GetNeighbouringTiles(mockBoard, figure.row, figure.col);

            for (int j = 0; j < neighbouringTiles.Length; j++)
            {
                Point indices            = neighbouringTiles[j];
                Tile  neighbouringTile   = mockBoard[indices.row][indices.col];

                if (currentTile.CanMoveOnTo(neighbouringTile) == true)
                {
                    //Move on to tile
                    mockBoard[figure.row][figure.col].isOccupied = false;
                    mockBoard[indices.row][indices.col].isOccupied = true;
                    figure.row = indices.row;
                    figure.col = indices.col;

                    Tile buildCurrentTile = mockBoard[figure.row][figure.col];
                    Point[] buildNeighbouringTiles = GetNeighbouringTiles(mockBoard, figure.row, figure.col);

                    for (int k = 0; k < buildNeighbouringTiles.Length; k++)
                    {
                        Point buildIndices = buildNeighbouringTiles[k];
                        Tile buildNeighbouringTile = mockBoard[buildIndices.row][buildIndices.col];

                        if (buildCurrentTile.CanBuildOn(buildNeighbouringTile) == true)
                        {
                            Move move = new Move(i, indices, buildIndices);

                            //Build on tile
                            buildNeighbouringTile.currentLevel++;

                            minimaxDuration = DateTime.Now.Ticks * 100 - minimaxStart;

                            if (minimaxDuration > MaxMinimaxDuration)
                            {
                                move.treeDepth = treeDepth;
                                move.result = Evaluate(ref mockBoard, aiFigures, humanFigures, indices, buildIndices);
                            }
                            else if (IsGameOver(mockBoard, enemyFigures) == true || mockBoard[figure.row][figure.col].currentLevel >= 3)
                            {
                                move.treeDepth = treeDepth;
                                move.result = (onTurn == OnTurn.AIPlayer ? Int32.MaxValue : Int32.MinValue); //trebao bih da imam u vidu treeDepth

                                //Reverse build
                                buildNeighbouringTile.currentLevel--;

                                //Reverse move
                                mockBoard[figure.row][figure.col].isOccupied = false;

                                figure.row = prevPos.row;
                                figure.col = prevPos.col;

                                mockBoard[figure.row][figure.col].isOccupied = true;


                                return move;
                            }
                            else
                            {
                                Move tempMove = Minimax(ref mockBoard, humanFigures, aiFigures, onTurn == OnTurn.AIPlayer ? OnTurn.HumanPlayer : OnTurn.AIPlayer, treeDepth + 1);

                                move.treeDepth = tempMove.treeDepth;
                                move.result = tempMove.result;
                            }

                            moves.Add(move);

                            //Reverse build
                            buildNeighbouringTile.currentLevel--;

                            //uslov za break
                            if (minimaxDuration > MaxMinimaxDuration)
                                break;
                        }
                    }

                    //Reverse move
                    mockBoard[figure.row][figure.col].isOccupied = false;

                    figure.row = prevPos.row;
                    figure.col = prevPos.col;

                    mockBoard[figure.row][figure.col].isOccupied = true;

                    //uslov za break
                    if (minimaxDuration > MaxMinimaxDuration)
                        break;
                }
            }
        }

        //ovde treba da se prodje kroz listu poteza i da se odabere najbolji u zavisnosti od toga da li je trenutno na potezu AI ili human
        Move bestMove = moves[0];
        foreach (Move move in moves)
        {
            //tražim
            if (onTurn == OnTurn.AIPlayer)
            {
                //tražim move sa maksimalnim result-om
                if (move.result > bestMove.result)
                {
                    bestMove = move;
                }
                else if (move.result == bestMove.result && move.treeDepth < bestMove.treeDepth)
                {
                    bestMove = move;
                }
            }
            else if (onTurn == OnTurn.HumanPlayer)
            {
                //tražim move sa minimalnim result-om
                if (move.result < bestMove.result)
                {
                    bestMove = move;
                }
                else if (move.result == bestMove.result && move.treeDepth < bestMove.treeDepth)
                {
                    bestMove = move;
                }
            }
        }

        return bestMove;
    }

    static void SetupFigure(GameObject aiFigure, Point aiPosition)
    {
        System.Random rand = new System.Random();
        int i = rand.Next(5);
        int j = rand.Next(5);

        PlayerController aiController   = aiFigure.GetComponent<PlayerController>();
        GameObject       tileToMoveOn   = board[i][j];
        TileController   tileController = tileToMoveOn.GetComponent<TileController>();

        while (tileController.isOccupied == true)
        {
            i = rand.Next(5);
            j = rand.Next(5);

            tileToMoveOn = board[i][j];
            tileController = tileToMoveOn.GetComponent<TileController>();
        }

        GameController.currentPlayer = aiFigure;
        aiController.Move(tileToMoveOn);

        aiPosition.row = i;
        aiPosition.col = j;
    }

    public static void Startup() //ukoliko je drugi igrač AI napravi tako da se spawn-uju 
    {
        //napraviću ovo potpuno random, provera mora da postoji da li je polje zauzeto ili ne
        //TODO: koriguj malo mesta na kojima se spawn-uju figure, ako budeš imao vremena

        for (int i = 0; i < aiFigures.Length; i++)
        {
            SetupFigure(aiFigures[i], aiPosition[i]);
        }

        //nakon ovoga swap
        if (GameModeController.IsAIVsAI() == true)
        {
            GameObject[] temp = aiFigures;

            aiFigures = huFigures;
            huFigures = temp;

            aiPosition = GetFigureCoords(aiFigures);
        }
    }


    static Point[] GetFigureCoords(GameObject[] figures)
    {
        Point[] points = new Point[figures.Length];

        for (int i = 0; i < figures.Length; i++)
        {
            Vector3 coords3D = figures[i].transform.position;

            points[i] = new Point(Convert.ToInt32(coords3D.z), Convert.ToInt32(coords3D.x));
        }

        return points;
    }


    public static void Move()
    {
        Tile[][] mockBoard = SetupMockBoard();

        Point[] humanFigures = GetFigureCoords(huFigures);

        minimaxStart = DateTime.Now.Ticks * 100; //in nanoseconds

        Move move = Minimax(ref mockBoard, humanFigures, aiPosition, OnTurn.AIPlayer, 0);

        minimaxDuration = DateTime.Now.Ticks * 100 - minimaxStart; //in nanoseconds

        Debug.Log("Minimax duration: " + minimaxDuration / 1000000000f); //prints duration in seconds

        //Izvršava se potez koji je algoritam izabrao kao optimalan
        Point toMoveOn  = move.whereToMoveOn;
        Point toBuildOn = move.whereToBuildOn;
        int   toMove    = move.figure;

        GameObject tileToMoveOn  = board[toMoveOn.row][toMoveOn.col];
        GameObject tileToBuildOn = board[toBuildOn.row][toBuildOn.col];

        GameController.currentPlayer  = aiFigures[toMove];
        PlayerController aiController = aiFigures[toMove].GetComponent<PlayerController>();
        aiController.Move(tileToMoveOn);
        aiController.Build(tileToBuildOn);


        //treba da se apdejtuje pozicija aiPosition
        aiPosition[toMove].row = toMoveOn.row;
        aiPosition[toMove].col = toMoveOn.col;

        //nakon ovoga swap-a
        if (GameModeController.IsAIVsAI() == true)
        {
            GameObject[] temp = aiFigures;

            aiFigures = huFigures;
            huFigures = temp;

            aiPosition = GetFigureCoords(aiFigures);
        }
    }

    //helper function
    void InitializeBoard()
    {
        board = new GameObject[RowCount][];
        for (int i = 0; i < RowCount; i++)
        {
            board[i] = new GameObject[ColCount];

            for (int j = 0; j < ColCount; j++)
            {
                board[i][j] = setBoard[ColCount * i + j];
            }
        }
    }
}
