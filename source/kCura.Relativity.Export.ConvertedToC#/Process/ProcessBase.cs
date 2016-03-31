using System;

namespace kCura.Windows.Process
{
	public abstract class ProcessBase : IRunable
	{

		private ProcessObserver _processObserver;
		private Controller _processController;

		private Guid _processID;
		protected abstract void Execute();

		public kCura.Windows.Process.ProcessObserver ProcessObserver {
			get { return _processObserver; }
		}

		public kCura.Windows.Process.Controller ProcessController {
			get { return _processController; }
		}

		protected ProcessBase()
		{
			_processObserver = new kCura.Windows.Process.ProcessObserver();
			_processController = new kCura.Windows.Process.Controller();
		}

		#region " Implements IRunable "

		public Guid ProcessID {
			get { return _processID; }
			set { _processID = value; }
		}

		public void StartProcess()
		{
			try {
				this.Execute();


				// removed this.  The base class should not raise this event.  Let the actual implementation raise it when it is truly complete.
				//_processObserver.RaiseProcessCompleteEvent()
			} catch (Exception ex) {
				_processObserver.RaiseFatalExceptionEvent(ex);
			}
		}

		#endregion

	}
}
