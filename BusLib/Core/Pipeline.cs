using BusLib.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusLib.Helper;

namespace BusLib.Core
{
    public class Pipeline<T>:SafeDisposable where T : IMessage
    {
        IFeatureHandler<T> _featureHandler=null;
        readonly IHandler<T> _handler;
        List<IFeatureHandler<T>> _featureHandlersCollection = new List<IFeatureHandler<T>>();

        IHandler<T> rootHandler => _featureHandler ?? _handler;

        public Pipeline(IHandler<T> handler)
        {
            _handler = handler;
        }

        public void RegisterFeatureDecorator(IFeatureHandler<T> featureHandler)
        {
            lock (this)
            {
                _featureHandler = featureHandler.Register(rootHandler);
                _featureHandlersCollection.Add(featureHandler);
            }
        }

        public void Invoke(T msg)
        {
            Execute(msg);
        }

        public void HandleSystemCommand(ISystemCommand systemCommand)
        {
            if(systemCommand is SystemFeatureToggleCommand systemFeatureToggle)
            {
                foreach(var fv in _featureHandlersCollection.Where(hnd=>hnd.GetType() == systemFeatureToggle.FeatureHandlerType))
                {
                    if (systemFeatureToggle.Enable)
                    {
                        fv.Enable();
                    }
                    else
                    {
                        fv.Disable();
                    }
                }
            }
        }

        private void Execute(T message)
        {
            //OnStart
            try
            {
                rootHandler.Handle(message);
                //OnSuccess
            }
            finally
            {
                //OnComplete
            }
        }

        protected override void Dispose(bool disposing)
        {
            Robustness.Instance.SafeCall(() =>
            {
                foreach (var featureHandler in _featureHandlersCollection)
                {
                    featureHandler.Dispose();
                }
                _featureHandlersCollection.Clear();
            });
            
            Robustness.Instance.SafeCall(()=> _handler.Dispose());

            base.Dispose(disposing);
        }
    }
}
