#include "YorkTrailCore.h"

YorkTrail::YorkTrailCore::YorkTrailCore()
{
    // デリゲートのハンドルを取得して固定
    //hDeleg = gcnew miniaudioStartCallbackDelegate(&YorkTrailCore::miniaudioStartCallback);
    //delegGCH = GCHandle::Alloc(hDeleg);

    //hMutex = CreateMutex(NULL, FALSE, NULL);
    //hEvent = CreateEvent(NULL, TRUE, FALSE, NULL);
    pMutex = new ma_mutex();
    ma_mutex_init(pMutex);
    hSoundTouch = soundtouch_createInstance();
    soundtouch_setSetting(hSoundTouch, 3, 50);// SETTING_SEQUENCE_MS default 40
    soundtouch_setSetting(hSoundTouch, 4, 20);// SETTING_SEEKWINDOW_MS default 15
    soundtouch_setSetting(hSoundTouch, 5, 6);// SETTING_OVERLAP_MS default 8
}

YorkTrail::YorkTrailCore::~YorkTrailCore()
{
    if (playerTask != nullptr)
    {
        delete playerTask;
    }
    /*
    // デリゲートのハンドルを固定解除して削除
    if (delegGCH.IsAllocated)
    {
        delegGCH.Free();
        delete hDeleg;
    }
    */
}

YorkTrail::YorkTrailCore::!YorkTrailCore()
{
    if (pSeekPoints != nullptr)
    {
        delete pSeekPoints;
    }
    ma_mutex_uninit(pMutex);
    if (pDevice != nullptr)
    {
        ma_device_uninit(pDevice);
    }

    if (pDecoder != nullptr)
    {
        ma_decoder_uninit(pDecoder);
    }

    soundtouch_destroyInstance(hSoundTouch);
}

void YorkTrail::YorkTrailCore::throwError(String^ loc, String^ msg)
{
    Debug::WriteLine(loc + ": " + msg);
    System::Windows::MessageBox::Show(loc + ": " + msg, "Error", MessageBoxButton::OK, MessageBoxImage::Error);
    //MessageBox::Show(loc + ": " + msg, "Error");
    /*
    Exception^ exc = gcnew Exception("Error");
    throw exc;
    delete exc;
    */
}

uint64_t YorkTrail::YorkTrailCore::posToFrame(float pos)
{
    if (pDecoder != nullptr)
    {
        return totalPCMFrames * pos * TotalFrameFactor;
    }
    else
    {
        if (pos == 0)
        {
            return 0;
        }
        else
        {
            return UINT64_MAX;
        }
    }
}

uint64_t YorkTrail::YorkTrailCore::frameToMillisecs(uint64_t frame)
{
    if (pDecoder != nullptr && pDecoder->outputSampleRate > 0)
    {
        return frame * 1000 / pDecoder->outputSampleRate;
    }
    else
    {
        return 0;
    }
}

List<String^>^ YorkTrail::YorkTrailCore::GetPlaybackDeviceList()
{
    /*
    ma_backend backends[] = {
    ma_backend_wasapi,
    ma_backend_dsound,
    ma_backend_winmm
    };

    ma_context_config config = ma_context_config_init();
    ma_context context;
    if (ma_context_init(backends, sizeof(backends) / sizeof(backends[0]), &config, &context) != MA_SUCCESS) {
        throwError("ma_context", "コンテキスト初期化エラー");
        return nullptr;
    }
    */
    ma_context context;
    if (ma_context_init(NULL, 0, NULL, &context) != MA_SUCCESS) {
        throwError("ma_context", "コンテキスト初期化エラー");
        return nullptr;
    }

    ma_device_info* pPlaybackInfos;
    ma_uint32 playbackCount;
    ma_device_info* pCaptureInfos;
    ma_uint32 captureCount;
    if (ma_context_get_devices(&context, &pPlaybackInfos, &playbackCount, &pCaptureInfos, &captureCount) != MA_SUCCESS) {
        throwError("ma_context_get_devices", "デバイスリストを取得できません");
        return nullptr;
    }

    List<String^>^ deviceList = gcnew List<String^>();
    deviceList->Add("デフォルトサウンドデバイス");

    for (ma_uint32 i = 0; i < playbackCount; i++)
    {
        auto name = gcnew String(pPlaybackInfos[i].name, 0, 256, Encoding::Default);
        deviceList->Add(name);
    }
    return deviceList;
}

