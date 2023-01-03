namespace Connr.Process;

public enum ProcessState
{
    NotStarted,
    
        Starting,
        
            Running,
                Stopping,
                Killing,
                
                    Ended,
                        EndedSuccess,
                        EndedError
}