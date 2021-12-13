using NUnit.Framework;
using System;
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
            vm.FileOpen(@"D:\Music\CD音源\アニメ＆ゲーム\非可逆\下川みくに - それが、愛でしょう\01 下川みくに - それが、愛でしょう.mp3");
        }

        [Test]
        public void FileOpenTest()
        {
            Assert.IsTrue(vm.FileOpen(@"D:\Music\CD音源\アニメ＆ゲーム\非可逆\下川みくに - それが、愛でしょう\01 下川みくに - それが、愛でしょう.mp3"));
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
            vm.LOnlyCommand.Execute(window);
            Thread.Sleep(1000);
            Assert.AreEqual(vm.Core.GetState(), State.Playing);
            vm.ROnlyCommand.Execute(window);
            Thread.Sleep(1000);
            Assert.AreEqual(vm.Core.GetState(), State.Playing);
            vm.MonoCommand.Execute(window);
            Thread.Sleep(1000);
            Assert.AreEqual(vm.Core.GetState(), State.Playing);
            vm.LMinusRCommand.Execute(window);
            Thread.Sleep(1000);
            Assert.AreEqual(vm.Core.GetState(), State.Playing);
            vm.StereoCommand.Execute(window);
            Thread.Sleep(1000);
            Assert.AreEqual(vm.Core.GetState(), State.Playing);
        }

        [Test]
        public void TempoTest()
        {
            vm.PlayCommand.Execute(window);
            vm.TempoDoubleCommand.Execute(window);
            Thread.Sleep(1000);
            Assert.AreEqual(vm.Core.GetRate(), 2.0f);
            Assert.AreEqual(vm.Core.GetState(), State.Playing);
            vm.TempoNormalCommand.Execute(window);
            Thread.Sleep(1000);
            Assert.AreEqual(vm.Core.GetRate(), 1.0f);
            Assert.AreEqual(vm.Core.GetState(), State.Playing);
            vm.TempoHalfCommand.Execute(window);
            Thread.Sleep(1000);
            Assert.AreEqual(vm.Core.GetRate(), 0.5f);
            Assert.AreEqual(vm.Core.GetState(), State.Playing);
            vm.TempoOneThirdCommand.Execute(window);
            Thread.Sleep(1000);
            Assert.AreEqual(vm.Core.GetRate(), 0.33f);
            Assert.AreEqual(vm.Core.GetState(), State.Playing);
            vm.TempoQuarterCommand.Execute(window);
            Thread.Sleep(1000);
            Assert.AreEqual(vm.Core.GetRate(), 0.25f);
            Assert.AreEqual(vm.Core.GetState(), State.Playing);
        }

        [Test]
        public void PitchTest()
        {
            vm.PlayCommand.Execute(window);
            vm.PitchDoubleCommand.Execute(window);
            Thread.Sleep(1000);
            Assert.AreEqual(vm.Core.GetPitch(), 2.0f);
            Assert.AreEqual(vm.Core.GetState(), State.Playing);
            vm.PitchQuadCommand.Execute(window);
            Thread.Sleep(1000);
            Assert.AreEqual(vm.Core.GetPitch(), 4.0f);
            Assert.AreEqual(vm.Core.GetState(), State.Playing);
            vm.PitchHalfCommand.Execute(window);
            Thread.Sleep(1000);
            Assert.AreEqual(vm.Core.GetPitch(), 0.5f);
            Assert.AreEqual(vm.Core.GetState(), State.Playing);
            vm.PitchNormalCommand.Execute(window);
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