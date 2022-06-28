using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using app.Components;
using app.Models;
using Microsoft.Extensions.Logging;

namespace app.Services
{
    public interface IMoveService
    {
        List<Move> GetLegalMoves(BoardState state, Position from);
        bool CheckChessMate(BoardState state, ColorEnum color);
        bool CanBeCaptured(BoardState state, Position pos);
        BoardState Move(BoardState state, Position @from, Position to);
        IEnumerable<Move> MoveHistory { get; }
    }

    public class MoveService : BaseService<MoveService>, IMoveService
    {
        internal readonly List<Move> _moveHistory = new();

        public MoveService(ICacher cacher,
            ILogger<MoveService> logger, IPerfService perfService) : base(cacher, logger, perfService)
        {
        }

        public IEnumerable<Move> MoveHistory => _moveHistory;

        public List<Move> GetLegalMoves(BoardState state, Position from)
        {
            // var cacheKey = new[] {state.Fen, from.ToString()};
            // var cached = Cacher.Get<List<Move>>(cacheKey);
            // if (cached.HasValue) return cached.Value;

            PerfService.TimeStart();
            var moves = GetMovesFrom(state, from);
            AddSpecialMoves(state, from, moves);
            // TODO: if king is under chess, then we are only interested of moves that stop that, so lets get those moves
            if (moves.Moves.Count == 0) return moves.Moves.Select(x => new Move(x.From, x.To)).ToList();
            var legalMoves = FilterOutIllegalMoves(state, moves);

            PerfService.TimeStop();
            // Cacher.Set(legalMoves, cacheKey);
            return legalMoves;
        }

        public bool CheckChessMate(BoardState state, ColorEnum color)
        {
            // PERF: could be cached?
            var pieces = state.GetPieces(color);
            var res = pieces.All(piece => !GetLegalMoves(state, piece.Pos).Any());
            // Logger.LogInformation($"[CheckChessMate]: {color}-{res}-{state.Fen}");
            return res;
        }

        public bool CanBeCaptured(BoardState state, Position pos) => CanBeCaptured(state, pos, state.WhosTurn);
        internal bool CanBeCaptured(BoardState state, Position pos, ColorEnum color)
        {
            Debug.Assert(state != null);
            Debug.Assert(pos != null);
            Debug.Assert(color != null);
            var who = color;
            var pieceColor = state[pos].Value.ToColorFromPiece();
            // var cacheKey = new[] {state.Fen, pos.ToString(), who.ToString()};
            // var cached = Cacher.Get<bool>(cacheKey);
            // if (cached.HasValue) return cached.Value;

            PerfService.TimeStart();
            var kingPos = pos;

            // check knight positions
            foreach (var offset in knightOffsets)
            {
                var piece = state[kingPos + offset];
                if (piece != null && piece.Value.ToColorFromPiece() != pieceColor &&
                    piece.Value.ToEnum() == PieceEnum.Knight)
                {
                    PerfService.TimeStop();
                    // Cacher.Set(true, new[] {state.Fen, pos.ToString(), who.ToString()});
                    return true;
                }
            }

            // check king
            foreach (var offset in kingOffsets)
            {
                var piece = state[kingPos + offset];
                if (piece != null && piece.Value.ToColorFromPiece() != who &&
                    piece.Value.ToEnum() == PieceEnum.King)
                {
                    PerfService.TimeStop();
                    // Cacher.Set(true, new[] {state.Fen, pos.ToString(), who.ToString()});
                    return true;
                }
            }

            // check horizontal / vertical
            foreach (var offset in rookDirections)
            {
                var current = kingPos;
                while (!Helper.OutOfBounds(current + offset))
                {
                    current += offset;
                    var piece = state[current];
                    if (piece == null) continue;
                    if (piece.Value.ToColorFromPiece() == pieceColor) break; // one of mine is blocking view
                    if (piece.Value.ToColorFromPiece() != who &&
                        (piece.Value.ToEnum() == PieceEnum.Queen || piece.Value.ToEnum() == PieceEnum.Rook))
                    {
                        PerfService.TimeStop();
                        // Cacher.Set(true, new[] {state.Fen, pos.ToString(), who.ToString()});
                        return true;
                    }


                    break;
                }
            }

            // check diag
            foreach (var offset in bishopDirections)
            {
                var current = kingPos;
                while (!Helper.OutOfBounds(current + offset))
                {
                    current += offset;
                    var piece = state[current];
                    if (piece == null) continue;
                    if (piece.Value.ToColorFromPiece() == pieceColor) break; // one of mine is blocking view
                    if (piece.Value.ToColorFromPiece() != who &&
                        (piece.Value.ToEnum() == PieceEnum.Queen || piece.Value.ToEnum() == PieceEnum.Bishop))
                    {
                        PerfService.TimeStop();
                        // Cacher.Set(true, new[] {state.Fen, pos.ToString(), who.ToString()});
                        return true;
                    }

                    break;
                }
            }

            // check pawn
            var pawnDirection = who == ColorEnum.Black ? 1 : -1;
            foreach (var offset in PawnOffsets.Invoke(pawnDirection).Where(x => x.Col != 0)
            ) // pawns can't capture straigt forward
            {
                var current = kingPos;
                if (Helper.OutOfBounds(current + offset)) continue;

                current += offset;
                var piece = state[current];
                if (piece == null) continue;
                if (piece.Value.ToColorFromPiece() == who) continue; // one of mine is blocking view
                if (piece.Value.ToColorFromPiece() != who && piece.Value.ToEnum() == PieceEnum.Pawn)
                {
                    PerfService.TimeStop();
                    // Cacher.Set(true, new[] {state.Fen, pos.ToString(), who.ToString()});
                    return true;
                }
            }

            PerfService.TimeStop();
            // Cacher.Set(false, new[] {state.Fen, pos.ToString(), who.ToString()});
            return false;
        }

