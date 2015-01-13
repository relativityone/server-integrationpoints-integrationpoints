using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Contracts
{
	/// <summary>
	/// First call made into the app domain to do any setup work required.
	/// It is expected that there is only one class that implements this interface
	/// per library.
	/// </summary>
	public interface IStartUp
	{
		/// <summary>
		/// The function that will do any setup work.
		/// </summary>
		void Execute();
	}
}
