namespace Connr.Process;

public interface IProcessContainer
{
    string Id { get; }
    Parameters Parameters { get; }
    Events Events { get; }
    IResult Result { get; set; }
    ProcessState State { get; }
    internal Tokens Tokens { get; }
    Statistics Statistics { get; }
    public void Start();
    public void Stop();
    public void Kill();
}