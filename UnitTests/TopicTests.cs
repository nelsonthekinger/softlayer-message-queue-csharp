using System;
using System.Collections.Generic;
using NUnit.Framework;
using SoftLayer.Messaging;
using System.Diagnostics;

namespace UnitTests
{
    [TestFixture]
    public class TopicTests : MessagingTest
    {
        [TestFixtureSetUp]
        protected override void Init()
        {
            base.Init();
            validClient.Authenticate();
        }

        [Test]
        public void CreateTopic()
        {
            string topicName = TestDataFactory.RandomTopicName();

            T("Topic " + topicName);

            Topic topic = new Topic(topicName);
            topic.Tags.AddRange(new string[] {"test", "csharp"});

            Topic topicRemote = validClient.CreateTopic(topic);
            Assert.NotNull(topicRemote);
            Assert.AreEqual(topicRemote.Name, topicName);
        }

        [Test]
        public void UpdateTopic()
        {
            string topicName = TestDataFactory.RandomTopicName();

            T("Topic " + topicName);
            Topic topic = new Topic(topicName);
            topic.Tags.AddRange(new string[] { "test", "csharp" });

            Topic topicRemote = validClient.CreateTopic(topic);
            Assert.NotNull(topicRemote);
            Assert.AreEqual(topicRemote.Name, topicName);

            string randomString = TestDataFactory.RandomHexString(32);
            T("Tagging with " + randomString);
            topicRemote.Tags.Add(randomString);

            Topic updatedTopic = validClient.UpdateTopic(topicRemote);
            Assert.NotNull(updatedTopic);
            Assert.AreEqual(updatedTopic.Name, topic.Name);
            Assert.IsTrue(updatedTopic.Tags.Contains(randomString));
        }

        [Test]
        public void GetTopic()
        {
            Topic topic = new Topic(TestDataFactory.RandomTopicName());
            Assert.NotNull(validClient.CreateTopic(topic));

            Topic remoteTopic = validClient.GetTopic(topic.Name);
            Assert.NotNull(remoteTopic);
            Assert.AreEqual(remoteTopic.Name, topic.Name);
        }

        [Test]
        public void GetTopicFailure()
        {
            Assert.Throws<TopicNotFoundException>(
                delegate { validClient.GetTopic(TestDataFactory.RandomTopicName() + "_bad"); });
        }

        [Test]
        public void DeleteTopic()
        {
            Topic topic = new Topic(TestDataFactory.RandomTopicName());
            T("Topic " + topic.Name);
            Assert.NotNull(validClient.CreateTopic(topic));

            bool result = false;
            Assert.DoesNotThrow(
                delegate { result = validClient.DeleteTopic(topic.Name); });
            Assert.IsTrue(result);
        }

        [Test]
        public void DeleteSubscribedTopic()
        {
            Topic topic = new Topic(TestDataFactory.RandomTopicName());
            T("Topic " + topic.Name);
            Assert.NotNull(validClient.CreateTopic(topic));

            bool result = false;
            Assert.DoesNotThrow(
                delegate { result = validClient.DeleteTopic(topic.Name, true); });
            Assert.IsTrue(result);
        }


        [Test]
        public void DeleteSubscribedTopicFailure()
        {
            Topic topic = new Topic(TestDataFactory.RandomTopicName());
            T("Topic " + topic.Name);
            Assert.NotNull(validClient.CreateTopic(topic));

            HttpTopicSubscription sub = new HttpTopicSubscription();
            sub.HttpMethod = HttpTopicSubscriptionMethod.GET;
            sub.URL = "http://into.oblivion/";

            Assert.NotNull(validClient.CreateTopicSubscription(topic, sub));

            bool result = false;
            Assert.Throws<TopicHasSubscriptionsException>(
                delegate { result = validClient.DeleteTopic(topic.Name); });
            Assert.IsFalse(result);
        }

        [Test]
        public void ListTopics()
        {
            List<Topic> topicList = null;

            Assert.DoesNotThrow(
                delegate { topicList = validClient.GetTopicList(); });
            Assert.IsNotNull(topicList);

            T(topicList.Count + " topics found in account");
        }

        [Test]
        public void ListTopicsWithNew()
        {
            Topic topic = new Topic(TestDataFactory.RandomTopicName());
            Topic newTopic = validClient.CreateTopic(topic);
            Assert.IsNotNull(newTopic);

            T("Created topic " + newTopic.Name);

            List<Topic> topicList = null;
            Assert.DoesNotThrow(
                delegate { topicList = validClient.GetTopicList(); });
            Assert.IsNotNull(topicList);
            Assert.IsTrue(topicList.Count > 0);

            Topic remoteTopic = topicList.Find(
                delegate(Topic tmpTopic)
                {
                    return (tmpTopic.Name == newTopic.Name);
                });

            Assert.IsNotNull(remoteTopic);
        }

