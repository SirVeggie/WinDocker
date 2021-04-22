using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinUtilities;

namespace Apprentice.GUI {

    public abstract class GuiContainer {
        public static List<GuiContainer> Collection { get; private set; } = new List<GuiContainer>();

        public Form GenericForm { get; protected set; }
        /// <summary>Handle to the form window</summary>
        public IntPtr Handle => Execute(() => GenericForm.Handle);
        /// <summary>The form's window object</summary>
        public Window Window { get; private set; }
        /// <summary>The thread the form is running on</summary>
        public Thread Thread { get; private set; }

        /// <summary>Triggers on form close</summary>
        public event Action OnClose;
        /// <summary>Triggers on form show</summary>
        public event Action OnShow;

        protected TaskCompletionSource<object> formAvailable = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
        protected TaskCompletionSource<object> formClosed = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

        /// <summary>Check if the form is still running</summary>
        public bool IsAlive => Thread?.IsAlive ?? false && !formClosed.Task.IsCompleted;

        /// <summary>Launch the form</summary>
        public void Launch() => BaseLaunch();

        /// <summary>Launch the form and wait for the form to spawn</summary>
        public async Task LaunchAndWait() {
            Launch();
            await WaitForForm();
        }

        /// <summary>Launch the form</summary>
        protected void BaseLaunch() {
            Thread = new Thread(CreateForm);
            Thread.SetApartmentState(ApartmentState.STA);
            Thread.IsBackground = true;
            Thread.Start();
        }

        private void CreateForm() {
            GenericForm = InitializeFormBase();
            GenericForm.Shown += (o, e) => {
                Window = new Window(Handle);
                formAvailable.SetResult(this);
                Collection.Add(this);
                OnShow?.Invoke();
            };
            GenericForm.FormClosed += (o, e) => {
                formClosed.SetResult(this);
                Collection.Remove(this);
                OnClose?.Invoke();
            };
            Application.Run(GenericForm);
        }

        protected abstract Form InitializeFormBase();

        /// <summary>Close the form</summary>
        public virtual void Close() {
            if (Thread == null)
                throw new Exception("Tried to close gui before launching it");
            if (IsAlive) {
                try {
                    GenericForm.Invoke((Action) GenericForm.Close);
                } catch (ObjectDisposedException) {
                    Console.WriteLine("Form was already closed");
                }
            }
        }

        /// <summary>Wait for the form window to be created</summary>
        public async Task WaitForForm() => await formAvailable.Task;
        /// <summary>Wait for the form window to close</summary>
        public async Task WaitForClose() => await formClosed.Task;

        /// <summary>Call a delegate on the form's thread for safe form handling</summary>
        public void Execute(Action action) => GuiTool.Execute(GenericForm, action);
        /// <summary>Call a delegate on the form's thread for safe form handling</summary>
        public T Execute<T>(Func<T> action) => GuiTool.Execute(GenericForm, action);
        /// <summary>Call a delegate on the form's thread for safe form handling</summary>
        public async Task Execute(Func<Task> asyncAction) => await GuiTool.Execute(GenericForm, asyncAction);
        /// <summary>Call a delegate on the form's thread for safe form handling</summary>
        public async Task<T> Execute<T>(Func<Task<T>> asyncAction) => await GuiTool.Execute(GenericForm, asyncAction);
    }

    public abstract class GuiContainer<T> : GuiContainer where T : Form {

        /// <summary>Retrieve the wrapped form object</summary>
        public T Form { get => (T) GenericForm ?? throw new Exception("The form is null, maybe it wasn't created yet?"); private set => GenericForm = value; }
        private Action<T> initAction;

        /// <summary>Launch the form, but perform some operation on it before it is shown</summary>
        public virtual void Launch(Action<T> initAction) {
            this.initAction = initAction;
            BaseLaunch();
        }

        protected sealed override Form InitializeFormBase() {
            var form = InitializeForm();
            initAction?.Invoke(form);
            return form;
        }

        /// <summary>Create and initialize the wrapped form. Remember to also call Launch() to run the form.</summary>
        protected abstract T InitializeForm();
    }
}
