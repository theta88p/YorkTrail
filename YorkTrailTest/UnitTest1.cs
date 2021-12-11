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
            Assert.IsTrue(Core.FileOpen(@"D:\Music\CD����\�A�j�����Q�[��\��t\����݂��� - ���ꂪ�A���ł��傤\01 ����݂��� - ���ꂪ�A���ł��傤.mp3", FileType.Mp3));
        }
    }
}