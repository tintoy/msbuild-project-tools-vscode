using System;
using System.ComponentModel;

namespace MSBuildProjectTools.LanguageServer
{
    /// <summary>
    ///     Defines an object base with necessary disposable implementation.
    /// </summary>
    /// <remarks>
    ///     Override the disposing method in children classes to perform cleanup.
    ///     Implement Finalize only on objects that require finalization.
    ///     There are performance costs associated with Finalize methods.
    /// </remarks>
    public abstract class DisposableObject
        : IDisposable
    {
        /// <summary>
        ///     Has the object been disposed?
        /// </summary>
        [Browsable(false)]
        public bool IsDisposed { get; private set; }

        /// <summary>
        ///     Is the object in the process of being disposed?
        /// </summary>
        [Browsable(false)]
        public bool IsDisposing { get; private set; }

        /// <summary>
        ///     Dispose of resources being used by the object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Dispose of resources being used by the object.
        /// </summary>
        /// <param name="disposing">
        ///     Explicit disposal?
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            try
            {
                if (!IsDisposed && disposing)
                {
                    IsDisposing = true;
                    Disposing();
                }
            }
            finally
            {
                IsDisposed = true;
                IsDisposing = false;
            }
        }

        /// <summary>
        ///     Overridden in implementing objects to perform actual clean-up.
        /// </summary>
        protected virtual void Disposing()
        {
        }

        /// <summary>
        ///     Check if the object has been disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     The object has been disposed.
        /// </exception>
        protected virtual void CheckDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().Name);
        }
    }
}