void YorkTrail::YorkTrailCore::SetPath(String^ p)
{
    if (playerTask != nullptr && playerTask->Status == TaskStatus::Running)
    {
        Stop();
    }

    extension = System::IO::Path::GetExtension(p);
    extension = extension->ToLower();

    if (pDecoder == nullptr)
    {
        pDecoder = new ma_decoder();
    }
    else
    {
        if (ma_decoder_uninit(pDecoder) != MA_SUCCESS)
        {
            throwError("ma_decoder", "デコーダの終了時にエラーが発生しました");
            return;
        }
    }

    if (pDevice == nullptr)
    {
        pDevice = new ma_device();
    }
    else
    {
        ma_device_uninit(pDevice);
    }

    marshal_context^ context = gcnew marshal_context();
    path = context->marshal_as<const char*>(p);

    ma_decoder_config pConfig = ma_decoder_config_init(ma_format_f32, 2, NULL);
    if (ma_decoder_init_file(path, &pConfig, pDecoder) != MA_SUCCESS)
    {
        throwError("ma_decoder", "デコーダの初期化時にエラーが発生しました");
        return;
    }

    delete context;
    totalPCMFrames = ma_decoder_get_length_in_pcm_frames(pDecoder);

    if (extension == ".mp3")
    {
        if (pSeekPoints == nullptr)
        {
            pSeekPoints = new std::vector<drmp3_seek_point>(1024);
        }
        drmp3* pMp3 = (drmp3*)pDecoder->pInternalDecoder;

        if (pMp3 != nullptr)
        {
            drmp3_uint32 seekPointCount = 1024;
            if (drmp3_calculate_seek_points(pMp3, &seekPointCount, pSeekPoints->data()))
            {
                drmp3_bind_seek_table(pMp3, seekPointCount, pSeekPoints->data());
            }
        }
    }

    ma_context macontext;
    if (ma_context_init(NULL, 0, NULL, &macontext) != MA_SUCCESS) {
        throwError("ma_context", "コンテキスト初期化エラー");
    }
    ma_device_info* pPlaybackInfos;
    ma_uint32 playbackCount;
    ma_device_info* pCaptureInfos;
    ma_uint32 captureCount;
    if (ma_context_get_devices(&macontext, &pPlaybackInfos, &playbackCount, &pCaptureInfos, &captureCount) != MA_SUCCESS) {
        throwError("ma_context_get_devices", "デバイスリストを取得できません");
    }

     /*
   // 固定したデリゲートのハンドルを指定
    using namespace System::Runtime::InteropServices;
    IntPtr ptr = Marshal::GetFunctionPointerForDelegate(hDeleg);
    */
    ma_device_config deviceConfig = ma_device_config_init(ma_device_type_playback);
    deviceConfig.playback.format = pDecoder->outputFormat;
    deviceConfig.playback.channels = pDecoder->outputChannels;
    deviceConfig.sampleRate = pDecoder->outputSampleRate;
    //deviceConfig.dataCallback = (ma_device_callback_proc)(callback_type)(void*)ptr;
    deviceConfig.dataCallback = callback;
    deviceConfig.pUserData = pDecoder;
    deviceConfig.periodSizeInMilliseconds = 20;

    if (playbackDevice > 0)
    {
        deviceConfig.playback.pDeviceID = &pPlaybackInfos[playbackDevice - 1].id;
    }

    if (ma_device_init(NULL, &deviceConfig, pDevice) != MA_SUCCESS)
    {
        throwError("ma_device", "デバイスの初期化時にエラーが発生しました");
        return;
    }

    if (ma_device_set_master_volume(pDevice, volume) != MA_SUCCESS)
    {
        throwError("ma_device", "音量変更時にエラーが発生しました");
        return;
    }

    if (pLpf == nullptr)
    {
        pLpf = new ma_lpf();
    }
    if (pHpf == nullptr)
    {
        pHpf = new ma_hpf();
    }
    if (pBpf == nullptr)
    {
        pBpf = new ma_bpf();
    }
    SetLPF(lpfFreq);
    SetHPF(hpfFreq);
    SetBPF(bpfFreq);

    soundtouch_setChannels(hSoundTouch, pDecoder->outputChannels);
    soundtouch_setSampleRate(hSoundTouch, pDecoder->outputSampleRate);
}

