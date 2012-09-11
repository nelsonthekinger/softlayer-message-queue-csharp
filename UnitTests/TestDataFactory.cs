using System;
using NUnit.Framework;

namespace UnitTests
{
    public static class TestDataFactory
    {
        public static string RandomQueueName()
        {
            Random rand = new Random(Randomizer.RandomSeed);
            return "csharpTest_Queue_" + rand.Next(1, int.MaxValue).ToString();
        }

        public static string RandomTopicName()
        {
            Random rand = new Random(Randomizer.RandomSeed);
            return "csharpTest_Topic_" + rand.Next(1, int.MaxValue).ToString();
        }

        public static string RandomHexString(int fixedLength = -1)
        {
            Random rand = new Random(Randomizer.RandomSeed);
            int iterationByteArrayLength = 32;

            string randomString = string.Empty;

            do {
                byte[] buffer = new byte[iterationByteArrayLength];
                rand.NextBytes(buffer);

                foreach (byte b in buffer) {
                    randomString += b.ToString("x");
                }
            } while (randomString.Length < fixedLength);

            if (fixedLength > -1) {
                randomString = randomString.Substring(0, fixedLength);
            }

            return randomString;
        }
    }

}
