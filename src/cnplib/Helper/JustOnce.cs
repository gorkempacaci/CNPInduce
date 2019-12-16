using System;
namespace CNP.Helper
{
    /// <summary>
    /// A delegate wrapper that lets the contained delegate called just once. Thread-safe.
    /// </summary>
    public class JustOnce<Tin>
    {
        readonly object callLock = new object();
        bool hasBeenCalledOnce = false;
        readonly Action<Tin> invokee;
        public JustOnce(Action<Tin> del)
        {
            invokee = del;
        }
        public void Invoke(Tin input)
        {
            lock (callLock)
            {
                if (hasBeenCalledOnce)
                {
                    throw new Exception("JustOnce: the delegate has already been called once.");
                }
                else
                {
                    invokee(input);
                    hasBeenCalledOnce = true;
                }
            }
        }
    }
}
