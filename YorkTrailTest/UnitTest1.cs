using NUnit.Framework;
using System;
using System.Reflection;
using YorkTrail;

namespace YorkTrailTest
{
    public class Tests
    {
        //public YorkTrailCore Core;
        public MainWindowViewModel vm;

        [SetUp]
        public void Setup()
        {
            //Core = new YorkTrailCore();
            vm = new MainWindowViewModel();
        }

        [Test]
        public void Test1()
        {
            //Type type = Core.GetType();
            //MethodInfo methodInfo = type.GetMethod("posToFrame", BindingFlags.NonPublic | BindingFlags.Instance);
            //methodInfo.Invoke();
            //bool res = Core.FileOpen(@"D:\Music\CD音源\アニメ＆ゲーム\非可逆\ワスレナゴハン\01 ワスレナゴハン.mp3", FileType.Mp3);
            bool res = vm.FileOpen(@"D:\Music\CD音源\アニメ＆ゲーム\非可逆\ワスレナゴハン\01 ワスレナゴハン.mp3");
            Assert.IsTrue(res);
        }
    }
}