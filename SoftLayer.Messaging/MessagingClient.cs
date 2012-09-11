using System;
using System.Collections.Generic;
using System.Configuration;
using RestSharp;
using SoftLayer.Messaging.Primitives;
using MessageList = System.Collections.Generic.List<SoftLayer.Messaging.Message>;
using QueueList = System.Collections.Generic.List<SoftLayer.Messaging.Queue>;
using TopicList = System.Collections.Generic.List<SoftLayer.Messaging.Topic>;
using TopicSubscriptionList = System.Collections.Generic.List<SoftLayer.Messaging.ITopicSubscription>;

namespace SoftLayer.Messaging
{
    public class MessagingClient
    {
        private static bool DebugRequests = false;

        private MessagingApi client = null;
        private EndpointConfig endpointConfig;
        private string dataCenter;
        private bool privateEndpoint = false;

        public bool UsePrivateEndpoint
        {
            get { return this.privateEndpoint; }
            set
            {
                if (value != privateEndpoint) {
                    client.ApiEndpoint =
                        endpointConfig.GetUrlForDatacenter(dataCenter, privateEndpoint);
                }

                this.privateEndpoint = value;
            }
        }

        public string CustomEndpoint
        {
            get { return client.ApiEndpoint; }
            set { client.ApiEndpoint = value; }
        }

        public MessagingClient()
        {
            System.Collections.Specialized.NameValueCollection appSettings =
                ConfigurationManager.AppSettings;

            if (!appSettings.HasKeys()) {
                throw new ConfigurationErrorsException("No app.settings options found.");
            }

            string accountId = appSettings.Get("SoftLayer.Messaging.AccountID");
            string userName = appSettings.Get("SoftLayer.Messaging.UserName");
            string apiKey = appSettings.Get("SoftLayer.Messaging.APIKey");
            this.dataCenter = appSettings.Get("SoftLayer.Messaging.DataCenter");
            bool usePrivateEndpoint = false;

            try {
                usePrivateEndpoint = Convert.ToBoolean(
                    appSettings.Get("SoftLayer.Messaging.PrivateEndpoint"));
            }
            catch (FormatException e) {
                throw new FormatException("PrivateEndpoint must be either true or false.", e);
            }

            this.endpointConfig = new EndpointConfig();
            this.endpointConfig.Load();

            this.client = new MessagingApi(accountId, userName, apiKey,
                endpointConfig.GetUrlForDatacenter(this.dataCenter, usePrivateEndpoint));
        }

        public MessagingClient(string accountId, string userName, string apiKey, string dataCenter, bool usePrivateEndpoint = false)
        {
            EndpointConfig endpointConfig = new EndpointConfig();
            endpointConfig.Load();
            string apiEndpoint = endpointConfig.GetUrlForDatacenter(dataCenter, usePrivateEndpoint);

            this.client = new MessagingApi(accountId, userName, apiKey, apiEndpoint);
            this.UsePrivateEndpoint = usePrivateEndpoint;
            this.dataCenter = dataCenter;
        }

        public bool Ping()
        {
            MessagingRequest request = new MessagingRequest("ping");
            request.HttpStatusSuccessCodes.Add(200);

            string pingResponse = client.Execute(request);

            return "OK" == pingResponse;
        }

        public bool Authenticate()
        {
            MessagingRequest request = new MessagingRequest("auth", Method.POST);
            request.HttpStatusSuccessCodes.Add(200);

            AuthResponse response = client.Execute<AuthResponse>(request);
            if (response.token == null || response.token.Length == 0) {
                throw new InvalidTokenException();
            }

            client.ApiToken = response.token;

            return true;
        }

        public MessagingStats GetStatsForHour()
        {
            return GetStatsForTimeUnit("hour");
        }

        public MessagingStats GetStatsForDay()
        {
            return GetStatsForTimeUnit("day");
        }

        public MessagingStats GetStatsForWeek()
        {
            return GetStatsForTimeUnit("week");
        }

        public MessagingStats GetStatsForMonth()
        {
            return GetStatsForTimeUnit("month");
        }