String^ YorkTrail::YorkTrailCore::GetFileInfo()
{
    if (pDecoder != nullptr && path != nullptr)
    {
        int ch;
        String^ samplerate;
        String^ bits;
        String^ type = extension->Substring(1);
        
        if (type == "wav")
        {
            auto dec = (drwav*)pDecoder->pInternalDecoder;
            ch = dec->channels;
            samplerate = dec->sampleRate.ToString() + "Hz ";
            bits = dec->bitsPerSample.ToString() + "bit ";
        }
        else if (type == "mp3")
        {
            auto dec = (drmp3*)pDecoder->pInternalDecoder;
            ch = dec->channels;
            samplerate = dec->sampleRate.ToString() + "Hz ";
            bits = "";
        }
        else if (type == "flac")
        {
            auto dec = (drflac*)pDecoder->pInternalDecoder;
            ch = dec->channels;
            samplerate = dec->sampleRate.ToString() + "Hz ";
            bits = dec->bitsPerSample.ToString() + "bit ";
        }
        else
        {
            return nullptr;
        }
        
        String^ fstr;
        String^ backend;
        
        switch (pDevice->pContext->backend)
        {
        case ma_backend_wasapi: backend = "WASAPI"; break;
        case ma_backend_dsound: backend = "DirectSound"; break;
        case ma_backend_winmm: backend = "MME"; break;
        default: ""; break;
        }

        String^ chStr;
        switch(ch)
        {
        case 1: chStr = "Mono "; break;
        case 2: chStr = "Stereo "; break;
        default: chStr = ch.ToString() + "ch "; break;
        }

        return type + " " + samplerate + bits + chStr + backend;
    }
}

bool YorkTrail::YorkTrailCore::IsFileLoaded()
{
    return pDecoder != nullptr && path != nullptr;
}

void YorkTrail::YorkTrailCore::FileClose()
{
    if (path != nullptr)
    {
        if (playerTask != nullptr && playerTask->Status == TaskStatus::Running)
        {
            Stop();
        }
        ma_decoder_uninit(pDecoder);
        ma_device_uninit(pDevice);
        pDecoder = nullptr;
        pDevice = nullptr;
        totalPCMFrames = 0;
        startPos = 0.0f;
        endPos = 1.0f;
        path = nullptr;
    }
}

void YorkTrail::YorkTrailCore::Start()
{
    if (playerTask != nullptr && playerTask->Status == TaskStatus::Running)
    {
        SetPosition(startPos);
    }
    else if (path != nullptr)
    {
        if (state != State::Pausing)
        {
            SetPosition(startPos);
        }
        state = State::Playing;
        auto action = gcnew System::Action(this, &YorkTrailCore::Play);
        playerTask = gcnew Task(action);
        playerTask->Start();
    }
}

void YorkTrail::YorkTrailCore::Pause()
{
    if (path != nullptr)
    {
        if (state == State::Playing)
        {
            state = State::Pausing;
        }
        else if (state == State::Pausing)
        {
            playerTask->Wait();
            Start();
        }
    }
}

void YorkTrail::YorkTrailCore::Stop()
{
    state = State::Stopped;
    if (playerTask != nullptr)
    {
        playerTask->Wait();
    }
    seek(0);
    curPos = 0.0f;
    curFrame = 0.0f;
    rmsL = -100.0f;
    rmsR = -100.0f;
    NotifyTimeChanged();
}

