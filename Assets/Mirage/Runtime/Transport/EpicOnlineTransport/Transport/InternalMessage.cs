namespace EpicChill.Transport
{
    public enum InternalMessage : byte
    {
        Connect,
        Disconnect,
        Accept,
        Data,
        TooManyUsers
    }
}
