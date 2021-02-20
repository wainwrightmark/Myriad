//using System;

//namespace Moggle
//{

//public record CheckTimeAction : IAction<MoggleState>
//{
//    /// <inheritdoc />
//    public MoggleState Reduce(MoggleState board)
//    {
//        if (board.FinishTime.HasValue && board.FinishTime.Value.Ticks < DateTime.Now.Ticks)
//            return board with { FinishTime = null };

//        return board;
//    }
//}

//}
