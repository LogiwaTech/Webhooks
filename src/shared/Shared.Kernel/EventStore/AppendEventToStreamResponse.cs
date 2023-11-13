namespace Shared.Kernel.EventStore;

public class AppendEventToStreamResponse
{
    public AppendEventToStreamResponse(ulong preparePosition, ulong commitPosition)
    {
        PreparePosition = preparePosition;
        CommitPosition = commitPosition;
    }

    public ulong CommitPosition { get;  }
    
    public ulong PreparePosition { get;  }
}