        private MessagingStats GetStatsForTimeUnit(string timeUnit)
        {
            MessagingStats stats = new MessagingStats();
            MessagingRequest request = new MessagingRequest("stats/{timeUnit}");
            request.AddUrlSegment("timeUnit", timeUnit.ToLower());
            request.HttpStatusSuccessCodes.Add(200);

            StatsResponse response = client.Execute<StatsResponse>(request);

            response.requests.ForEach(
                delegate(StatsMeasurementResponse tmpStat)
                {
                    stats.Requests.Add(StatsMeasurementResponse.ConvertDateKey(tmpStat.key), tmpStat.value);
                });

            response.notifications.ForEach(
                delegate(StatsMeasurementResponse tmpStat)
                {
                    stats.Notifications.Add(StatsMeasurementResponse.ConvertDateKey(tmpStat.key), tmpStat.value);
                });

            return stats;
        }

        public QueueList GetQueueList()
        {
            return GetQueueList((IEnumerable<string>)null);
        }

        public QueueList GetQueueList(string tagName)
        {
            return GetQueueList(new string[] { tagName });
        }

        public QueueList GetQueueList(params string[] tagList)
        {
            return GetQueueList(tagList);
        }

        public QueueList GetQueueList(IEnumerable<string> tagList)
        {
            QueueList queueList = new QueueList();
            MessagingRequest request = new MessagingRequest("queues");
            request.HttpStatusSuccessCodes.Add(200);

            if (tagList != null) {
                request.AddParameter("tags", new TagList(tagList).ToString(","));
            }

            EntityListResponse<QueueResponse> queueListResponse =
                client.Execute<EntityListResponse<QueueResponse>>(request);

            foreach (QueueResponse tmpQueue in queueListResponse.items) {
                Queue queue = new Queue(tmpQueue.name);
                queue.Expiration = tmpQueue.expiration;
                queue.VisibilityInterval = tmpQueue.visibility_interval;
                queue.Tags = new TagList(tmpQueue.tags);

                queueList.Add(queue);
            }

            return queueList;
        }

        public TopicList GetTopicList()
        {
            return GetTopicList((IEnumerable<string>)null);
        }

        public TopicList GetTopicList(bool populateSubscriptions)
        {
            return GetTopicList(null, populateSubscriptions);
        }

        public TopicList GetTopicList(params string[] tagList)
        {
            return GetTopicList(tagList);
        }

        public TopicList GetTopicList(IEnumerable<string> tagList, bool populateSubscriptions = false)
        {
            TopicList topicList = new TopicList();
            MessagingRequest request = new MessagingRequest("topics");
            request.HttpStatusSuccessCodes.Add(200);

            if (tagList != null) {
                request.AddParameter("tags", new TagList(tagList).ToString(","));
            }

            EntityListResponse<TopicResponse> topicListResponse =
                client.Execute<EntityListResponse<TopicResponse>>(request);

            foreach (TopicResponse tmpTopic in topicListResponse.items) {
                Topic topic = new Topic(tmpTopic.name);

                if (tmpTopic.tags != null) {
                    topic.Tags = new TagList(tmpTopic.tags);
                }

                if (populateSubscriptions) {
                    topic.Subscriptions = GetTopicSubscriptionList(topic);
                }

                topicList.Add(topic);
            }

            return topicList;
        }

        public TopicSubscriptionList GetTopicSubscriptionList(Topic topic)
        {
            return GetTopicSubscriptionList(topic.Name);
        }

        public TopicSubscriptionList GetTopicSubscriptionList(string topicName)
        {
            TopicSubscriptionList subList = new TopicSubscriptionList();
            MessagingRequest request = new MessagingRequest("topics/{topicName}/subscriptions");
            request.AddUrlSegment("topicName", topicName);
            request.HttpStatusSuccessCodes.Add(200);

            EntityListResponse<SubscriptionResponse> topicSubscriptionResponse =
                client.Execute<EntityListResponse<SubscriptionResponse>>(request);

            foreach (SubscriptionResponse tmpSubscription in topicSubscriptionResponse.items) {
                if (tmpSubscription.endpoint_type.ToLower() == "http") {
                    HttpTopicSubscription sub = new HttpTopicSubscription();
                    sub.ID = tmpSubscription.id;
                    sub.Body = tmpSubscription.endpoint.body;
                    sub.URL = tmpSubscription.endpoint.url;
                    sub.Headers = tmpSubscription.endpoint.headers;
                    sub.Parameters = tmpSubscription.endpoint.@params;

                    switch (tmpSubscription.endpoint.method.ToUpper()) {
                        case "POST":
                            sub.HttpMethod = HttpTopicSubscriptionMethod.POST;
                            break;
                        case "GET":
                            sub.HttpMethod = HttpTopicSubscriptionMethod.GET;
                            break;
                        case "PUT":
                            sub.HttpMethod = HttpTopicSubscriptionMethod.PUT;
                            break;
                    }

                    subList.Add(sub);
                }
                else if (tmpSubscription.endpoint_type.ToLower() == "queue") {
                    QueueTopicSubscription sub = new QueueTopicSubscription();
                    sub.ID = tmpSubscription.id;
                    sub.QueueName = tmpSubscription.endpoint.queue_name;
                    subList.Add(sub);
                }
            }

            return subList;
        }

