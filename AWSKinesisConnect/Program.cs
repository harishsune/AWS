using Amazon.Kinesis;
using Amazon.Kinesis.Model;
using System;
using System.IO;
using System.Text;
using System.Threading;

namespace AWSKinesisConnect
{
    class Program
    {
        public static AmazonKinesisClient oClient = null;
        public static string myStreamName = "LiveStream";
        static void Main(string[] args)
        {
            try
            {


                oClient = new AmazonKinesisClient(Properties.Settings.Default.AccessKey, Properties.Settings.Default.SecretAccess, Properties.Settings.Default.SessionToken, Amazon.RegionEndpoint.USEast1);

                //Create Stream
                CreateStream();

                //Wait For stream to available
                WaitForStreamToBecomeAvailable(myStreamName);

                //Put random records
                PutRecords();

                Console.Read();

            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }


        }

        /// <summary>
        /// 
        /// </summary>
        private static void CreateStream()
        {
            CreateStreamRequest createStreamRequest = new CreateStreamRequest();
            createStreamRequest.StreamName = myStreamName;
            oClient.CreateStream(createStreamRequest);
        }

        /// <summary>
        /// This method waits a maximum of 10 minutes for the specified stream to become active.
        /// <param name="myStreamName">Name of the stream whose active status is waited upon.</param>
        /// </summary>
        private static bool WaitForStreamToBecomeAvailable(string myStreamName)
        {
            var deadline = DateTime.UtcNow + TimeSpan.FromMinutes(10);
            while (DateTime.UtcNow < deadline)
            {
                DescribeStreamRequest describeStreamReq = new DescribeStreamRequest();
                describeStreamReq.StreamName = myStreamName;
                DescribeStreamResponse describeResult = oClient.DescribeStream(describeStreamReq);
                string streamStatus = describeResult.HttpStatusCode.ToString();
                Console.Error.WriteLine("  - current state: " + streamStatus);
                if (streamStatus == StreamStatus.ACTIVE)
                {
                    return true;
                }
                Thread.Sleep(TimeSpan.FromSeconds(20));
            }

            throw new Exception("Stream " + myStreamName + " never went active.");
        }

        /// <summary>
        /// 
        /// </summary>
        private static void PutRecords()
        {
            Console.Error.WriteLine("Putting records in stream : " + myStreamName);

            // Write 10 UTF-8 encoded records to the stream.
            for (int j = 0; j < 10; ++j)
            {
                PutRecordRequest requestRecord = new PutRecordRequest();
                requestRecord.StreamName = myStreamName;
                requestRecord.Data = new MemoryStream(Encoding.UTF8.GetBytes("testData-" + j));
                requestRecord.PartitionKey = "partitionKey-" + j;
                PutRecordResponse putResult = oClient.PutRecord(requestRecord);
                Console.Error.WriteLine(
                    String.Format("Successfully putrecord {0}:\n\t partition key = {1,15}, shard ID = {2}",
                        j, requestRecord.PartitionKey, putResult.ShardId));
            }
        }
    }
}