        public BoardState Move(BoardState state, Position from, Position to)
        {
            _moveHistory.Add(new Move(from, to));
            // check here if "en passant"
            if (IsPassant(state, from, to))
            {
                var r = state.WhosTurn == ColorEnum.Black ? -1 : 1;
                state.Capture(new(to.Col, to.Row + r));
            }

            var newState = state.Move(from, to);


            // check if castling
            if (newState[to] != null)
            {
                var colDir = to.Col - from.Col;
                if (newState[to].Value.ToEnum() == PieceEnum.King && Math.Abs(from.Col - to.Col) > 1)
                {
                    if (colDir < 0)
                        newState.MoveWithState(new(1, @from.Row), new(4, @from.Row));
                    else
                        newState.MoveWithState(new(8, @from.Row), new(6, @from.Row));
                }
            }
            else
            {
                Logger.LogWarning("Could not check for castling! No piece!");
            }


            return newState;
        }

        internal bool IsPassant(BoardState state, Position from, Position to)
        {
            if (state[from].Value.ToEnum() != PieceEnum.Pawn) return false;
            if (from.Row != 4 && from.Row != 5) return false; 
            
            var colD = from.Col - to.Col;
            var rowD = from.Row - to.Row;
            var p = state[new(from.Col - colD, from.Row)];
            if (p != null && p.Value.ToColorFromPiece() != state.WhosTurn && 
                p.Value.ToEnum() == PieceEnum.Pawn) return true;
            return false;
        }


        private void AddSpecialMoves(BoardState state, Position from, (char Piece, List<PieceMove> Moves) moves)
        {
            switch (moves.Piece)
            {
                case 'p':
                    if (from.Row == 2 && state[new(from.Col, 3)] == null)
                    {
                        moves.Moves.Add(new PieceMove(from, from + new Position(0, 2), true, false));
                    }

                    // CLEANUP: refactor this together with the one below
                    if (from.Row == 5 && _moveHistory.Any())
                    {
                        var lastMove = _moveHistory.Last();
                        if (lastMove.From.Row == 7 && lastMove.To.Row == 5 && lastMove.To.Col == @from.Col - 1)
                            moves.Moves.Add(new PieceMove(from, from + new Position(-1, 1), true, false));
                        if (lastMove.From.Row == 7 && lastMove.To.Row == 5 && lastMove.To.Col == @from.Col + 1)
                            moves.Moves.Add(new PieceMove(from, from + new Position(1, 1), true, false));
                    }


                    break;
                case 'P':
                    if (from.Row == 7 && state[new(from.Col, 6)] == null)
                    {
                        moves.Moves.Add(new PieceMove(from, from + new Position(0, -2), true, false));
                    }

                    if (from.Row == 4 && _moveHistory.Any())
                    {
                        var lastMove = _moveHistory.Last();
                        if (lastMove.From.Row == 2 && lastMove.To.Row == 4 && lastMove.To.Col == @from.Col - 1)
                            moves.Moves.Add(new PieceMove(from, from + new Position(-1, -1), true, false));
                        if (lastMove.From.Row == 2 && lastMove.To.Row == 4 && lastMove.To.Col == @from.Col + 1)
                            moves.Moves.Add(new PieceMove(from, from + new Position(1, -1), true, false));
                    }

                    break;
            }
        }