        public Queue GetQueue(string name)
        {
            MessagingRequest request = new MessagingRequest("queues/{queueName}", Method.GET);
            request.AddUrlSegment("queueName", name);
            request.HttpStatusSuccessCodes.Add(200);
            request.HttpStatusExceptionMap.Add(404, typeof(QueueNotFoundException));

            QueueResponse response = client.Execute<QueueResponse>(request);

            Queue tmpQueue = new Queue();
            tmpQueue.Name = response.name;
            tmpQueue.Expiration = response.expiration;
            tmpQueue.VisibilityInterval = response.visibility_interval;
            tmpQueue.Tags = new TagList(response.tags);

            return tmpQueue;
        }

        public Topic GetTopic(string name, bool getSubscriptions = false)
        {
            MessagingRequest request = new MessagingRequest("topics/{topicName}", Method.GET);
            request.AddUrlSegment("topicName", name);
            request.HttpStatusSuccessCodes.Add(200);
            request.HttpStatusExceptionMap.Add(404, typeof(TopicNotFoundException));

            TopicResponse response = client.Execute<TopicResponse>(request);

            Topic tmpTopic = new Topic();
            tmpTopic.Name = response.name;
            tmpTopic.Tags = new TagList(response.tags);

            if (getSubscriptions) {
                tmpTopic.Subscriptions = GetTopicSubscriptionList(tmpTopic.Name);
            }

            return tmpTopic;
        }

        public Queue CreateQueue(string name)
        {
            return CreateQueue(name, Queue.DefaultExpiration, Queue.DefaultVisibilityInterval, new TagList());
        }

        public Queue CreateQueue(string name, TagList tags)
        {
            return CreateQueue(name, Queue.DefaultExpiration, Queue.DefaultVisibilityInterval, tags);
        }

        public Queue CreateQueue(string name, int expiration)
        {
            return CreateQueue(name, expiration, Queue.DefaultVisibilityInterval, new TagList());
        }

        public Queue CreateQueue(string name, int expiration, int visibilityInterval)
        {
            return CreateQueue(name, expiration, Queue.DefaultVisibilityInterval, new TagList());
        }

        public Queue CreateQueue(string name, int expiration, int visibilityInterval, TagList tags)
        {
            Queue newQueue = new Queue(name);
            newQueue.Expiration = expiration;
            newQueue.VisibilityInterval = visibilityInterval;
            newQueue.Tags = tags;

            return CreateQueue(newQueue);
        }

        public Queue CreateQueue(Queue queue)
        {
            return CreateOrUpdateQueue(queue, true);
        }

        public Queue UpdateQueue(Queue queue)
        {
            return CreateOrUpdateQueue(queue, false);
        }

        private Queue CreateOrUpdateQueue(Queue queue, bool create = true)
        {
            MessagingRequest request = new MessagingRequest("queues/{queueName}", Method.PUT);
            request.AddUrlSegment("queueName", queue.Name);
            request.HttpStatusSuccessCodes.Add(200);
            request.HttpStatusExceptionMap.Add(400, typeof(BadRequestException));

            if (create) {
                request.HttpStatusSuccessCodes.Add(201);
            }
            else {
                request.HttpStatusSuccessCodes.Add(200);
            }

            object postObj = new {
                name = queue.Name,
                expiration = queue.Expiration,
                visibility_interval = queue.VisibilityInterval,
                tags = queue.Tags
            };

            if (DebugRequests) {
                Console.WriteLine("-> " + request.JsonSerializer.Serialize(postObj));
            }

            request.AddBody(postObj);

            QueueResponse response = client.Execute<QueueResponse>(request);

            queue.Name = response.name;
            queue.Expiration = response.expiration;
            queue.VisibilityInterval = response.visibility_interval;
            queue.Tags = new TagList(response.tags);

            return queue;
        }