void YorkTrail::YorkTrailCore::seek(uint64_t frame)
{
    if (pDecoder != nullptr && pDevice != nullptr)
    {
        ma_mutex_lock(pMutex);
        if (ma_decoder_seek_to_pcm_frame(pDecoder, frame) != MA_SUCCESS)
        {
            throwError("ma_decoder", "シーク時にエラーが発生しました");
        }
        ma_mutex_unlock(pMutex);
    }
}

void YorkTrail::YorkTrailCore::SetFrame(uint64_t frame)
{
    seek(frame);
    curFrame = frame;
}

void YorkTrail::YorkTrailCore::SeekRelative(long ms)
{
    if (pDecoder != nullptr && path != nullptr)
    {
        uint64_t targetFrame;
        int64_t addFrame = (int64_t)pDecoder->outputSampleRate * ms / 1000;
        if ((int64_t)curFrame + addFrame < 0)
        {
            targetFrame = 0;
        }
        else
        {
            targetFrame = curFrame + addFrame;
        }
        SetFrame(targetFrame);
    }
}

uint64_t YorkTrail::YorkTrailCore::GetTime()
{
    if (pDecoder != nullptr)
    {
        curTime = frameToMillisecs(curFrame);
    }
    return curTime;
}

YorkTrail::State YorkTrail::YorkTrailCore::GetState()
{
    return state;
}

float YorkTrail::YorkTrailCore::GetPosition()
{
    if (pDecoder != nullptr)
    {
        curPos = (float)curFrame / totalPCMFrames / TotalFrameFactor;
    }
    return curPos;
}

void YorkTrail::YorkTrailCore::SetPosition(float pos)
{
    if (pDecoder != nullptr && path != nullptr)
    {
        curPos = pos;
        uint64_t targetFrame = totalPCMFrames * pos * TotalFrameFactor;
        seek(targetFrame);
        curFrame = targetFrame;
    }
}

void YorkTrail::YorkTrailCore::SetVolume(float vol)
{
    if (pDevice != nullptr)
    {
        if (ma_device_set_master_volume(pDevice, vol) != MA_SUCCESS)
        {
            throwError("ma_device", "音量変更時にエラーが発生しました");
        }
    }
    volume = vol;
}

float YorkTrail::YorkTrailCore::GetVolume()
{
    float vol = volume;
    if (pDevice != nullptr)
    {
        if (ma_device_get_master_volume(pDevice, &vol) != MA_SUCCESS)
        {
            throwError("ma_device", "音量取得時にエラーが発生しました");
        }
    }
    return vol;
}

void YorkTrail::YorkTrailCore::SetRate(float rate)
{
    if (pDevice != nullptr)
    {
        ma_mutex_lock(pMutex);
        soundtouch_setTempo(hSoundTouch, rate);
        ma_mutex_unlock(pMutex);
    }
    playbackRate = rate;
}

float YorkTrail::YorkTrailCore::GetRate()
{
    return playbackRate;
}

void YorkTrail::YorkTrailCore::SetPitch(float pitch)
{
    if (pDevice != nullptr)
    {
        ma_mutex_lock(pMutex);
        soundtouch_setPitch(hSoundTouch, pitch);
        ma_mutex_unlock(pMutex);
    }
    playbackPitch = pitch;
}

float YorkTrail::YorkTrailCore::GetPitch()
{
    return playbackPitch;
}

void YorkTrail::YorkTrailCore::SetLPF(float value)
{
    if (pDecoder != nullptr)
    {
        ma_mutex_lock(pMutex);
        ma_lpf_config config = ma_lpf_config_init(ma_format_f32, pDecoder->outputChannels, pDecoder->outputSampleRate, value, MA_MAX_FILTER_ORDER);
        if (ma_lpf_init(&config, pLpf) != MA_SUCCESS) {
            throwError("ma_lpf_init", "LPFの初期化時にエラーが発生しました");
            ma_mutex_unlock(pMutex);
        }
        ma_mutex_unlock(pMutex);
    }
    lpfFreq = value;
}

