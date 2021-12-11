using NUnit.Framework;
using YorkTrail;

namespace YorkTrailTest
{
    public class Tests
    {
        public YorkTrailCore Core;

        [SetUp]
        public void Setup()
        {
            Core = new YorkTrailCore();
        }

        [Test]
        public void Test1()
        {
            Assert.IsTrue(Core.FileOpen(@"D:\Music\CD音源\アニメ＆ゲーム\非可逆\下川みくに - それが、愛でしょう\01 下川みくに - それが、愛でしょう.mp3", FileType.Mp3));
        }
    }
}