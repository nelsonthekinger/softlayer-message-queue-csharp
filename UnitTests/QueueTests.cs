using System;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using SoftLayer.Messaging;

namespace UnitTests
{
    [TestFixture]
    public class QueueTests : MessagingTest
    {
        [TestFixtureSetUp]
        protected override void Init()
        {
            base.Init();
            validClient.Authenticate();
        }


        [Test]
        public void ListQueues()
        {
            List<Queue> queueList = null;

            Assert.DoesNotThrow(
                delegate { queueList = validClient.GetQueueList(); });
            Assert.IsNotNull(queueList);

            T(queueList.Count + " queues found in account");
        }

        [Test]
        public void ListQueuesWithNew()
        {
            Queue newQueue = validClient.CreateQueue(TestDataFactory.RandomQueueName());
            Assert.IsNotNull(newQueue);

            T("Created queue " + newQueue.Name);

            List<Queue> queueList = null;
            Assert.DoesNotThrow(
                delegate { queueList = validClient.GetQueueList(); });
            Assert.IsNotNull(queueList);
            Assert.IsTrue(queueList.Count > 0);

            Queue remoteQueue = queueList.Find(
                delegate(Queue tmpQueue)
                {
                    return (tmpQueue.Name == newQueue.Name);
                });

            Assert.IsNotNull(remoteQueue);
        }

        [Test]
        public void CreateQueue()
        {
            string queueName = TestDataFactory.RandomQueueName();

            T("Queue " + queueName);
            Queue queue = validClient.CreateQueue(queueName);

            Assert.NotNull(queue);
            Assert.AreEqual(queueName, queue.Name);
        }

        [Test]
        public void UpdateQueue()
        {
            string queueName = TestDataFactory.RandomQueueName();

            T("Queue " + queueName);
            Queue queue = validClient.CreateQueue(queueName);

            Assert.NotNull(queue);
            Assert.AreEqual(queueName, queue.Name);

            string randomString = TestDataFactory.RandomHexString(32);
            T("Tagging with " + randomString);

            queue.Tags.Add(randomString);
            Assert.IsTrue(queue.Tags.Contains(randomString));

            Queue updatedQueue = validClient.UpdateQueue(queue);
            Assert.NotNull(updatedQueue);
            Assert.AreEqual(updatedQueue.Name, queue.Name);
            Assert.IsTrue(updatedQueue.Tags.Contains(randomString));
        }

        [Test]
        public void GetQueue()
        {
            string queueName = TestDataFactory.RandomQueueName();

            T("Queue " + queueName);
            Queue queue = validClient.CreateQueue(queueName);

            Assert.NotNull(queue);
            Assert.AreEqual(queueName, queue.Name);

            Queue remoteQueue = validClient.GetQueue(queueName);
            Assert.NotNull(remoteQueue);
            Assert.AreEqual(remoteQueue.Name, queue.Name);
        }

        [Test]
        public void GetQueueFailure()
        {
            string queueName = "bad" + TestDataFactory.RandomQueueName();

            T("Queue " + queueName);

            Assert.Throws<QueueNotFoundException>(
                delegate { validClient.GetQueue(queueName); });
        }

        [Test]
        public void DeleteQueue()
        {
            string queueName = TestDataFactory.RandomQueueName();

            T("Queue " + queueName);
            Queue queue = validClient.CreateQueue(queueName);

            Assert.NotNull(queue);
            Assert.AreEqual(queueName, queue.Name);

            bool result = false;
            Assert.DoesNotThrow(
                delegate { result = validClient.DeleteQueue(queueName); });
            Assert.IsTrue(result);
        }

        [Test]
        public void DeleteNonEmptyQueueFailure()
        {
            string queueName = TestDataFactory.RandomQueueName();

            T("Queue " + queueName);
            Queue queue = validClient.CreateQueue(queueName);

            Assert.NotNull(queue);
            Assert.AreEqual(queueName, queue.Name);

            validClient.PublishToQueue(queue,
                new Message(TestDataFactory.RandomHexString()));

            Assert.Throws<QueueNotEmptyException>(
                delegate { validClient.DeleteQueue(queueName, false); });
        }

        [Test]
        public void DeleteNonEmptyQueue()
        {
            string queueName = TestDataFactory.RandomQueueName();

            T("Queue " + queueName);
            Queue queue = validClient.CreateQueue(queueName);

            Assert.NotNull(queue);
            Assert.AreEqual(queueName, queue.Name);

            validClient.PublishToQueue(queue,
                new Message(TestDataFactory.RandomHexString()));

            bool result = false;
            Assert.DoesNotThrow(
                delegate { result = validClient.DeleteQueue(queueName, true); });
            Assert.IsTrue(result);
        }

