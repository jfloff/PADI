// Ex A1 - Ver comentários no código
// Ex A2 - Ver comentários no código
// Ex A3 - Invocação de um Delegate a null irá resultar num NullPointerException
// Ex A4 - Claro, ver o exemplo actual
// Ex A5 - Não temos a garantia disso, pode o nome método interromper os métodos que
//         estavam subscritos pelo 'novo' delegates
// Ex A6 - Ver código

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
        a = new MyDelegate(Hello); // Ex A1 - Atribuição de Métodos
        // Create the delegate object b that references
        // the method Goodbye:
        b = new MyDelegate(Goodbye); // Ex A1 - Atribuição de Métodos
        // The two delegates, a and b, are composed to form c:
        c = a + b; // Ex A1 - Adição de Métodos
        // Remove a from the composed delegate, leaving d,
        // which calls only the method Goodbye:
        d = c - a; // Ex A1 - Subtração de métodos

        Console.WriteLine("Invoking delegate a:");
        a("A"); // Ex A1 - Invocação
        Console.WriteLine("Invoking delegate b:");
        b("B"); // Ex A1 - Invocação
        Console.WriteLine("Invoking delegate c:");
        c("C"); // Ex A1 - Invocação // Ex A2 - Não é garantida a ordem de invocação
        Console.WriteLine("Invoking delegate d:");
        d("D"); // Ex A1 - Invocação // Ex A2 - Não é garantida a ordem de invocação
        // Ex A6
        Console.WriteLine("Invoking method delegate:");
        callDelegate(a);

        Console.Read();
    }
}
