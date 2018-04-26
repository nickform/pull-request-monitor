using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApprovalTests;
using ApprovalTests.Reporters;
using Castle.Core.Internal;
using Castle.MicroKernel;
using Castle.MicroKernel.Handlers;
using Castle.Windsor;
using Castle.Windsor.Diagnostics;
using NUnit.Framework;

namespace PullRequestMonitor.UnitTest
{
    [TestFixture]
    public class WindsorInstallerTest : InstallerTest
    {
        public WindsorInstallerTest()
        {
            WindsorContainer = new WindsorContainer();
            WindsorContainer.Install(new WindsorInstaller());
        }

        protected override IWindsorContainer WindsorContainer { get; }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CheckPotentiallyMisconfiguredComponents()
        {
            CheckPotentiallyMisconfiguredComponentsCore();
        }


        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CheckNonPublicConstructors()
        {
            ApprovedNonPublicConstructorsCore();
        }
    }

    public abstract class InstallerTest
    {
        protected abstract IWindsorContainer WindsorContainer { get; }

        private static IEnumerable<IHandler> GetPotentiallyMisconfiguredComponents(IWindsorContainer container)
        {
            var host = container.Kernel.GetSubSystem(SubSystemConstants.DiagnosticsKey) as IDiagnosticsHost;
            var diagnostic = host.GetDiagnostic<IPotentiallyMisconfiguredComponentsDiagnostic>();
            var handlers = diagnostic.Inspect();
            return handlers;
        }

        private static IEnumerable<IHandler> GetAllComponents(IWindsorContainer container)
        {
            var host = container.Kernel.GetSubSystem(SubSystemConstants.DiagnosticsKey) as IDiagnosticsHost;
            var diagnostic = host.GetDiagnostic<IAllComponentsDiagnostic>();
            var handlers = diagnostic.Inspect();
            return handlers;
        }

        protected void CheckPotentiallyMisconfiguredComponentsCore()
        {
            var handlers = GetPotentiallyMisconfiguredComponents(WindsorContainer);
            var message = new StringBuilder();
            var inspector = new DependencyInspector(message);
            foreach (var handler in handlers)
            {
                ((IExposeDependencyInfo) handler).ObtainDependencyDetails(inspector);
            }
            Approvals.Verify(message.ToString());
        }

        protected void ApprovedNonPublicConstructorsCore()
        {
            var handlers = GetAllComponents(WindsorContainer);
            var message = new StringBuilder();
            foreach (var handler in handlers)
            {
                if (!handler.ComponentModel.HasClassServices)
                    continue;

                if (handler.ComponentModel.Constructors.IsNullOrEmpty())
                {
                    message.AppendFormat("Cannot construct service {0} due to missing public constructor", handler);
                    message.AppendLine();
                }
            }
            Approvals.Verify(message.ToString());
        }

        [Test]
        public void PrintContainer()
        {
            var handlers = GetAllComponents(WindsorContainer);
            Console.WriteLine("{0} components were registered.", handlers.Count());
            foreach (var handler in handlers)
            {
                Console.Write("Model: {0}", handler.ComponentModel.Implementation.Name);
                foreach (var service in handler.ComponentModel.Services)
                {
                    Console.Write(" / ");
                    Console.Write(service.Name);
                }
                Console.Write("\n");
            }
        }
    }
}