        [Test]
        public void PushSingleToQueue()
        {
            string queueName = TestDataFactory.RandomQueueName();

            T("Queue " + queueName);
            Queue queue = validClient.CreateQueue(queueName);

            Assert.NotNull(queue);
            Assert.AreEqual(queueName, queue.Name);

            string randomBody = TestDataFactory.RandomHexString();
            Message msg = new Message();
            msg.Fields["test Key"] = TestDataFactory.RandomHexString(10);
            msg.Body = randomBody;

            Assert.AreEqual(randomBody, msg.Body);

            Message returnMsg = validClient.PublishToQueue(queue, msg);
            Assert.IsNotEmpty(returnMsg.ID);
            Assert.AreEqual(returnMsg.Body, msg.Body);
            Assert.AreEqual(returnMsg.Fields["test Key"], msg.Fields["test Key"]);

            T("Msg ID:    " + returnMsg.ID);
            T("Msg Body:  " + returnMsg.Body);
            T("Msg Field: testKey=" + returnMsg.Fields["test Key"]);
        }

        [Test]
        public void PushMultipleToQueue()
        {
            string queueName = TestDataFactory.RandomQueueName();

            T("Queue " + queueName);
            Queue queue = validClient.CreateQueue(queueName);
            T(" - interval: " + queue.VisibilityInterval + " sec");

            Assert.NotNull(queue);
            Assert.AreEqual(queueName, queue.Name);

            List<Message> originalMsgs = new List<Message>();
            List<Message> pushedMsgs = new List<Message>();

            Stopwatch timer = Stopwatch.StartNew();
            for (int ii = 0; ii < 1000; ii++) {
                string randomBody = TestDataFactory.RandomHexString();

                Message msg = new Message();
                msg.Body = randomBody;
                originalMsgs.Add(msg);

                Assert.AreEqual(randomBody, msg.Body);

                Message pushedMsg = validClient.PublishToQueue(queue, msg);

                TC(".");
                if (ii > 0 && ii % 100 == 0) {
                    TC(ii + "\n");
                }

                Assert.IsNotNull(pushedMsg);
                Assert.IsNotEmpty(pushedMsg.ID);
                Assert.AreEqual(pushedMsg.Body, msg.Body);

                pushedMsgs.Add(pushedMsg);
            }

            timer.Stop();

            Assert.IsNotEmpty(originalMsgs);
            Assert.IsNotEmpty(pushedMsgs);
            Assert.AreEqual(pushedMsgs.Count, originalMsgs.Count);

            T("Generated " + originalMsgs.Count + " messages");
            T(String.Format(
                "Pushed {0} messages in {1:f}sec ({2:f}/sec)",
                pushedMsgs.Count,
                timer.Elapsed.TotalSeconds,
                pushedMsgs.Count / timer.Elapsed.TotalSeconds));

            for (int ii = 0; ii < pushedMsgs.Count; ii++) {
                Message pushedMsg = pushedMsgs[ii];
                Message originalMsg = originalMsgs.Find(
                    delegate(Message tmpMsg)
                    {
                        return (tmpMsg.Body == pushedMsg.Body);
                    });

                Assert.IsNotNull(originalMsg);

                T(String.Format("Pushed message {0:d}: {1} -> {2}",
                    ii, pushedMsg.ID, originalMsg.Body));

                Assert.AreEqual(originalMsg.Body, pushedMsg.Body);
            }
        }

        [Test]
        public void PopSingleFromQueue()
        {
            string queueName = TestDataFactory.RandomQueueName();

            T("Queue " + queueName);
            Queue queue = validClient.CreateQueue(queueName);

            Assert.NotNull(queue);
            Assert.AreEqual(queueName, queue.Name);

            string randomBody = TestDataFactory.RandomHexString();

            Message originalMsg = new Message();
            originalMsg.Body = randomBody;

            Assert.AreEqual(randomBody, originalMsg.Body);

            Message pushedMsg = validClient.PublishToQueue(queue, originalMsg);
            Assert.IsNotEmpty(pushedMsg.ID);
            Assert.AreEqual(pushedMsg.Body, originalMsg.Body);

            T("Pushed Msg ID:   " + pushedMsg.ID);
            T("Pushed Msg Body: " + pushedMsg.Body);

            Message poppedMsg = null;

            Assert.DoesNotThrow(
                delegate { poppedMsg = validClient.PopMessage(queue); });
            Assert.IsNotNull(poppedMsg);
            Assert.IsNotEmpty(poppedMsg.ID);
            Assert.AreEqual(poppedMsg.ID, pushedMsg.ID);
            Assert.AreEqual(poppedMsg.Body, pushedMsg.Body);

            T("Popped Msg ID:   " + poppedMsg.ID);
            T("Popped Msg Body: " + poppedMsg.Body);
        }


