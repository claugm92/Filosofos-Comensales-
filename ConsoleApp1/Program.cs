using System;
using System.Threading;

public struct PhilosopherData
{
    public int IdFilosofo;
    public Mutex PalilloDerecho;
    public Mutex PalilloIzquierdo;
    public int CantidadAComer;
    public int TotalFood;
}



public class Philosopher : WorkerThread
{
    public Philosopher(object data) : base(data) { }

    protected override void Run()
    {
        //IsAlive objeto Data de la clase WorkerThread lo convierte en la estructura PhilosopherData
        PhilosopherData pd = (PhilosopherData) Data;

        //genera r en forma aleatoria entre 0 y el id del filosofo
        Random r = new Random(pd.IdFilosofo);

        //muestra un mensaje de confirmacion de creacion de filosofo
        Console.WriteLine("Philosopher {0} Listo", pd.IdFilosofo);


        /*Encapsula objetos específicos del sistema operativo que esperan acceso exclusivo a recursos compartidos.*/
        //En este caso son los palillos izquierdo y derecho de la estructura PD
        WaitHandle[] WaitHandlePalillos = new WaitHandle[] { pd.PalilloIzquierdo, pd.PalilloDerecho };


        //inicia el ciclo mientras cada uno tenga comida en su plato
        while (pd.TotalFood > 0)
        {
            //espera por la obtencion de los palillos 
            /*
             * WaitHandle.WaitAll Method
             * Espera a que todos los elementos de la matriz especificada reciban una señal.
             * 
             */
            WaitHandle.WaitAll(WaitHandlePalillos);


            //Resta la cantidad a comer 
            pd.TotalFood -= pd.CantidadAComer;

            Console.WriteLine("Filosofo {0} esta comiendo {1} de sus {2} de comida", pd.IdFilosofo, pd.CantidadAComer, pd.TotalFood);

            //lo pone a comer en un tiempo random entre 1 y 5 segundos
            Thread.Sleep(r.Next(1000, 5000));

            //Libera los palillos con el mutex
            try
            {
                pd.PalilloDerecho.ReleaseMutex();
                pd.PalilloIzquierdo.ReleaseMutex();
                

            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }


            
            //Pone a pensar el filosofo durante un tiempo aleatorio entre 1 y 5 segundos
            Thread.Sleep(r.Next(1000, 5000));
            Console.WriteLine("Filosofo {0} esta pensando", pd.IdFilosofo);
        }
        //como ya no tiene comida el filoso termino de comer
        Console.WriteLine("Filosofo {0} termino su comida", pd.IdFilosofo);
    }
}


//Clase Restaurant en donde se crean los filosofos y los palillos
public class Restaurant
{

    public static void Main()
    {
        //Crea un array de mutex de tamaño 5, un mutex por cada palillo
        Mutex[] ArrayPalillosMutex = new Mutex[5];


        //inicializa en falso cada uno de los mutex
        for (int i = 0; i < 5; i++)
            ArrayPalillosMutex[i] = new Mutex(false);

        
        //Crea los 5 filosofos
        for (int i = 0; i < 5; i++)
        {
            //cada filosofo tiene su Estructura pd
            PhilosopherData pd;

            //Asigna el id i+1 para que no empiece en 0 cada uno de los ID de los filosofos
            pd.IdFilosofo = i + 1;

            /*Conditional	x ? y : z	Evaluates to y if x is true, z if x is false*/
            // si (i - 1 >= 0 ) es verdadero entonces hace (i-1), sino 4

            /*
             * Codigo original pero se me hace que esta mal, en esta parte guarda el numero del palillo de cada uno de los 
             * filosofos en la estructura, me parece que el error esta en la asignacion del numero de palillo de la derecha
             * si seguis la secuencia se obtiene
             * (palillo izq, palillo der)
             * (0,4)
             * (1,4)
             * (2,1)
             * (3,2)
             * (4,3) 
             * 
             * pero si lo dibujas no coinciden con cada filosofo
             
            pd.PalilloDerecho = ArrayPalillosMutex[i - 1 >= 0 ? (i - 1) : 4];
            pd.PalilloIzquierdo = ArrayPalillosMutex[i];
            pd.CantidadAComer = 5;
            pd.TotalFood = 35;
            Philosopher p = new Philosopher(pd);
            p.Start();
            */

            /*yo creo que la secuencia correcta es asi*/
            pd.PalilloDerecho = ArrayPalillosMutex[i == 4 ? 0 : (i + 1)];
            pd.PalilloIzquierdo = ArrayPalillosMutex[i];
            pd.CantidadAComer = 5;
            pd.TotalFood = 35;
            Philosopher p = new Philosopher(pd);
            p.Start();
            Console.WriteLine("Filosofo {0} palillo izq:{1} palillo der{2} ",pd.IdFilosofo,i,(i == 4 ? 0 : (i + 1)));

        }

        //Console.ReadLine();
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

    /// Constructors

    public WorkerThread(object data)
    {
        this.ThreadData = data;
    }

    public WorkerThread()
    {
        ThreadData = null;
    }


    /// Public Methods



    /// Start the worker thread
    public void Start()
    {
        thisThread = new Thread(new ThreadStart(this.Run));
        thisThread.Start();
    }

    /// Stop the current thread.  Abort causes
    /// a ThreadAbortException to be raised within
    /// the thread
    public void Stop()
    {
        thisThread.Abort();
        while (thisThread.IsAlive) ;
        thisThread = null;
    }

    /// To be implemented by derived threads
    protected abstract void Run();
}
 
