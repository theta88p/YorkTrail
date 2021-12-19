#include "YorkTrailCore.h"

YorkTrail::YorkTrailCore::YorkTrailCore()
{
    pMutex = new ma_mutex();
    ma_mutex_init(pMutex);
    hSoundTouch = soundtouch_createInstance();
    SetSoundTouchParam(80, 30, 8);
    pRubberBand = nullptr;
    stretchMethod = StretchMethod::SoundTouch;
}

YorkTrail::YorkTrailCore::~YorkTrailCore()
{
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
}

uint64_t YorkTrail::YorkTrailCore::posToFrame(double pos)
{
    if (pDecoder != nullptr)
    {
        return totalPCMFrames * pos;
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
        auto name = gcnew String(pPlaybackInfos[i].name, 0, 256, System::Text::Encoding::Default);
        deviceList->Add(name);
    }
    return deviceList;
}

bool YorkTrail::YorkTrailCore::FileOpen(String^ p, FileType t)
{
    extension = System::IO::Path::GetExtension(p);
    extension = extension->ToLower();
    fileType = t;

    if (pDecoder == nullptr)
    {
        pDecoder = new ma_decoder();
    }
    else
    {
        if (ma_decoder_uninit(pDecoder) != MA_SUCCESS)
        {
            throwError("ma_decoder", "デコーダの終了時にエラーが発生しました");
            return false;
        }
    }

    marshal_context^ context = gcnew marshal_context();
    path = context->marshal_as<const char*>(p);

    ma_decoder_config pConfig = ma_decoder_config_init(ma_format_f32, 2, NULL);
    if (ma_decoder_init_file(path, &pConfig, pDecoder) != MA_SUCCESS)
    {
        throwError("ma_decoder", "デコーダの初期化時にエラーが発生しました");
        return false;
    }

    delete context;
    totalPCMFrames = ma_decoder_get_length_in_pcm_frames(pDecoder);
    endFrame = totalPCMFrames;
    Debug::WriteLine(totalPCMFrames);

    if (t == FileType::Mp3)
    {
        // 実際のフレーム数がTotalPCMFramesより少ない時があるのでこうする
        totalPCMFrames -= 10000;

        if (pSeekPoints == nullptr)
        {
            pSeekPoints = new std::vector<drmp3_seek_point>(1024);
        }
        auto dec = (ma_mp3*)pDecoder->pBackend;

        if (dec != nullptr)
        {
            drmp3_uint32 seekPointCount = 1024;
            if (drmp3_calculate_seek_points(&dec->dr, &seekPointCount, pSeekPoints->data()))
            {
                drmp3_bind_seek_table(&dec->dr, seekPointCount, pSeekPoints->data());
            }
        }
    }

    if (!DeviceOpen())
    {
        return false;
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
    pRubberBand = new RubberBand::RubberBandStretcher((size_t)pDecoder->outputSampleRate, (size_t)pDecoder->outputChannels
        , RubberBand::RubberBandStretcher::OptionProcessRealTime | RubberBand::RubberBandStretcher::OptionPitchHighQuality);

    return true;
}

bool YorkTrail::YorkTrailCore::DeviceOpen()
{
    if (pDevice == nullptr)
    {
        pDevice = new ma_device();
    }
    else
    {
        ma_device_uninit(pDevice);
    }

    ma_context macontext;
    if (ma_context_init(NULL, 0, NULL, &macontext) != MA_SUCCESS) {
        throwError("ma_context", "コンテキスト初期化エラー");
        return false;
    }
    ma_device_info* pPlaybackInfos;
    ma_uint32 playbackCount;
    ma_device_info* pCaptureInfos;
    ma_uint32 captureCount;
    if (ma_context_get_devices(&macontext, &pPlaybackInfos, &playbackCount, &pCaptureInfos, &captureCount) != MA_SUCCESS) {
        throwError("ma_context_get_devices", "デバイスリストを取得できません");
        return false;
    }

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
        return false;
    }

    if (ma_device_set_master_volume(pDevice, volume) != MA_SUCCESS)
    {
        throwError("ma_device", "音量変更時にエラーが発生しました");
        return false;
    }

    return true;
}

