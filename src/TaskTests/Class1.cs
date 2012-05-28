using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Tasks;

namespace TaskTests
{
    [TestFixture]
    public class Class1
    {
        [TestCase]
        public void testTask()
        {
            Console.WriteLine("starting");
            var task = Task.Factory.StartNew(() =>
                                      {
                                          Console.WriteLine("doing a task");
                                          int total = 1;
                                          var random = new Random();
                                          for (int i=0; i<= 1000; i++)
                                          {
                                              total = total + (random.Next(10 + i)+1);
                                          }
                                          Thread.Sleep(10000);
                                          Console.WriteLine("done with {0}", total);
                                      });
            task.Wait();
        }

        [TestCase]
        public void testTaskWithAsyncRead()
        {
            var webRequest = CreateWebRequest();
            Func<AsyncCallback, object, IAsyncResult> begin =
                (cb, s) => webRequest.BeginGetResponse(cb, s);
            var task = Task.Factory.FromAsync(begin, EndMethodForWebRequest(webRequest), null);
            task.Wait();
        }

        [TestCase]
        public void testContinuationTask()
        {
            var webRequest = CreateWebRequest();
            Func<AsyncCallback, object, IAsyncResult> begin =
                (cb, s) => webRequest.BeginGetResponse(cb, s);
            var task1 = Task.Factory.FromAsync(begin, EndMethodForWebRequest(webRequest), null);
            var task2 = task1.ContinueWith(task =>
                                               {
                                                   Assert.IsTrue(task.IsCompleted);
                                                   Assert.IsFalse(task.IsFaulted);
                                                   Assert.IsNull(task.Exception);
                                                   int total = 1;
                                                   var random = new Random();
                                                   for (int i = 0; i <= 1000; i++)
                                                   {
                                                       total = total + (random.Next(10 + i) + 1);
                                                   }
                                                   Console.WriteLine("done with {0}", total);
                                               }, TaskContinuationOptions.None);
            task2.Wait();
            task1.Wait();
        }

        [TestCase]
        public void testFailureContinuationTask()
        {
            var webRequest = CreateWebRequest();
            Func<AsyncCallback, object, IAsyncResult> begin =
                (cb, s) => webRequest.BeginGetResponse(cb, s);
            var task1 = Task.Factory.FromAsync(begin, result => { throw new Exception("haha, cant catch me"); }, null);
            var didEverything = false;
            var task2 = task1.ContinueWith(task =>
                                               {
                                                   Assert.IsFalse(task.IsCompleted);
                                                   Assert.IsTrue(task.IsFaulted);
                                                   Assert.IsNotNull(task.Exception);
                                                   Console.WriteLine(task.Exception);
                                                   int total = 1;
                                                   var random = new Random();
                                                   for (int i = 0; i <= 1000; i++)
                                                   {
                                                       total = total + (random.Next(10 + i) + 1);
                                                   }
                                                   Console.WriteLine("done with {0}", total);
                                                   didEverything = true;
                                               }, TaskContinuationOptions.None);
            task2.Wait();
            task1.Wait();
            Assert.IsTrue(didEverything);
        }


        [TestCase]
        public void testOnFailureContinuationTask()
        {
            var webRequest = CreateWebRequest();
            Func<AsyncCallback, object, IAsyncResult> begin =
                (cb, s) => webRequest.BeginGetResponse(cb, s);
            var task1 = Task.Factory.FromAsync(begin, result => { throw new Exception("haha, cant catch me"); }, null);
            var didEverything = false;
            var task2 = task1.ContinueWith(task =>
            {
                int total = 1;
                var random = new Random();
                for (int i = 0; i <= 1000; i++)
                {
                    total = total + (random.Next(10 + i) + 1);
                }
                Console.WriteLine("done with {0}", total);
                didEverything = true;
            }, TaskContinuationOptions.OnlyOnFaulted);
            task2.Wait();
            task1.Wait();
            Assert.IsTrue(didEverything);
        }

        [TestCase]
        public void testSuccessfulContinuationTask()
        {
            var webRequest = CreateWebRequest();
            Func<AsyncCallback, object, IAsyncResult> begin =
                (cb, s) => webRequest.BeginGetResponse(cb, s);
            var task1 = Task.Factory.FromAsync(begin, EndMethodForWebRequest(webRequest), null);
            var didEverything = false;
            var task2 = task1.ContinueWith(task =>
            {
                int total = 1;
                var random = new Random();
                for (int i = 0; i <= 1000; i++)
                {
                    total = total + (random.Next(10 + i) + 1);
                }
                Console.WriteLine("done with {0}", total);
                didEverything = true;
            }, TaskContinuationOptions.NotOnFaulted);
            task2.Wait();
            task1.Wait();
            Assert.IsTrue(didEverything);
        }

        private static Action<IAsyncResult> EndMethodForWebRequest(HttpWebRequest webRequest)
        {
            return ar =>
                       {
                           var response = webRequest.EndGetResponse(ar);
                           Stream responseStream = response.GetResponseStream();
                           Console.WriteLine("\nThe contents of the Html page are : ");
                           Encoding encode = Encoding.GetEncoding("utf-8");
                           StreamReader readStream = new StreamReader(responseStream, encode);
                           Char[] read = new Char[256];
                           int count = readStream.Read(read, 0, 256);
                           Console.WriteLine("HTML...\r\n");
                           while (count > 0)
                           {
                               // Dumps the 256 characters on a string and displays the string to the console.
                               var str = new String(read, 0, count);
                               Console.Write(str);
                               count = readStream.Read(read, 0, 256);
                           }
                           Console.WriteLine("");
                       };
        }

        private static HttpWebRequest CreateWebRequest()
        {
            return (HttpWebRequest) System.Net.WebRequest.Create("http://www.bing.com");
        }

    }


}