        [Test]
        public void PopMultipleFromQueue()
        {
            string queueName = TestDataFactory.RandomQueueName();
            int numberOfTestMessages = 1000;

            T("Queue " + queueName);
            Queue queue = validClient.CreateQueue(queueName);
            T(" - interval: " + queue.VisibilityInterval + " sec");

            Assert.NotNull(queue);
            Assert.AreEqual(queueName, queue.Name);

            List<Message> originalMsgs = new List<Message>();
            List<Message> pushedMsgs = new List<Message>();
            for (int ii = 0; ii < numberOfTestMessages; ii++) {
                string randomBody = "(" + ii + ") " + TestDataFactory.RandomHexString(15);

                Message msg = new Message();
                msg.Body = randomBody;
                originalMsgs.Add(msg);

                Assert.AreEqual(randomBody, msg.Body);

                Message pushedMsg = validClient.PublishToQueue(queue, msg);

                Assert.IsNotNull(pushedMsg);
                Assert.IsNotEmpty(pushedMsg.ID);
                Assert.AreEqual(pushedMsg.Body, msg.Body);

                pushedMsgs.Add(pushedMsg);

                TC(".");
                if (ii > 0 && ii % 100 == 0) {
                    TC(ii + "\n");
                }
            }

            Assert.IsNotEmpty(originalMsgs);
            Assert.IsNotEmpty(pushedMsgs);

            T("Generated " + originalMsgs.Count + " messages");
            T("Pushed " + pushedMsgs.Count + " messages");

            List<Message> poppedMsgs = new List<Message>();
            Assert.DoesNotThrow(
                delegate
                {
                    int iterPoppedCount = 0;
                    do {
                        T("Popping...");
                        List<Message> tmpMsgs = validClient.PopMessages(queue, Queue.MaxMessagesPerPop);
                        iterPoppedCount = tmpMsgs.Count;
                        poppedMsgs.AddRange(tmpMsgs);

                        T(" - Popped " + tmpMsgs.Count);
                        T(String.Format(" - i:{0}, p:{1}, t:{2}",
                            iterPoppedCount, pushedMsgs.Count, poppedMsgs.Count));
                    } while (iterPoppedCount > 0);
                });
            Assert.IsNotEmpty(poppedMsgs);
            Assert.AreEqual(poppedMsgs.Count, pushedMsgs.Count);

            for (int ii = 0; ii < poppedMsgs.Count; ii++) {
                Message poppedMsg = poppedMsgs[ii];
                Message pushedMsg = pushedMsgs.Find(
                    delegate(Message tmpMsg)
                    {
                        return (tmpMsg.ID == poppedMsg.ID);
                    });

                Assert.IsNotNull(pushedMsg);

                T(String.Format("Popped message {0:d}: {1} / {2}",
                    ii, poppedMsg.ID, pushedMsg.ID));

                Assert.AreEqual(poppedMsg.Body, pushedMsg.Body);
            }
        }

        [Test]
        public void DeleteMessage()
        {
            string queueName = TestDataFactory.RandomQueueName();

            T("Queue " + queueName);
            Queue queue = validClient.CreateQueue(queueName);

            Assert.NotNull(queue);
            Assert.AreEqual(queueName, queue.Name);

            Message msg = validClient.PublishToQueue(queue,
                new Message(TestDataFactory.RandomHexString()));

            bool result = false;
            Assert.DoesNotThrow(
                delegate { result = validClient.DeleteMessage(queueName, msg.ID); });
            Assert.IsTrue(result);
        }

        [Test]
        public void DeleteMessageFailure()
        {
            string queueName = TestDataFactory.RandomQueueName();

            T("Queue " + queueName);
            Queue queue = validClient.CreateQueue(queueName);

            Assert.NotNull(queue);
            Assert.AreEqual(queueName, queue.Name);
            Assert.Throws<MessageNotFoundException>(
                delegate { validClient.DeleteMessage(queueName, "abcdefg"); });
        }
    }
}
