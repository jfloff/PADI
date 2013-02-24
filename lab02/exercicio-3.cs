using System;

delegate void ThrWork();

class ThrPool {

	public ThrPool(int thrNum, int bufSize) {
		// TODO	
	}

	public void AssyncInvoke(ThrWork action) {
		// TODO
	}
}


class A {
	private int _id;
	
	public A(int id) {
		_id = id;
	}

	public void DoWorkA() {
		Console.WriteLine("A-{0}",_id);
	}
}


class B {
	private int _id;
	
	public B(int id) {
		_id = id;
	}

	public void DoWorkB() {
		Console.WriteLine("B-{0}",_id);
	}
}


class Test
{
    public static void Main()
    {
	    ThrPool tpool = new ThrPool(5,10);
	    ThrWork work = null;
	    for (int i = 0; i < 5; i++) {
		A a = new A(i);
	    	tpool.AssyncInvoke(new ThrWork(a.DoWorkA));
		B b = new B(i);
		tpool.AssyncInvoke(new ThrWork(b.DoWorkB));
	    }
	    Console.ReadLine();
    }
}