void YorkTrail::YorkTrailCore::SetHPF(float value)
{
    if (pDecoder != nullptr)
    {
        ma_mutex_lock(pMutex);
        ma_hpf_config config = ma_hpf_config_init(ma_format_f32, pDecoder->outputChannels, pDecoder->outputSampleRate, value, MA_MAX_FILTER_ORDER);
        if (ma_hpf_init(&config, pHpf) != MA_SUCCESS) {
            throwError("ma_hpf_init", "HPFの初期化時にエラーが発生しました");
            ma_mutex_unlock(pMutex);
        }
        ma_mutex_unlock(pMutex);
    }
    hpfFreq = value;
}

void YorkTrail::YorkTrailCore::SetBPF(float value)
{
    if (pDecoder != nullptr)
    {
        ma_mutex_lock(pMutex);
        ma_bpf_config config = ma_bpf_config_init(ma_format_f32, pDecoder->outputChannels, pDecoder->outputSampleRate, value, MA_MAX_FILTER_ORDER);
        if (ma_bpf_init(&config, pBpf) != MA_SUCCESS) {
            throwError("ma_bpf_init", "BPFの初期化時にエラーが発生しました");
            ma_mutex_unlock(pMutex);
        }
        ma_mutex_unlock(pMutex);
    }
    bpfFreq = value;
}

bool YorkTrail::YorkTrailCore::GetBypass()
{
    return isBypass;
}

void YorkTrail::YorkTrailCore::SetBypass(bool value)
{
    ma_mutex_lock(pMutex);
    isBypass = value;
    ma_mutex_unlock(pMutex);
}

void YorkTrail::YorkTrailCore::SetChannels(Channels ch)
{
    if (pDevice != nullptr)
    {
        ma_mutex_lock(pMutex);
        channels = ch;
        ma_mutex_unlock(pMutex);
    }
    else
    {
        channels = ch;
    }
}

YorkTrail::Channels YorkTrail::YorkTrailCore::GetChannels()
{
    return channels;
}

void YorkTrail::YorkTrailCore::Play()
{
    ma_result result;

    if (ma_device_start(pDevice) != MA_SUCCESS)
    {
        throwError("ma_device_start", "デバイスをスタートできません");
        return;
    }

    while (ma_device_is_started(pDevice) && state == State::Playing)
    {
        // 実際のフレーム数がTotalPCMFramesより少ない時があるのでこうする
        if (curFrame >= totalPCMFrames * endPos * TotalFrameFactor)
        {
            if (isLoop)
            {
                SetPosition(startPos);
            }
            else
            {
                break;
            }
        }
        Sleep(10);
    }

    ma_device_stop(pDevice);

    // 終了するまで待つ
    while (ma_device_get_state(pDevice) == MA_STATE_STARTED || ma_device_get_state(pDevice) == MA_STATE_STOPPING)
    {
        Sleep(10);
    }
}

