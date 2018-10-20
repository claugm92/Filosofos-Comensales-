/*
Quote from 

C# and the .NET Framework
by Bob Powell 

*/
using System;
using System.Threading;

public struct PhilosopherData
{
    public int PhilosopherId;
    public Mutex RightChopStick;
    public Mutex LeftChopStick;
    public int AmountToEat;
    public int TotalFood;
}



public class Philosopher : WorkerThread
{
    public Philosopher(object data) : base(data) { }

    protected override void Run()
    {
        PhilosopherData pd = (PhilosopherData)Data;
        Random r = new Random(pd.PhilosopherId);
        Console.WriteLine("Philosopher {0} ready", pd.PhilosopherId);
        WaitHandle[] chopSticks = new WaitHandle[] { pd.LeftChopStick, pd.RightChopStick };

        while (pd.TotalFood > 0)
        {
            //Get both chop sticks
            WaitHandle.WaitAll(chopSticks);
            Console.WriteLine("Philosopher {0} eating {1} of {2} food", pd.PhilosopherId, pd.AmountToEat, pd.TotalFood);
            pd.TotalFood -= pd.AmountToEat;
            Thread.Sleep(r.Next(1000, 5000));

            //Release the chopsticks
            Console.WriteLine("Philosopher {0} thinking", pd.PhilosopherId);
            pd.RightChopStick.ReleaseMutex();
            pd.LeftChopStick.ReleaseMutex();

            //Think for a random time length
            Thread.Sleep(r.Next(1000, 5000));
        }
        Console.WriteLine("Philosopher {0} finished", pd.PhilosopherId);
    }
}


public class Restaurant
{

    public static void Main()
    {
        Mutex[] chopSticks = new Mutex[5];

        //init the chopSticks
        for (int i = 0; i < 5; i++)
            chopSticks[i] = new Mutex(false);

        //Create the Five Philosophers
        for (int i = 0; i < 5; i++)
        {
            PhilosopherData pd;
            pd.PhilosopherId = i + 1;
            pd.RightChopStick = chopSticks[i - 1 >= 0 ? (i - 1) : 4];
            pd.LeftChopStick = chopSticks[i];
            pd.AmountToEat = 5;
            pd.TotalFood = 35;
            Philosopher p = new Philosopher(pd);
            p.Start();
        }

        Console.ReadLine();
    }
}
public abstract class WorkerThread
{

    private object ThreadData;
    private Thread thisThread;


    //Properties
    public object Data
    {
        get { return ThreadData; }
        set { ThreadData = value; }
    }

    public object IsAlive
    {
        get { return thisThread == null ? false : thisThread.IsAlive; }
    }

    /// <summary>
    /// Constructors
    /// </summary>

    public WorkerThread(object data)
    {
        this.ThreadData = data;
    }

    public WorkerThread()
    {
        ThreadData = null;
    }

    /// <summary>
    /// Public Methods
    /// </summary>

    /// <summary>
    /// Start the worker thread
    /// </summary>
    public void Start()
    {
        thisThread = new Thread(new ThreadStart(this.Run));
        thisThread.Start();
    }

    /// <summary>
    /// Stop the current thread.  Abort causes
    /// a ThreadAbortException to be raised within
    /// the thread
    /// </summary>
    public void Stop()
    {
        thisThread.Abort();
        while (thisThread.IsAlive) ;
        thisThread = null;
    }

    /// <summary>
    /// To be implemented by derived threads
    /// </summary>
    protected abstract void Run();
}