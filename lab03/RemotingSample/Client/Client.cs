using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Net.Sockets;

namespace RemotingSample {

	class Client {

		static void Main() {
			TcpChannel channel = new TcpChannel();
			ChannelServices.RegisterChannel(channel,true);

			MyRemoteObject obj = (MyRemoteObject) Activator.GetObject(
				typeof(MyRemoteObject),
				"tcp://localhost:8086/MyRemoteObjectName");

	 		try
	 		{
	 			Console.WriteLine(obj.MetodoOla());
	 		}
	 		catch(SocketException)
	 		{
	 			System.Console.WriteLine("Could not locate server");
	 		}

			Console.ReadLine();
		}
	}
}