void YorkTrail::YorkTrailCore::timeStretch(std::vector<float> &frames, std::vector<float> &output, float rate)
{
    if (!output.empty())
    {
        output.clear();
    }
    int blockSize = 2048;
    int start1 = 0;
    int start2 = blockSize * rate;
    int overlap = 512;
    int zeroCrossPoint;

    //0.5ブロックずつ交互に配置
    while (output.size() + blockSize * rate < frames.size() / rate)
    {
        // 最大となるゼロクロス点を見つける
        for (int i = start1 + blockSize - 1; i >= start1; i--)
        {
            if (i >= frames.size())
            {
                zeroCrossPoint = frames.size() - 1;
                break;
            }
            else if ((frames[i - 1] < 0 && frames[i] > 0) || (frames[i - 1] > 0 && frames[i] < 0) || frames[i] == 0)
            {
                zeroCrossPoint = i;
                break;
            }
            else if (i <= start1)
            {
                zeroCrossPoint = start1 + blockSize - 1;
                break;
            }
        }
        /*
        zeroCrossPoint = start1 + blockSize - 1;
        if (zeroCrossPoint > frames.size())
        {
            zeroCrossPoint = frames.size() - 1;
        }
        */
        for (int i = start1; i <= zeroCrossPoint; i++)
        {
            if (start1 > 0 && i < start1 + overlap && start2 + i - start1 < frames.size())
            {
                float vol = sin((float)(i - start1) / overlap);
                float val = frames[start2 + i - start1] * (1.0f - vol) + frames[i] * vol;
                output.push_back((output[output.size() - 2] + output.back() + val) / 3);
            }
            else
            {
                output.push_back(frames[i]);
            }
        }
        start1 = zeroCrossPoint + 1;
        for (int i = start2 + blockSize * (-2.0f * rate + 2) - 1; i >= start2; i--)
        {
            if (i >= frames.size())
            {
                zeroCrossPoint = frames.size() - 1;
                break;
            }
            else if ((frames[i - 1] < 0 && frames[i] > 0) || (frames[i - 1] > 0 && frames[i] < 0) || frames[i] == 0)
            {
                zeroCrossPoint = i;
                break;
            }
            else if (i <= start2)
            {
                zeroCrossPoint = start2 + blockSize - 1;
                break;
            }
        }
        /*
        zeroCrossPoint = start2 + blockSize - 1;
        if (zeroCrossPoint > frames.size())
        {
            zeroCrossPoint = frames.size() - 1;
        }
        */
        for (int i = start2; i <= zeroCrossPoint; i++)
        {
            if (i >= (int64_t)frames.size())
            {
                break;
            }
            if (i < start2 + overlap && start1 + i - start2 < frames.size())
            {
                float vol = sin((float)(i - start2) / overlap);
                float val = frames[start1 + i - start2] * (1.0f - vol) + frames[i] * vol;
                output.push_back((output[output.size() - 2] + output.back() + val) / 3);
            }
            else
            {
                output.push_back(frames[i]);
            }
        }
        start2 = zeroCrossPoint + 1;
    }

    //足りない分を足す
    start2 = frames.size() - (frames.size() / rate - output.size());
    for (int i = start2; i < frames.size(); i++)
    {
        if (i < start2 + overlap && start1 + i - start2 < frames.size())
        {
            float vol = sin((float)(i - (start2 - overlap)) / overlap);
            output.push_back(frames[start1 + i - start2] * (1.0f - vol) + frames[i] * vol);
        }
        else
        {
            output.push_back(frames[i]);
        }
    }
}
/*
void YorkTrail::YorkTrailCore::pitchShift(std::vector<float> &frames, float pitch, float mix)
{
    // エフェクターのパラメーター
    //float mix = 1.0f;  // ピッチシフターの効き具合。0.0～1.0の間
    //float pitch = 12.0f; // どれだけ音を高く(低く)するか。-12～12程度

    // リングバッファのinteravalを設定する
    // とりあえず1000サンプル程度とする
    // (intervalはリングバッファ(https://vstcpp.wpblog.jp/?p=1505 より)
    int delaysample = frames.size() / 2;
    ringbufL->SetInterval(delaysample * 2);
    ringbufR->SetInterval(delaysample * 2);

    // 読み込み速度を速めた後の読み込み位置
    float pos1 = 0.0f;
    float pos2 = 0.0f;

    // ピッチから読み込み速度を計算
    // -12半音(1オクターブ下)で-0.5、12半音(1オクターブ上)で1となるようにする
    float speed = pow(2.0f, pitch / 12.0f) - 1.0f;

    // 入力信号をピッチシフトする
    for (int i = 0; i < frames.size(); i += 2)
    {
        // 読み込み位置を移動した際の前後の整数値を取得(あとで線形補間するため)
        int a1 = (int)pos1;
        int a2 = pos1 + 1;
        int b1 = (int)pos2;
        int b2 = pos2 + 1;

        // 前後の整数値から読み込み位置の値を線形補間で割り出す
        // 線形補間で割り出した結果はピッチシフト後の信号となる
        float lerpL1 = lerp(ringbufL->Read(a1), ringbufL->Read(a2), pos1 - (float)a1);
        float lerpR1 = lerp(ringbufR->Read(a1), ringbufR->Read(a2), pos1 - (float)a1);
        float lerpL2 = lerp(ringbufL->Read(b1), ringbufL->Read(b2), pos2 - (float)b1);
        float lerpR2 = lerp(ringbufR->Read(b1), ringbufR->Read(b2), pos2 - (float)b1);

        // 読み込み位置を更新する
        // posが大きく(小さく)なりすぎないように定期的に 0 に戻す
        // 大きくなった時にリングバッファのintervalを超えてはいけないので、
        // とりあえずdelaysample* 0.9とする。
        pos1 += speed;
        pos2 += speed;

        float w1, w2;
        int fadesample = 4;

        // Interleaveなのでサンプル数/2
        if (abs(pos1) >= delaysample)
        {
            w1 = 0.0f;
            pos1 = 0.0f;
        }
        else if (abs(pos1) >= delaysample / 2)
        {
            w1 = 0.0f;
        }
        else if (abs(pos1) >= delaysample / 2 - fadesample)
        {
            w1 = -1.0f / fadesample * (pos1 - delaysample / 2);
        }
        else if (abs(pos1) >= fadesample && abs(pos1) < delaysample / 2 - fadesample)
        {
            w1 = 1.0f;
        }
        else if (abs(pos1) < fadesample && abs(pos1) >= 0)
        {
            w1 = 1.0f / fadesample * pos1;
        }

        if (abs(pos2) >= delaysample)
        {
            pos2 = 0.0f;
        }
        w2 = 1.0f - w1;

        lerpL1 = lerpL1 * w1;
        lerpR1 = lerpR1 * w1;
        lerpL2 = lerpL2 * w2;
        lerpR2 = lerpR2 * w2;

        // ディレイ信号として入力信号をリングバッファに書き込み
        ringbufL->Write(frames[i]);
        ringbufR->Write(frames[i + 1]);

        // リングバッファの状態を更新する
        ringbufL->Update();
        ringbufR->Update();

        // 入力信号にピッチシフト信号を混ぜて出力する
        frames[i] = (1.0f - mix) * frames[i] + mix * (lerpL1 + lerpL2);
        frames[i + 1] = (1.0f - mix) * frames[i + 1] + mix * (lerpR1 + lerpR2);
    }
}
*/
float YorkTrail::YorkTrailCore::lerp(float v1, float v2, float t)
{
    //三次補完
    float rate = t * t * (3.0f - 2.0f * t);
    return (1.0f - rate) * v1 + rate * v2;
    //return (1.0f - t) * v1 + t * v2;
}

