using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using app.Models;
using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("tests")]

namespace app.Services
{
    public interface IManager
    {
        List<Move> GetLegalMoves(Position from);
        char? GetPiece(Position position);
        IEnumerable<Move> GetHistory();
        bool Move(Position from, Position to);
        List<char> GetCaptures(ColorEnum who);
        event Action<Move> OnMove;
        ColorEnum GetTurn();
        void Reset();
        void Promote(PieceInfo info);
        PieceInfo? CanPromote();
        string? CheckStatus(ColorEnum color);
        bool MyTurn(Position from);
        bool CanBeCaptured(Position position);
    }

    public interface IManagerForAi
    {
        ColorEnum WhosTurn { get; }
        BoardState State { get; }
        List<PieceInfo> GetPieces(BoardState state);
        bool Move(Position from, Position to);
    }

    public class Manager : BaseService<Manager>, IManager, IManagerForAi
    {
        private BoardState _state;
        private readonly IMoveService _moveService;


        public Manager(IMoveService moveService,
            ICacher cacher, ILogger<Manager> logger,
            IPerfService perfService) : base(cacher, logger, perfService)
        {
            _moveService = moveService;
            _state = new BoardState("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w");
            // _state = new BoardState("1rbq1b1r/pppp3p/3kp/6NQ/1P2R/P1N4B/2P/3RK b");
            // _state = new BoardState("8/p1P1pk1p/6p/4B/r2N1Kn/5b/P/3q w"); // promotion white
            // _state = new BoardState("r1bqkbnr/pppppppp/2n/3P/4P/2P/PP3PPP/RNBQKBNR b"); // not solved
            // _state = new BoardState("k/p1K/P1n1p/7p/8/5b/q/8 b"); // chessmate, not solved   
        }

        public List<Move> GetLegalMoves(Position from) => GetLegalMoves(_state, from);

        internal List<Move> GetLegalMoves(BoardState state, Position from) => _moveService.GetLegalMoves(state, @from);

        public char? GetPiece(Position position) => _state[position];

        public IEnumerable<Move> GetHistory() => _moveService.MoveHistory;

        public bool MyTurn(Position from) => _state.WhosTurn == _state[from].Value.ToColorFromPiece();
        public bool CanBeCaptured(Position position) => _moveService.CanBeCaptured(_state, position);

        public bool Move(Position from, Position to)
        {
            // you can't move to same place
            if (from == to || !MyTurn(from)) return false;
            // check that the move is legal
            var moves = _moveService.GetLegalMoves(_state, from);
            if (moves.All(x => x.To != to)) return false;

            Logger.LogInformation($"[{_state.WhosTurn}] {from} -> {to}");
            var t = Move(_state, from, to);
            _state = t;

            return true;
        }

        public List<PieceInfo> GetPieces(BoardState state)
        {
            var cacheKey = new[] {state.Fen};
            var cached = Cacher.Get<List<PieceInfo>>(cacheKey);
            if (cached.HasValue) return cached.Value;

            PerfService.TimeStart();
            var res = state.GetPieces(state.WhosTurn);
            PerfService.TimeStop();

            Cacher.Set(res.ToList(), cacheKey);
            return res.ToList();
        }

        internal BoardState Move(BoardState state, Position from, Position to)
        {
            PerfService.TimeStart();
            var res = _moveService.Move(state, from, to);
            PerfService.TimeStop();
            return res;
        }

        public List<char> GetCaptures(ColorEnum who) => GetCaptures(_state, who);

        internal List<char> GetCaptures(BoardState state, ColorEnum who)
        {
            PerfService.TimeStart();
            var res = state.Captures.Where(x =>
                    who == ColorEnum.Black && char.IsLower(x) || who == ColorEnum.White && char.IsUpper(x))
                .ToList();

            PerfService.TimeStop();

            if (res.Count == 0) return new();
            var s = res.Select(x => x.ToString()).Aggregate((sum, item) => sum + "," + item);
            // Logger.LogInformation($"Returning {s}");
            return res;
        }

        public event Action<Move>? OnMove;
        public ColorEnum GetTurn() => WhosTurn;