        public Topic CreateTopic(string topicName)
        {
            Topic topic = new Topic(topicName);

            return CreateOrUpdateTopic(topic, true);
        }

        public Topic CreateTopic(string topicName, TagList tags)
        {
            Topic topic = new Topic(topicName);
            topic.Tags = tags;

            return CreateOrUpdateTopic(topic, true);
        }

        public Topic CreateTopic(Topic topic)
        {
            return CreateOrUpdateTopic(topic, true);
        }

        public Topic UpdateTopic(Topic topic)
        {
            return CreateOrUpdateTopic(topic, false);
        }

        private Topic CreateOrUpdateTopic(Topic topic, bool create = true)
        {
            MessagingRequest request = new MessagingRequest("topics/{topicName}", Method.PUT);
            request.AddUrlSegment("topicName", topic.Name);
            request.HttpStatusSuccessCodes.Add(200);
            request.HttpStatusExceptionMap.Add(400, typeof(BadRequestException));

            if (create) {
                request.HttpStatusSuccessCodes.Add(201);
            }
            else {
                request.HttpStatusSuccessCodes.Add(200);
            }

            object postObj = new {
                tags = topic.Tags
            };

            if (DebugRequests) {
                Console.WriteLine("-> " + request.JsonSerializer.Serialize(postObj));
            }

            request.AddBody(postObj);

            TopicResponse response = client.Execute<TopicResponse>(request);

            topic.Name = response.name;
            topic.Tags = new TagList(response.tags);

            return topic;
        }

        public HttpTopicSubscription CreateTopicSubscription(Topic topic, HttpTopicSubscription subscription)
        {
            MessagingRequest request = new MessagingRequest("topics/{topicName}/subscriptions", Method.POST);
            request.AddUrlSegment("topicName", topic.Name);
            request.HttpStatusSuccessCodes.Add(200);
            request.HttpStatusExceptionMap.Add(400, typeof(BadRequestException));
            request.HttpStatusSuccessCodes.Add(201);

            object postObj = new {
                endpoint_type = "http",
                endpoint = new {
                    method = subscription.HttpMethod.ToString(),
                    url = subscription.URL,
                    // Escaping the params member with an @ is required, as "params" is a reserved word in C#.
                    @params = subscription.Parameters,
                    headers = subscription.Headers,
                    body = subscription.Body
                }
            };

            if (DebugRequests) {
                Console.WriteLine("-> " + request.JsonSerializer.Serialize(postObj));
            }

            request.AddBody(postObj);

            SubscriptionResponse response = client.Execute<SubscriptionResponse>(request);
            subscription.ID = response.id;
            subscription.URL = response.endpoint.url;
            subscription.Headers = response.endpoint.headers;
            subscription.Parameters = response.endpoint.@params;
            subscription.Body = response.endpoint.body;

            switch (response.endpoint.method) {
                case "GET":
                    subscription.HttpMethod = HttpTopicSubscriptionMethod.GET;
                    break;
                case "POST":
                    subscription.HttpMethod = HttpTopicSubscriptionMethod.POST;
                    break;
                case "PUT":
                    subscription.HttpMethod = HttpTopicSubscriptionMethod.PUT;
                    break;
            }

            return subscription;
        }

        public QueueTopicSubscription CreateTopicSubscription(Topic topic, QueueTopicSubscription subscription)
        {
            MessagingRequest request = new MessagingRequest("topics/{topicName}/subscriptions", Method.POST);
            request.AddUrlSegment("topicName", topic.Name);
            request.HttpStatusSuccessCodes.Add(200);
            request.HttpStatusExceptionMap.Add(400, typeof(BadRequestException));
            request.HttpStatusSuccessCodes.Add(201);

            object postObj = new {
                endpoint_type = "queue",
                endpoint = new {
                    queue_name = subscription.QueueName
                }
            };

            if (DebugRequests) {
                Console.WriteLine("-> " + request.JsonSerializer.Serialize(postObj));
            }

            request.AddBody(postObj);

            SubscriptionResponse response = client.Execute<SubscriptionResponse>(request);
            subscription.ID = response.id;
            subscription.QueueName = response.endpoint.queue_name;

            return subscription;
        }

