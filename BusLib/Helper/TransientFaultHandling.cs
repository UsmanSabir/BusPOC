using BusLib.BatchEngineCore.Exceptions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BusLib.Helper
{
    public class TransientFaultHandling
    {
        public static bool IsTransient(Exception ex)
        {
            if (ex is WebException webException)
            {
                return webException.Status == WebExceptionStatus.ProtocolError || webException.Status == WebExceptionStatus.ConnectionClosed || 
                       webException.Status == WebExceptionStatus.Timeout || webException.Status == WebExceptionStatus.RequestCanceled;
            }
            else if(ex is SqlException sqlException)
            {
                return IsSqlExceptionTransient(sqlException);
            }
            else if (ex is TimeoutException)
            {
                return true;
            }
            else if(ex is IOException ioe)
            {
                bool transnt = ioe.Message.Contains("An existing connection was forcibly closed");
                return transnt;
            }
            else if(ex is FrameworkException)
            {
                return false;
            }
            else if (ex is NotImplementedException)
            {
                return false;
            }

            bool retry = ex.Message.Contains("An existing connection was forcibly closed") || ex.Message.Contains("Timeout expired"); //"Timeout expired.  The timeout period elapsed prior to obtaining a connection from the pool.  This may have occurred because all pooled connections were in use and max pool size was reached."
            return retry;
            return true; //todo rethink
        }

        public static bool IsCircuitBreaker(Exception ex)
        {
            return IsTransient(ex);//todo
        }

        private static bool IsSqlExceptionTransient(SqlException sqlException)
        {
            var canRetry = CanRetryOnSqlExceptions(sqlException);
            if (canRetry)
                return true;

            // Enumerate through all errors found in the exception.
            foreach (SqlError err in sqlException.Errors)
            {
                switch (err.Number)
                {
                    // SQL Error Code: 40501
                    // The service is currently busy. Retry the request after 10 seconds. Code: (reason code to be decoded).
                    case 40501: // ThrottlingCondition.ThrottlingErrorNumber:
                        // Decode the reason code from the error message to determine the grounds for throttling.
                        //var condition = ThrottlingCondition.FromError(err);

                        //// Attach the decoded values as additional attributes to the original SQL exception.
                        //sqlException.Data[condition.ThrottlingMode.GetType().Name] =
                        //    condition.ThrottlingMode.ToString();
                        //sqlException.Data[condition.GetType().Name] = condition;

                        return true;

                    // SQL Error Code: 10928
                    // Resource ID: %d. The %s limit for the database is %d and has been reached.
                    case 10928:
                    // SQL Error Code: 10929
                    // Resource ID: %d. The %s minimum guarantee is %d, maximum limit is %d and the current usage for the database is %d. 
                    // However, the server is currently too busy to support requests greater than %d for this database.
                    case 10929:
                    // SQL Error Code: 10053
                    // A transport-level error has occurred when receiving results from the server.
                    // An established connection was aborted by the software in your host machine.
                    case 10053:
                    // SQL Error Code: 10054
                    // A transport-level error has occurred when sending the request to the server. 
                    // (provider: TCP Provider, error: 0 - An existing connection was forcibly closed by the remote host.)
                    case 10054:
                    // SQL Error Code: 10060
                    // A network-related or instance-specific error occurred while establishing a connection to SQL Server. 
                    // The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server 
                    // is configured to allow remote connections. (provider: TCP Provider, error: 0 - A connection attempt failed 
                    // because the connected party did not properly respond after a period of time, or established connection failed 
                    // because connected host has failed to respond.)"}
                    case 10060:
                    // SQL Error Code: 40197
                    // The service has encountered an error processing your request. Please try again.
                    case 40197:
                    // SQL Error Code: 40540
                    // The service has encountered an error processing your request. Please try again.
                    case 40540:
                    // SQL Error Code: 40613
                    // Database XXXX on server YYYY is not currently available. Please retry the connection later. If the problem persists, contact customer 
                    // support, and provide them the session tracing ID of ZZZZZ.
                    case 40613:
                    // SQL Error Code: 40143
                    // The service has encountered an error processing your request. Please try again.
                    case 40143:
                    // SQL Error Code: 233
                    // The client was unable to establish a connection because of an error during connection initialization process before login. 
                    // Possible causes include the following: the client tried to connect to an unsupported version of SQL Server; the server was too busy 
                    // to accept new connections; or there was a resource limitation (insufficient memory or maximum allowed connections) on the server. 
                    // (provider: TCP Provider, error: 0 - An existing connection was forcibly closed by the remote host.)
                    case 233:
                    // SQL Error Code: 64
                    // A connection was successfully established with the server, but then an error occurred during the login process. 
                    // (provider: TCP Provider, error: 0 - The specified network name is no longer available.) 
                    case 64:
                    // DBNETLIB Error Code: 20
                    // The instance of SQL Server you attempted to connect to does not support encryption.
                    case 20:
                    case -2: // timeout
                    //case 0://todo US: The connection is broken and recovery is not possible.  The connection is marked by the server as unrecoverable.  No attempt was made to restore the connection.
                        return true;
                        //break;
                    
                        
                }
            }
            return false;
        }

        

        private static bool CanRetryOnSqlExceptions(SqlException ex)
        {
            bool isConnectionError = false;
            bool isDeadlockError = false;
            bool isTimeoutError = false;

            foreach (SqlError error in ex.Errors)
            {
                switch (error.Number)
                {
                    case 2: //A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server is configured to allow remote connections. (provider: Named Pipes Provider, error: 40 - Could not open a connection to SQL Server)
                    case 20: //The instance of SQL Server you attempted to connect to does not support encryption. (PMcE: amazingly, this is transient)
                    case 64: //A connection was successfully established with the server, but then an error occurred during the login process.
                    case 232: //A transport level error : By US
                    //case 4060: //Cannot open database "DB" requested by the login. The login failed.
                    //case 18456: //Login failed for user 'sa'.
                    case 233: //The client was unable to establish a connection because of an error during connection initialization process before login
                    case 10053: //transport level error
                    case 10054: //transport level error
                    case 10060: //network or instance error
                    case 10061: //network or instance error
                    case 10928: //Azure out of resources
                    case 10929: //Azure out of resources
                    case 40143: //connection could not be initialized
                    case 40197: //the service encountered an error
                    case 40501: //service is busy
                    case 40613: //database unavailable
                        isConnectionError = true;
                        break;
                    case 3960: //snapshot isolation error
                    case 1205: //deadlock
                        isDeadlockError = true;
                        break;
                    case -2: //timeout
                        isTimeoutError = true;
                        break;
                    case 50000: //explicit TSQL RAISERROR
                        //see if this is a rethrow if a deadlock..
                        if (error.Message.Contains("Error 1205"))
                        {
                            isDeadlockError = true;
                        }
                        break;
                }
            }

            return isConnectionError || isTimeoutError; // isDeadlockError==false;
        }
    }
}
