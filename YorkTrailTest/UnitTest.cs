using NUnit.Framework;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using YorkTrail;

namespace YorkTrailTest
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class Tests
    {
        public MainWindow window;
        public MainWindowViewModel vm;

        [SetUp]
        public void Setup()
        {
            window = new MainWindow();
            vm = (MainWindowViewModel)window.DataContext;
            vm.FileOpen("test.flac");
        }

        [Test]
        public void FileOpenTest()
        {
            Assert.IsTrue(vm.FileOpen("test.flac"));
            vm.FileClose();
            Assert.IsNull(vm.FilePath);
        }

        [Test]
        public void SeekTest()
        {
            Assert.IsNotNull(vm.FilePath);
            vm.Position = 0;
            Assert.AreEqual(vm.Core.GetPosition(), 0);
            vm.Position = 1;
            Assert.AreEqual(vm.Core.GetPosition(), 1);
            vm.Position = 0;
            vm.FRCommand.Execute(window);
            Assert.AreEqual(vm.Core.GetPosition(), 0);
            vm.Position = 1;
            vm.FFCommand.Execute(window);
            Assert.AreEqual(vm.Core.GetPosition(), 1);
        }

        [Test]
        public void PlayTest()
        {
            vm.PlayCommand.Execute(window);
            Thread.Sleep(1000);
            vm.PauseCommand.Execute(window);
            Thread.Sleep(1000);
            vm.StopCommand.Execute(window);
        }

        [Test]
        public void ChannelTest()
        {
            vm.PlayCommand.Execute(window);
            vm.Channels = Channels.LOnly;
            Thread.Sleep(1000);
            Assert.AreEqual(vm.Core.GetState(), State.Playing);
            vm.Channels = Channels.ROnly;
            Thread.Sleep(1000);
            Assert.AreEqual(vm.Core.GetState(), State.Playing);
            vm.Channels = Channels.Mono;
            Thread.Sleep(1000);
            Assert.AreEqual(vm.Core.GetState(), State.Playing);
            vm.Channels = Channels.LMinusR;
            Thread.Sleep(1000);
            Assert.AreEqual(vm.Core.GetState(), State.Playing);
            vm.Channels = Channels.Stereo;
            Thread.Sleep(1000);
            Assert.AreEqual(vm.Core.GetState(), State.Playing);
        }

        [Test]
        public void TempoTest()
        {
            vm.PlayCommand.Execute(window);
            vm.Ratio = 2.0f;
            Thread.Sleep(1000);
            Assert.AreEqual(vm.Core.GetRatio(), 2.0f);
            Assert.AreEqual(vm.Core.GetState(), State.Playing);
            vm.Ratio = 1.0f;
            Thread.Sleep(1000);
            Assert.AreEqual(vm.Core.GetRatio(), 1.0f);
            Assert.AreEqual(vm.Core.GetState(), State.Playing);
            vm.Ratio = 0.5f;
            Thread.Sleep(1000);
            Assert.AreEqual(vm.Core.GetRatio(), 0.5f);
            Assert.AreEqual(vm.Core.GetState(), State.Playing);
            vm.Ratio = 0.33f;
            Thread.Sleep(1000);
            Assert.AreEqual(vm.Core.GetRatio(), 0.33f);
            Assert.AreEqual(vm.Core.GetState(), State.Playing);
            vm.Ratio = 0.25f;
            Thread.Sleep(1000);
            Assert.AreEqual(vm.Core.GetRatio(), 0.25f);
            Assert.AreEqual(vm.Core.GetState(), State.Playing);
        }

        [Test]
        public void PitchTest()
        {
            vm.PlayCommand.Execute(window);
            vm.Pitch = 2.0f;
            Thread.Sleep(1000);
            Assert.AreEqual(vm.Core.GetPitch(), 2.0f);
            Assert.AreEqual(vm.Core.GetState(), State.Playing);
            vm.Pitch = 4.0f;
            Thread.Sleep(1000);
            Assert.AreEqual(vm.Core.GetPitch(), 4.0f);
            Assert.AreEqual(vm.Core.GetState(), State.Playing);
            vm.Pitch = 0.5f;
            Thread.Sleep(1000);
            Assert.AreEqual(vm.Core.GetPitch(), 0.5f);
            Assert.AreEqual(vm.Core.GetState(), State.Playing);
            vm.Pitch = 1.0f;
            Thread.Sleep(1000);
            Assert.AreEqual(vm.Core.GetPitch(), 1.0f);
        }

        [Test]
        public void FilterTest()
        {
            vm.PlayCommand.Execute(window);
            vm.LpfEnabled = true;
            Thread.Sleep(1000);
            Assert.AreEqual(vm.Core.LpfEnabled, true);
            Assert.AreEqual(vm.Core.GetState(), State.Playing);
            vm.HpfEnabled = true;
            Thread.Sleep(1000);
            Assert.AreEqual(vm.Core.HpfEnabled, true);
            Assert.AreEqual(vm.Core.GetState(), State.Playing);
            vm.BpfEnabled = true;
            Thread.Sleep(1000);
            Assert.AreEqual(vm.Core.BpfEnabled, true);
            Assert.AreEqual(vm.Core.GetState(), State.Playing);
        }
    }
}