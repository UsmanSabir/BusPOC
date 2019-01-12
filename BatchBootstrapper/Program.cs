using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BatchBootstrapper.Process;
using BusImpl;
using BusImpl.Redis;
using BusLib;
using BusLib.BatchEngineCore;
using BusLib.BatchEngineCore.PubSub;
using BusLib.BatchEngineCore.Volume;
using BusLib.Infrastructure;
using BusLib.JobSchedular;
using BusLib.JobScheduler;
using BusLib.ProcessLocks;

namespace BatchBootstrapper
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Ready");

            
            //var redisConnection = "10.39.100.27";// "localhost";// "172.31.15.19";
            var redisConnection = "localhost";// "172.31.15.19";

            //var serializer = Bootstrap.GetSerializer();

            //IProcessDataStorage storage=new RedisProcessStorage(NodeSettings.Instance.Name, redisConnection, serializer);

            //IPubSubFactory factory= new RedisPubSubFactory(redisConnection);
            //IDistributedMutexFactory mutexFactory=new DistributedMutexFactory(redisConnection);
            var container = Bootstrap.Initialize(redisConnection);


            BatchEngineService service = container.Resolve<BatchEngineService>(); // new BatchEngineService(new StatePersistenceService(), new EntityFactory());
            var jobScheduler = container.Resolve<IJobScheduler>();
            try
            {
                service.Start();

                //IJobScheduler jobScheduler=new JobSchedulerInternal();

                //service.SubmitSingleProcess<TestDDProcess>(new JobCriteria() { BranchId = 1, CompanyId = 1, ProcessingDate = DateTime.Now, ReferenceId = 1, SubTenantId = 1 }, false);
                //service.SubmitSingleProcess<TestCCProcess>(new JobCriteria() { BranchId = 1, CompanyId = 1, ProcessingDate = DateTime.Now, ReferenceId = 1, SubTenantId = 1 }, false);
                //service.SubmitSingleProcess<TestCCProcess>(new JobCriteria() { BranchId = 1, CompanyId = 1, ProcessingDate = DateTime.Now, ReferenceId = 1, SubTenantId = 1 }, false);

                jobScheduler.CreateJob(2, new List<JobCriteria>() { new JobCriteria() { BranchId = 1, CompanyId = 1, ProcessingDate = DateTime.Now, ReferenceId = 1, SubTenantId = 1 } }, "System");

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.WriteLine("Done");
            Console.ReadLine();
            service.Stop();

            Console.WriteLine("Stopped <enter> to exit");
            Console.ReadLine();

        }
    }
}
