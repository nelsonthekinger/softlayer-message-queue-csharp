SoftLayer Message Queue - C# .NET Client
========================================
This code provides .NET v3.5 bindings written in C# to communicate with the
[SoftLayer Message Queue API](http://sldn.softlayer.com/reference/messagequeueapi).


Requirements
------------
The following are required to build and use the Message Queue library:

* Microsoft .NET 3.5 Client Profile or higher (we've tested in 4.0 as well)
* RestSharp v104.0+ (this is available via NuGet, if you use it; otherwise, a
  pre-built binary is included in the `Dependencies` folder. We've also
  included this specific release of RestSharp under `Dependencies\RestSharp` as
  a git submodule, in case you wish to build this specific release yourself).
* A SoftLayer customer account with message queue service already activated.
  *(Since the service is usage-based, the initial order required to activate it
  on your account does not charge anything. During each billing period
  thereafter, you will be billed based on your usage over that period.)*
 

Building and Using
------------------
You may include the code in your Visual Studio solution as a reference and build
it from there alongside your main project, or you can build separately and add
the binary to your project as a reference.

Once you have included the client as a reference, you can use the
`SoftLayer.Messaging` namespace in your code and follow the usage examples below
to get started.

If you wish to run the supplied unit tests, you'll need NUnit with its shadow
copying disabled.


Usage Examples
--------------
Once you know your user name, account ID, and API key, and datacenter (all can
be obtained from the SoftLayer Customer Portal), you can pass them into a new
MessagingClient instance. This instance provides the conduit through which you
must pass Queue, Message, Topic, and ITopicSubscription-based objects to the
Message Queue API with your requests.


### Authentication (directly in code)

First, make sure you're using the Messaging namespace:

    using SoftLayer.Messaging;

Next, you can initialize a client session to the API:

    string myUserName = "happycustomer";
    string myAccountId = "5yc2z";
    string myApiKey = "98b0c8158e633d5c5ed63ad24584cadfa0e6e047c4c9e7e3adb3368aaa02964";
    string myDatacenter = "DAL05";

    MessagingClient client = new MessagingClient(myUserName, myAccountId, myApiKey, myDatacenter);
    try {
        client.Authenticate();
    }
    catch (UnauthorizedException e) {
        Console.WriteLine("Exception: " + e.Message);
    }


### Authentication (via app.config)

Alternatively, you may use your application's app.config file to store your
Message Queue credentials. An example is located under
**SoftLayer.Messaging\example.config**, but follows this format:

    <!--your application's app.config-->
    <?xml version="1.0" encoding="utf-8" ?>
    <configuration>
      <appSettings>
        <add key="SoftLayer.Messaging.AccountID" value="5yc2z" />
        <add key="SoftLayer.Messaging.UserName" value="happycustomer" />
        <add key="SoftLayer.Messaging.APIKey" value="98b0c8158e633d5c5ed63ad24584cadfa0e6e047c4c9e7e3adb3368aaa02964" />
        <add key="SoftLayer.Messaging.DataCenter" value="dal05" />
        <add key="SoftLayer.Messaging.PrivateEndpoint" value="false" />
      </appSettings>
    </configuration>

You can then call the `MessagingClient` constructor with no arguments, and it
will use the settings you've defined in app.config.

    MessagingClient client = new MessagingClient();

### Queues

After successfully authenticating, you can create a new queue several ways:

    Queue widgetQueue = client.CreateQueue("widget_queue");

    // or...

    Queue widgetQueue = new Queue("widget_queue");
    client.CreateQueue(widgetQueue);

Once you've created your new queue, you can begin pushing messages to it easily:

    Message myMessage = new Message("Hello, world!");
    client.PublishToQueue(widgetQueue, myMessage);

    // or...

    Message myMessage = client.PublishToQueue(
        widgetQueue,
        new Message("Hello, world!"));

The Message object you receive back from `MessagingClient::PublishToQueue()` will
provide the resulting unique message ID. 

While not always necessary, you might wish to keep track of the message ID so
that you can explicitly delete it later once you've retrieved it from the queue,
or to accomodate more advanced scenarios like redundancy checks within your
application that are based on a message's unique ID.


### Topics

Topics allow for more advanced scenarios where you may wish to publish messages
to multiple queues, or even notify remote endpoints via HTTP/HTTPS, but they
work much the same way on the client side. Just like queues, you can publish
messages directly to them.

For additional information on how topics work and the kinds of scenarios they
can accomodate, see our 
[Exploring Topics guide](http://sldn.softlayer.com/article/Message-Queue-Exploring-Topics).

Creating a new topic is just as straightforward as creating a new queue:

    Topic widgetsTopic = client.CreateTopic("widgets");

    // or:

    Topic widgetsTopic = new Topic("widgets");
    client.createTopic(widgetsTopic);

Since topics simply serve as a single point of input that routes messages to as
many different places as needed, they don't store messages themselves, so at
least one topic subscription is necessary.

Now that the widgets topic exists, we'll create a `QueueTopicSubscription` to
allow our `widget_queue` to receive messages published to the `widgets` topic:

    QueueTopicSubscription queueSubscription = 
        new QueueTopicSubscription(widgetQueue.Name);
    client.CreateTopicSubscription(widgetsTopic, queueSubscription);

We may also wish to receive an HTTP POST to a remote URL each time a message is
published to `widgets`. In this case, we use a `HttpTopicSubscription` instance,
optionally specifying additional query string parameters (via the `Parameters`
property) HTTP headers (via the `Headers` property), or, in the case of a POST
or PUT, a corresponding HTTP body (the `Body` property).

HTTP notifications can include variables that include various bits of
information about each message published to a topic. These variables are
enclosed by curly braces ("{" and "}") and can be included in the subscription's
URL, headers, body, and URL parameters.

    HttpTopicSubscription httpSubscription = new
        HttpTopicSubscription("https://example.com/api/topic_notify");
    httpSubscription.HttpMethod = HttpTopicSubscriptionMethod.POST;
    httpSubscription.Parameters.Add("topic_id", "{topic_id}");
    httpSubscription.Parameters.Add("subscription_id", "{subscription_id}");
    httpSubscription.Headers.Add("X-Topic-Id", "{topic_id}");
    httpSubscription.Headers.Add("X-Subscription-Id", "{subscription_id}");
    httpSubscription.Body =
        "{\"topic_id\":\"{topic_id}\",\"body\":\"{body}\",\"widget\":\"{widget\"}";

    client.CreateTopicSubscription("widgets", httpSubscription);

From now on, any messages published to our topic ``widgets`` will publish into
the previously created ``widget_queue`` queue, and an HTTP POST request will
also be sent to the URL we've specified in the HTTP subscription. If we include
a field in each message with the key "widget", its value will also be
substituted within the HTTP notification body.

Publishing to a topic is nearly identical as publishing to a lone queue:

    // Simplest form:
    client.PublishToTopic(widgetsTopic, new Message("Hello, universe!"));

    // Alternatively:
    Message myMessage = new Message("Hello, universe!");
    client.PublishToTopic("widgets", myMessage);

### Consuming messages

Messages submitted through queues and topics can be consumed by periodically
polling a queue for messages.

Assuming we've created a method called `processMessage`, which accepts a Message
object as its only argument and returns a boolean value indicating whether or
not the message was successfully processed, the main message polling loop might
look like the following:

    string queueName = "widget_queue";
    int sleepTimeMsec = 2000; // 2 seconds

    while (true) {
        Message message = client.PopMessage(queueName);

        if (message != null) {
            if (processMessage(message)) {
                client.DeleteMessage(queueName, message.ID);
            }
        }

        System.Threading.Thread.Sleep(sleepTimeMsec);
    }

The above ensures messages would only be deleted after being successfully
processed; you can tune your visibility intervals over time to ensure another
worker doesn't pick up the same message within the average amount of time it
takes to process a single message.

Messages can also be popped in groups by calling the
`MessagingClient::PopMessages` method (plural) instead, which will always return
a List<Message>, even if empty.

    string queueName = "widget_queue";
    int maxPopCount = 5;

    while (true) {
        List<Message> messages = client.PopMessages(queueName, maxPopCount);

        foreach (Message message in messages) {
            if (processMessage(message)) {
                client.DeleteMessage(queueName, message.ID);
            }
        }
    }

### Deleting topics and queues

If you use queues and topics in more of a time-based manner you may wish to
often delete them. This is accomplished with `DeleteQueue` and `DeleteTopic`:

    client.DeleteQueue(widgetsQueue); // providing a Queue object is allowed
    client.DeleteQueue("myqueue");

    client.DeleteTopic(widgetsTopic); // providing a Topic object is allowed
    client.DeleteTopic("mytopic");

Subscriptions can also be deleted individually using `DeleteTopicSubscription`.

Queues cannot be deleted unless you have consumed or deleted all messages from
them; to force the deletion of the queue and all its messages, you can
optionally pass a true boolean value as the third argument to `DeleteQueue`.

Topics follow the same rule--if subscriptions are attached to a topic and you
wish to delete the topic without having to iterate through and delete each
subscription individually, you can pass in a true boolean value to `DeleteTopic`
as an optional second argument.

Need more help?
---------------

For additional guidance and information, check out the
[Message Queue API reference](http://sldn.softlayer.com/reference/messagequeueapi) 
or the [SoftLayer Developer Network forum](https://forums.softlayer.com/forumdisplay.php?f=27).

For specific issues with the C# client library, get in touch with us via the
[SoftLayer Developer Network forum](https://forums.softlayer.com/forumdisplay.php?f=27)
or the [Issues page on our GitHub repository](https://github.com/softlayer/softlayer-message-queue-csharp/issues).

License
-------

Copyright (c) 2012 SoftLayer Technologies, Inc.

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
of the Software, and to permit persons to whom the Software is furnished to do
so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