void YorkTrail::YorkTrailCore::toSplited(std::vector<float>& input, std::vector<float>& outputL, std::vector<float>& outputR)
{
    if (!outputL.empty())
    {
        outputL.clear();
    }
    if (!outputR.empty())
    {
        outputR.clear();
    }

    for (int i = 0; i < input.size(); i += 2)
    {
        outputL.push_back(input[i]);
        outputR.push_back(input[i + 1]);
    }
}

void YorkTrail::YorkTrailCore::toInterleaved(std::vector<float>& inputL, std::vector<float>& inputR, std::vector<float>& output)
{
    if (!output.empty())
    {
        output.clear();
    }
    for (int i = 0; i < inputL.size(); i++)
    {
        output.push_back(inputL[i]);
        output.push_back(inputR[i]);
    }
}

void YorkTrail::YorkTrailCore::calcVolume(float vol, std::vector<float> &input)
{
    for (uint64_t i = 0; i < input.size(); i++)
    {
        input[i] *= vol;
    }
}

void YorkTrail::YorkTrailCore::calcRMS(std::vector<float> &input, float &outputL, float &outputR)
{
    float volL = 0;
    float volR = 0;
    for (uint64_t i = 0; i < input.size(); i += 2)
    {
        volL += pow(input[i], 2);
        volR += pow(input[i + 1], 2);
    }
    float valL = 20.0f * log10(sqrt(volL / input.size()));
    float valR = 20.0f * log10(sqrt(volR / input.size()));
    
    outputL = (std::isinf(valL)) ? -100.0f : valL;
    outputR = (std::isinf(valR)) ? -100.0f : valR;
}

