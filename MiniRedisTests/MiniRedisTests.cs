using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MiniRedisTests
{
    [TestClass]
    public class MiniRedisTests
    {
        //please, run the server first
        public async Task<HttpResponseMessage> CallServer(string command)
        {
            var client = new HttpClient();
            return await client.GetAsync(
                "http://localhost:8080/?cmd=" + command);
        }

        [TestMethod]
        public async Task SetElements_GetInDifferentTime()
        {
            // Arrange
            const string command1 = "set key \"value\"";
            const string command2 = "get key";
            const string command3 = "set key2 \"value2\" ex 3";
            const string command4 = "get key2";

            // Act
            var response1 = await CallServer(command1);
            var response2 = await CallServer(command2);
            var response3 = await CallServer(command3);
            var response4 = await CallServer(command4);

            var respMessage1 = response1.Content.ReadAsStringAsync().Result;
            var respMessage2 = response2.Content.ReadAsStringAsync().Result;
            var respMessage3 = response3.Content.ReadAsStringAsync().Result;
            var respMessage4 = response4.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.IsTrue(response1.IsSuccessStatusCode);
            Assert.IsTrue(response2.IsSuccessStatusCode);
            Assert.IsTrue(response3.IsSuccessStatusCode);
            Assert.IsTrue(response4.IsSuccessStatusCode);
            
            Assert.AreEqual("Ok", respMessage1);
            Assert.AreEqual("value",respMessage2);
            
            Assert.AreEqual("Ok",respMessage3);
            Assert.AreEqual("value2",respMessage4);

            await Task.Run(async () => await Task.Delay(2000));

            var responseAfterSomeTime = await CallServer(command4);
            var respMessage5 = responseAfterSomeTime.Content.ReadAsStringAsync().Result;

            Assert.AreEqual("value2", respMessage5);

            await Task.Run(async () => await Task.Delay(1000));

            var responseAfterExpiredTime = await CallServer(command4);
            var respMessage6 = responseAfterExpiredTime.Content.ReadAsStringAsync().Result;

            Assert.AreEqual("null", respMessage6);
        }
        [TestMethod]
        public async Task GetCommands()
        {
            // Arrange
            const string command1 = "set key \"value\"";
            const string command2 = "get key";
            const string command3 = "get key \"value\"";
            const string command4 = "get key2";

            // Act
            var response1 = await CallServer(command1);
            var response2 = await CallServer(command2);
            var response3 = await CallServer(command3);
            var response4 = await CallServer(command4);

            var respMessage1 = response1.Content.ReadAsStringAsync().Result;
            var respMessage2 = response2.Content.ReadAsStringAsync().Result;
            var respMessage3 = response3.Content.ReadAsStringAsync().Result;
            var respMessage4 = response4.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.IsTrue(response1.IsSuccessStatusCode);
            Assert.IsTrue(response2.IsSuccessStatusCode);
            Assert.IsTrue(response3.IsSuccessStatusCode);
            Assert.IsTrue(response4.IsSuccessStatusCode);

            Assert.AreEqual("Ok", respMessage1);
            Assert.AreEqual("value", respMessage2);

            Assert.AreEqual("Ok", respMessage3);
            Assert.AreEqual("null", respMessage4);
        }
        [TestMethod]
        public async Task Delete_TryGet()
        {
            // Arrange
            const string command1 = "set key \"value\"";
            const string command2 = "get key";
            const string command3 = "set key2 \"value2\"";
            const string command4 = "get key2";
            const string command5 = "del key";
            const string command6 = "del key key2";

            // Act
            //clean the base 
            await CallServer("del key key2");
            var setKey = await CallServer(command1);
            var getKey = await CallServer(command2);
            var setKey2 = await CallServer(command3);
            var getKey2 = await CallServer(command4);
            var deleteKey = await CallServer(command5);
            var getKeyAfterDelete = await CallServer(command2);
            var setKeyAgain = await CallServer(command1);
            var getKeyAgain = await CallServer(command2);
            var deleteAll = await CallServer(command6);
            var tryDeleteAfterDelete = await CallServer(command5);
            var getKeyAfterDeleteAll = await CallServer(command2);
            var getKey2AfterDeleteAll = await CallServer(command4);
            
            var respSetKey = setKey.Content.ReadAsStringAsync().Result;
            var respGetKey = getKey.Content.ReadAsStringAsync().Result;
            var respSetKey2 = setKey2.Content.ReadAsStringAsync().Result;
            var respDeleteKey = deleteKey.Content.ReadAsStringAsync().Result;
            var respGetKeyAfterDelete = getKeyAfterDelete.Content.ReadAsStringAsync().Result;
            var respSetKeyAgain = setKeyAgain.Content.ReadAsStringAsync().Result;
            var respGetKeyAgain = getKeyAgain.Content.ReadAsStringAsync().Result;
            var respDeleteAll = deleteAll.Content.ReadAsStringAsync().Result;
            var respTryDeleteAfterDelete = tryDeleteAfterDelete.Content.ReadAsStringAsync().Result;
            var respGetKeyAfterDeleteAll = getKeyAfterDeleteAll.Content.ReadAsStringAsync().Result;
            var respGetKey2AfterDeleteAll = getKey2AfterDeleteAll.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.IsTrue(setKey.IsSuccessStatusCode);
            Assert.IsTrue(getKey.IsSuccessStatusCode);
            Assert.IsTrue(setKey2.IsSuccessStatusCode);
            Assert.IsTrue(getKey2.IsSuccessStatusCode);
            Assert.IsTrue(deleteKey.IsSuccessStatusCode);
            Assert.IsTrue(getKeyAfterDelete.IsSuccessStatusCode);
            Assert.IsTrue(setKeyAgain.IsSuccessStatusCode);
            Assert.IsTrue(getKeyAgain.IsSuccessStatusCode);
            Assert.IsTrue(deleteAll.IsSuccessStatusCode);
            Assert.IsTrue(tryDeleteAfterDelete.IsSuccessStatusCode);
            Assert.IsTrue(getKeyAfterDeleteAll.IsSuccessStatusCode);
            Assert.IsTrue(getKey2AfterDeleteAll.IsSuccessStatusCode);

            Assert.AreEqual("Ok", respSetKey);
            Assert.AreEqual("value", respGetKey);
            Assert.AreEqual("Ok", respSetKey2);
            Assert.AreEqual("1", respDeleteKey);
            Assert.AreEqual("null", respGetKeyAfterDelete);
            Assert.AreEqual("Ok", respSetKeyAgain);
            Assert.AreEqual("value", respGetKeyAgain);
            Assert.AreEqual("2", respDeleteAll);
            Assert.AreEqual("0", respTryDeleteAfterDelete);
            Assert.AreEqual("null", respGetKeyAfterDeleteAll);
            Assert.AreEqual("null", respGetKey2AfterDeleteAll);
        }
        [TestMethod]
        public async Task SetValue_GetDBSize()
        {
            // Arrange
            const string command1 = "set key \"value\"";
            const string command2 = "set key2 \"value2\"";
            const string command3 = "dbsize";
            const string command4 = "del key";
            
            // Act
            var setKey = await CallServer(command1);
            var setKey2 = await CallServer(command2);
            var getDbSize = await CallServer(command3);
            var deleteKey = await CallServer(command4);
            var getDbSizeAfterDelete = await CallServer(command3);
            
            var respSetKey = setKey.Content.ReadAsStringAsync().Result;
            var respSetKey2 = setKey2.Content.ReadAsStringAsync().Result;
            var respGetDbSize = getDbSize.Content.ReadAsStringAsync().Result;
            var respDeleteKey = deleteKey.Content.ReadAsStringAsync().Result;
            var respGetKeyAfterDelete = getDbSizeAfterDelete.Content.ReadAsStringAsync().Result;
            
            // Assert
            Assert.IsTrue(setKey.IsSuccessStatusCode);
            Assert.IsTrue(setKey2.IsSuccessStatusCode);
            Assert.IsTrue(getDbSize.IsSuccessStatusCode);
            Assert.IsTrue(deleteKey.IsSuccessStatusCode);
            Assert.IsTrue(getDbSizeAfterDelete.IsSuccessStatusCode);

            Assert.AreEqual("Ok", respSetKey);
            Assert.AreEqual("Ok", respSetKey2);
            Assert.AreEqual("2", respGetDbSize);
            Assert.AreEqual("1", respDeleteKey);
            Assert.AreEqual("1", respGetKeyAfterDelete);
        }
        [TestMethod]
        public async Task SetValues_TryIncrease()
        {
            // Arrange
            const string command1 = "set key \"1\"";
            const string command2 = "set key2 \"a\"";
            const string command3 = "incr key";
            const string command4 = "incr key2 ";
            const string command5 = "get key";
            const string command6 = "get key2";

            // Act
            //clean the base 
            await CallServer("del key key2");
            var setKey = await CallServer(command1);
            var setKey2 = await CallServer(command2);
            var increaseKey = await CallServer(command3);
            var increaseKey2 = await CallServer(command4);
            var getKey = await CallServer(command5);
            var getKey2 = await CallServer(command6);
            
            var respSetKey = setKey.Content.ReadAsStringAsync().Result;
            var respSetKey2 = setKey2.Content.ReadAsStringAsync().Result;
            var respIncreaseKey = increaseKey.Content.ReadAsStringAsync().Result;
            var respIncreaseKey2 = increaseKey2.Content.ReadAsStringAsync().Result;
            var respGetKey = getKey.Content.ReadAsStringAsync().Result;
            var respGetKey2 = getKey2.Content.ReadAsStringAsync().Result;
            
            // Assert
            Assert.IsTrue(setKey.IsSuccessStatusCode);
            Assert.IsTrue(setKey2.IsSuccessStatusCode);
            Assert.IsTrue(increaseKey.IsSuccessStatusCode);
            Assert.IsTrue(increaseKey2.IsSuccessStatusCode);
            Assert.IsTrue(getKey.IsSuccessStatusCode);
            Assert.IsTrue(getKey2.IsSuccessStatusCode);

            Assert.AreEqual("Ok", respSetKey);
            Assert.AreEqual("Ok", respSetKey2);
            Assert.AreEqual("Ok", respIncreaseKey);
            Assert.AreEqual("Ok", respIncreaseKey2);
            Assert.AreEqual("2", respGetKey);
            Assert.AreEqual("a", respGetKey2);
        }
        [TestMethod]
        public async Task ZAddValues_TryIncrease()
        {
            // Arrange
            const string command1 = "zadd key 1 \"1\"";
            const string command2 = "zadd key 2 \"13\"";
            const string command3 = "Zadd key 1 \"akgpaeo\"";
            const string command4 = "incr key";
            const string command5 = "zrange key 0 -1";

            // Act
            //clean the base 
            await CallServer("del key");
            var zaddKey1 = await CallServer(command1);
            var zaddKey2 = await CallServer(command2);
            var zaddKey3 = await CallServer(command3);
            var increaseKey = await CallServer(command4);
            var zrangeKey = await CallServer(command5);
            
            var respZAddKey1 = zaddKey1.Content.ReadAsStringAsync().Result;
            var respZAddKey2 = zaddKey2.Content.ReadAsStringAsync().Result;
            var respZAddKey3 = zaddKey3.Content.ReadAsStringAsync().Result;
            var respIncreaseKey = increaseKey.Content.ReadAsStringAsync().Result;
            var respZRangeKey = zrangeKey.Content.ReadAsStringAsync().Result;
            
            // Assert
            Assert.IsTrue(zaddKey1.IsSuccessStatusCode);
            Assert.IsTrue(zaddKey2.IsSuccessStatusCode);
            Assert.IsTrue(zaddKey3.IsSuccessStatusCode);
            Assert.IsTrue(increaseKey.IsSuccessStatusCode);
            Assert.IsTrue(zrangeKey.IsSuccessStatusCode);

            Assert.AreEqual("Ok", respZAddKey1);
            Assert.AreEqual("Ok", respZAddKey2);
            Assert.AreEqual("Ok", respZAddKey3);
            Assert.AreEqual("Ok", respIncreaseKey);
            Assert.AreEqual("1:2\n1:akgpaeo\n2:14\n", respZRangeKey);
        }
        [TestMethod]
        public async Task ZAddValues_GetZCard()
        {
            // Arrange
            const string command1 = "zadd key 1 \"4\"";
            const string command2 = "zadd key 2 \"1\"";
            const string command3 = "Zadd key 1 \"grzg\"";
            const string command4 = "Zadd key 1 \"asko\"";
            const string command5 = "Zadd key 1 \"2\"";
            const string command6 = "zrange key 0 -1";
            const string command7 = "zcard key";
            // Act
            //clean the base 
            await CallServer("del key");
            var zaddKey1 = await CallServer(command1);
            var zaddKey2 = await CallServer(command2);
            var zaddKey3 = await CallServer(command3);
            var zaddKey4 = await CallServer(command4);
            var zaddKey5 = await CallServer(command5);
            var zrangeKey = await CallServer(command6);
            var zcard = await CallServer(command7);
            
            var respZAddKey1 = zaddKey1.Content.ReadAsStringAsync().Result;
            var respZAddKey2 = zaddKey2.Content.ReadAsStringAsync().Result;
            var respZAddKey3 = zaddKey3.Content.ReadAsStringAsync().Result;
            var respZAddKey4 = zaddKey4.Content.ReadAsStringAsync().Result;
            var respZAddKey5 = zaddKey5.Content.ReadAsStringAsync().Result;
            var respZRangeKey = zrangeKey.Content.ReadAsStringAsync().Result;
            var respZCard = zcard.Content.ReadAsStringAsync().Result;
            
            // Assert
            Assert.IsTrue(zaddKey1.IsSuccessStatusCode);
            Assert.IsTrue(zaddKey2.IsSuccessStatusCode);
            Assert.IsTrue(zaddKey3.IsSuccessStatusCode);
            Assert.IsTrue(zaddKey4.IsSuccessStatusCode);
            Assert.IsTrue(zaddKey5.IsSuccessStatusCode);
            Assert.IsTrue(zrangeKey.IsSuccessStatusCode);
            Assert.IsTrue(zcard.IsSuccessStatusCode);

            Assert.AreEqual("Ok", respZAddKey1);
            Assert.AreEqual("Ok", respZAddKey2);
            Assert.AreEqual("Ok", respZAddKey3);
            Assert.AreEqual("Ok", respZAddKey4);
            Assert.AreEqual("Ok", respZAddKey5);
            Assert.AreEqual("1:2\n1:4\n1:asko\n1:grzg\n2:1\n", respZRangeKey);
            Assert.AreEqual("5", respZCard);
        }
        [TestMethod]
        public async Task ZAddValues_GetZRank()
        {
            // Arrange
            const string command1 = "zadd key 1 \"4\"";
            const string command2 = "zrank key \"4\"";
            const string command3 = "zadd key 2 \"1\"";
            const string command4 = "zrank key \"1\"";
            const string command5 = "Zadd key 1 \"grzg\"";
            const string command6 = "Zadd key 1 \"asko\"";
            const string command7 = "Zrank key \"grzg\"";
            const string command8 = "Zrank key \"asko\"";
            const string command9 = "Zadd key 1 \"2\"";
            const string command10 = "zrank key \"2\"";
            const string command11 = "zrank key \"No existo\"";
            // Act
            //clean the base 
            await CallServer("del key");
            var zaddKey1 = await CallServer(command1);
            var zrankKey1 = await CallServer(command2);
            var zaddKey2 = await CallServer(command3);
            var zrankKey2 = await CallServer(command4);
            var zaddKey3 = await CallServer(command5);
            var zaddKey4 = await CallServer(command6);
            var zrankKey3 = await CallServer(command7);
            var zrankKey4 = await CallServer(command8);
            var zaddKey5 = await CallServer(command9);
            var zrankKey5 = await CallServer(command10);
            var zrankKey1AfterInsert = await CallServer(command2);
            var zrankNonExistentValue = await CallServer(command11);
            
            var respZAddKey1 = zaddKey1.Content.ReadAsStringAsync().Result;
            var respZRankKey1 = zrankKey1.Content.ReadAsStringAsync().Result;
            var respZAddKey2 = zaddKey2.Content.ReadAsStringAsync().Result;
            var respZRankKey2 = zrankKey2.Content.ReadAsStringAsync().Result;
            var respZAddKey3 = zaddKey3.Content.ReadAsStringAsync().Result;
            var respZAddKey4 = zaddKey4.Content.ReadAsStringAsync().Result;
            var respZRankKey3 = zrankKey3.Content.ReadAsStringAsync().Result;
            var respZRankKey4 = zrankKey4.Content.ReadAsStringAsync().Result;
            var respZAddKey5 = zaddKey5.Content.ReadAsStringAsync().Result;
            var respZRankKey5 = zrankKey5.Content.ReadAsStringAsync().Result;
            var respZRankKey1AfterInsert = zrankKey1AfterInsert.Content.ReadAsStringAsync().Result;
            var respZRankNonExistentValue = zrankNonExistentValue.Content.ReadAsStringAsync().Result;
            
            // Assert
            Assert.IsTrue(zaddKey1.IsSuccessStatusCode);
            Assert.IsTrue(zrankKey1.IsSuccessStatusCode);
            Assert.IsTrue(zaddKey2.IsSuccessStatusCode);
            Assert.IsTrue(zrankKey2.IsSuccessStatusCode);
            Assert.IsTrue(zaddKey3.IsSuccessStatusCode);
            Assert.IsTrue(zaddKey4.IsSuccessStatusCode);
            Assert.IsTrue(zrankKey3.IsSuccessStatusCode);
            Assert.IsTrue(zrankKey4.IsSuccessStatusCode);
            Assert.IsTrue(zaddKey5.IsSuccessStatusCode);
            Assert.IsTrue(zrankKey5.IsSuccessStatusCode);
            Assert.IsTrue(zrankKey1AfterInsert.IsSuccessStatusCode);
            Assert.IsTrue(zrankNonExistentValue.IsSuccessStatusCode);
            
            Assert.AreEqual("Ok", respZAddKey1);
            Assert.AreEqual("0", respZRankKey1);
            Assert.AreEqual("Ok", respZAddKey2);
            Assert.AreEqual("1", respZRankKey2);
            Assert.AreEqual("Ok", respZAddKey3);
            Assert.AreEqual("Ok", respZAddKey4);
            Assert.AreEqual("2", respZRankKey3);
            Assert.AreEqual("1", respZRankKey4);
            Assert.AreEqual("Ok", respZAddKey5);
            Assert.AreEqual("0", respZRankKey5);
            Assert.AreEqual("1", respZRankKey1AfterInsert);
            Assert.AreEqual("null", respZRankNonExistentValue);
        }
        [TestMethod]
        public async Task ZAddValues_GetZRange()
        {
            // Arrange
            const string command1 = "zadd key 1 \"4\"";
            const string command2 = "zadd key 2 \"1\"";
            const string command3 = "Zadd key 1 \"grzg\"";
            const string command4 = "Zadd key 1 \"asko\"";
            const string command5 = "Zadd key 1 \"2\"";
            const string command6 = "zrange key 0 -1";
            const string command7 = "zrange key 0 0";
            const string command8 = "zrange key 1 2";
            const string command9 = "zrange key 2 2";
            const string command10 = "zrange key -1 1";
            
            // Act
            //clean the base 
            await CallServer("del key");
            var zaddKey1 = await CallServer(command1);
            var zaddKey2 = await CallServer(command2);
            var zaddKey3 = await CallServer(command3);
            var zaddKey4 = await CallServer(command4);
            var zaddKey5 = await CallServer(command5);
            var zrange1 = await CallServer(command6);
            var zrange2 = await CallServer(command7);
            var zrange3 = await CallServer(command8);
            var zrange4 = await CallServer(command9);
            var zrange5 = await CallServer(command10);
            
            var respZAddKey1 = zaddKey1.Content.ReadAsStringAsync().Result;
            var respZAddKey2 = zaddKey2.Content.ReadAsStringAsync().Result;
            var respZAddKey3 = zaddKey3.Content.ReadAsStringAsync().Result;
            var respZAddKey4 = zaddKey4.Content.ReadAsStringAsync().Result;
            var respZAddKey5 = zaddKey5.Content.ReadAsStringAsync().Result;
            var respZRange1 = zrange1.Content.ReadAsStringAsync().Result;
            var respZRange2 = zrange2.Content.ReadAsStringAsync().Result;
            var respZRange3 = zrange3.Content.ReadAsStringAsync().Result;
            var respZRange4 = zrange4.Content.ReadAsStringAsync().Result;
            var respZRange5 = zrange5.Content.ReadAsStringAsync().Result;
            
            // Assert
            Assert.IsTrue(zaddKey1.IsSuccessStatusCode);
            Assert.IsTrue(zaddKey2.IsSuccessStatusCode);
            Assert.IsTrue(zaddKey3.IsSuccessStatusCode);
            Assert.IsTrue(zaddKey4.IsSuccessStatusCode);
            Assert.IsTrue(zaddKey5.IsSuccessStatusCode);
            Assert.IsTrue(zrange1.IsSuccessStatusCode);
            Assert.IsTrue(zrange2.IsSuccessStatusCode);
            Assert.IsTrue(zrange3.IsSuccessStatusCode);
            Assert.IsTrue(zrange4.IsSuccessStatusCode);
            Assert.IsTrue(zrange5.IsSuccessStatusCode);
            
            Assert.AreEqual("Ok", respZAddKey1);
            Assert.AreEqual("Ok", respZAddKey2);
            Assert.AreEqual("Ok", respZAddKey3);
            Assert.AreEqual("Ok", respZAddKey4);
            Assert.AreEqual("Ok", respZAddKey5);
            Assert.AreEqual("1:2\n1:4\n1:asko\n1:grzg\n2:1\n", respZRange1);
            Assert.AreEqual("1:2\n", respZRange2);
            Assert.AreEqual("1:4\n1:asko\n", respZRange3);
            Assert.AreEqual("1:asko\n", respZRange4);
            Assert.AreEqual("2:1\n1:2\n1:4\n", respZRange5);
        }
        [TestMethod]
        public async Task ZAddValues_WithoutScore()
        {
            // Arrange
            const string command1 = "zadd key \"4\"";
            
            // Act
            //clean the base 
            await CallServer("del key");
            var zaddKey1 = await CallServer(command1);

            var respZAddKey1 = zaddKey1.Content.ReadAsStringAsync().Result;
            
            // Assert
            Assert.IsFalse(zaddKey1.IsSuccessStatusCode);
            Assert.AreEqual("you must need insert all values to add in ZAdd command", respZAddKey1);
        }
        [TestMethod]
        public async Task ZAddValues_WithoutIntegerScore()
        {
            // Arrange
            const string command1 = "zadd key a \"4\"";

            // Act
            //clean the base 
            await CallServer("del key");
            var zaddKey1 = await CallServer(command1);

            var respZAddKey1 = zaddKey1.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.IsFalse(zaddKey1.IsSuccessStatusCode);
            Assert.AreEqual("you must need insert a integer score", respZAddKey1);
        }
        [TestMethod]
        public async Task SetValues_WithoutQuotes()
        {
            // Arrange
            const string command1 = "set key a";

            // Act
            //clean the base 
            await CallServer("del key");
            var zaddKey1 = await CallServer(command1);

            var respZAddKey1 = zaddKey1.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.IsFalse(zaddKey1.IsSuccessStatusCode);
            Assert.AreEqual("you must need to declare a value with quotes(ex:\"your-value\") after key in set command", respZAddKey1);
        }
    }
}