        [Test]
        public void PushSingleToTopic()
        {
            string topicName = TestDataFactory.RandomTopicName();

            T("Topic " + topicName);
            Topic newTopic = new Topic(topicName);
            Topic topic = validClient.CreateTopic(newTopic);

            Assert.NotNull(topic);
            Assert.AreEqual(topicName, topic.Name);

            string randomBody = TestDataFactory.RandomHexString();
            Message msg = new Message();
            msg.Fields["test Key"] = TestDataFactory.RandomHexString(10);
            msg.Body = randomBody;

            Assert.AreEqual(randomBody, msg.Body);

            Message returnMsg = validClient.PublishToTopic(topic, msg);
            Assert.IsNotEmpty(returnMsg.ID);
            Assert.AreEqual(returnMsg.Body, msg.Body);
            Assert.AreEqual(returnMsg.Fields["test Key"], msg.Fields["test Key"]);

            T("Msg ID:    " + returnMsg.ID);
            T("Msg Body:  " + returnMsg.Body);
            T("Msg Field: testKey=" + returnMsg.Fields["test Key"]);
        }

        [Test]
        public void PushMultipleToTopic()
        {
            string topicName = TestDataFactory.RandomTopicName();

            T("Topic " + topicName);
            Topic topic = validClient.CreateTopic(topicName);

            Assert.NotNull(topic);
            Assert.AreEqual(topicName, topic.Name);

            List<Message> originalMsgs = new List<Message>();
            List<Message> pushedMsgs = new List<Message>();

            Stopwatch timer = Stopwatch.StartNew();
            for (int ii = 0; ii < 100; ii++) {
                string randomBody = TestDataFactory.RandomHexString();

                Message msg = new Message();
                msg.Body = randomBody;
                originalMsgs.Add(msg);

                Assert.AreEqual(randomBody, msg.Body);

                Message pushedMsg = validClient.PublishToTopic(topic, msg);

                Console.Write(".");
                if (ii > 0 && ii % 10 == 0) {
                    Console.Write(ii + "\n");
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
        public void PushMultipleToSubscribedTopic()
        {
            string topicName = TestDataFactory.RandomTopicName();

            T("Topic " + topicName);
            Queue queue = validClient.CreateQueue("csharp_notify_queue_" + TestDataFactory.RandomQueueName());
            Topic topic = validClient.CreateTopic(topicName);
            QueueTopicSubscription sub = validClient.CreateTopicSubscription(
                topic, new QueueTopicSubscription(queue.Name));

            Assert.NotNull(queue);
            Assert.NotNull(topic);
            Assert.NotNull(sub);
            Assert.AreEqual(topicName, topic.Name);
            Assert.AreEqual(sub.QueueName, queue.Name);

            List<Message> originalMsgs = new List<Message>();
            List<Message> pushedMsgs = new List<Message>();

            Stopwatch timer = Stopwatch.StartNew();
            for (int ii = 0; ii < 100; ii++) {
                string randomBody = "(" + ii + ") " + TestDataFactory.RandomHexString(20);

                Message msg = new Message();
                msg.Body = randomBody;
                originalMsgs.Add(msg);

                Assert.AreEqual(randomBody, msg.Body);

                Message pushedMsg = validClient.PublishToTopic(topic, msg);

                Console.Write(".");
                if (ii > 0 && ii % 10 == 0) {
                    Console.Write(ii + "\n");
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

            int poppedCount = 0, expectedPopCount = pushedMsgs.Count;
            int perIterationWaitMsec = 100, totalWait = 0;
            List<Message> poppedMessages = new List<Message>();

            do {
                System.Threading.Thread.Sleep(perIterationWaitMsec);
                totalWait += perIterationWaitMsec;

                List<Message> tmpPoppedMessages = validClient.PopMessages(queue, 10);
                poppedCount += tmpPoppedMessages.Count;

                if (tmpPoppedMessages.Count > 0) {
                    T("Popped " + poppedMessages.Count + " messages:");
                    tmpPoppedMessages.ForEach(delegate(Message m)
                    {
                        validClient.DeleteMessage(queue, m);
                        T(String.Format(" - {0}: {1}", m.ID, m.Body));
                    });
                }

                poppedMessages.AddRange(tmpPoppedMessages);
            } while (poppedCount < expectedPopCount);

            Assert.Greater(poppedCount, 0);
            Assert.AreEqual(poppedCount, pushedMsgs.Count);

            T(String.Format("Total of {0}msec for {1} messages to reach queue.",
                totalWait, poppedCount));

            for (int ii = 0; ii < pushedMsgs.Count; ii++) {
                Message pushedMsg = pushedMsgs[ii];
                Message poppedMsg = poppedMessages.Find(
                    delegate(Message tmpMsg)
                    {
                        return (tmpMsg.Body == pushedMsg.Body);
                    });

                Assert.IsNotNull(poppedMsg);
                Assert.AreEqual(pushedMsg.Body, poppedMsg.Body);
                T(String.Format("Popped message {0:d}: {1} -> {2} {3}",
                    ii, pushedMsg.ID, pushedMsg.ID, poppedMsg.Body));
            }
        }
    }
}
 
