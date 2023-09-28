using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CodeExamples.Lib {

    /**
     * This class is used to wrap a given method call and callback in a thread.  The thread is executed when start is called.
     * The callback is called once the given method is finished executing.
     */
    public class ThreadedExecuter<T> where T : class {
        public delegate void CallBackDelegate(T returnValue);
        public delegate T MethodDelegate();
        private CallBackDelegate callback;
        private MethodDelegate method;

        private Thread t;

        /**
         * method parameter is the method that will execute on the separate thread, and callback is the method that will 
         * be called when the method parameter is returned.
         * 
         * Whatever the method parameter returns is passed as a parameter to the callback method.
         */
        public ThreadedExecuter(MethodDelegate method, CallBackDelegate callback) {
            this.method = method;
            this.callback = callback;
            t = new Thread(this.Process);
        }

        /**
         * start the thread
         */
        public void Start() {
            t.Start();
        }

        /**
         *abort the thread  - the callback method is called with null for the parameters.
         */
        public void Abort() {
            t.Abort();
            callback(null); 
        }

        /**
         * call the method parameter, and return the data from the method call and use it as a parameter in the callback.
         */
        private void Process() {
            T stuffReturned = method();
            callback(stuffReturned);
        }
    }
}
