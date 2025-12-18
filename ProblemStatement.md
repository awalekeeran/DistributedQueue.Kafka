Problem Statement
----------------------
Design an In-Memory Distributed Queue like Kafka.

Requirements
---------------
1) The queue should be in-memory and should not require access to the file system.
2) There can be multiple topics in the queue.
3) A (string) message can be published on a topic by a producer/publisher and consumers/subscribers can subscribe to the topic to receive the messages.
4) There can be multiple producers and consumers.
5) A producer can publish to multiple topics.
6) A consumer can listen to multiple topics.
7) The consumer should print "<consumer_id> received <message>" on receiving the message.
8) The queue system should be multi-threaded, i.e., messages can be produced or consumed in parallel by different producers/consumers.

Input/Output Format
---------------------
- You do not need to take input from the command-line.
- Create 2 topics: topic1 and topic2
- Create 2 producers: producer1, and producer2
- Create 5 consumers: consumer1, consumer2, consumer3, consumer4, and consumer5
- Make all 5 consumers subscribe to topic1
- Make consumers 1, 3, and 4 subscribe to topic2
- Make producer1 publish message "Message 1" to topic1
- Make producer1 publish message "Message 2" to topic1
- Make producer2 publish message "Message 3" to topic1
- Make producer1 publish message "Message 4" to topic2
- Make producer2 publish message "Message 5" to topic2

Expectations
---------------------
- Make sure that you have a working and demonstrable code
- Make sure that the code is functionally correct
- Code should be modular and readable
- Separation of concern should be addressed
- Please do not write everything in a single file (if not coding in C#)
- Code should easily accommodate new requirements and minimal changes
- There should be a main method from where the code could be easily testable
- [Optional] Write unit tests, if possible
- We can create a GUI as frontend too for sections like Producers, Consumers, Consumer Groups, etc
- Going forward I want to have it plug and play like when I need to add a producer or consumer to a topic,etc

Optional Requirements
---------------------
- Allow consumer groups. A consumer group can have multiple consumers and consumers mention their consumer group while subscribing to a topic. A message can be consumed by only one consumer per consumer group.