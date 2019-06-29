namespace BMP2Tile
{
    /// <summary>
    /// My own exception type. Allows distinguishing error message exceptions from other bad things.
    /// </summary>
    public class AppException : System.Exception
    {
        public AppException(string message) : base(message) {}
    }
}