        internal List<Move> FilterOutIllegalMoves(BoardState state, (char Piece, List<PieceMove> Moves) moves)
        {
            var (piece, pieceMoves) = moves;
            // var cacheKey = new[] {state.Fen, pieceMoves.First().From.ToString()};
            // var cached = Cacher.Get<List<Move>>(cacheKey);
            // if (cached.HasValue) return cached.Value;

            PerfService.TimeStart();

            var res = new List<Move>();
            var myColor = piece.ToColorFromPiece();
            var pieceE = piece.ToEnum();
            foreach (var move in pieceMoves.Where(x => !Helper.OutOfBounds(x.To)))
            {
                var capturePiece = state[move.To];
                // Shouldn't be needed?
                if (capturePiece != null && capturePiece.Value.ToEnum() == PieceEnum.King) continue;
                if (capturePiece != null && !move.CaptureAllowed) continue;
                if (capturePiece != null && capturePiece.Value.ToColorFromPiece() == myColor) continue;
                if (capturePiece == null && !move.MoveAllowed) continue;
                if (pieceE != PieceEnum.King)
                {
                    var movedState = state.Move(move.From, move.To);
                    if (CanBeCaptured(movedState, movedState.GetPosition(myColor.ToKingChar()), myColor)) continue;
                }

                if (pieceE == PieceEnum.King)
                {
                    var movedState = state.Move(move.From, move.To);
                    if (CanBeCaptured(movedState, move.To, myColor)) continue;
                }


                res.Add(new Move(move.From, move.To));
            }

            PerfService.TimeStop();

            // Cacher.Set(res, cacheKey);
            return res;
        }

        

        internal (char Piece, List<PieceMove> Moves) GetMovesFrom(BoardState state, Position from)
        {
            // PERF: cache?
            PerfService.TimeStart();
            var piece = state[from];
            var moves = piece.Value.ToEnum() switch
            {
                PieceEnum.Bishop => GetMovesBishop(state, piece.Value.ToColorFromPiece(), from),
                PieceEnum.King => GetMovesKing(state, from),
                PieceEnum.Knight => GetMovesKnight(from),
                PieceEnum.Queen => GetMovesQueen(state, piece.Value.ToColorFromPiece(), from),
                PieceEnum.Rook => GetMovesRook(state, piece.Value.ToColorFromPiece(), from),
                PieceEnum.Pawn => GetMovesPawn(from, piece.Value.ToColorFromPiece()),
            };
            PerfService.TimeStop();
            return (piece.Value, moves);
        }


        internal List<PieceMove> GetMovesBishop(BoardState state, ColorEnum color, Position from)
        {
            var myColor = color;
            var res = new List<PieceMove>();
            foreach (var direction in bishopDirections)
            {
                var current = from;
                while (!Helper.OutOfBounds(current + direction))
                {
                    current += direction;
                    var pieceAtdest = state[current];
                    if (pieceAtdest != null)
                    {
                        if (pieceAtdest.Value.ToColorFromPiece() == myColor) break;
                        res.Add(new PieceMove(from, current));
                        break;
                    }

                    res.Add(new PieceMove(from, current));
                }
            }

            return res;
        }

        internal List<PieceMove> GetMovesKing(BoardState state, Position from)
        {
            void CheckCastling(Position haventMoved, List<PieceMove> pieceMoves, Position offset)
            {
                if (CanBeCaptured(state, from)) return;
                
                if (@from.Row == haventMoved.Row && HaventMoved(haventMoved) && LineOfSight(state, from, haventMoved))
                {
                    pieceMoves.Add(new PieceMove(@from, @from + offset, true, false));
                }
            }

            var res = new List<PieceMove>();
            foreach (var offset in kingOffsets)
            {
                // special move (castling)
                if (Math.Abs(offset.Col) > 1 && from.Col == 5)
                {
                    if (state is {WhosTurn: ColorEnum.Black})
                        CheckCastling(new(offset.Col < -1 ? 1 : 8, 1), res, offset);
                    else if (state is {WhosTurn: ColorEnum.White})
                        CheckCastling(new(offset.Col < -1 ? 1 : 8, 8), res, offset);
                }
                else if (Math.Abs(offset.Col) != 2)
                {
                    res.Add(new PieceMove(from, from + offset));
                }
            }

            return res;
        }

        private static bool LineOfSight(BoardState state, Position @from, Position to)
        {
            /*
             * No pieces should be between to two positions
             */
            var dx = to.Col - from.Col;
            var dy = from.Row - to.Row;
            var dir = new Position(dx, dy);
            var current = from;
            current += dir;
            while (current != to && !Helper.OutOfBounds(current))
            {
                if (state[current].HasValue) return false;
                current += dir;    
            }

            return true;
        }

