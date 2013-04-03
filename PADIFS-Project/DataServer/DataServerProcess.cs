﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedLibrary;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;

namespace DataServer
{
    class DataServerProcess : MarshalByRefObject, IDataServer, IDataServerPM
    {
        public static void Main(string[] args)
        {
            if (args.Length != 2)
                throw new Exception();

            TcpChannel channel = new TcpChannel(Convert.ToInt32(args[1]));
            ChannelServices.RegisterChannel(channel, true);
            Console.WriteLine(args[0]);
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(DataServerProcess),
                args[0],
                WellKnownObjectMode.Singleton);

            Console.WriteLine("Data Server " + args[0] + " Started");

            // Notify Metadata Server

            System.Console.ReadLine();
        }

        public void Freeze()
        {
            Console.WriteLine("FREEZE DATA SERVER");
        }

        public void Unfreeze()
        {
            Console.WriteLine("UNFREEZE DATA SERVER");
        }

        public void Fail()
        {
            Console.WriteLine("FAIL DATA SERVER");
        }

        public void Recover()
        {
            Console.WriteLine("RECOVER DATA SERVER");
        }

        public void Read()
        {
            Console.WriteLine("READ DATA SERVER FILE");
        }

        public void Write()
        {
            Console.WriteLine("WRITE DATA SERVER FILE");
        }
    }
}