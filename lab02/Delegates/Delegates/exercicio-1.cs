// Ex A1 - Ver coment�rios no c�digo
// Ex A2 - Ver coment�rios no c�digo
// Ex A3 - Invoca��o de um Delegate a null ir� resultar num NullPointerException
// Ex A4 - Claro, ver o exemplo actual
// Ex A5 - N�o temos a garantia disso, pode o nome m�todo interromper os m�todos que
//         estavam subscritos pelo 'novo' delegates
// Ex A6 - Ver c�digo

using System;

delegate void MyDelegate(string s);

class MyClass
{
    public static void Hello(string s)
    {
        Console.WriteLine("  Hello, {0}!", s);
    }

    public static void Goodbye(string s)
    {
        Console.WriteLine("  Goodbye, {0}!", s);
    }

    public static void callDelegate(MyDelegate d)
    {
        d("I'm inside a method! MADNESS!");
    }

    public static void Main()
    {
        MyDelegate a, b, c, d;

        // Create the delegate object a that references
        // the method Hello:
        a = new MyDelegate(Hello); // Ex A1 - Atribui��o de M�todos
        // Create the delegate object b that references
        // the method Goodbye:
        b = new MyDelegate(Goodbye); // Ex A1 - Atribui��o de M�todos
        // The two delegates, a and b, are composed to form c:
        c = a + b; // Ex A1 - Adi��o de M�todos
        // Remove a from the composed delegate, leaving d,
        // which calls only the method Goodbye:
        d = c - a; // Ex A1 - Subtra��o de m�todos

        Console.WriteLine("Invoking delegate a:");
        a("A"); // Ex A1 - Invoca��o
        Console.WriteLine("Invoking delegate b:");
        b("B"); // Ex A1 - Invoca��o
        Console.WriteLine("Invoking delegate c:");
        c("C"); // Ex A1 - Invoca��o // Ex A2 - N�o � garantida a ordem de invoca��o
        Console.WriteLine("Invoking delegate d:");
        d("D"); // Ex A1 - Invoca��o // Ex A2 - N�o � garantida a ordem de invoca��o
        // Ex A6
        Console.WriteLine("Invoking method delegate:");
        callDelegate(a);

        Console.Read();
    }
}