        private bool HaventMoved(Position pos)
        {
            return _moveHistory.All(x => x.From != pos);
        }
        // kingOffsets.Select(x => new PieceMove(from, from + x)).ToList();

        internal List<PieceMove> GetMovesKnight(Position from) =>
            knightOffsets.Select(x => new PieceMove(from, from + x)).ToList();

        internal List<PieceMove> GetMovesQueen(BoardState state, ColorEnum color, Position from)
        {
            var myColor = color;
            var res = new List<PieceMove>();
            foreach (var direction in queenDirections)
            {
                var current = from;
                while (!Helper.OutOfBounds(current + direction))
                {
                    current += direction;
                    var pieceAtdest = state[current];
                    if (pieceAtdest != null)
                    {
                        if (pieceAtdest.Value.ToColorFromPiece() == myColor) break;
                        res.Add(new PieceMove(from, current));
                        break;
                    }

                    res.Add(new PieceMove(from, current));
                }
            }

            return res;
        }

        internal List<PieceMove> GetMovesRook(BoardState state, ColorEnum color, Position from)
        {
            var myColor = color;
            var res = new List<PieceMove>();
            foreach (var direction in rookDirections)
            {
                var current = from;
                while (!Helper.OutOfBounds(current + direction))
                {
                    current += direction;
                    var pieceAtdest = state[current];
                    if (pieceAtdest != null)
                    {
                        if (pieceAtdest.Value.ToColorFromPiece() == myColor) break;
                        res.Add(new PieceMove(from, current));
                        break;
                    }

                    res.Add(new PieceMove(from, current));
                }
            }

            return res;
        }

        private static readonly Func<int, List<Position>> PawnOffsets = direction => new List<Position>
        {
            new(0, direction), new(-1, direction), new(1, direction)
        };

        internal List<PieceMove> GetMovesPawn(Position from, ColorEnum color)
        {
            var moveOffset = color == ColorEnum.White ? -1 : 1;

            var offsets = PawnOffsets.Invoke(moveOffset).Select(x =>
                x.Col == 0
                    ? new PieceMove(from, from + new Position(0, x.Row), true, false)
                    : new(from, from + new Position(x.Col, x.Row), false, true));
            return offsets.ToList();
        }


        private static List<Position> bishopDirections = new()
        {
            new(-1, -1), new(1, -1), new(1, 1), new(-1, 1)
        };

        private static List<Position> kingOffsets = new()
        {
            new(-1, -1), new(0, -1), new(1, -1),
            new(-1, 0), new(1, 0),
            new(-1, 1), new(0, 1), new(1, 1),
            new(2, 0), new(-2, 0) // castling
        };

        private static List<Position> knightOffsets = new()
        {
            new(-2, -1), new(-1, -2), new(1, -2), new(2, -1),
            new(2, 1), new(1, 2), new(-1, 2), new(-2, 1)
        };

        private static List<Position> queenDirections = new()
        {
            new(-1, -1), new(1, -1), new(1, 1), new(-1, 1),
            new(0, -1), new(1, 0), new(0, 1), new(-1, 0)
        };

        private static List<Position> rookDirections = new()
        {
            new(-1, 0), new(0, -1), new(1, 0), new(0, 1)
        };


        // foreach (var direction in captureOffset)
        //     {
        //         var current = from;
        //         while (true)
        //         {
        //             var pos = current + direction;
        //             if (Helpers.OutOfBounds(pos)) break;
        //             var piece = GetPiece(state, pos);
        //             if (SameTeam(state, piece)) break;
        //             validMoves.Add((pos, piece != null ? piece.Value.ToEnum() : null));
        //             // PERF: if we send along this info, we might not need to get it later
        //             if (piece != null)
        //                 break; // added capture, so we shouldn't continue searching this direction
        //             current += direction;
        //         }
        //     }
        //     
        //     // fast
        //     // PerfService.TimeStart();
        //     if (isDirectional)
        //     {
        //         
        //
        //         return;
        //     }
        //
        //     foreach (var position in captureOffsetOrDirections)
        //     {
        //         var pos = from + position;
        //         if (OutOfBounds(pos)) continue;
        //         var piece = GetPiece(state, pos);
        //         if (SameTeam(state, piece)) continue;
        //         // can I use capture positions as moving positions?
        //         if (!canMoveOnCapture && piece == null) continue;
        //         validMoves.Add(pos);
        //     }
        //
        //     foreach (var position in movingOffsets)
        //     {
        //         var pos = from + position;
        //         if (OutOfBounds(pos)) continue;
        //         var piece = GetPiece(state, pos);
        //         if (piece != null) continue;
        //         validMoves.Add(pos);
        //     }
        //
        //     // PerfService.TimeStop();
        // }
    }
}