String^ YorkTrail::YorkTrailCore::GetFileInfo()
{
    if (pDecoder != nullptr && pDevice != nullptr)
    {
        int ch;
        String^ samplerate;
        String^ bits;
        String^ typeName;
        
        if (fileType == FileType::Wav)
        {
            typeName = "wav";
            auto dec = (ma_wav*)pDecoder->pBackend;
            ch = dec->dr.channels;
            samplerate = dec->dr.sampleRate.ToString() + "Hz ";
            bits = dec->dr.bitsPerSample.ToString() + "bit ";
        }
        else if (fileType == FileType::Mp3)
        {
            typeName = "mp3";
            auto dec = (ma_mp3*)pDecoder->pBackend;
            ch = dec->dr.channels;
            samplerate = dec->dr.sampleRate.ToString() + "Hz ";
            bits = "";
        }
        else if (fileType == FileType::Flac)
        {
            typeName = "flac";
            auto dec = (ma_flac*)pDecoder->pBackend;
            ch = dec->dr->channels;
            samplerate = dec->dr->sampleRate.ToString() + "Hz ";
            bits = dec->dr->bitsPerSample.ToString() + "bit ";
        }
        else
        {
            return nullptr;
        }
        
        String^ fstr;
        String^ backend = gcnew String(ma_get_backend_name(pDevice->pContext->backend));

        String^ chStr;
        switch(ch)
        {
        case 1: chStr = "Mono "; break;
        case 2: chStr = "Stereo "; break;
        default: chStr = ch.ToString() + "ch "; break;
        }

        return typeName + " " + samplerate + bits + chStr + backend;
    }
}

bool YorkTrail::YorkTrailCore::IsFileLoaded()
{
    return pDecoder != nullptr && pDevice != nullptr;
}

void YorkTrail::YorkTrailCore::FileClose()
{
    if (pDecoder != nullptr)
    {
        ma_decoder_uninit(pDecoder);
        ma_device_uninit(pDevice);
        pDecoder = nullptr;
        pDevice = nullptr;
        totalPCMFrames = 0;
        curFrame = 0;
        startFrame = 0;
        endFrame = 0;
        path = nullptr;
        soundtouch_clear(hSoundTouch);
        pRubberBand->~RubberBandStretcher();
        pRubberBand = nullptr;
        NotifyTimeChanged();
    }
}

void YorkTrail::YorkTrailCore::DeviceClose()
{
    if (pDevice != nullptr)
    {
        ma_device_uninit(pDevice);
        pDevice = nullptr;
    }
}

void YorkTrail::YorkTrailCore::Pause()
{
    if (state == State::Playing)
    {
        state = State::Pausing;
    }
}

void YorkTrail::YorkTrailCore::Stop()
{
    if (state == State::Pausing)
    {
        state = State::Stopped;
    }
    else if (state == State::Playing)
    {
        state = State::Stopping;
    }
}

void YorkTrail::YorkTrailCore::ResetRMS()
{
    rmsL = -100.0f;
    rmsR = -100.0f;
    NotifyTimeChanged();
}

void YorkTrail::YorkTrailCore::Seek(int64_t frame)
{
    if (pDecoder != nullptr)
    {
        frame = max(frame, 0);
        frame = min(frame, totalPCMFrames);

        ma_mutex_lock(pMutex);
        if (ma_decoder_seek_to_pcm_frame(pDecoder, frame) != MA_SUCCESS)
        {
            throwError("ma_decoder", "シーク時にエラーが発生しました");
        }
        curFrame = (uint64_t)frame;
        ma_mutex_unlock(pMutex);
    }
}

void YorkTrail::YorkTrailCore::SetFrame(uint64_t frame)
{
    Seek(frame);
}

void YorkTrail::YorkTrailCore::SeekRelative(long ms)
{
    if (pDecoder != nullptr)
    {
        int64_t addFrame = (int64_t)pDecoder->outputSampleRate * ms / 1000;
        int64_t targetFrame = curFrame + addFrame;
        Seek(targetFrame);
    }
}

