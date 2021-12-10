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
            //bool res = Core.FileOpen(@"D:\Music\CD����\�A�j�����Q�[��\��t\���X���i�S�n��\01 ���X���i�S�n��.mp3", FileType.Mp3);
            bool res = vm.FileOpen(@"D:\Music\CD����\�A�j�����Q�[��\��t\���X���i�S�n��\01 ���X���i�S�n��.mp3");
            Assert.IsTrue(res);
        }
    }
}