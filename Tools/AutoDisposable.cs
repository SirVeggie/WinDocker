using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apprentice.Tools {
    /// <summary>Inherits <see cref="IDisposable"/> and automatically calls Dispose when garbage collected.</summary>
    public abstract class AutoDisposable : IDisposable {
        protected bool disposed;

        public void Dispose() {
            if (!disposed) {
                disposed = true;
                Dispose_();
            }
        }

        public abstract void Dispose_();
        ~AutoDisposable() => Dispose();
    }
}
