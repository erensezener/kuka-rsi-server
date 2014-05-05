//////////////////////////////////////////////////////////////////////////////
///KUKA Roboter GmbH
///Bluecherstr. 144
///86165 Augsburg
///Germany
///
///Topic:          C# Code:    Server-apllication with module RSI object: ST_Ethernet
///                    Best effort: Microsoft Windows System and Console Application Projekt.            
///Date:           21. 08 2008
///Developer:  Rajko Rolke
//////////////////////////////////////////////////////////////////////////////


      private static void anyfunction()
      {   
         // starting communication by separate process
         System.Threading.Thread secondThread;
         secondThread = new System.Threading.Thread(new System.Threading.ThreadStart(StartListening));
         secondThread.Start();         
      }
      
      // second thread
      private static void StartListening() 
      {
         uint Port = 6008;                     // port number TCP/IP
         uint AddressListIdx = 0;               // network card index         
         System.Xml.XmlDocument SendXML = new System.Xml.XmlDocument();   // XmlDocument pattern
         System.Net.Sockets.Socket listener;         // create system socket
         System.Net.Sockets.Socket handler;         // create system socket
            
         // Data buffer for incoming data.
         byte[] bytes = new Byte[1024];
               
         // Establish the local endpoint for the  socket.
         // Dns.GetHostName returns the name of the 
         // host running the application.
         System.Net.IPHostEntry ipHostInfo = System.Net.Dns.Resolve(System.Net.Dns.GetHostName());
         System.Net.IPAddress ipAddress = ipHostInfo.AddressList[AddressListIdx];                        
         System.Net.IPEndPoint localEndPoint = new System.Net.IPEndPoint(ipAddress, (int)Port);            

         // Create a TCP/IP socket.
         listener = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
                        
         // open Socket and listen on network
         listener.Bind(localEndPoint);
         listener.Listen(1);

         // Program is suspended while waiting for an incoming connection.
         // bind the first request
         handler = listener.Accept();
               
         // no connections are income 
         listener.Close();      
               
         // string members for incoming and outgoing data
         String strReceive = null;
         String strSend = null;

         // load sending data by external file
         SendXML.PreserveWhitespace = true;
         SendXML.Load("ExternalData.xml");                  
                                       
         while(true)
         {                                                
            // wait for data and receive bytes
            int bytesRec = handler.Receive(bytes);
            if (bytesRec == 0)
            {                           
               break; // Client closed Socket
            }
                        
            // convert bytes to string
            strReceive = String.Concat(strReceive,System.Text.Encoding.ASCII.GetString(bytes,0,bytesRec));
                     
            // take a look to the end of data
            if ((strReceive.LastIndexOf("</Rob>")) == -1)
            {                                                
               continue;
            }
            else 
            {                                        
               // mirror the IPO counter you received yet                        
               strSend = SendXML.InnerXml;
               strSend = mirrorIPOC(strReceive,strSend);
                  
               // send data as requested 
               byte[] msg = System.Text.Encoding.ASCII.GetBytes(strSend);
               handler.Send(msg,0,msg.Length,System.Net.Sockets.SocketFlags.None);               
            }
            strReceive = null;
         }
      }
         
                     
      // send immediately incoming IPO counter to have a timestamp
      private static string mirrorIPOC(string receive, string send)
      {
         // separate IPO counter as string
         int startdummy = receive.IndexOf("<IPOC>")+6;
         int stopdummy = receive.IndexOf("</IPOC>");
         string Ipocount =  receive.Substring(startdummy,stopdummy-startdummy);                        
         
         // find the insert position      
         startdummy = send.IndexOf("<IPOC>")+6;
         stopdummy = send.IndexOf("</IPOC>");
            
         // remove the old value an insert the actualy value
         send = send.Remove(startdummy,stopdummy-startdummy);
         send = send.Insert(startdummy,Ipocount); 

         // send back the string
         return send;
      }

