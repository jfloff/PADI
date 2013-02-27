//A thread pool ThrPool é inicializada com um conjunto de N threads.
//A aplicação submete um delegate do tipo ThrWork para execução assíncrona invocando o método AssyncInvoke.
//Os pedidos de invocação são colocados num buffer gerido de forma circular.
//Threads que estejam livres podem consumir pedidos, caso contrário ficam bloqueadas.
//Quando uma thread consome um pedido, retira-o do buffer e executa o pedidoe volta a verificar se existem pedidos pendentes.


using System;
using System.Collections;
using System.Threading;

class ThrPool
{

    int thrNum;
    ArrayList pool;

    public ThrPool(int thrNum, int bufSize)
    {
        this.thrNum = thrNum;
        pool = new ArrayList(bufSize);
    }

    public void AssyncInvoke(ThreadStart action)
    {
        Thread t = new Thread(action);
        t.Start();
        t.Join();
    }
}


class A
{
    private int _id;

    public A(int id)
    {
        _id = id;
    }

    public void DoWorkA()
    {
        Console.WriteLine("A-{0}", _id);
    }
}


class B
{
    private int _id;

    public B(int id)
    {
        _id = id;
    }

    public void DoWorkB()
    {
        Console.WriteLine("B-{0}", _id);
    }
}


class Test
{
    public static void Main()
    {
        ThrPool tpool = new ThrPool(5, 10);
        ThreadStart work = null;

        for (int i = 0; i < 5; i++)
        {
            A a = new A(i);
            tpool.AssyncInvoke(new ThreadStart(a.DoWorkA));

            B b = new B(i);
            tpool.AssyncInvoke(new ThreadStart(b.DoWorkB));
        }
        Console.ReadLine();
    }
}