        public void Reset() => _state = new BoardState("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w");

        public void Promote(PieceInfo info) => _state = _state.Promote(info.Pos, info.Piece);

        public PieceInfo? CanPromote() => CanPromote(_state);

        internal PieceInfo? CanPromote(BoardState state)
        {
            // TODO: as we always check promotions AFTER the move (the FEN changed)
            // we maybe should rethink this and use an event to signal for promotion?
            // FOR THIS REASON THE BELOW COLORS ARE SWITCHED!!
            // Logger.LogInformation($"Checking promotions: {state.Fen}");
            if (!state.Captures.Any(x =>
                state.WhosTurn == ColorEnum.Black && char.IsUpper(x) ||
                state.WhosTurn == ColorEnum.White && char.IsLower(x)))
            {
                // Logger.LogWarning("nothing to promote");
                return null;
            }

            // white
            if (state.WhosTurn == ColorEnum.Black)
            {
                for (var col = 1; col < 9; col++)
                {
                    var pos = new Position(col, 1);
                    var p = state[pos];
                    if (p == null) continue;
                    if (p.Value != 'P') continue;

                    return new(p.Value, pos);
                }
            }
            // black
            else
            {
                for (var col = 1; col < 9; col++)
                {
                    var pos = new Position(col, 8);
                    var p = state[pos];
                    if (p == null) continue;
                    if (p.Value != 'p') continue;

                    return new(p.Value, pos);
                }
            }

            return null;
        }

        public string? CheckStatus(ColorEnum color) => CheckStatus(_state, color);

        // private int maxDepth = -1;
        // private string savedFen;
        // public async Task CacheMoves()
        // {
        //     maxDepth++;
        //     var (res, changed) = await RecursiveCheck(_state, 0, maxDepth,
        //         () => savedFen != _state.Fen);
        //         
        //     Logger.LogInformation($"Cached down to depth {maxDepth}");
        //     if (changed)
        //     {
        //         savedFen = _state.Fen;
        //         maxDepth = 0;
        //     }
        // }

        private async ValueTask<(List<MoveScore>, bool)> RecursiveCheck(BoardState state, int depth, int maxDepth,
            Func<bool> shouldWeBreak)
        {
            await Task.Delay(100);
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
                var pieces = GetPieces(state).ToList();
                foreach (var legalMoves in pieces.Select(pieceInfo => _moveService.GetLegalMoves(state, pieceInfo.Pos)))
                {
                    // add initial scores for captures
                    possibleMoves.AddRange(legalMoves.Select(x => new MoveScore
                    {
                        Move = x,
                        Score = GetScore(state, x.To)
                    }));
                }

                Cacher.Set(possibleMoves, cacheKey);
            }

            if (shouldWeBreak() || depth >= maxDepth)
            {
                PerfService.TimeStop();
                return (possibleMoves, true);
            }

            foreach (var possibleMove in possibleMoves.OrderByDescending(x => x.Score))
            {
                var newState = state.Move(possibleMove.Move.From, possibleMove.Move.To);
                var (results, done) = await RecursiveCheck(
                    newState, depth + 1, maxDepth, shouldWeBreak);
                var bestMoveFromOpponent = results.OrderByDescending(x => x.Score).FirstOrDefault();

                possibleMove.Score -= bestMoveFromOpponent?.Score ?? -10000;
                possibleMove.NextMove = bestMoveFromOpponent;
                if (shouldWeBreak()) return (possibleMoves, true);
            }

            PerfService.TimeStop();
            return (possibleMoves, false);
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

        internal string? CheckStatus(BoardState state, ColorEnum color)
        {
            var res = _moveService.CheckChessMate(state, color);
            if (res) return color.ToString();
            return null;
        }

        public ICollection<PieceInfo> GetPieces()
        {
            return _state.GetPieces(_state.WhosTurn);
        }

        public ColorEnum WhosTurn => InternalWhosTurn(_state);
        internal ColorEnum InternalWhosTurn(BoardState state) => state.WhosTurn;
        public string Fen => _state.Fen;
        public BoardState State => _state;
    }
}