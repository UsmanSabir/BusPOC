using System;
using System.Collections.Generic;
using System.Threading;
using BusLib;
using BusLib.BatchEngineCore;
using BusLib.BatchEngineCore.Volume;
using BusLib.Helper;
using BusLib.Infrastructure;
using BusLib.Serializers;

namespace BusImpl
{
    class VolumeHandler: IVolumeHandler
    {
        private readonly IResolver _resolver;
        private readonly ISerializersFactory _serializersFactory;

        public VolumeHandler(IResolver resolver)
        {
            _resolver = resolver;
            _serializersFactory = SerializersFactory.Instance;
        }

        public void Handle<T>(IEnumerable<T> volume, IProcessExecutionContextWithVolume processContext, CancellationToken token)
        {
            if (volume==null)
            {
                processContext.Logger.Info("No volume returned");
                return;
            }

            bool hasVolume = false;
            var serializer = _serializersFactory.GetSerializer<T>();
            int currentCount = 0;
            int batchSize = 100;

            
            try
            {

                Bus.HandleDbCommand(new DbAction(DbActions.Transaction, () => //create transaction in Db Bus pipeline
                {
                    var volumeDeletedCount = ExecuteNonQuery(Constant.SQLDeleteProcessVolume, processContext.ProcessState.Id);
                    if (volumeDeletedCount > 0)
                    {
                        processContext.Logger.Warn($"Existing volume {volumeDeletedCount} deleting");
                    }
                }));
            
                //using (var unitOfWork = ) //IsolationLevel.Snapshot //todo check isolation level with db team, is it enabled by default
                {
                    foreach (var v in volume)
                    {
                        var payLoad = serializer.SerializeToString(v);
                        IReadWritableTaskState state = CreateNew();

                        state.Payload = payLoad;
                        state.ProcessId = processContext.ProcessState.Id;
                        state.CurrentState = ResultStatus.Empty.Name;

                        currentCount++;
                        if (currentCount >= batchSize)
                        {
                            token.ThrowIfCancellationRequested();

                            hasVolume = true;
                            //Persist();
                            currentCount = 0;
                            //Clear();//remove persisted items
                        }
                    }
                    //if (Entity.Count>0)
                    {
                        token.ThrowIfCancellationRequested();
                        hasVolume = true;
                       // Persist();
                    }
                
                    //Commit();
                }
            }
            finally
            {
                //dispose / rollback
            }

            if(hasVolume)
                processContext.SetVolumeGenerated();

        }

        private IReadWritableTaskState CreateNew()
        {
            throw new NotImplementedException();
        }

        private int ExecuteNonQuery(string sQLDeleteProcessVolume, long id)
        {
            throw new NotImplementedException();
        }

        private Bus _bus;
        protected Bus Bus
        {
            get { return _bus ?? (_bus = _resolver.Resolve<Bus>()); }
        }

        public IReadWritableTaskState GetNextTaskWithTransaction(out ITransaction transaction)
        {
            transaction = null;
            int tries = 0;
            ITransaction trans = null;
            
            Retry:

            tries++;
            try
            {
                Bus.HandleDbCommand(new DbAction(DbActions.Transaction, () =>
                {
                    //begin trans
                    ReadWithSql(Constant.SQLReadDataQueue, DateTime.UtcNow);
                    //if (Entity.Count > 0)
                    {
                        trans = new TransactionWrapper(); 
                        //batchTaskWrapper = new BatchTaskWrapper(Entity.First());
                    }
                    //else
                    //{
                    //    return null;
                    //}

                }));
                transaction = trans;
                return null; //todo batchTaskWrapper;

            }
            catch (Exception)
            {
                trans?.Dispose();
                if(tries<3)
                    goto Retry;
                
                //throw;
                return null;

            }
        }

        private void ReadWithSql(string sQLReadDataQueue, DateTime utcNow)
        {
            throw new NotImplementedException();
        }
    }
}