using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using CNP.Helper;
using CNP.Language;
namespace CNP.Search
{
    public class ProgramSearch
    {
        readonly int programMaxHeight;
        readonly int threadCount;
        int busyThreadCount = 0;
        object busyThreadCountMonitor = new object();
        public int SearchedProgramsCount = 0;
        ConcurrentQueue<Program> searchQueue = new ConcurrentQueue<Program>();
        List<ProgramSearchThread> threadObjects;
        List<Thread> systemThreads;
        IProgramSearchReceiver searchReceiver;

        public ProgramSearch(ObservedProgram initialHole, IProgramSearchReceiver receiver, int heightLimit, int tCount = 4)
        {
            searchReceiver = receiver;
            programMaxHeight = heightLimit;
            threadCount = tCount;
            searchQueue.Enqueue(initialHole);
        }
        public void WaitUntilDone()
        {
            foreach(Thread t in systemThreads)
            {
                try
                {
                    t.Join();
                }
                catch(Exception e)
                {
                    throw e;
                }
            }
        }
        public void Start()
        {
            threadObjects = new List<ProgramSearchThread>(threadCount);
            systemThreads = new List<Thread>(threadCount);
            for (int i = 0; i < threadCount; i++)
            {
                ProgramSearchThread pst = new ProgramSearchThread(this);
                threadObjects.Add(pst);
                Thread t = new Thread(pst.Start);
                systemThreads.Add(t);
                t.Start();
            }
        }
        ///// <summary>
        ///// Blocks until all threads are successfully joined.
        ///// </summary>
        //public void Stop(Action callback)
        //{
        //    searchReceiver = null;
        //    threadObjects.ForEach(t => t.Stop());
        //    WaitUntilDone();
        //    callback?.Invoke();
        //}
        /// <summary>
        /// Returns the next program in the top of the queue. Outputs a program that is not closed.
        /// True if Take() was successfull. False if there is nothing in the queue.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the search is terminated throws InvalidOperationException.</exception>
        public bool TryTake(out Program program, out Action<IEnumerable<Program>> queueCallback)
        {
            lock (busyThreadCountMonitor)
            {
                if (searchReceiver == null)
                {
                    throw new InvalidOperationException("Search is terminated.");
                } else if (busyThreadCount == 0 && searchQueue.Count == 0)
                {
                    throw new InvalidOperationException("Search is finished.");
                } else if (searchQueue.TryDequeue(out program))
                {
                    busyThreadCount++;
                    queueCallback = new JustOnce<IEnumerable<Program>>(queue).Invoke;
                    return true;
                }
                else
                {
                    program = null;
                    queueCallback = null;
                    return false;
                }
            }
        }
        void queue(IEnumerable<Program> ps)
        {
            lock(busyThreadCountMonitor)
            {
                busyThreadCount--;
            }
            if (searchReceiver == null)
            {
                return;
            }
            foreach(Program p in ps)
            {
                if (p.IsClosed)
                {
                    searchReceiver?.FoundNewProgram(p);
                }
                else if (p.Height < programMaxHeight)
                {
                    searchQueue.Enqueue(p);
                }
            }
        }
    }
}
