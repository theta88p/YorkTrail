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
#include "YorkTrailCore.h"

YorkTrail::YorkTrailCore::YorkTrailCore()
{
    pMutex = new ma_mutex();
    ma_mutex_init(pMutex);
    hSoundTouch = soundtouch_createInstance();
    SetSoundTouchParam(80, 30, 8);
    stretchMethod = StretchMethod::RubberBand;
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
    
    if (hBpm != nullptr)
    {
        bpm_destroyInstance(hBpm);
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
    if (pCurrentDecoder != nullptr)
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
    if (pCurrentDecoder != nullptr && pCurrentDecoder->outputSampleRate > 0)
    {
        return frame * 1000 / pCurrentDecoder->outputSampleRate;
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

void YorkTrail::YorkTrailCore::SetPlaybackDevice(int index)
{
    if (pDevice != nullptr)
    {
        if (pCurrentDecoder != nullptr)
        {
            Stop();
        }
        DeviceClose();
        playbackDevice = index;
        DeviceOpen(pCurrentDecoder);
    }
    else
    {
        playbackDevice = index;
    }
}

int YorkTrail::YorkTrailCore::GetPlaybackDevice()
{
    return playbackDevice;
}

int YorkTrail::YorkTrailCore::decoderInit(ma_decoder* %dec, String^ p, ma_uint32 outputChannels, ma_uint32 outputSampleRate)
{
    if (dec == nullptr)
    {
        dec = new ma_decoder();
    }

    marshal_context^ context = gcnew marshal_context();
    auto path = context->marshal_as<const char*>(p);

    ma_decoder_config config = ma_decoder_config_init(ma_format_f32, outputChannels, outputSampleRate);
    if (ma_decoder_init_file(path, &config, dec) != MA_SUCCESS)
    {
        throwError("ma_decoder", "デコーダの初期化時にエラーが発生しました");
        return false;
    }
    return true;
}

bool YorkTrail::YorkTrailCore::StemFilesOpen(String^ folder)
{
    if (!decoderInit(pDecoderVocals, Path::Combine(folder, "vocals.flac"), NULL, NULL))
    {
        return false;
    }
    if (!decoderInit(pDecoderDrums, Path::Combine(folder, "drums.flac"), NULL, NULL))
    {
        return false;
    }
    if (!decoderInit(pDecoderBass, Path::Combine(folder, "bass.flac"), NULL, NULL))
    {
        return false;
    }
    if (!decoderInit(pDecoderPiano, Path::Combine(folder, "piano.flac"), NULL, NULL))
    {
        return false;
    }
    if (!decoderInit(pDecoderOther, Path::Combine(folder, "other.flac"), NULL, NULL))
    {
        return false;
    }
    return true;
}

void YorkTrail::YorkTrailCore::StemFilesClose()
{
    if (pDecoderVocals != nullptr)
    {
        ma_decoder_uninit(pDecoderVocals);
        ma_decoder_uninit(pDecoderDrums);
        ma_decoder_uninit(pDecoderBass);
        ma_decoder_uninit(pDecoderPiano);
        ma_decoder_uninit(pDecoderOther);
    }
}

bool YorkTrail::YorkTrailCore::SwitchDecoderToSource()
{
    return switchDecoder(pDecoder);
}

bool YorkTrail::YorkTrailCore::SwitchDecoderToStems()
{
    return switchDecoder(pDecoderVocals);
}

bool YorkTrail::YorkTrailCore::switchDecoder(ma_decoder* dec)
{
    if (dec == nullptr)
    {
        throwError("SwitchDecoder", "デコーダがnullです");
        return false;
    }

    if (state == State::Playing)
    {
        Stop();
        while (state != State::Stopped)
        {
            Sleep(10);
        }
    }

    if (!DeviceOpen(dec))
    {
        return false;
    }

    ma_uint32 channels = dec->outputChannels;
    ma_uint32 sampleRate = dec->outputSampleRate;
    totalPCMFrames = getTotalPCMFrames(dec);
    pCurrentDecoder = dec;

    soundtouch_setChannels(hSoundTouch, channels);
    soundtouch_setSampleRate(hSoundTouch, sampleRate);

    if (pRubberBand != nullptr)
    {
        pRubberBand->~RubberBandStretcher();
    }

    pRubberBand = new RubberBand::RubberBandStretcher((size_t)sampleRate, (size_t)channels
        , RubberBand::RubberBandStretcher::OptionProcessRealTime | RubberBand::RubberBandStretcher::OptionPitchHighQuality);

    return true;
}

ma_uint64 YorkTrail::YorkTrailCore::getTotalPCMFrames(ma_decoder* dec)
{
    ma_uint64* pFrames = new ma_uint64();
    if (ma_decoder_get_length_in_pcm_frames(dec, pFrames) != MA_SUCCESS)
    {
        throwError("ma_decoder_get_length_in_pcm_frames", "総フレーム数の取得中にエラーが発生しました");
    }
    ma_uint64 res = *pFrames;
    delete pFrames;
    return res;
}

bool YorkTrail::YorkTrailCore::FileOpen(String^ p, FileType t)
{
    filePath = p;
    extension = System::IO::Path::GetExtension(p);
    extension = extension->ToLower();
    fileType = t;

    if (!decoderInit(pDecoder, p, NULL, NULL))
    {
        return false;
    }
    pCurrentDecoder = pDecoder;
    totalPCMFrames = getTotalPCMFrames(pCurrentDecoder);
    endFrame = totalPCMFrames;
    Debug::WriteLine(totalPCMFrames);

    if (t == FileType::Mp3)
    {
        // 実際のフレーム数がTotalPCMFramesより少ない時があるのでこうする
        //totalPCMFrames -= 10000;

        if (pSeekPoints == nullptr)
        {
            pSeekPoints = new std::vector<drmp3_seek_point>(1024);
        }
        auto backend = (ma_mp3*)pDecoder->pBackend;

        if (backend != nullptr)
        {
            drmp3_uint32 seekPointCount = 1024;
            if (drmp3_calculate_seek_points(&backend->dr, &seekPointCount, pSeekPoints->data()))
            {
                drmp3_bind_seek_table(&backend->dr, seekPointCount, pSeekPoints->data());
            }
        }
    }
 
    if (!DeviceOpen(pDecoder))
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
    SetLpfFreq(lpfFreq);
    SetHpfFreq(hpfFreq);
    SetBpfFreq(bpfFreq);

    soundtouch_setChannels(hSoundTouch, pDecoder->outputChannels);
    soundtouch_setSampleRate(hSoundTouch, pDecoder->outputSampleRate);
    hBpm = bpm_createInstance(pDecoder->outputChannels, pDecoder->outputSampleRate);

    pRubberBand = new RubberBand::RubberBandStretcher((size_t)pDecoder->outputSampleRate, (size_t)pDecoder->outputChannels
        , RubberBand::RubberBandStretcher::OptionProcessRealTime | RubberBand::RubberBandStretcher::OptionPitchHighQuality);

    SetRatio(playbackRatio);
    SetPitch(playbackPitch);

    return true;
}

bool YorkTrail::YorkTrailCore::DeviceOpen(ma_decoder* dec)
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
    deviceConfig.playback.format = dec->outputFormat;
    deviceConfig.playback.channels = dec->outputChannels;
    deviceConfig.sampleRate = dec->outputSampleRate;
    //deviceConfig.dataCallback = (ma_device_callback_proc)(callback_type)(void*)ptr;
    deviceConfig.dataCallback = callback;
    deviceConfig.pUserData = dec;
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

List<float>^ YorkTrail::YorkTrailCore::GetVolumeList(int start, int count, int split)
{
    auto startTime = std::chrono::system_clock::now();

    float outputL, outputR;
    ma_uint32 volumeCount = count;
    ma_uint64 frameStep = totalPCMFrames / volumeCount / split;
    ma_uint64 readFrameCount = min(frameStep, 100);
    std::vector<float> buff(readFrameCount * 2);
    List<float>^ res = gcnew List<float>(count);

    ma_decoder* tempDecoder = new ma_decoder();
    decoderInit(tempDecoder, filePath, NULL, NULL);

    auto now = std::chrono::system_clock::now();
    Debug::WriteLine("Decoder Initialized " + std::chrono::duration_cast<std::chrono::milliseconds>(std::chrono::system_clock::now() - now).count());

    for (int i = start; i < start + count; i++)
    {
        ma_decoder_seek_to_pcm_frame(tempDecoder, frameStep * i + frameStep / 2);
        ma_uint64 frameRead;
        if (ma_decoder_read_pcm_frames(tempDecoder, buff.data(), readFrameCount, &frameRead) != MA_SUCCESS)
        {
            //throwError("ma_decoder", "デコードエラー");
        }

        if (frameRead < readFrameCount) {
            break;
        }

        calcRMS(buff, readFrameCount, tempDecoder->outputChannels, outputL, outputR);
        float outputLR = (outputL + outputR) / 2;
        outputLR = (outputLR < -70.0f) ? -70.0f : outputLR;
        res->Add(outputLR);
    }
    Debug::WriteLine("Calc End " + std::chrono::duration_cast<std::chrono::milliseconds>(std::chrono::system_clock::now() - now).count());
    now = std::chrono::system_clock::now();
    ma_decoder_uninit(tempDecoder);
    return res;
}

// 重い処理なので別スレッドで実行する
bool YorkTrail::YorkTrailCore::SeparateStem(String^ destFolder)
{
    progress = 0;
    auto stemSeparator = StemSeparator();
    std::error_code err = stemSeparator.Init();
    if (err)
    {
        throwError("spleeter", "spleeterの初期化時にエラーが発生しました");
        return false;
    }

    auto dest = Utils::Cli2Native(destFolder);
    auto encode = FlacEncode();
    if (encode.Init(dest))
    {
        throwError("FLAC", "FLACの初期化時にエラーが発生しました");
        return false;
    }

    ma_uint32 targetSampleRate = 44100;
    ma_uint32 targetChannels = 2;
    ma_uint64 currentFrame = 0;
    ma_uint64 processFrames = targetSampleRate * 10;
    ma_uint64 preloadFrames = targetSampleRate;
    ma_uint64 overlapFrames = targetSampleRate / 10;
    
    ma_decoder* tempDecoder = new ma_decoder();
    decoderInit(tempDecoder, filePath, targetChannels, targetSampleRate);

    std::vector<float> decoded((processFrames + preloadFrames + overlapFrames) * targetChannels);
    std::vector<std::vector<float>> outputs(5, std::vector<float>((processFrames + preloadFrames + overlapFrames) * targetChannels));
    std::vector<std::vector<FLAC__int32>> outputs_int32(5, std::vector<FLAC__int32>(processFrames * targetChannels));
    std::vector<std::vector<float>> overlap(5, std::vector<float>(overlapFrames * targetChannels));

    ma_uint64 tempTotalPCMFrames = getTotalPCMFrames(tempDecoder);

    while (currentFrame < tempTotalPCMFrames)
    {
        ma_uint64 readFrames;
        ma_uint64 requireFrames;
        ma_uint64 requirePreloadFrames;

        if (currentFrame > 0 && currentFrame > preloadFrames)
        {
            requireFrames = processFrames + preloadFrames + overlapFrames;
            requirePreloadFrames = preloadFrames;
        }
        else
        {
            requireFrames = processFrames + overlapFrames;
            requirePreloadFrames = 0;
        }

        ma_decoder_seek_to_pcm_frame(tempDecoder, currentFrame);
        if (ma_decoder_read_pcm_frames(tempDecoder, decoded.data(), requireFrames, &readFrames) != MA_SUCCESS)
        {
            //throwError("ma_decoder", "デコード中にエラーが発生しました");
        }

        if (stemSeparator.Process(decoded, outputs, readFrames))
        {
            throwError("StemSeparator", "Stem分離時にエラーが発生しました");
            return false;
        }
        ma_uint64 outputFrames = readFrames - requirePreloadFrames;

        for (int i = 0; i < 5; i++)
        {
            float volumeFactor = (currentFrame > 0) ? 0.0f : 1.0f;

            for (int j = 0; j < outputFrames * 2; j++)
            {
                if (currentFrame > 0 && j < overlapFrames * 2)
                {
                    outputs_int32[i][j] = toInt32(outputs[i][j + requirePreloadFrames * 2] * volumeFactor + overlap[i][j]);

                    if (j % 2 > 0)
                    {
                        volumeFactor = min(volumeFactor + 1.0f / overlapFrames * 2, 1.0f);
                    }
                }
                else if (currentFrame < totalPCMFrames - outputFrames && j >= (outputFrames - overlapFrames) * 2)
                {
                    overlap[i][j - (outputFrames - overlapFrames) * 2] = outputs[i][j + requirePreloadFrames * 2] * volumeFactor;

                    if (j % 2 > 0)
                    {
                        volumeFactor = max(volumeFactor - 1.0f / overlapFrames * 2, 0.0f);
                    }
                }
                else
                {
                    outputs_int32[i][j] = toInt32(outputs[i][j + requirePreloadFrames * 2]);
                }
            }
        }

        if (encode.Process(outputs_int32, outputFrames - overlapFrames))
        {
            throwError("FLAC", "FLACエンコード時にエラーが発生しました");
            return false;
        }
        
        if (processFrames + overlapFrames > readFrames)
        {
            break;
        }
        else if (currentFrame > 0)
        {
            currentFrame += outputFrames - overlapFrames;
        }
        else
        {
            currentFrame += outputFrames - preloadFrames - overlapFrames;
        }

        progress = (double)currentFrame / tempTotalPCMFrames;
    }

    encode.Uninit();
    ma_decoder_uninit(tempDecoder);
    stemSeparator.Uninit();

    return true;
}

double YorkTrail::YorkTrailCore::GetProgress()
{
    return progress;
}

bool YorkTrail::YorkTrailCore::IsFileLoaded()
{
    return pDecoder != nullptr && pDevice != nullptr;
}

void YorkTrail::YorkTrailCore::FileClose()
{
    StemFilesClose();

    if (pDecoder != nullptr)
    {
        ma_decoder_uninit(pDecoder);
        pDecoder = nullptr;
        ma_device_uninit(pDevice);
        pDevice = nullptr;
        totalPCMFrames = 0;
        curFrame = 0;
        startFrame = 0;
        endFrame = 0;
        filePath = nullptr;
        soundtouch_clear(hSoundTouch);
        bpm_destroyInstance(hBpm);
        hBpm = nullptr;
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
    rmsL = -70.0f;
    rmsR = -70.0f;
    NotifyTimeChanged();
}

void YorkTrail::YorkTrailCore::Seek(int64_t frame)
{
    if (pDecoder != nullptr)
    {
        frame = max(frame, 0);
        frame = min(frame, totalPCMFrames);

        ma_mutex_lock(pMutex);

        if (pCurrentDecoder == pDecoder)
        {
            if (ma_decoder_seek_to_pcm_frame(pDecoder, frame) != MA_SUCCESS)
            {
                throwError("ma_decoder", "シーク時にエラーが発生しました");
            }
        }
        else
        {
            int res = 0;
            res += ma_decoder_seek_to_pcm_frame(pDecoderVocals, frame);
            res += ma_decoder_seek_to_pcm_frame(pDecoderDrums, frame);
            res += ma_decoder_seek_to_pcm_frame(pDecoderBass, frame);
            res += ma_decoder_seek_to_pcm_frame(pDecoderPiano, frame);
            res += ma_decoder_seek_to_pcm_frame(pDecoderOther, frame);
            if (res != MA_SUCCESS)
            {
                throwError("ma_decoder", "シーク時にエラーが発生しました");
            }
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

void YorkTrail::YorkTrailCore::SetStemVolumes(float vocals, float drums, float bass, float piano, float other)
{
    volumeVocals = vocals;
    volumeDrums = drums;
    volumeBass = bass;
    volumePiano = piano;
    volumeOther = other;
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
        soundtouch_clear(hSoundTouch);
        soundtouch_setTempo(hSoundTouch, ratio);
        pRubberBand->reset();
        pRubberBand->setTimeRatio(1.0 / (double)ratio);
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
        soundtouch_clear(hSoundTouch);
        soundtouch_setPitch(hSoundTouch, pitch);
        pRubberBand->reset();
        pRubberBand->setPitchScale((double)pitch);
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

void YorkTrail::YorkTrailCore::SetLpfFreq(float value)
{
    if (pDecoder != nullptr)
    {
        ma_mutex_lock(pMutex);
        ma_lpf_config config = ma_lpf_config_init(ma_format_f32, pCurrentDecoder->outputChannels, pCurrentDecoder->outputSampleRate, value, MA_MAX_FILTER_ORDER);
        if (ma_lpf_init(&config, NULL, pLpf) != MA_SUCCESS)
        {
            throwError("ma_lpf_init", "LPFの初期化時にエラーが発生しました");
        }
        ma_mutex_unlock(pMutex);
    }
    lpfFreq = value;
}

void YorkTrail::YorkTrailCore::SetHpfFreq(float value)
{
    if (pDecoder != nullptr)
    {
        ma_mutex_lock(pMutex);
        ma_hpf_config config = ma_hpf_config_init(ma_format_f32, pCurrentDecoder->outputChannels, pCurrentDecoder->outputSampleRate, value, MA_MAX_FILTER_ORDER);
        if (ma_hpf_init(&config, NULL, pHpf) != MA_SUCCESS)
        {
            throwError("ma_hpf_init", "HPFの初期化時にエラーが発生しました");
        }
        ma_mutex_unlock(pMutex);
    }
    hpfFreq = value;
}

void YorkTrail::YorkTrailCore::SetBpfFreq(float value)
{
    if (pDecoder != nullptr)
    {
        ma_mutex_lock(pMutex);
        ma_bpf_config config = ma_bpf_config_init(ma_format_f32, pCurrentDecoder->outputChannels, pCurrentDecoder->outputSampleRate, value, 2);
        if (ma_bpf_init(&config, NULL, pBpf) != MA_SUCCESS)
        {
            throwError("ma_bpf_init", "BPFの初期化時にエラーが発生しました");
        }
        ma_mutex_unlock(pMutex);
    }
    bpfFreq = value;
}

float YorkTrail::YorkTrailCore::GetLpfFreq()
{
    return lpfFreq;
}

float YorkTrail::YorkTrailCore::GetHpfFreq()
{
    return hpfFreq;
}

float YorkTrail::YorkTrailCore::GetBpfFreq()
{
    return bpfFreq;
}

void YorkTrail::YorkTrailCore::SetLpfEnabled(bool value)
{
    lpfEnabled = value;
}

void YorkTrail::YorkTrailCore::SetHpfEnabled(bool value)
{
    hpfEnabled = value;
}

void YorkTrail::YorkTrailCore::SetBpfEnabled(bool value)
{
    bpfEnabled = value;
}

bool YorkTrail::YorkTrailCore::GetLpfEnabled()
{
    return lpfEnabled;
}

bool YorkTrail::YorkTrailCore::GetHpfEnabled()
{
    return hpfEnabled;
}

bool YorkTrail::YorkTrailCore::GetBpfEnabled()
{
    return bpfEnabled;
}

void YorkTrail::YorkTrailCore::SetLoop(bool value)
{
    isLoop = value;
}

bool YorkTrail::YorkTrailCore::GetLoop()
{
    return isLoop;
}

float YorkTrail::YorkTrailCore::GetRmsL()
{
    return rmsL;
}

float YorkTrail::YorkTrailCore::GetRmsR()
{
    return rmsR;
}

void YorkTrail::YorkTrailCore::SetBPMCalc(bool input)
{
    isBPMCalc = input;
}

float YorkTrail::YorkTrailCore::GetBPM()
{
    return bpm_getBpm(hBpm);
}

void YorkTrail::YorkTrailCore::SetBypass(bool value)
{
    ma_mutex_lock(pMutex);
    isBypass = value;
    ma_mutex_unlock(pMutex);
}

bool YorkTrail::YorkTrailCore::GetBypass()
{
    return isBypass;
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

void YorkTrail::YorkTrailCore::SetStretchMethod(YorkTrail::StretchMethod value)
{
    ma_mutex_lock(pMutex);
    stretchMethod = value;
    ma_mutex_unlock(pMutex);
}

YorkTrail::StretchMethod YorkTrail::YorkTrailCore::GetStretchMethod()
{
    return stretchMethod;
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
    if (filePath == nullptr)
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
    while (ma_device_get_state(pDevice) == ma_device_state_started || ma_device_get_state(pDevice) == ma_device_state_stopping)
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

float YorkTrail::YorkTrailCore::clip(const float input)
{
    float x = input;
    x = ((x < -1) ? -1 : ((x > 1) ? 1 : x));    /* clip */
    return x;

}

FLAC__int32 YorkTrail::YorkTrailCore::toInt32(const float input)
{
    float x = input;
    x = clip(x);
    x = x * 32767.0f;                           /* -1..1 to -32767..32767 */

    return (FLAC__int32)x;
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
        outputL = (val[0] < -70.0f) ? -70.0f : val[0];
        outputR = outputL;
    }
    else
    {
        outputL = (val[0] < -70.0f) ? -70.0f : val[0];
        outputR = (val[0] < -70.0f) ? -70.0f : val[1];
    }
}

void YorkTrail::YorkTrailCore::miniaudioStartCallback(ma_device* pDevice, void* pOutput, const void* pInput, ma_uint32 frameCount)
{
    ma_mutex_lock(pMutex);

    ma_uint32 outputChannels = pCurrentDecoder->outputChannels;
    ma_uint32 outputSampleRate = pCurrentDecoder->outputSampleRate;
    ma_uint32 bufferSize = frameCount * outputChannels;
    std::vector<float> decodedFrames(bufferSize * playbackRatio * 2, 0.0f);
    std::vector<float> processedFrames(bufferSize * 2, 0.0f);
    ma_uint64 frameRead;
    ma_uint64 frameProcessed;
    ma_uint64 requireFrames = (isBypass) ? frameCount : frameCount * playbackRatio;

    // Stem再生
    if (pCurrentDecoder != pDecoder)
    {
        std::vector<std::vector<float>> df(5, std::vector<float>(bufferSize * playbackRatio * 2, 0.0f));
        std::vector<ma_uint64> fr(5);
        std::vector<ma_result> res(5);
        res[0] = ma_decoder_read_pcm_frames(pDecoderVocals, df[0].data(), requireFrames, &fr[0]);
        res[1] = ma_decoder_read_pcm_frames(pDecoderDrums, df[1].data(), requireFrames, &fr[1]);
        res[2] = ma_decoder_read_pcm_frames(pDecoderBass, df[2].data(), requireFrames, &fr[2]);
        res[3] = ma_decoder_read_pcm_frames(pDecoderPiano, df[3].data(), requireFrames, &fr[3]);
        res[4] = ma_decoder_read_pcm_frames(pDecoderOther, df[4].data(), requireFrames, &fr[4]);
        /*
        for (int i = 0; i < 5; i++)
        {
            if (res[i] != MA_SUCCESS)
            {
                return;
            }
        }
        */

        if (fr[0] == fr[1] && fr[0] == fr[2] && fr[0] == fr[3] && fr[0] == fr[4])
        {
            frameRead = fr[0];

            for (ma_uint32 i = 0; i < fr[0] * 2; i++)
            {
                float x = 0;
                x += df[0][i] * volumeVocals;
                x += df[1][i] * volumeDrums;
                x += df[2][i] * volumeBass;
                x += df[3][i] * volumePiano;
                x += df[4][i] * volumeOther;
                decodedFrames[i] = clip(x);
            }
        }
    }
    else
    {
        ma_decoder_read_pcm_frames(pDecoder, decodedFrames.data(), requireFrames, &frameRead);
    }

    if (frameRead < requireFrames) {
        // Reached the end.
        Debug::WriteLine("Cur:{0} Count:{1} Read:{2} Total:{3}", curFrame, requireFrames, frameRead, totalPCMFrames);
        //totalPCMFrames = curFrame + frameRead;
        if (!isLoop)
        {
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
                float** deintInputFrames = new float* [outputChannels];
                float** deintOutputFrames = new float* [outputChannels];

                for (ma_uint32 i = 0; i < outputChannels; i++)
                {
                    deintInputFrames[i] = new float[frameRead * 2];
                    deintOutputFrames[i] = new float[frameCount * 2];
                }

                toDeinterleaved(decodedFrames, deintInputFrames, outputChannels, frameRead);
                pRubberBand->process(deintInputFrames, frameRead, false);
                size_t frameRequired = pRubberBand->getSamplesRequired();
                int available = pRubberBand->available();
                if (available < frameCount)
                {
                    Debug::WriteLine("Rubber Band processed: {0}/{1} frames", frameRequired, frameCount);
                    for (ma_uint32 i = 0; i < outputChannels; i++)
                    {
                        delete[] deintInputFrames[i];
                        delete[] deintOutputFrames[i];
                    }
                    delete[] deintInputFrames;
                    delete[] deintOutputFrames;
                    ma_mutex_unlock(pMutex);
                    return;
                }
                frameProcessed = pRubberBand->retrieve(deintOutputFrames, frameCount);
                toInterleaved(deintOutputFrames, processedFrames, outputChannels, frameProcessed);

                for (int i = 0; i < outputChannels; i++)
                {
                    delete[] deintInputFrames[i];
                    delete[] deintOutputFrames[i];
                }
                delete[] deintInputFrames;
                delete[] deintOutputFrames;
            }
            else
            {
                soundtouch_putSamples(hSoundTouch, decodedFrames.data(), frameRead);
                frameProcessed = soundtouch_receiveSamples(hSoundTouch, processedFrames.data(), frameCount);
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
            std::copy(decodedFrames.begin(), decodedFrames.end(), processedFrames.begin());
        }

        if (lpfEnabled)
        {
            ma_lpf_process_pcm_frames(pLpf, processedFrames.data(), processedFrames.data(), frameCount);
        }
        if (hpfEnabled)
        {
            ma_hpf_process_pcm_frames(pHpf, processedFrames.data(), processedFrames.data(), frameCount);
        }
        if (bpfEnabled)
        {
            ma_bpf_process_pcm_frames(pBpf, processedFrames.data(), processedFrames.data(), frameCount);
        }

        if (outputChannels >= 2)
        {
            switch (channels)
            {
            case Channels::LOnly:
                for (ma_uint32 i = 0; i < frameCount * outputChannels; i += 2)
                {
                    processedFrames[i + 1] = processedFrames[i];
                }
                break;
            case Channels::ROnly:
                for (ma_uint32 i = 0; i < frameCount * outputChannels; i += 2)
                {
                    processedFrames[i] = processedFrames[i + 1];
                }
                break;
            case Channels::Mono:
                for (ma_uint32 i = 0; i < frameCount * outputChannels; i += 2)
                {
                    float val = (processedFrames[i] + processedFrames[i + 1]) / 2;
                    processedFrames[i] = processedFrames[i + 1] = val;
                }
                break;
            case Channels::LMinusR:
                for (ma_uint32 i = 0; i < frameCount * outputChannels; i += 2)
                {
                    float val = (processedFrames[i] - processedFrames[i + 1]) / 2;
                    processedFrames[i] = processedFrames[i + 1] = val;
                }
                break;
            case Channels::Stereo:
                break;
            }
        }
    }
    else
    {
        std::copy(decodedFrames.begin(), decodedFrames.end(), processedFrames.begin());
    }
    
    if (isBPMCalc)
    {
        bpm_putSamples(hBpm, processedFrames.data(), frameProcessed);
    }
    float l, r;
    calcRMS(processedFrames, frameCount, outputChannels, l, r);
    rmsL = l;
    rmsR = r;

    std::memcpy(pOutput, processedFrames.data(), sizeof(float) * frameProcessed * outputChannels);

    ma_mutex_unlock(pMutex);
}

void YorkTrail::callback(ma_device* pDevice, void* pOutput, const void* pInput, ma_uint32 frameCount)
{
    YorkTrailCore^ core = YorkTrailHandleHolder::GetYorkTrailCore();
    core->miniaudioStartCallback(pDevice, pOutput, pInput, frameCount);
}
