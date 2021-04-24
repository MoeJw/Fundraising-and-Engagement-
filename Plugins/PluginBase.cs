using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;

namespace Plugins
{
	public abstract class PluginBase : IPlugin
	{
		protected class LocalPluginContext
		{
			internal IServiceProvider ServiceProvider
			{
				get;
				private set;
			}

			internal IOrganizationService OrganizationService
			{
				get;
				private set;
			}

			internal IPluginExecutionContext PluginExecutionContext
			{
				get;
				private set;
			}

			internal IServiceEndpointNotificationService NotificationService
			{
				get;
				private set;
			}

			internal ITracingService TracingService
			{
				get;
				private set;
			}

			private LocalPluginContext()
			{
			}

			internal LocalPluginContext(IServiceProvider serviceProvider)
			{
				if (serviceProvider == null)
				{
					throw new ArgumentNullException("serviceProvider");
				}
				PluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
				TracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
				NotificationService = (IServiceEndpointNotificationService)serviceProvider.GetService(typeof(IServiceEndpointNotificationService));
				IOrganizationServiceFactory organizationServiceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
				OrganizationService = organizationServiceFactory.CreateOrganizationService(null);
			}

			internal void Trace(string message)
			{
				if (!string.IsNullOrWhiteSpace(message) && TracingService != null)
				{
					if (PluginExecutionContext == null)
					{
						TracingService.Trace(message);
						return;
					}
					TracingService.Trace("{0}, Correlation Id: {1}, Initiating User: {2}", message, PluginExecutionContext.CorrelationId, PluginExecutionContext.InitiatingUserId);
				}
			}
		}

		private Collection<Tuple<int, string, string, Action<LocalPluginContext>>> registeredEvents;

		protected Collection<Tuple<int, string, string, Action<LocalPluginContext>>> RegisteredEvents
		{
			get
			{
				if (registeredEvents == null)
				{
					registeredEvents = new Collection<Tuple<int, string, string, Action<LocalPluginContext>>>();
				}
				return registeredEvents;
			}
		}

		protected string ChildClassName
		{
			get;
			private set;
		}

		internal PluginBase(Type childClassName)
		{
			ChildClassName = childClassName.ToString();
		}

		public void Execute(IServiceProvider serviceProvider)
		{
			if (serviceProvider == null)
			{
				throw new ArgumentNullException("serviceProvider");
			}
			LocalPluginContext localPluginContext = new LocalPluginContext(serviceProvider);
			localPluginContext.Trace(string.Format(CultureInfo.InvariantCulture, "Entered {0}.Execute()", ChildClassName));
			try
			{
				ExecuteCrmPlugin(localPluginContext);
			}
			catch (FaultException<OrganizationServiceFault> ex)
			{
				localPluginContext.Trace(string.Format(CultureInfo.InvariantCulture, "Exception: {0}", ex.ToString()));
				throw;
			}
			catch(Exception e)
            {
				localPluginContext.Trace(string.Format(CultureInfo.InvariantCulture, "Exception: {0}", e.ToString()));
			}
			finally
			{
				localPluginContext.Trace(string.Format(CultureInfo.InvariantCulture, "Exiting {0}.Execute()", ChildClassName));
			}
		}

		protected virtual void ExecuteCrmPlugin(LocalPluginContext localcontext)
		{
		}
	}
}
