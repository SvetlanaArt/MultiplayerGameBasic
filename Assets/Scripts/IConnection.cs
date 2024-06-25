using System;

public interface IConnection
{
    public event Action<GameMessageType> OnConnectionEvent;
}