        public MessageList PublishToQueue(Queue queue, MessageList messages)
        {
            return PublishToQueue(queue.Name, messages);
        }

        public MessageList PublishToQueue(string queueName, MessageList messages)
        {
            MessageList responses = new MessageList();
            foreach (Message tmpMessage in messages) {
                responses.Add(PublishToQueue(queueName, tmpMessage));
            }

            return responses;
        }

        public Message PublishToQueue(Queue queue, Message message)
        {
            return PublishToQueue(queue.Name, message);
        }

        public Message PublishToQueue(string queueName, Message message)
        {
            MessagingRequest request = new MessagingRequest("queues/{queueName}/messages", Method.POST);
            request.AddUrlSegment("queueName", queueName);
            request.HttpStatusSuccessCodes.Add(201);
            request.HttpStatusExceptionMap.Add(404, typeof(QueueNotFoundException));

            object postObj = new {
                body = message.Body,
                fields = message.Fields,
                visibility_delay = message.VisibilityDelay,
                visibility_interval = message.VisibilityInterval
            };

            if (DebugRequests) {
                Console.WriteLine("-> " + request.JsonSerializer.Serialize(postObj));
            }

            request.AddBody(postObj);

            MessageResponse response = client.Execute<MessageResponse>(request);
            Message tmpMessage = new Message();
            tmpMessage.ID = response.id;
            tmpMessage.Body = response.body;
            tmpMessage.Fields = new FieldList(response.fields);
            tmpMessage.VisibilityDelay = response.visibility_delay;
            tmpMessage.VisibilityInterval = response.visibility_interval;
            tmpMessage.SetInitialEntryTime(response.initial_entry_time);

            return tmpMessage;
        }

        public MessageList PublishToTopic(Topic topic, MessageList messages)
        {
            return PublishToTopic(topic.Name, messages);
        }

        public MessageList PublishToTopic(string topicName, MessageList messages)
        {
            MessageList responses = new MessageList();
            foreach (Message tmpMessage in messages) {
                responses.Add(PublishToTopic(topicName, tmpMessage));
            }

            return responses;
        }

        public Message PublishToTopic(Topic topic, Message message)
        {
            return PublishToTopic(topic.Name, message);
        }

        public Message PublishToTopic(string topicName, Message message)
        {
            MessagingRequest request = new MessagingRequest("topics/{topicName}/messages", Method.POST);
            request.AddUrlSegment("topicName", topicName);
            request.HttpStatusSuccessCodes.Add(201);
            request.HttpStatusExceptionMap.Add(404, typeof(TopicNotFoundException));

            object postObj = new {
                body = message.Body,
                fields = message.Fields,
                visibility_delay = message.VisibilityDelay
            };

            if (DebugRequests) {
                Console.WriteLine("-> " + request.JsonSerializer.Serialize(postObj));
            }

            request.AddBody(postObj);

            MessageResponse response = client.Execute<MessageResponse>(request);
            Message tmpMessage = new Message();
            tmpMessage.ID = response.id;
            tmpMessage.Body = response.body;
            tmpMessage.Fields = new FieldList(response.fields);
            tmpMessage.VisibilityDelay = response.visibility_delay;
            tmpMessage.SetInitialEntryTime(response.initial_entry_time);

            return tmpMessage;
        }

        public Message PopMessage(Queue queue)
        {
            return PopMessage(queue.Name);
        }

        public Message PopMessage(string queueName)
        {
            MessageList messages = PopMessages(queueName, 1);
            if (messages.Count == 0) {
                return null;
            }

            return messages[0];
        }

        public MessageList PopMessages(Queue queue, int maxMessages = 1)
        {
            return PopMessages(queue.Name, maxMessages);
        }

