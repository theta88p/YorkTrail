#pragma once
#include <windows.h>
#include <sstream>
#include <vector>
#include <msclr/marshal.h>
//#include <chrono>

#include "extras/dr_flac.h"    // Enables FLAC decoding.
#include "extras/dr_mp3.h"     // Enables MP3 decoding.
#include "extras/dr_wav.h"     // Enables WAV decoding.

#define MINIAUDIO_IMPLEMENTATION
#include "miniaudio.h"

#include "source/SoundTouchDLL/SoundTouchDLL.h"
#include "rubberband/RubberBandStretcher.h"

using namespace System;
using namespace System::Text;
using namespace System::Diagnostics;
using namespace System::Windows;
using namespace System::Runtime::InteropServices;
using namespace msclr::interop;
using namespace System::Collections::Generic;
using namespace System::Threading;
using namespace System::Threading::Tasks;

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
		bool FileOpen(String^ path, FileType type);
		bool IsFileLoaded();
		void FileClose();
		bool DeviceOpen();
		void DeviceClose();
		void Start();
		void Pause();
		void Stop();
		void ResetRMS();
		void SetFrame(uint64_t frame);
		void SeekRelative(long ms);
		uint64_t GetTime();
		uint64_t GetTotalMilliSeconds();
		State GetState();
		double GetPosition();
		void SetPosition(double pos);
		void SetStartPosition(double pos);
		void SetEndPosition(double pos);
		void SetVolume(float vol);
		float GetVolume();
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
		void SetLoop(bool value);
		bool GetLoop();
		StretchMethod GetStretchMethod();
		void SetStretchMethod(StretchMethod value);
		void SetSoundTouchParam(int seq, int window, int overlap);
		void miniaudioStartCallback(ma_device* pDevice, void* pOutput, const void* pInput, ma_uint32 frameCount);

		delegate void NotifyTimeChangedEventHandler();
		event NotifyTimeChangedEventHandler^ NotifyTimeChanged;

	private:
		HANDLE hSoundTouch;
		RubberBand::RubberBandStretcher* pRubberBand;
		ma_decoder* pDecoder;
		ma_device* pDevice;
		ma_lpf* pLpf;
		ma_hpf* pHpf;
		ma_bpf* pBpf;
		std::vector<drmp3_seek_point>* pSeekPoints;

		ma_mutex* pMutex;
		const char* path;
		String^ extension;
		FileType fileType;
		uint32_t encodingFormat;
		float volume = 1.0f;
		uint64_t totalPCMFrames = 0;
		uint32_t displayUpdateCounter = 0;
		State state = State::Stopped;
		uint64_t curFrame = 0;
		uint64_t startFrame = 0;
		uint64_t endFrame = 0;
		Channels channels = Channels::Stereo;
		float playbackRatio = 1.0f;
		float playbackPitch = 1.0f;
		int playbackDevice = 0;
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
		// 画面更新の頻度 (値xレイテンシ)
		uint32_t displayUpdateCycle = 2;
		StretchMethod stretchMethod;

		uint64_t posToFrame(double pos);
		uint64_t frameToMillisecs(uint64_t frames);
		void Seek(int64_t frames);
		void timeStretch(std::vector<float> &frames, std::vector<float> &ouput, float ratio);
		//void pitchShift(std::vector<float> &frames, float pitch, float mix);
		float lerp(float v1, float v2, float t);
		void toDeinterleaved(std::vector<float>& input, float* const* output, int channels, int frameCount);
		void toInterleaved(const float* const* input, std::vector<float> &output, int channels, int frameCount);
		void calcVolume(float vol, std::vector<float> &input);
		void calcRMS(std::vector<float>& input, int frameCount, int channels, float& outputL, float& outputR);
		void throwError(String^ loc, String^ msg);
	};

	public ref class YorkTrailHandleHolder
	{
	public:
		static YorkTrail::YorkTrailCore^ hYorkTrailCore = gcnew YorkTrail::YorkTrailCore();
	};

	void callback(ma_device* pDevice, void* pOutput, const void* pInput, ma_uint32 frameCount);
};