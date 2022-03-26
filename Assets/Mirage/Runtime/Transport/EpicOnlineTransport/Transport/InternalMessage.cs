namespace EpicServicesPeer
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