        public MessageList PopMessages(string queueName, int maxMessages = 1)
        {
            if (maxMessages <= 0 || maxMessages > Queue.MaxMessagesPerPop) {
                throw new ArgumentOutOfRangeException("maxMessages");
            }

            MessageList messages = new MessageList();

            MessagingRequest request = new MessagingRequest("queues/{queueName}/messages", Method.GET);
            request.AddUrlSegment("queueName", queueName);
            request.AddParameter("batch", maxMessages, ParameterType.GetOrPost);
            request.HttpStatusSuccessCodes.Add(200);
            request.HttpStatusExceptionMap.Add(404, typeof(QueueNotFoundException));

            MessageListResponse response = client.Execute<MessageListResponse>(request);

            response.items.ForEach(delegate(MessageResponse tmpMessage)
            {
                Message message = new Message();
                message.ID = tmpMessage.id;
                message.Body = tmpMessage.body;
                message.SetInitialEntryTime(tmpMessage.initial_entry_time);
                message.VisibilityDelay = tmpMessage.visibility_delay;
                message.VisibilityInterval = tmpMessage.visibility_interval;

                if (tmpMessage.fields != null) {
                    message.Fields = new FieldList(tmpMessage.fields);
                }

                messages.Add(message);
            });

            return messages;
        }

        public bool DeleteQueue(Queue queue, bool forceDeletion = false)
        {
            return DeleteQueue(queue.Name, forceDeletion);
        }

        public bool DeleteQueue(string queueName, bool forceDeletion = false)
        {
            MessagingRequest request = new MessagingRequest("queues/{queueName}", Method.DELETE);
            request.AddUrlSegment("queueName", queueName);
            request.HttpStatusSuccessCodes.Add(202);
            request.HttpStatusExceptionMap.Add(404, typeof(QueueNotFoundException));
            request.HttpStatusExceptionMap.Add(409, typeof(QueueNotEmptyException));

            if (forceDeletion) {
                request.AddParameter("force", "true", ParameterType.GetOrPost);
            }

            ApiResponse response = client.Execute<ApiResponse>(request);

            return true;
        }

        public bool DeleteTopic(Topic topic, bool forceDeletion = false)
        {
            return DeleteTopic(topic.Name, forceDeletion);
        }

        public bool DeleteTopic(string topicName, bool forceDeletion = false)
        {
            MessagingRequest request = new MessagingRequest("topics/{topicName}", Method.DELETE);
            request.AddUrlSegment("topicName", topicName);
            request.HttpStatusSuccessCodes.Add(202);
            request.HttpStatusExceptionMap.Add(404, typeof(TopicNotFoundException));
            request.HttpStatusExceptionMap.Add(409, typeof(TopicHasSubscriptionsException));

            if (forceDeletion) {
                request.AddParameter("force", "true", ParameterType.GetOrPost);
            }

            ApiResponse response = client.Execute<ApiResponse>(request);

            return true;
        }

        public bool DeleteTopicSubscription(Topic topic, ITopicSubscription subscription)
        {
            return DeleteTopicSubscription(topic.Name, subscription.GetID());
        }

        public bool DeleteTopicSubscription(Topic topic, string subscriptionId)
        {
            return DeleteTopicSubscription(topic.Name, subscriptionId);
        }

        public bool DeleteTopicSubscription(string topicName, string subscriptionId)
        {
            MessagingRequest request = new MessagingRequest("topics/{topicName}/subscriptions/{subscriptionId}", Method.DELETE);
            request.AddUrlSegment("topicName", topicName);
            request.AddUrlSegment("subscriptionId", subscriptionId);
            request.HttpStatusSuccessCodes.Add(202);
            request.HttpStatusExceptionMap.Add(404, typeof(TopicSubscriptionNotFoundException));

            ApiResponse response = client.Execute<ApiResponse>(request);

            return true;
        }

        public bool DeleteMessage(Queue queue, Message message)
        {
            return DeleteMessage(queue.Name, message.ID);
        }

        public bool DeleteMessage(Queue queue, string messageId)
        {
            return DeleteMessage(queue.Name, messageId);
        }

        public bool DeleteMessage(string queueName, string messageId)
        {
            MessagingRequest request = new MessagingRequest("queues/{queueName}/messages/{messageId}", Method.DELETE);
            request.AddUrlSegment("queueName", queueName);
            request.AddUrlSegment("messageId", messageId);
            request.HttpStatusSuccessCodes.Add(202);
            request.HttpStatusExceptionMap.Add(404, typeof(MessageNotFoundException));

            ApiResponse response = client.Execute<ApiResponse>(request);

            return true;
        }

    }
}
