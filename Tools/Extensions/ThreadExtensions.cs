using System.Threading;

namespace Apprentice.Tools.Extensions {
    public static class ThreadExtensions {

        public static void RunStaJoin(this Thread thread) {
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }

        public static Thread RunSTA(this Thread thread) {
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return thread;
        }
    }
}
