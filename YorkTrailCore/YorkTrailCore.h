#pragma once
#include <windows.h>
#include <sstream>
#include <vector>
#include <msclr/marshal.h>

#include "extras/dr_flac.h"    // Enables FLAC decoding.
#include "extras/dr_mp3.h"     // Enables MP3 decoding.
#include "extras/dr_wav.h"     // Enables WAV decoding.

#define MINIAUDIO_IMPLEMENTATION
#include "miniaudio.h"

#include "SoundTouchDLL.h"

using namespace System;
using namespace System::Text;
using namespace System::Diagnostics;
using namespace System::Windows;
using namespace System::Runtime::InteropServices;
using namespace msclr::interop;
using namespace System::Collections::Generic;
using namespace System::Threading;
using namespace System::Threading::Tasks;

// mp3でトータルフレーム数が合わないので係数をかけて少なめにする
constexpr float TotalFrameFactor = 0.9995;

namespace YorkTrail
{
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
		L,
		R,
		LPlusR,
		LMinusR
	};

	public ref class YorkTrailCore
	{
	public:
		int playbackDevice = 0;
		float startPos = 0.0f;
		float endPos = 1.0f;
		float rmsL = -100.0f;
		float rmsR = -100.0f;
		float lpfFreq = 22000.0f;
		float hpfFreq = 10.0f;
		float bpfFreq = 400.0f;
		bool LpfEnabled = false;
		bool HpfEnabled = false;
		bool BpfEnabled = false;
		bool isLoop = false;
		uint32_t displayUpdateCycle = 2;// 画面更新の頻度 (値xレイテンシ)

		YorkTrailCore();
		~YorkTrailCore();
		!YorkTrailCore();
		String^ GetFileInfo();
		bool FileOpen(String^ path);
		bool IsFileLoaded();
		void FileClose();
		void Start();
		void Pause();
		void Stop();
		void ResetRMS();
		void SetFrame(uint64_t frame);
		void SeekRelative(long ms);
		uint64_t GetTime();
		uint64_t GetTotalMilliSeconds();
		State GetState();
		float GetPosition();
		void SetPosition(float pos);
		void SetVolume(float vol);
		float GetVolume();
		void SetRate(float rate);
		float GetRate();
		void SetPitch(float pitch);
		float GetPitch();
		void SetLPF(float freq);
		void SetHPF(float freq);
		void SetBPF(float freq);
		bool GetBypass();
		void SetBypass(bool bypass);
		void SetChannels(Channels ch);
		Channels GetChannels();
		List<String^>^ GetPlaybackDeviceList();
		void SetSoundTouchParam(int seq, int window, int overlap);
		void miniaudioStartCallback(ma_device* pDevice, void* pOutput, const void* pInput, ma_uint32 frameCount);

		delegate void NotifyTimeChangedEventHandler();
		event NotifyTimeChangedEventHandler^ NotifyTimeChanged;

	private:
		//Task^ playerTask;

		HANDLE hSoundTouch;
		ma_decoder* pDecoder;
		ma_device* pDevice;
		ma_lpf* pLpf;
		ma_hpf* pHpf;
		ma_bpf* pBpf;
		std::vector<drmp3_seek_point>* pSeekPoints;

		ma_mutex* pMutex;
		const char* path;
		String^ extension;
		float volume = 1.0f;
		uint64_t totalPCMFrames = 0;
		uint32_t displayUpdateCounter = 0;
		State state = State::Stopped;
		uint64_t curFrame = 0;
		Channels channels = Channels::Stereo;
		float curPos = 0;
		float playbackRate = 1.0f;
		float playbackPitch = 1.0f;
		bool isBypass = false;

		uint64_t posToFrame(float pos);
		uint64_t frameToMillisecs(uint64_t frames);
		void Seek(uint64_t frames);
		void timeStretch(std::vector<float> &frames, std::vector<float> &ouput, float rate);
		//void pitchShift(std::vector<float> &frames, float pitch, float mix);
		float lerp(float v1, float v2, float t);
		void toSplited(std::vector<float>& input, std::vector<float>& outputL, std::vector<float>& outputR);
		void toInterleaved(std::vector<float> &inputL, std::vector<float>& inputR, std::vector<float> &output);
		void calcVolume(float vol, std::vector<float> &input);
		void calcRMS(std::vector<float>& input, float& outputL, float& outputR);
		void throwError(String^ loc, String^ msg);

		// 以下コールバックを渡すのに必要な設定
		//using callback_type = void(__stdcall*)(void);
		//delegate void miniaudioStartCallbackDelegate(ma_device* pDevice, void* pOutput, const void* pInput, ma_uint32 frameCount);
		//miniaudioStartCallbackDelegate^ hDeleg;
		//GCHandle delegGCH;
	};

	public ref class YorkTrailHandleHolder
	{
	public:
		static YorkTrail::YorkTrailCore^ hYorkTrailCore = gcnew YorkTrail::YorkTrailCore();
	};

	void callback(ma_device* pDevice, void* pOutput, const void* pInput, ma_uint32 frameCount);
};