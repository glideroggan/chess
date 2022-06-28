#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using app.Models;
using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("tests")]

namespace app.Services
{
    public interface IAiService
    {
        // Task<Move?> CalculateMove();
        bool Working { get; set; }
        Task MakeMove();
    }

    public class AiService : BaseService<AiService>, IAiService
    {
        private static Random Rand = new Random((int) DateTime.Now.Ticks);
        private IManagerForAi _manager;
        private IMoveService _moveService;

        public AiService(IMoveService moveService,
            ICacher cacher,
            ILogger<AiService> logger, IManagerForAi manager,
            IPerfService perfService)
            : base(cacher, logger, perfService)
        {
            _manager = manager;
            _moveService = moveService;
        }

        public bool Working { get; set; }

        public async Task MakeMove()
        {
            // Logger.LogInformation($"FEN: {_manager.Fen} - {_manager.WhosTurn}");
            Debug.Assert(_manager.WhosTurn == ColorEnum.Black,
                "AI should only make moves when its his turn");
            var state = _manager.State;
            PerfService.On();
            PerfService.TimeStart();

            var info = await CalculateMove(state, 1500);

            PerfService.TimeStop();
            PerfService.LogOut();
            PerfService.Off();

            MoveScore move = null;
            // favor moves
            var goodMoves = info.Moves.Where(x => x.TotalScore > 0);
            if (goodMoves.Any())
            {
                Logger.LogInformation($"[Good move]");
                move = goodMoves.OrderByDescending(x => x.TotalScore).First();
                DoMove(move);
                return;
            }

            // no favor of each side
            var zeroMoves = info.Moves.Where(x => x.TotalScore >= 0);
            if (zeroMoves.Any())
            {
                Logger.LogInformation($"[No favor]");
                var list = zeroMoves.ToList();
                move = list[Rand.Next(list.Count)];
                DoMove(move);
                return;
            }

            // minimize damage
            var min = info.Moves
                .GroupBy(
                    x => x.TotalScore,
                    x => x,
                    (group, moves) => 
                        new {Score = group, Moves = moves.ToList()})
                .OrderByDescending(x => x.Score).First();
            move = min.Moves[Rand.Next(min.Moves.Count())];
            Logger.LogInformation($"[Minimize damage]");
            DoMove(move);
        }

        private void DoMove(MoveScore moveScore)
        {
            moveScore.LogThinking(Logger);
            _manager.Move(moveScore.Move.From, moveScore.Move.To);
        }

        internal async Task<(List<MoveScore>? Moves, Dictionary<int, int> CompletedMoves, int MaxDepth)>
            CalculateMove(BoardState currentState, int maxTimeInMs)
        {
            PerfService.TimeStart();

            var maxDepth = -1;
            List<MoveScore> graph;
            var timer = Stopwatch.StartNew();
            var completedMoves = new Dictionary<int, int>();
            while (true)
            {
                maxDepth++;
                bool done;
                graph = await RecursiveCheck(currentState, 0, maxDepth,
                    () => timer.ElapsedMilliseconds > maxTimeInMs, completedMoves, 1);
                if (graph.Count == 0) return (null, completedMoves, maxDepth);
                if (timer.ElapsedMilliseconds > maxTimeInMs && maxDepth != 0) break;
            }

            graph = graph.Select(x => new MoveScore
            {
                Move = x.Move,
                NextMove = x.NextMove,
                Score = x.Score,
                TotalScore = SumScore(x)
            }).ToList();
            Logger.LogInformation($"Simulated {completedMoves[maxDepth - 1]} moves to a depth of {maxDepth - 1}");
            
            
            PerfService.TimeStop();
            return (graph, completedMoves, maxDepth);
        }

        internal async Task<(List<MoveScore>? Moves, Dictionary<int, int> CompletedMoves, int MaxDepth)>
            InternalCalculateMove(BoardState currentState, int maxDepth)
        {
            var completedMoves = new Dictionary<int, int>();
            var graph = await RecursiveCheck(currentState, 0, maxDepth, () => false, completedMoves, 1);
            if (graph.Count == 0) return (null, completedMoves, maxDepth);

            graph = graph.Select(x => new MoveScore
            {
                Move = x.Move,
                NextMove = x.NextMove,
                Score = x.Score,
                TotalScore = SumScore(x)
            }).ToList();
            return (graph, completedMoves, maxDepth);
        }

        private int SumScore(MoveScore? moveScore)
        {
            var current = moveScore;
            var res = current?.Score ?? 0;
            while (current != null)
            {
                res += current.Score;
                current = current.NextMove;
            }

            return res;
        }

        private async ValueTask<List<MoveScore>> RecursiveCheck(BoardState state, int depth, int maxDepth,
            Func<bool> shouldWeBreak, IDictionary<int, int> completedMoves, int multiplier)
        {
            PerfService.TimeStart();
            if (!completedMoves.ContainsKey(depth)) completedMoves[depth] = 0;
            var possibleMoves = new List<MoveScore>();

            var cacheKey = new[] {state.Fen, depth.ToString()};
            var cached = Cacher.Get<List<MoveScore>>(cacheKey);
            if (cached.HasValue)
            {
                possibleMoves = cached.Value;
            }
            else
            {
                // get all pieces for current player
                var pieces = _manager.GetPieces(state).ToList();
                foreach (var legalMoves in pieces.Select(pieceInfo => _moveService.GetLegalMoves(state, pieceInfo.Pos)))
                {
                    // add initial scores for captures
                    possibleMoves.AddRange(legalMoves.Select(x => new MoveScore
                    {
                        Move = x,
                        Score = GetScore(state, x.To) * multiplier
                    }));
                }

                Cacher.Set(possibleMoves, cacheKey);
            }

            if (shouldWeBreak() || depth >= maxDepth)
            {
                PerfService.TimeStop();
                return possibleMoves;
            }

            var orderedScores = multiplier > 0
                ? possibleMoves.OrderByDescending(x => x.Score)
                : possibleMoves.OrderBy(x => x.Score);
            
            foreach (var possibleMove in orderedScores)
            {
                completedMoves[depth]++;
                var newState = state.Move(possibleMove.Move.From, possibleMove.Move.To);
                var results = await RecursiveCheck(newState, depth + 1, maxDepth, shouldWeBreak, completedMoves, multiplier*-1);
                var opponentOrderedScores = multiplier > 0
                    ? results.OrderBy(x => x.Score)
                    : results.OrderByDescending(x => x.Score);
                var bestMoveFromOpponent = opponentOrderedScores.FirstOrDefault();

                possibleMove.Score += bestMoveFromOpponent == null ? 1000 : 0; 
                possibleMove.NextMove = bestMoveFromOpponent;
                if (shouldWeBreak()) return possibleMoves;
            }

            PerfService.TimeStop();
            return possibleMoves;
        }

        private int GetScore(BoardState state, Position pos)
        {
            var p = state[pos];
            return p == null ? 0 : Score(p.Value.ToEnum());
        }

        private int Score(PieceEnum type)
        {
            return type switch
            {
                PieceEnum.Bishop => 10,
                PieceEnum.King => 100000,
                PieceEnum.Knight => 9,
                PieceEnum.Pawn => 2,
                PieceEnum.Queen => 20,
                PieceEnum.Rook => 8,
            };
        }
    }
}