void YorkTrail::YorkTrailCore::miniaudioStartCallback(ma_device* pDevice, void* pOutput, const void* pInput, ma_uint32 frameCount)
{
    ma_mutex_lock(pMutex);

    ma_uint32 bufferSize = frameCount * pDecoder->outputChannels;
    std::vector<float> decodedFrames(bufferSize * 2, 0.0f);
    std::vector<float> processdFrames(bufferSize * 2, 0.0f);
    ma_uint64 framesRead;
    ma_uint64 framesProcessed;

    if (isBypass)
    {
        framesRead = ma_decoder_read_pcm_frames(pDecoder, decodedFrames.data(), frameCount);
        if (framesRead < frameCount) {
            // Reached the end.
        }
    }
    else
    {
        framesRead = ma_decoder_read_pcm_frames(pDecoder, decodedFrames.data(), frameCount * playbackRate);
        if (framesRead < frameCount * playbackRate) {
            // Reached the end.
        }
    }

    curFrame += framesRead;
    if (displayUpdateCounter >= displayUpdateCycle - 1)
    {
        NotifyTimeChanged();
        displayUpdateCounter = 0;
    }
    else
    {
        displayUpdateCounter++;
    }

    if (!isBypass)
    {
        if (playbackPitch != 1.0f || playbackRate != 1.0f)
        {
            soundtouch_putSamples(hSoundTouch, decodedFrames.data(), frameCount * playbackRate);
            framesProcessed = soundtouch_receiveSamples(hSoundTouch, processdFrames.data(), frameCount);
            if (framesProcessed == 0)
            {
                ma_mutex_unlock(pMutex);
                return;
            }
        }
        else
        {
            std::copy(decodedFrames.begin(), decodedFrames.end(), processdFrames.begin());
        }

        if (useLpf)
        {
            ma_lpf_process_pcm_frames(pLpf, processdFrames.data(), processdFrames.data(), frameCount);
        }
        if (useHpf)
        {
            ma_hpf_process_pcm_frames(pHpf, processdFrames.data(), processdFrames.data(), frameCount);
        }
        if (useBpf)
        {
            ma_bpf_process_pcm_frames(pBpf, processdFrames.data(), processdFrames.data(), frameCount);
        }

        if (pDecoder->outputChannels >= 2)
        {
            switch (channels)
            {
            case Channels::L:
                for (uint32_t i = 0; i < frameCount * pDecoder->outputChannels; i += 2)
                {
                    processdFrames[i + 1] = processdFrames[i];
                }
                break;
            case Channels::R:
                for (uint32_t i = 0; i < frameCount * pDecoder->outputChannels; i += 2)
                {
                    processdFrames[i] = processdFrames[i + 1];
                }
                break;
            case Channels::LPlusR:
                for (uint32_t i = 0; i < frameCount * pDecoder->outputChannels; i += 2)
                {
                    float val = (processdFrames[i] + processdFrames[i + 1]) / 2;
                    processdFrames[i] = processdFrames[i + 1] = val;
                }
                break;
            case Channels::LMinusR:
                for (uint32_t i = 0; i < frameCount * pDecoder->outputChannels; i += 2)
                {
                    float val = (processdFrames[i] - processdFrames[i + 1]) / 2;
                    processdFrames[i] = processdFrames[i + 1] = val;
                }
                break;
            case Channels::Stereo:
                break;
            }
        }
    }
    else
    {
        std::copy(decodedFrames.begin(), decodedFrames.end(), processdFrames.begin());
    }

    float l, r;
    calcRMS(processdFrames, l, r);
    rmsL = l;
    rmsR = r;

    std::memcpy(pOutput, processdFrames.data(), sizeof(float) * frameCount * pDecoder->outputChannels);

    ma_mutex_unlock(pMutex);
}

void YorkTrail::callback(ma_device* pDevice, void* pOutput, const void* pInput, ma_uint32 frameCount)
{
    YorkTrailCore^ core = YorkTrailHandleHolder::hYorkTrailCore;
    core->miniaudioStartCallback(pDevice, pOutput, pInput, frameCount);
}