uint64_t YorkTrail::YorkTrailCore::GetTime()
{
    uint64_t curTime = 0;

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

double YorkTrail::YorkTrailCore::GetPosition()
{
    double pos = 0;
    if (pDecoder != nullptr)
    {
        pos = (double)curFrame / totalPCMFrames;
    }
    return pos;
}

uint64_t YorkTrail::YorkTrailCore::GetTotalMilliSeconds()
{
    return frameToMillisecs(totalPCMFrames);
}

void YorkTrail::YorkTrailCore::SetPosition(double pos)
{
    if (pDecoder != nullptr)
    {
        int64_t targetFrame = totalPCMFrames * pos;
        Seek(targetFrame);
    }
}

void YorkTrail::YorkTrailCore::SetStartPosition(double pos)
{
    int64_t targetFrame = max((int64_t)totalPCMFrames * pos, 0);
    startFrame = (uint64_t)targetFrame;
}

void YorkTrail::YorkTrailCore::SetEndPosition(double pos)
{
    int64_t targetFrame = min((int64_t)totalPCMFrames * pos, totalPCMFrames);
    endFrame = (uint64_t)targetFrame;
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

void YorkTrail::YorkTrailCore::SetRatio(float ratio)
{
    if (pDecoder != nullptr)
    {
        ma_mutex_lock(pMutex);
        playbackRatio = ratio;
        if (stretchMethod == StretchMethod::SoundTouch)
        {
            soundtouch_clear(hSoundTouch);
            soundtouch_setTempo(hSoundTouch, ratio);
        }
        else if (stretchMethod == StretchMethod::RubberBand)
        {
            //pRubberBand->reset();
            pRubberBand->setTimeRatio(1.0 / (double)ratio);
        }
        ma_mutex_unlock(pMutex);
    }
    else
    {
        playbackRatio = ratio;
    }
}

float YorkTrail::YorkTrailCore::GetRatio()
{
    return playbackRatio;
}

void YorkTrail::YorkTrailCore::SetPitch(float pitch)
{
    if (pDecoder != nullptr)
    {
        ma_mutex_lock(pMutex);
        playbackPitch = pitch;
        if (stretchMethod == StretchMethod::SoundTouch)
        {
            soundtouch_clear(hSoundTouch);
            soundtouch_setPitch(hSoundTouch, pitch);
        }
        else if (stretchMethod == StretchMethod::RubberBand)
        {
            //pRubberBand->reset();
            pRubberBand->setPitchScale((double)pitch);
        }
        ma_mutex_unlock(pMutex);
    }
    else
    {
        playbackPitch = pitch;
    }
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
    ma_mutex_lock(pMutex);
    channels = ch;
    ma_mutex_unlock(pMutex);
}

YorkTrail::Channels YorkTrail::YorkTrailCore::GetChannels()
{
    return channels;
}

void YorkTrail::YorkTrailCore::SetSoundTouchParam(int seq, int window, int overlap)
{
    soundtouch_setSetting(hSoundTouch, 3, seq);// SETTING_SEQUENCE_MS default 40
    soundtouch_setSetting(hSoundTouch, 4, window);// SETTING_SEEKWINDOW_MS default 15
    soundtouch_setSetting(hSoundTouch, 5, overlap);// SETTING_OVERLAP_MS default 8
}

// 別スレッドで動作させる
void YorkTrail::YorkTrailCore::Start()
{
    if (path == nullptr)
        return;

    state = State::Playing;
    ma_result result;

    if (ma_device_start(pDevice) != MA_SUCCESS)
    {
        throwError("ma_device_start", "デバイスをスタートできません");
        return;
    }

    while (state == State::Playing)
    {
        if (!ma_device_is_started(pDevice))
        {
            // 再スタートしたときの処理
            bool started = false;
            for (int i = 0; i < 100; i++)
            {
                if (ma_device_is_started(pDevice))
                {
                    started = true;
                    break;
                }
                Sleep(10);
            }
            if (!started)
            {
                break;
            }
        }
        if (curFrame >= endFrame)
        {
            if (isLoop)
            {
                Seek(startFrame);
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

    if (state != State::Pausing)
    {
        state = State::Stopped;
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

float YorkTrail::YorkTrailCore::lerp(float v1, float v2, float t)
{
    //三次補完
    float rate = t * t * (3.0f - 2.0f * t);
    return (1.0f - rate) * v1 + rate * v2;
}

void YorkTrail::YorkTrailCore::toDeinterleaved(std::vector<float>& input, float* const* output, int channels, int frameCount)
{
    for (int i = 0; i < frameCount; i++)
    {
        for (int j = 0; j < channels; j++)
        {
            output[j][i] = input[i * 2 + j];
        }
    }
}

void YorkTrail::YorkTrailCore::toInterleaved(const float* const* input, std::vector<float>& output, int channels, int frameCount)
{
    if (!output.empty())
    {
        output.clear();
    }
    for (int i = 0; i < frameCount; i++)
    {
        for (int j = 0; j < channels; j++)
        {
            output.push_back(input[j][i]);
        }
    }
}

void YorkTrail::YorkTrailCore::calcVolume(float vol, std::vector<float> &input)
{
    for (int i = 0; i < input.size(); i++)
    {
        input[i] *= vol;
    }
}

void YorkTrail::YorkTrailCore::calcRMS(std::vector<float> &input, int frameCount, int channels, float &outputL, float &outputR)
{
    int requireCh = min(channels, 2);
    std::vector<float> sum(requireCh);
    std::vector<float> val(requireCh);
    int bufferSize = frameCount * channels;

    for (int i = 0; i < bufferSize; i += channels)
    {
        for (int j = 0; j < requireCh; j++)
        {
            sum[j] += pow(input[i + j], 2);
        }
    }

    for (int i = 0; i < requireCh; i++)
    {
        val[i] = 20.0 * log10(sqrt(sum[i] / frameCount));
    }
    
    if (requireCh < 2)
    {
        outputL = (std::isinf(val[0])) ? -100.0f : val[0];
        outputR = outputL;
    }
    else
    {
        outputL = (std::isinf(val[0])) ? -100.0f : val[0];
        outputR = (std::isinf(val[1])) ? -100.0f : val[1];
    }
}

void YorkTrail::YorkTrailCore::miniaudioStartCallback(ma_device* pDevice, void* pOutput, const void* pInput, ma_uint32 frameCount)
{
    ma_mutex_lock(pMutex);

    ma_uint32 bufferSize = frameCount * pDecoder->outputChannels;
    std::vector<float> decodedFrames(bufferSize * playbackRatio * 2, 0.0f);
    std::vector<float> processdFrames(bufferSize * 2, 0.0f);
    ma_uint32 frameRead;
    ma_uint32 frameProcessed;

    if (isBypass)
    {
        frameRead = ma_decoder_read_pcm_frames(pDecoder, decodedFrames.data(), frameCount);
        if (frameRead < frameCount) {
            // Reached the end.
            Debug::WriteLine("Cur:{0}\r\nRead:{1}\r\nTotal:{2}", curFrame, frameRead, totalPCMFrames);
            //totalPCMFrames = min(curFrame + frameRead, totalPCMFrames);
            state = State::Stopping;
        }
    }
    else
    {
        ma_uint64 requireFrames = frameCount * playbackRatio;
        frameRead = ma_decoder_read_pcm_frames(pDecoder, decodedFrames.data(), requireFrames);
        if (frameRead < requireFrames) {
            // Reached the end.
            Debug::WriteLine("Cur:{0} Count:{1} Read:{2} Total:{3}", curFrame, requireFrames, frameRead, totalPCMFrames);
            //totalPCMFrames = curFrame + frameRead;
            state = State::Stopping;
        }
    }

    frameProcessed = frameRead;
    curFrame += frameRead;
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
        if (playbackPitch != 1.0f || playbackRatio != 1.0f)
        {
            if (stretchMethod == StretchMethod::RubberBand)
            {
                float** deintInputFrames = new float* [pDecoder->outputChannels];
                float** deintOutputFrames = new float* [pDecoder->outputChannels];

                for (int i = 0; i < pDecoder->outputChannels; i++)
                {
                    deintInputFrames[i] = new float[frameRead * 2];
                    deintOutputFrames[i] = new float[frameCount * 2];
                }

                toDeinterleaved(decodedFrames, deintInputFrames, pDecoder->outputChannels, frameRead);
                pRubberBand->process(deintInputFrames, frameRead, false);
                size_t frameRequired = pRubberBand->getSamplesRequired();
                int available = pRubberBand->available();
                if (available < frameCount)
                {
                    Debug::WriteLine("Rubber Band processed: {0}/{1} frames", frameRequired, frameCount);
                    for (int i = 0; i < pDecoder->outputChannels; i++)
                    {
                        delete deintInputFrames[i];
                        delete deintOutputFrames[i];
                    }
                    delete deintInputFrames;
                    delete deintOutputFrames;
                    ma_mutex_unlock(pMutex);
                    return;
                }
                frameProcessed = pRubberBand->retrieve(deintOutputFrames, frameCount);
                toInterleaved(deintOutputFrames, processdFrames, pDecoder->outputChannels, frameProcessed);

                for (int i = 0; i < pDecoder->outputChannels; i++)
                {
                    delete deintInputFrames[i];
                    delete deintOutputFrames[i];
                }
                delete deintInputFrames;
                delete deintOutputFrames;
            }
            else
            {
                soundtouch_putSamples(hSoundTouch, decodedFrames.data(), frameRead);
                frameProcessed = soundtouch_receiveSamples(hSoundTouch, processdFrames.data(), frameCount);
                if (frameProcessed < frameCount)
                {
                    Debug::WriteLine("SoundTouch processed: {0}/{1} frames", frameProcessed, frameCount);
                    ma_mutex_unlock(pMutex);
                    return;
                }
            }
        }
        else
        {
            std::copy(decodedFrames.begin(), decodedFrames.end(), processdFrames.begin());
        }

        if (LpfEnabled)
        {
            ma_lpf_process_pcm_frames(pLpf, processdFrames.data(), processdFrames.data(), frameCount);
        }
        if (HpfEnabled)
        {
            ma_hpf_process_pcm_frames(pHpf, processdFrames.data(), processdFrames.data(), frameCount);
        }
        if (BpfEnabled)
        {
            ma_bpf_process_pcm_frames(pBpf, processdFrames.data(), processdFrames.data(), frameCount);
        }

        if (pDecoder->outputChannels >= 2)
        {
            switch (channels)
            {
            case Channels::LOnly:
                for (int i = 0; i < frameCount * pDecoder->outputChannels; i += 2)
                {
                    processdFrames[i + 1] = processdFrames[i];
                }
                break;
            case Channels::ROnly:
                for (int i = 0; i < frameCount * pDecoder->outputChannels; i += 2)
                {
                    processdFrames[i] = processdFrames[i + 1];
                }
                break;
            case Channels::Mono:
                for (int i = 0; i < frameCount * pDecoder->outputChannels; i += 2)
                {
                    float val = (processdFrames[i] + processdFrames[i + 1]) / 2;
                    processdFrames[i] = processdFrames[i + 1] = val;
                }
                break;
            case Channels::LMinusR:
                for (int i = 0; i < frameCount * pDecoder->outputChannels; i += 2)
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
    calcRMS(processdFrames, frameCount, pDecoder->outputChannels, l, r);
    rmsL = l;
    rmsR = r;

    std::memcpy(pOutput, processdFrames.data(), sizeof(float) * frameProcessed * pDecoder->outputChannels);

    ma_mutex_unlock(pMutex);
}

void YorkTrail::callback(ma_device* pDevice, void* pOutput, const void* pInput, ma_uint32 frameCount)
{
    YorkTrailCore^ core = YorkTrailHandleHolder::hYorkTrailCore;
    core->miniaudioStartCallback(pDevice, pOutput, pInput, frameCount);
}
