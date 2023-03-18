/*
    YorkTrail
    Copyright (C) 2021 theta

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
#pragma once
#include <windows.h>
#include <sstream>
#include <vector>
#include <msclr/marshal.h>
#include <msclr/marshal_cppstd.h>
#include <chrono>

#define MINIAUDIO_IMPLEMENTATION
#include "miniaudio.h"

#include "source/SoundTouchDLL/SoundTouchDLL.h"
#include "rubberband/RubberBandStretcher.h"

#ifdef _M_CEE
#undef _M_CEE
#endif // _M_CEE

#include "StemSeparator.h"
#include "FlacEncode.h"
#include "Utils.h"

using namespace System;
using namespace System::Text;
using namespace System::Diagnostics;
using namespace System::Windows;
using namespace System::Runtime::InteropServices;
using namespace msclr::interop;
using namespace System::Collections::Generic;
using namespace System::Threading;
using namespace System::Threading::Tasks;
using namespace System::IO;

namespace YorkTrail
{
	public enum class FileType
	{
		Unknown,
		Wav,
		Mp3,
		Flac
	};

	public enum class State
	{
		Stopped,
		Stopping,
		Playing,
		Pausing
	};

	public enum class Channels
	{
		Stereo,
		LOnly,
		ROnly,
		Mono,
		LMinusR
	};

	public enum class StretchMethod
	{
		SoundTouch,
		RubberBand,
		//Internal
	};

	public ref class YorkTrailCore
	{
	public:
		YorkTrailCore();
		~YorkTrailCore();
		!YorkTrailCore();
		String^ GetFileInfo();
		bool FileOpen(String^ filePath, FileType type);
		bool StemFilesOpen(String^ filePath);
		void StemFilesClose();
		List<float>^ GetVolumeList(int start, int count, int split);
		bool IsFileLoaded();
		void FileClose();
		bool DeviceOpen(ma_decoder* pDecoder);
		void DeviceClose();
		void Start();
		void Pause();
		void Stop();
		void ResetRMS();
		void SetFrame(uint64_t frame);
		void SeekRelative(long ms);
		ma_uint64 GetTime();
		ma_uint64 GetTotalMilliSeconds();
		State GetState();
		double GetPosition();
		void SetPosition(double pos);
		void SetStartPosition(double pos);
		void SetEndPosition(double pos);
		void SetVolume(float vol);
		float GetVolume();
		void SetStemVolumes(float vocals, float drums, float bass, float piano, float other);
		void SetRatio(float ratio);
		float GetRatio();
		void SetPitch(float pitch);
		float GetPitch();
		void SetLpfFreq(float freq);
		void SetHpfFreq(float freq);
		void SetBpfFreq(float freq);
		float GetLpfFreq();
		float GetHpfFreq();
		float GetBpfFreq();
		void SetLpfEnabled(bool value);
		void SetHpfEnabled(bool value);
		void SetBpfEnabled(bool value);
		bool GetLpfEnabled();
		bool GetHpfEnabled();
		bool GetBpfEnabled();
		bool GetBypass();
		void SetBypass(bool bypass);
		void SetChannels(Channels ch);
		Channels GetChannels();
		List<String^>^ GetPlaybackDeviceList();
		void SetPlaybackDevice(int index);
		int GetPlaybackDevice();
		float GetRmsL();
		float GetRmsR();
		float GetBPM();
		void SetBPMCalc(bool input);
		void SetLoop(bool value);
		bool GetLoop();
		StretchMethod GetStretchMethod();
		void SetStretchMethod(StretchMethod value);
		void SetSoundTouchParam(int seq, int window, int overlap);
		bool SeparateStem(String^ destFolder);
		double GetProgress();
		bool SwitchDecoderToSource();
		bool SwitchDecoderToStems();
		void CancelStemSeparate();
		void miniaudioStartCallback(ma_device* pDevice, void* pOutput, const void* pInput, ma_uint32 frameCount);

		delegate void NotifyTimeChangedEventHandler();
		event NotifyTimeChangedEventHandler^ NotifyTimeChanged;

	private:
		HANDLE hSoundTouch;
		HANDLE hBpm;
		RubberBand::RubberBandStretcher* pRubberBand;
		ma_decoder* pDecoder;
		ma_decoder* pDecoderVocals;
		ma_decoder* pDecoderDrums;
		ma_decoder* pDecoderBass;
		ma_decoder* pDecoderPiano;
		ma_decoder* pDecoderOther;
		ma_decoder* pCurrentDecoder;
		ma_device* pDevice;
		ma_lpf* pLpf;
		ma_hpf* pHpf;
		ma_bpf* pBpf;
		std::vector<drmp3_seek_point>* pSeekPoints;

		ma_mutex* pMutex;
		String^ filePath;
		String^ extension;
		FileType fileType;
		uint32_t encodingFormat;
		float volume = 1.0f;
		ma_uint64 totalPCMFrames = 0;
		ma_uint32 displayUpdateCounter = 0;
		State state = State::Stopped;
		ma_uint64 curFrame = 0;
		ma_uint64 startFrame = 0;
		ma_uint64 endFrame = 0;
		Channels channels = Channels::Stereo;
		float playbackRatio = 1.0f;
		float playbackPitch = 1.0f;
		int playbackDevice = 0;
		float volumeVocals = 1.0f;
		float volumeDrums = 1.0f;
		float volumeBass = 1.0f;
		float volumePiano = 1.0f;
		float volumeOther = 1.0f;
		float rmsL = -100.0f;
		float rmsR = -100.0f;
		float lpfFreq = 22000.0f;
		float hpfFreq = 10.0f;
		float bpfFreq = 400.0f;
		bool lpfEnabled = false;
		bool hpfEnabled = false;
		bool bpfEnabled = false;
		bool isLoop = false;
		bool isBypass = false;
		bool isBPMCalc = false;
		// 画面更新の頻度 (値xレイテンシ)
		ma_uint32 displayUpdateCycle = 2;
		StretchMethod stretchMethod;
		double progress = 0;
		bool stemSeparateIsCancelled = false;

		ma_uint64 posToFrame(double pos);
		ma_uint64 frameToMillisecs(uint64_t frames);
		void Seek(int64_t frames);
		void timeStretch(std::vector<float> &frames, std::vector<float> &ouput, float ratio);
		//void pitchShift(std::vector<float> &frames, float pitch, float mix);
		float lerp(float v1, float v2, float t);
		void toDeinterleaved(std::vector<float>& input, float* const* output, int channels, int frameCount);
		void toInterleaved(const float* const* input, std::vector<float> &output, int channels, int frameCount);
		FLAC__int32 toInt32(const float input);
		float clip(const float input);
		void calcVolume(float vol, std::vector<float> &input);
		void calcRMS(std::vector<float>& input, int frameCount, int channels, float& outputL, float& outputR);
		void throwError(String^ loc, String^ msg);
		int decoderInit(ma_decoder* %pDecoder, String^ filePath, ma_uint32 outputChannels, ma_uint32 outputSampleRate);
		ma_uint64 getTotalPCMFrames(ma_decoder* pDecoder);
		bool switchDecoder(ma_decoder* pDecoder);
	};


	public ref class YorkTrailHandleHolder
	{
	public:
		static YorkTrail::YorkTrailCore^ hYorkTrailCore = gcnew YorkTrail::YorkTrailCore();
	};

	void callback(ma_device* pDevice, void* pOutput, const void* pInput, ma_uint32 frameCount);
};